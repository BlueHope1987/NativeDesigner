using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Controls
{
    public enum CanvasTool
    {
        Select,
        Connect
    }

    public class DrawingCanvas : Control
    {
        private DrawingDocument _document = new DrawingDocument();
        private float _zoom = 1.0f;
        private PointF _offset = new PointF(0, 0);
        private CanvasTool _currentTool = CanvasTool.Select;
        private bool _isDragging = false;
        private bool _isConnecting = false;
        private bool _isSelecting = false;
        private PointF _dragStart;
        private PointF _lastMousePos;
        private ShapeBase _dragShape;
        private List<ShapeBase> _draggingShapes = new List<ShapeBase>();
        private PointF[] _dragOriginalPositions;
        private ShapeBase _connectStartShape;
        private PointF _connectStartPoint;
        private PointF _connectCurrentPoint;
        private RectangleF _selectionRect;
        private ShapeBase _hoveredShape;
        private Connection _hoveredConnection;

        private BufferedGraphics _bufferedGraphics;
        private BufferedGraphicsContext _bufferContext;
        private Size _bufferSize;

        public DrawingCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            this.BackColor = GlobalConfig.Instance.CanvasBackground;
            this.AllowDrop = true;
            this.Focus();

            _bufferContext = BufferedGraphicsManager.Current;
            _bufferContext.MaximumBuffer = new Size(4096, 4096);

            GlobalConfig.Instance.Changed += (s, e) => Invalidate();
            _document.DocumentChanged += (s, e) => Invalidate();
        }

        public DrawingDocument Document { get { return _document; } }

        public CanvasTool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                _currentTool = value;
                if (value == CanvasTool.Select)
                    Cursor = Cursors.Default;
                else if (value == CanvasTool.Connect)
                    Cursor = Cursors.Cross;
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = Math.Max(0.1f, Math.Min(5.0f, value));
                Invalidate();
            }
        }

        public PointF Offset
        {
            get { return _offset; }
            set { _offset = value; Invalidate(); }
        }

        public event EventHandler SelectionChanged;
        public event EventHandler DocumentModified;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bufferedGraphics == null || _bufferSize != ClientSize)
            {
                _bufferedGraphics?.Dispose();
                _bufferedGraphics = _bufferContext.Allocate(e.Graphics, ClientRectangle);
                _bufferSize = ClientSize;
            }

            Graphics g = _bufferedGraphics.Graphics;
            g.Clear(BackColor);

            if (GlobalConfig.Instance.AntiAlias)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            }

            g.TranslateTransform(_offset.X, _offset.Y);
            g.ScaleTransform(_zoom, _zoom);

            DrawGrid(g);
            DrawConnections(g);
            DrawShapes(g);
            DrawRubberBand(g);
            DrawSelectionRect(g);

            g.ResetTransform();
            _bufferedGraphics.Render(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            if (!GlobalConfig.Instance.ShowGrid) return;

            float gridSize = GlobalConfig.Instance.GridSize;
            float left = (-_offset.X) / _zoom;
            float top = (-_offset.Y) / _zoom;
            float right = left + ClientSize.Width / _zoom;
            float bottom = top + ClientSize.Height / _zoom;

            float startX = (float)Math.Floor(left / gridSize) * gridSize;
            float startY = (float)Math.Floor(top / gridSize) * gridSize;

            using (Pen pen = new Pen(GlobalConfig.Instance.GridColor))
            {
                pen.Width = 0.5f / _zoom;

                for (float x = startX; x <= right; x += gridSize)
                {
                    g.DrawLine(pen, x, top, x, bottom);
                }
                for (float y = startY; y <= bottom; y += gridSize)
                {
                    g.DrawLine(pen, left, y, right, y);
                }
            }
        }

        private void DrawShapes(Graphics g)
        {
            var shapes = _document.Shapes.OrderBy(s => s.ZOrder).ToList();
            foreach (var shape in shapes)
            {
                if (shape.Visible && shape.Parent == null)
                    shape.Draw(g, _zoom);
            }
        }

        private void DrawConnections(Graphics g)
        {
            foreach (var conn in _document.Connections)
            {
                conn.Draw(g, _zoom);
            }
        }

        private void DrawRubberBand(Graphics g)
        {
            if (!_isConnecting) return;

            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 1.5f / _zoom))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.DrawLine(pen, _connectStartPoint, _connectCurrentPoint);
            }
        }

        private void DrawSelectionRect(Graphics g)
        {
            if (!_isSelecting) return;

            using (Brush brush = new SolidBrush(Color.FromArgb(30, 0, 120, 215)))
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 1f / _zoom))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                g.FillRectangle(brush, _selectionRect);
                g.DrawRectangle(pen, _selectionRect.X, _selectionRect.Y, _selectionRect.Width, _selectionRect.Height);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();

            PointF worldPos = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Middle)
            {
                _lastMousePos = new PointF(e.X, e.Y);
                Cursor = Cursors.Hand;
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (_currentTool == CanvasTool.Connect)
            {
                StartConnection(worldPos);
                return;
            }

            var shape = _document.HitTestShape(worldPos);
            var conn = _document.HitTestConnection(worldPos, 6f / _zoom);

            if (shape != null)
            {
                if (!shape.Selected && (Control.ModifierKeys & Keys.Control) != Keys.Control)
                    _document.ClearSelection();

                shape.Selected = true;
                _dragShape = shape;
                _isDragging = true;
                _dragStart = worldPos;

                _draggingShapes = _document.GetSelectedShapes();
                _dragOriginalPositions = _draggingShapes.Select(s => new PointF(s.X, s.Y)).ToArray();

                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (conn != null)
            {
                _document.ClearSelection();
                conn.Selected = true;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _document.ClearSelection();
                _isSelecting = true;
                _dragStart = worldPos;
                _selectionRect = new RectangleF(worldPos.X, worldPos.Y, 0, 0);
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            PointF worldPos = ScreenToWorld(e.Location);

            if (e.Button == MouseButtons.Middle)
            {
                float dx = (e.X - _lastMousePos.X) / _zoom;
                float dy = (e.Y - _lastMousePos.Y) / _zoom;
                _offset.X += e.X - _lastMousePos.X;
                _offset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = new PointF(e.X, e.Y);
                Invalidate();
                return;
            }

            if (_isDragging && _draggingShapes.Count > 0)
            {
                float dx = worldPos.X - _dragStart.X;
                float dy = worldPos.Y - _dragStart.Y;

                for (int i = 0; i < _draggingShapes.Count; i++)
                {
                    var s = _draggingShapes[i];
                    float newX = _dragOriginalPositions[i].X + dx;
                    float newY = _dragOriginalPositions[i].Y + dy;

                    if (GlobalConfig.Instance.SnapToGrid)
                    {
                        float grid = GlobalConfig.Instance.GridSize;
                        newX = (float)Math.Round(newX / grid) * grid;
                        newY = (float)Math.Round(newY / grid) * grid;
                    }

                    s.X = newX;
                    s.Y = newY;
                }

                DocumentModified?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
            else if (_isConnecting)
            {
                _connectCurrentPoint = worldPos;
                Invalidate();
            }
            else if (_isSelecting)
            {
                float x = Math.Min(_dragStart.X, worldPos.X);
                float y = Math.Min(_dragStart.Y, worldPos.Y);
                float w = Math.Abs(worldPos.X - _dragStart.X);
                float h = Math.Abs(worldPos.Y - _dragStart.Y);
                _selectionRect = new RectangleF(x, y, w, h);
                Invalidate();
            }
            else
            {
                var shape = _document.HitTestShape(worldPos);
                var conn = _document.HitTestConnection(worldPos, 6f / _zoom);

                if (shape != _hoveredShape || conn != _hoveredConnection)
                {
                    if (_hoveredShape != null) _hoveredShape.Hovered = false;
                    _hoveredShape = shape;
                    if (_hoveredShape != null) _hoveredShape.Hovered = true;
                    _hoveredConnection = conn;
                    Invalidate();
                }

                if (_currentTool == CanvasTool.Connect)
                    Cursor = (shape != null) ? Cursors.Cross : Cursors.Default;
                else
                    Cursor = (shape != null) ? Cursors.SizeAll : Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Middle)
            {
                Cursor = _currentTool == CanvasTool.Connect ? Cursors.Cross : Cursors.Default;
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            if (_isDragging)
            {
                _isDragging = false;
                _dragShape = null;
                _draggingShapes.Clear();
                _dragOriginalPositions = null;
                Invalidate();
            }

            if (_isConnecting)
            {
                EndConnection(ScreenToWorld(e.Location));
            }

            if (_isSelecting)
            {
                _isSelecting = false;
                var shapes = _document.GetShapesInRect(_selectionRect);
                foreach (var s in shapes)
                    s.Selected = true;
                if (shapes.Count > 0)
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            float oldZoom = _zoom;
            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            Zoom *= zoomDelta;

            PointF worldPos = ScreenToWorld(new Point(e.X, e.Y), oldZoom);
            _offset.X = e.X - worldPos.X * _zoom;
            _offset.Y = e.Y - worldPos.Y * _zoom;

            Invalidate();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent(typeof(ToolboxItem)))
            {
                e.Effect = DragDropEffects.Copy;
                Invalidate();
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            if (!e.Data.GetDataPresent(typeof(ToolboxItem))) return;

            var item = e.Data.GetData(typeof(ToolboxItem)) as ToolboxItem;
            if (item?.CreateShape == null) return;

            Point clientPt = PointToClient(new Point(e.X, e.Y));
            PointF worldPos = ScreenToWorld(clientPt);

            var shape = item.CreateShape();
            shape.X = worldPos.X - shape.Width / 2f;
            shape.Y = worldPos.Y - shape.Height / 2f;

            if (GlobalConfig.Instance.SnapToGrid)
            {
                float grid = GlobalConfig.Instance.GridSize;
                shape.X = (float)Math.Round(shape.X / grid) * grid;
                shape.Y = (float)Math.Round(shape.Y / grid) * grid;
            }

            _document.AddShape(shape);

            var container = _document.Shapes
                .OfType<CloudNativeDesigner.Shapes.ContainerShape>()
                .FirstOrDefault(c => c.HitTest(worldPos) && c != shape);
            if (container != null)
            {
                container.AddChild(shape);
            }

            _document.ClearSelection();
            shape.Selected = true;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DocumentModified?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _document.ClearSelection();
                _isConnecting = false;
                _isDragging = false;
                _isSelecting = false;
                CurrentTool = CanvasTool.Select;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
            else if ((e.Control || e.KeyCode == Keys.ControlKey) && e.KeyCode == Keys.A)
            {
                foreach (var s in _document.Shapes)
                    s.Selected = true;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        private void StartConnection(PointF worldPos)
        {
            var shape = _document.HitTestShape(worldPos);
            if (shape != null)
            {
                _connectStartShape = shape;
                _connectStartPoint = shape.GetNearestConnectionPoint(worldPos);
                _connectCurrentPoint = worldPos;
                _isConnecting = true;
            }
        }

        private void EndConnection(PointF worldPos)
        {
            _isConnecting = false;
            var endShape = _document.HitTestShape(worldPos);

            if (_connectStartShape != null && endShape != null && _connectStartShape != endShape)
            {
                var conn = new Connection
                {
                    FromShape = _connectStartShape,
                    ToShape = endShape,
                    Mode = GlobalConfig.Instance.DefaultConnectionMode,
                    FromPoint = _connectStartShape.GetNearestConnectionPoint(endShape.Center),
                    ToPoint = endShape.GetNearestConnectionPoint(_connectStartShape.Center)
                };
                _document.AddConnection(conn);
                DocumentModified?.Invoke(this, EventArgs.Empty);
            }

            _connectStartShape = null;
            Invalidate();
        }

        public void DeleteSelected()
        {
            var shapes = _document.GetSelectedShapes().ToList();
            var conns = _document.GetSelectedConnections().ToList();

            foreach (var c in conns)
                _document.RemoveConnection(c);
            foreach (var s in shapes)
                _document.RemoveShape(s);

            SelectionChanged?.Invoke(this, EventArgs.Empty);
            DocumentModified?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void BringToFront()
        {
            foreach (var s in _document.GetSelectedShapes())
                _document.BringToFront(s);
            Invalidate();
        }

        public void SendToBack()
        {
            foreach (var s in _document.GetSelectedShapes())
                _document.SendToBack(s);
            Invalidate();
        }

        public PointF ScreenToWorld(Point screenPt, float? zoomOverride = null)
        {
            float z = zoomOverride ?? _zoom;
            return new PointF((screenPt.X - _offset.X) / z, (screenPt.Y - _offset.Y) / z);
        }

        public Point WorldToScreen(PointF worldPt)
        {
            return new Point((int)(worldPt.X * _zoom + _offset.X), (int)(worldPt.Y * _zoom + _offset.Y));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _bufferedGraphics?.Dispose();
                _bufferContext?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

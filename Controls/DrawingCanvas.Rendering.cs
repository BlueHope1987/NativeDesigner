using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Controls
{
    public partial class DrawingCanvas : Control
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bufferedGraphics == null || _bufferSize != ClientSize)
            {
                if (_bufferedGraphics != null)
                    _bufferedGraphics.Dispose();
                _bufferedGraphics = _bufferContext.Allocate(e.Graphics, ClientRectangle);
                _bufferSize = ClientSize;
            }

            Graphics g = _bufferedGraphics.Graphics;
            DrawBackground(g);

            if (GlobalConfig.Instance.AntiAlias)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            }

            g.TranslateTransform(_offset.X, _offset.Y);
            g.ScaleTransform(_zoom, _zoom);

            DrawGrid(g);
            DrawShapes(g);
            DrawConnections(g);
            DrawRubberBand(g);
            DrawSelectionRect(g);

            g.ResetTransform();
            _bufferedGraphics.Render(e.Graphics);
        }

        private void DrawBackground(Graphics g)
        {
            Color bgColor = GlobalConfig.Instance.CanvasBackground;
            Color centerColor = GlobalConfig.Instance.GradientCenterColor;

            using (LinearGradientBrush hBrush = new LinearGradientBrush(
                new PointF(0, 0),
                new PointF(ClientSize.Width, 0),
                bgColor,
                centerColor))
            {
                hBrush.WrapMode = WrapMode.Tile;
                g.FillRectangle(hBrush, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            float cx = ClientSize.Width / 2f;
            float cy = ClientSize.Height / 2f;
            float rw = ClientSize.Width * 0.6f;
            float rh = ClientSize.Height * 0.6f;
            if (rw > 1f && rh > 1f)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(cx - rw / 2f, cy - rh / 2f, rw, rh);
                    using (PathGradientBrush pgb = new PathGradientBrush(path))
                    {
                        Color overlayColor = Color.FromArgb(
                            Math.Min(255, centerColor.R + 10),
                            Math.Min(255, centerColor.G + 10),
                            Math.Min(255, centerColor.B + 15));
                        pgb.CenterColor = overlayColor;
                        pgb.SurroundColor = Color.Transparent;
                        pgb.SetSigmaBellShape(0.6f, 1.0f);
                        g.FillPath(pgb, path);
                    }
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            if (!GlobalConfig.Instance.ShowGrid)
                return;

            float gridSize = GlobalConfig.Instance.GridSize;
            float left = (-_offset.X) / _zoom;
            float top = (-_offset.Y) / _zoom;
            float right = left + ClientSize.Width / _zoom;
            float bottom = top + ClientSize.Height / _zoom;

            float startX = (float)Math.Floor(left / gridSize) * gridSize;
            float startY = (float)Math.Floor(top / gridSize) * gridSize;

            Color gridColor = GlobalConfig.Instance.GridColor;
            using (Pen pen = new Pen(gridColor))
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
            List<ShapeBase> shapes = _document.Shapes;
            for (int i = 0; i < shapes.Count; i++)
            {
                ShapeBase shape = shapes[i];
                if (shape.Visible && shape.Parent == null)
                {
                    shape.Draw(g, _zoom);

                    if (shape is ContainerShape)
                    {
                        ContainerShape cs = (ContainerShape)shape;
                        RectangleF bodyRect = cs.Bounds;
                        bodyRect.Y += cs.HeaderHeight;
                        bodyRect.Height -= cs.HeaderHeight;
                        DrawConnectionsForContainer(g, cs, bodyRect);
                    }
                }
            }
        }

        private void DrawConnections(Graphics g)
        {
            List<Connection> connections = _document.Connections;
            for (int i = 0; i < connections.Count; i++)
            {
                Connection conn = connections[i];
                bool bothInSameContainer = false;
                if (conn.FromShape != null && conn.ToShape != null &&
                    conn.FromShape.Parent != null && conn.FromShape.Parent == conn.ToShape.Parent)
                {
                    bothInSameContainer = true;
                }
                if (!bothInSameContainer)
                {
                    conn.Draw(g, _zoom);
                }
            }
        }

        private void DrawConnectionsForContainer(Graphics g, ShapeBase parentShape, RectangleF clipRect)
        {
            List<Connection> connections = _document.Connections;
            for (int i = 0; i < connections.Count; i++)
            {
                Connection conn = connections[i];
                if (conn.FromShape != null && conn.ToShape != null &&
                    conn.FromShape.Parent == parentShape && conn.ToShape.Parent == parentShape)
                {
                    GraphicsState state = g.Save();
                    g.Clip = new Region(clipRect);
                    conn.Draw(g, _zoom);
                    g.Restore(state);
                }
            }
        }

        private void DrawRubberBand(Graphics g)
        {
            if (!_isConnecting)
                return;

            Color rbColor = GlobalConfig.Instance.RubberBandColor;
            using (Pen pen = new Pen(rbColor, 1.5f / _zoom))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, _connectStartPoint, _connectCurrentPoint);
            }
        }

        private void DrawSelectionRect(Graphics g)
        {
            if (!_isSelecting)
                return;

            Color selColor = GlobalConfig.Instance.RubberBandColor;
            using (Brush brush = new SolidBrush(Color.FromArgb(30,
                selColor.R, selColor.G, selColor.B)))
            using (Pen pen = new Pen(selColor, 1f / _zoom))
            {
                pen.DashStyle = DashStyle.Dash;
                g.FillRectangle(brush, _selectionRect);
                g.DrawRectangle(pen, _selectionRect.X, _selectionRect.Y,
                    _selectionRect.Width, _selectionRect.Height);
            }
        }
    }
}

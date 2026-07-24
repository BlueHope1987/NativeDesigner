using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Controls
{
    /// <summary>
    /// 自定义图形构建器。
    /// 可通过添加/删除顶点和拖动顶点来创建多边形图形。
    /// </summary>
    public class CustomShapeDialog : Form
    {
        private Panel _canvasPanel;
        private TextBox _txtName;
        private Button _btnAddVertex;
        private Button _btnDeleteVertex;
        private Button _btnClosePath;
        private Button _btnOk;
        private Button _btnCancel;
        private CheckBox _chkFilled;
        private ColorDialog _colorDialog;

        private List<PointF> _vertices = new List<PointF>();
        private int _dragIndex = -1;
        private int _selectedVertex = -1;
        private bool _closedPath = false;
        private bool _filled = true;
        private Color _fillColor = Color.FromArgb(220, 240, 255);
        private Color _borderColor = Color.FromArgb(80, 120, 180);
        private ShapeType _resultShapeType = null;

        public ShapeType ResultShapeType { get { return _resultShapeType; } }

        public CustomShapeDialog()
        {
            this.Text = "创建自定义图形";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.ClientSize = new Size(520, 450);

            _colorDialog = new ColorDialog();

            // 画布区域
            _canvasPanel = new Panel();
            _canvasPanel.Location = new Point(10, 40);
            _canvasPanel.Size = new Size(350, 340);
            _canvasPanel.BackColor = Color.White;
            _canvasPanel.BorderStyle = BorderStyle.FixedSingle;
            _canvasPanel.Paint += new PaintEventHandler(OnCanvasPaint);
            _canvasPanel.MouseDown += new MouseEventHandler(OnCanvasMouseDown);
            _canvasPanel.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            _canvasPanel.MouseUp += new MouseEventHandler(OnCanvasMouseUp);
            _canvasPanel.DoubleClick += new EventHandler(OnCanvasDoubleClick);
            this.Controls.Add(_canvasPanel);

            // 名称
            Label lblName = new Label();
            lblName.Text = "名称：";
            lblName.Location = new Point(10, 14);
            lblName.AutoSize = true;
            this.Controls.Add(lblName);

            _txtName = new TextBox();
            _txtName.Location = new Point(60, 11);
            _txtName.Size = new Size(150, 22);
            _txtName.Text = "CustomShape";
            this.Controls.Add(_txtName);

            // 右侧按钮
            _btnAddVertex = new Button();
            _btnAddVertex.Text = "添加顶点";
            _btnAddVertex.Location = new Point(375, 40);
            _btnAddVertex.Size = new Size(130, 32);
            _btnAddVertex.Click += new EventHandler(OnAddVertex);
            this.Controls.Add(_btnAddVertex);

            _btnDeleteVertex = new Button();
            _btnDeleteVertex.Text = "删除顶点";
            _btnDeleteVertex.Location = new Point(375, 80);
            _btnDeleteVertex.Size = new Size(130, 32);
            _btnDeleteVertex.Click += new EventHandler(OnDeleteVertex);
            _btnDeleteVertex.Location = new Point(375, 80);
            this.Controls.Add(_btnDeleteVertex);

            _btnClosePath = new Button();
            _btnClosePath.Text = "闭合路径";
            _btnClosePath.Location = new Point(375, 120);
            _btnClosePath.Size = new Size(130, 32);
            _btnClosePath.Click += new EventHandler(OnToggleClosePath);
            this.Controls.Add(_btnClosePath);

            _chkFilled = new CheckBox();
            _chkFilled.Text = "填充";
            _chkFilled.Checked = true;
            _chkFilled.Location = new Point(375, 162);
            _chkFilled.Size = new Size(130, 24);
            _chkFilled.CheckedChanged += new EventHandler(OnFilledChanged);
            this.Controls.Add(_chkFilled);

            Button btnFillColor = new Button();
            btnFillColor.Text = "填充颜色...";
            btnFillColor.Location = new Point(375, 192);
            btnFillColor.Size = new Size(130, 28);
            btnFillColor.Click += new EventHandler(OnPickFillColor);
            this.Controls.Add(btnFillColor);

            Button btnBorderColor = new Button();
            btnBorderColor.Text = "边框颜色...";
            btnBorderColor.Location = new Point(375, 228);
            btnBorderColor.Size = new Size(130, 28);
            btnBorderColor.Click += new EventHandler(OnPickBorderColor);
            this.Controls.Add(btnBorderColor);

            Label lblHint = new Label();
            lblHint.Text = "操作提示：\n· 单击画布添加顶点\n· 拖动顶点调整位置\n· 双击顶点删除\n· 至少需要3个顶点";
            lblHint.Location = new Point(375, 270);
            lblHint.Size = new Size(135, 90);
            lblHint.ForeColor = Color.FromArgb(100, 100, 100);
            this.Controls.Add(lblHint);

            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(330, 410);
            _btnOk.Size = new Size(80, 30);
            this.Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(420, 410);
            _btnCancel.Size = new Size(80, 30);
            this.Controls.Add(_btnCancel);

            // 预置一个默认三角形
            _vertices.Add(new PointF(100, 40));
            _vertices.Add(new PointF(40, 250));
            _vertices.Add(new PointF(260, 250));
            _closedPath = true;
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_vertices.Count == 0)
                return;

            // 构建路径
            GraphicsPath path = new GraphicsPath();
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (i == 0)
                    path.StartFigure();
                path.AddLine(_vertices[i], _vertices[(i + 1) % _vertices.Count]);
            }
            if (_closedPath)
                path.CloseFigure();

            // 绘制填充
            if (_filled && _vertices.Count >= 3)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(180, _fillColor)))
                {
                    g.FillPath(brush, path);
                }
            }

            // 绘制线条
            using (Pen pen = new Pen(_borderColor, 2f))
            {
                for (int i = 0; i < _vertices.Count - 1; i++)
                {
                    g.DrawLine(pen, _vertices[i], _vertices[i + 1]);
                }
                if (_closedPath && _vertices.Count >= 3)
                {
                    g.DrawLine(pen, _vertices[_vertices.Count - 1], _vertices[0]);
                }
            }

            // 绘制顶点手柄
            for (int i = 0; i < _vertices.Count; i++)
            {
                RectangleF handle = new RectangleF(_vertices[i].X - 5, _vertices[i].Y - 5, 10, 10);
                bool isSelected = (i == _selectedVertex);
                using (Brush brush = new SolidBrush(isSelected ? Color.FromArgb(0, 120, 215) : Color.White))
                using (Pen pen = new Pen(_borderColor, isSelected ? 2f : 1f))
                {
                    g.FillRectangle(brush, handle);
                    g.DrawRectangle(pen, handle.X, handle.Y, handle.Width, handle.Height);
                }
            }
        }

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int hitIdx = HitTestVertex(e.Location);
                if (hitIdx >= 0)
                {
                    _selectedVertex = hitIdx;
                    _dragIndex = hitIdx;
                    _canvasPanel.Cursor = Cursors.SizeAll;
                }
                else
                {
                    // 单击空白处添加顶点
                    _vertices.Add(new PointF(e.X, e.Y));
                    _selectedVertex = _vertices.Count - 1;
                    _canvasPanel.Invalidate();
                }
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragIndex >= 0)
            {
                _vertices[_dragIndex] = new PointF(e.X, e.Y);
                _canvasPanel.Invalidate();
            }
            else
            {
                int hitIdx = HitTestVertex(e.Location);
                _canvasPanel.Cursor = hitIdx >= 0 ? Cursors.SizeAll : Cursors.Cross;
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (_dragIndex >= 0)
            {
                _dragIndex = -1;
                _canvasPanel.Cursor = Cursors.Default;
            }
        }

        private void OnCanvasDoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            int hitIdx = HitTestVertex(me.Location);
            if (hitIdx >= 0)
            {
                _vertices.RemoveAt(hitIdx);
                if (_selectedVertex >= _vertices.Count)
                    _selectedVertex = _vertices.Count - 1;
                _canvasPanel.Invalidate();
            }
        }

        private int HitTestVertex(Point pt)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                float dx = _vertices[i].X - pt.X;
                float dy = _vertices[i].Y - pt.Y;
                if (dx * dx + dy * dy <= 64) // 8px 半径
                    return i;
            }
            return -1;
        }

        private void OnAddVertex(object sender, EventArgs e)
        {
            // 在最后一个顶点附近添加
            PointF newPt;
            if (_vertices.Count > 0)
            {
                PointF last = _vertices[_vertices.Count - 1];
                newPt = new PointF(last.X + 30, last.Y + 30);
            }
            else
            {
                newPt = new PointF(175, 170);
            }
            _vertices.Add(newPt);
            _selectedVertex = _vertices.Count - 1;
            _canvasPanel.Invalidate();
        }

        private void OnDeleteVertex(object sender, EventArgs e)
        {
            if (_selectedVertex >= 0 && _selectedVertex < _vertices.Count)
            {
                _vertices.RemoveAt(_selectedVertex);
                if (_selectedVertex >= _vertices.Count)
                    _selectedVertex = _vertices.Count - 1;
                _canvasPanel.Invalidate();
            }
        }

        private void OnToggleClosePath(object sender, EventArgs e)
        {
            _closedPath = !_closedPath;
            _btnClosePath.Text = _closedPath ? "打开路径" : "闭合路径";
            _canvasPanel.Invalidate();
        }

        private void OnFilledChanged(object sender, EventArgs e)
        {
            _filled = _chkFilled.Checked;
            _canvasPanel.Invalidate();
        }

        private void OnPickFillColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _fillColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _fillColor = _colorDialog.Color;
                _canvasPanel.Invalidate();
            }
        }

        private void OnPickBorderColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _borderColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _borderColor = _colorDialog.Color;
                _canvasPanel.Invalidate();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (this.DialogResult == DialogResult.OK)
            {
                if (_vertices.Count < 3)
                {
                    MessageBox.Show("自定义图形至少需要 3 个顶点", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }

                string name = _txtName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                    name = "CustomShape";

                BuildShapeType(name);
            }
        }

        private void BuildShapeType(string name)
        {
            ShapeType st = new ShapeType();
            st.Name = name;
            st.Category = "自定义";
            st.DefaultWidth = 120;
            st.DefaultHeight = 100;

            // 将顶点坐标归一化到 0~1 范围
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (_vertices[i].X < minX) minX = _vertices[i].X;
                if (_vertices[i].Y < minY) minY = _vertices[i].Y;
                if (_vertices[i].X > maxX) maxX = _vertices[i].X;
                if (_vertices[i].Y > maxY) maxY = _vertices[i].Y;
            }
            float rangeX = maxX - minX;
            float rangeY = maxY - minY;
            if (rangeX < 1) rangeX = 1;
            if (rangeY < 1) rangeY = 1;

            List<PointF> normalized = new List<PointF>();
            for (int i = 0; i < _vertices.Count; i++)
            {
                float nx = (_vertices[i].X - minX) / rangeX;
                float ny = (_vertices[i].Y - minY) / rangeY;
                normalized.Add(new PointF(nx, ny));
            }

            // 构建多边形 RenderCommand
            List<RenderCommand> cmds = new List<RenderCommand>();

            // 多边形
            RenderCommand polyCmd = new RenderCommand();
            polyCmd.CommandType = RenderCommandType.Polygon;
            polyCmd.PolygonPoints = normalized.ToArray();
            polyCmd.X = 0;
            polyCmd.Y = 0;
            polyCmd.Width = 1;
            polyCmd.Height = 1;
            polyCmd.FillColor = _filled ? _fillColor : Color.Transparent;
            polyCmd.StrokeColor = _borderColor;
            polyCmd.StrokeWidth = 2f;
            polyCmd.Fill = _filled;
            cmds.Add(polyCmd);

            st.RenderCommands = cmds;

            // 设置默认颜色
            st.DefaultFillColor = new XmlColor(_filled ? _fillColor : Color.Transparent);
            st.DefaultBorderColor = new XmlColor(_borderColor);
            st.DefaultTextColor = new XmlColor(Color.FromArgb(40, 40, 40));

            _resultShapeType = st;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Controls
{
    /// <summary>
    /// 自定义图形构建器（带状态和行为编辑）。
    /// 支持多边形顶点编辑、状态定义、右键菜单行为定义。
    /// </summary>
    public class CustomShapeDialog : Form
    {
        // 通用控件
        private TextBox _txtName;
        private TabControl _tabControl;
        private Button _btnOk;
        private Button _btnCancel;

        // === 图形标签页 ===
        private Panel _canvasPanel;
        private Button _btnAddVertex;
        private Button _btnDeleteVertex;
        private Button _btnClosePath;
        private CheckBox _chkFilled;
        private ColorDialog _colorDialog;

        private List<PointF> _vertices = new List<PointF>();
        private int _dragIndex = -1;
        private int _selectedVertex = -1;
        private bool _closedPath = false;
        private bool _filled = true;
        private Color _fillColor = Color.FromArgb(220, 240, 255);
        private Color _borderColor = Color.FromArgb(80, 120, 180);

        // === 状态标签页 ===
        private ListBox _listStates;
        private Button _btnAddState;
        private Button _btnEditState;
        private Button _btnDeleteState;
        private List<ShapeState> _states = new List<ShapeState>();

        // === 行为标签页 ===
        private ListBox _listActions;
        private Button _btnAddAction;
        private Button _btnEditAction;
        private Button _btnDeleteAction;
        private List<ShapeAction> _actions = new List<ShapeAction>();

        private ShapeType _resultShapeType = null;
        public ShapeType ResultShapeType { get { return _resultShapeType; } }

        /// <summary>创建新自定义图形</summary>
        public CustomShapeDialog() : this(null) { }

        /// <summary>编辑现有自定义图形</summary>
        public CustomShapeDialog(ShapeType editShape)
        {
            this.Text = (editShape == null) ? "创建自定义图形" : "编辑自定义图形";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.ClientSize = new Size(540, 500);

            _colorDialog = new ColorDialog();

            // 名称（位于标签页上方）
            Label lblName = new Label();
            lblName.Text = "名称：";
            lblName.Location = new Point(10, 14);
            lblName.AutoSize = true;
            this.Controls.Add(lblName);

            _txtName = new TextBox();
            _txtName.Location = new Point(60, 11);
            _txtName.Size = new Size(200, 22);
            _txtName.Text = (editShape == null) ? "CustomShape" : editShape.Name;
            this.Controls.Add(_txtName);

            // 标签页
            _tabControl = new TabControl();
            _tabControl.Location = new Point(10, 40);
            _tabControl.Size = new Size(510, 410);
            this.Controls.Add(_tabControl);

            BuildGeometryTab();
            BuildStateTab();
            BuildActionTab();

            // 确定/取消
            _btnOk = new Button();
            _btnOk.Text = "确定";
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(350, 460);
            _btnOk.Size = new Size(80, 30);
            this.Controls.Add(_btnOk);

            _btnCancel = new Button();
            _btnCancel.Text = "取消";
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(440, 460);
            _btnCancel.Size = new Size(80, 30);
            this.Controls.Add(_btnCancel);

            // 加载现有图形数据
            if (editShape != null)
            {
                LoadFromShapeType(editShape);
            }
            else
            {
                // 预置默认三角形
                _vertices.Add(new PointF(100, 40));
                _vertices.Add(new PointF(40, 250));
                _vertices.Add(new PointF(260, 250));
                _closedPath = true;
            }
        }

        #region 标签页构建

        private void BuildGeometryTab()
        {
            TabPage page = new TabPage("图形");
            page.BorderStyle = BorderStyle.None;

            // 画布区域
            _canvasPanel = new Panel();
            _canvasPanel.Location = new Point(10, 10);
            _canvasPanel.Size = new Size(350, 340);
            _canvasPanel.BackColor = Color.White;
            _canvasPanel.BorderStyle = BorderStyle.FixedSingle;
            _canvasPanel.Paint += new PaintEventHandler(OnCanvasPaint);
            _canvasPanel.MouseDown += new MouseEventHandler(OnCanvasMouseDown);
            _canvasPanel.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            _canvasPanel.MouseUp += new MouseEventHandler(OnCanvasMouseUp);
            _canvasPanel.DoubleClick += new EventHandler(OnCanvasDoubleClick);
            page.Controls.Add(_canvasPanel);

            // 右侧按钮
            int x = 375;
            int y = 10;

            _btnAddVertex = new Button();
            _btnAddVertex.Text = "添加顶点";
            _btnAddVertex.Location = new Point(x, y);
            _btnAddVertex.Size = new Size(120, 30);
            _btnAddVertex.Click += new EventHandler(OnAddVertex);
            page.Controls.Add(_btnAddVertex);
            y += 36;

            _btnDeleteVertex = new Button();
            _btnDeleteVertex.Text = "删除顶点";
            _btnDeleteVertex.Location = new Point(x, y);
            _btnDeleteVertex.Size = new Size(120, 30);
            _btnDeleteVertex.Click += new EventHandler(OnDeleteVertex);
            page.Controls.Add(_btnDeleteVertex);
            y += 36;

            _btnClosePath = new Button();
            _btnClosePath.Text = "闭合路径";
            _btnClosePath.Location = new Point(x, y);
            _btnClosePath.Size = new Size(120, 30);
            _btnClosePath.Click += new EventHandler(OnToggleClosePath);
            page.Controls.Add(_btnClosePath);
            y += 40;

            _chkFilled = new CheckBox();
            _chkFilled.Text = "填充";
            _chkFilled.Checked = true;
            _chkFilled.Location = new Point(x, y);
            _chkFilled.Size = new Size(120, 24);
            _chkFilled.CheckedChanged += new EventHandler(OnFilledChanged);
            page.Controls.Add(_chkFilled);
            y += 30;

            Button btnFillColor = new Button();
            btnFillColor.Text = "填充颜色...";
            btnFillColor.Location = new Point(x, y);
            btnFillColor.Size = new Size(120, 28);
            btnFillColor.Click += new EventHandler(OnPickFillColor);
            page.Controls.Add(btnFillColor);
            y += 34;

            Button btnBorderColor = new Button();
            btnBorderColor.Text = "边框颜色...";
            btnBorderColor.Location = new Point(x, y);
            btnBorderColor.Size = new Size(120, 28);
            btnBorderColor.Click += new EventHandler(OnPickBorderColor);
            page.Controls.Add(btnBorderColor);
            y += 44;

            Label lblHint = new Label();
            lblHint.Text = "操作提示：\n· 单击画布添加顶点\n· 拖动顶点调整位置\n· 双击顶点删除\n· 至少需要3个顶点";
            lblHint.Location = new Point(x, y);
            lblHint.Size = new Size(125, 90);
            lblHint.ForeColor = Color.FromArgb(100, 100, 100);
            page.Controls.Add(lblHint);

            _tabControl.TabPages.Add(page);
        }

        private void BuildStateTab()
        {
            TabPage page = new TabPage("状态");
            page.BorderStyle = BorderStyle.None;

            _listStates = new ListBox();
            _listStates.Location = new Point(10, 10);
            _listStates.Size = new Size(360, 340);
            _listStates.BorderStyle = BorderStyle.FixedSingle;
            _listStates.DrawMode = DrawMode.OwnerDrawFixed;
            _listStates.DrawItem += new DrawItemEventHandler(OnDrawStateItem);
            page.Controls.Add(_listStates);

            int x = 385;
            int y = 10;

            _btnAddState = new Button();
            _btnAddState.Text = "添加...";
            _btnAddState.Location = new Point(x, y);
            _btnAddState.Size = new Size(110, 28);
            _btnAddState.Click += new EventHandler(OnAddState);
            page.Controls.Add(_btnAddState);
            y += 34;

            _btnEditState = new Button();
            _btnEditState.Text = "编辑...";
            _btnEditState.Location = new Point(x, y);
            _btnEditState.Size = new Size(110, 28);
            _btnEditState.Click += new EventHandler(OnEditState);
            page.Controls.Add(_btnEditState);
            y += 34;

            _btnDeleteState = new Button();
            _btnDeleteState.Text = "删除";
            _btnDeleteState.Location = new Point(x, y);
            _btnDeleteState.Size = new Size(110, 28);
            _btnDeleteState.Click += new EventHandler(OnDeleteState);
            page.Controls.Add(_btnDeleteState);
            y += 44;

            Label lblHint = new Label();
            lblHint.Text = "提示：\n状态定义图形的不同\n视觉外观。运行时\n可通过行为菜单切换\n状态。";
            lblHint.Location = new Point(x, y);
            lblHint.Size = new Size(110, 90);
            lblHint.ForeColor = Color.FromArgb(100, 100, 100);
            page.Controls.Add(lblHint);

            _tabControl.TabPages.Add(page);
        }

        private void BuildActionTab()
        {
            TabPage page = new TabPage("行为");
            page.BorderStyle = BorderStyle.None;

            _listActions = new ListBox();
            _listActions.Location = new Point(10, 10);
            _listActions.Size = new Size(360, 340);
            _listActions.BorderStyle = BorderStyle.FixedSingle;
            page.Controls.Add(_listActions);

            int x = 385;
            int y = 10;

            _btnAddAction = new Button();
            _btnAddAction.Text = "添加...";
            _btnAddAction.Location = new Point(x, y);
            _btnAddAction.Size = new Size(110, 28);
            _btnAddAction.Click += new EventHandler(OnAddAction);
            page.Controls.Add(_btnAddAction);
            y += 34;

            _btnEditAction = new Button();
            _btnEditAction.Text = "编辑...";
            _btnEditAction.Location = new Point(x, y);
            _btnEditAction.Size = new Size(110, 28);
            _btnEditAction.Click += new EventHandler(OnEditAction);
            page.Controls.Add(_btnEditAction);
            y += 34;

            _btnDeleteAction = new Button();
            _btnDeleteAction.Text = "删除";
            _btnDeleteAction.Location = new Point(x, y);
            _btnDeleteAction.Size = new Size(110, 28);
            _btnDeleteAction.Click += new EventHandler(OnDeleteAction);
            page.Controls.Add(_btnDeleteAction);
            y += 44;

            Label lblHint = new Label();
            lblHint.Text = "提示：\n行为定义图形在右键\n菜单中可用的操作。\n类型：切换状态 / \n宿主回调。";
            lblHint.Location = new Point(x, y);
            lblHint.Size = new Size(110, 100);
            lblHint.ForeColor = Color.FromArgb(100, 100, 100);
            page.Controls.Add(lblHint);

            _tabControl.TabPages.Add(page);
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 从现有 ShapeType 解析多边形顶点、颜色、状态和行为
        /// </summary>
        private void LoadFromShapeType(ShapeType st)
        {
            _txtName.Text = st.Name;

            // 解析顶点
            if (st.RenderCommands != null && st.RenderCommands.Count > 0)
            {
                RenderCommand polyCmd = null;
                foreach (RenderCommand cmd in st.RenderCommands)
                {
                    if (cmd.CommandType == RenderCommandType.Polygon)
                    {
                        polyCmd = cmd;
                        break;
                    }
                }

                if (polyCmd != null && polyCmd.PolygonPoints != null && polyCmd.PolygonPoints.Length >= 3)
                {
                    float canvasW = 300f;
                    float canvasH = 300f;
                    float offsetX = 25f;
                    float offsetY = 20f;

                    _vertices.Clear();
                    foreach (PointF pt in polyCmd.PolygonPoints)
                    {
                        _vertices.Add(new PointF(offsetX + pt.X * canvasW, offsetY + pt.Y * canvasH));
                    }
                    _closedPath = true;
                    _fillColor = polyCmd.FillColor;
                    _borderColor = polyCmd.StrokeColor;
                    _filled = polyCmd.Fill;
                }
            }

            _chkFilled.Checked = _filled;

            // 加载状态
            if (st.DefaultStates != null)
            {
                foreach (ShapeState state in st.DefaultStates)
                {
                    ShapeState copy = new ShapeState();
                    copy.Name = state.Name;
                    copy.FillColor = new XmlColor(state.FillColor.ToColor());
                    copy.BorderColor = new XmlColor(state.BorderColor.ToColor());
                    copy.TextColor = new XmlColor(state.TextColor.ToColor());
                    copy.HeaderColor = new XmlColor(state.HeaderColor.ToColor());
                    copy.Priority = state.Priority;
                    _states.Add(copy);
                }
                RefreshStateList();
            }

            // 加载行为
            if (st.CustomActions != null)
            {
                foreach (ShapeAction action in st.CustomActions)
                {
                    ShapeAction copy = new ShapeAction();
                    copy.Name = action.Name;
                    copy.ActionType = action.ActionType;
                    copy.TargetState = action.TargetState;
                    copy.CallbackName = action.CallbackName;
                    copy.IconName = action.IconName;
                    _actions.Add(copy);
                }
                RefreshActionList();
            }
        }

        #endregion

        #region 图形标签页事件

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_vertices.Count == 0)
                return;

            GraphicsPath path = new GraphicsPath();
            for (int i = 0; i < _vertices.Count; i++)
            {
                if (i == 0)
                    path.StartFigure();
                path.AddLine(_vertices[i], _vertices[(i + 1) % _vertices.Count]);
            }
            if (_closedPath)
                path.CloseFigure();

            if (_filled && _vertices.Count >= 3)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(180, _fillColor)))
                {
                    g.FillPath(brush, path);
                }
            }

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
                if (dx * dx + dy * dy <= 64)
                    return i;
            }
            return -1;
        }

        private void OnAddVertex(object sender, EventArgs e)
        {
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

        #endregion

        #region 状态标签页事件

        private void RefreshStateList()
        {
            _listStates.Items.Clear();
            foreach (ShapeState state in _states)
            {
                _listStates.Items.Add(state.Name);
            }
        }

        private void OnDrawStateItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _states.Count)
                return;

            e.DrawBackground();
            ShapeState state = _states[e.Index];

            // 绘制颜色预览块
            Rectangle colorRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 3, 20, e.Bounds.Height - 6);
            using (Brush brush = new SolidBrush(state.FillColor.ToColor()))
            using (Pen pen = new Pen(state.BorderColor.ToColor(), 1))
            {
                e.Graphics.FillRectangle(brush, colorRect);
                e.Graphics.DrawRectangle(pen, colorRect);
            }

            // 绘制状态名
            using (Brush textBrush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(state.Name, e.Font, textBrush,
                    e.Bounds.X + 30, e.Bounds.Y + 2);
            }

            e.DrawFocusRectangle();
        }

        private void OnAddState(object sender, EventArgs e)
        {
            using (ShapeStateEditDialog dlg = new ShapeStateEditDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultState != null)
                {
                    _states.Add(dlg.ResultState);
                    RefreshStateList();
                    _listStates.SelectedIndex = _listStates.Items.Count - 1;
                }
            }
        }

        private void OnEditState(object sender, EventArgs e)
        {
            int idx = _listStates.SelectedIndex;
            if (idx < 0 || idx >= _states.Count)
                return;

            using (ShapeStateEditDialog dlg = new ShapeStateEditDialog(_states[idx]))
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultState != null)
                {
                    _states[idx] = dlg.ResultState;
                    RefreshStateList();
                }
            }
        }

        private void OnDeleteState(object sender, EventArgs e)
        {
            int idx = _listStates.SelectedIndex;
            if (idx < 0 || idx >= _states.Count)
                return;

            if (MessageBox.Show(string.Format("确定删除状态 \"{0}\"？", _states[idx].Name),
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _states.RemoveAt(idx);
                RefreshStateList();
            }
        }

        #endregion

        #region 行为标签页事件

        private void RefreshActionList()
        {
            _listActions.Items.Clear();
            foreach (ShapeAction action in _actions)
            {
                string typeStr = (action.ActionType == ShapeActionType.StateChange) ? "[切换状态]" : "[宿主回调]";
                _listActions.Items.Add(string.Format("{0} {1}", typeStr, action.Name));
            }
        }

        private void OnAddAction(object sender, EventArgs e)
        {
            using (ShapeActionEditDialog dlg = new ShapeActionEditDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultAction != null)
                {
                    _actions.Add(dlg.ResultAction);
                    RefreshActionList();
                    _listActions.SelectedIndex = _listActions.Items.Count - 1;
                }
            }
        }

        private void OnEditAction(object sender, EventArgs e)
        {
            int idx = _listActions.SelectedIndex;
            if (idx < 0 || idx >= _actions.Count)
                return;

            using (ShapeActionEditDialog dlg = new ShapeActionEditDialog(_actions[idx]))
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.ResultAction != null)
                {
                    _actions[idx] = dlg.ResultAction;
                    RefreshActionList();
                }
            }
        }

        private void OnDeleteAction(object sender, EventArgs e)
        {
            int idx = _listActions.SelectedIndex;
            if (idx < 0 || idx >= _actions.Count)
                return;

            if (MessageBox.Show(string.Format("确定删除行为 \"{0}\"？", _actions[idx].Name),
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _actions.RemoveAt(idx);
                RefreshActionList();
            }
        }

        #endregion

        #region 构建结果

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

            // 归一化顶点
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

            // 构建 RenderCommand
            List<RenderCommand> cmds = new List<RenderCommand>();
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

            // 默认颜色
            st.DefaultFillColor = new XmlColor(_filled ? _fillColor : Color.Transparent);
            st.DefaultBorderColor = new XmlColor(_borderColor);
            st.DefaultTextColor = new XmlColor(Color.FromArgb(40, 40, 40));

            // 状态
            st.DefaultStates = new List<ShapeState>();
            foreach (ShapeState s in _states)
            {
                ShapeState copy = new ShapeState();
                copy.Name = s.Name;
                copy.FillColor = new XmlColor(s.FillColor.ToColor());
                copy.BorderColor = new XmlColor(s.BorderColor.ToColor());
                copy.TextColor = new XmlColor(s.TextColor.ToColor());
                copy.HeaderColor = new XmlColor(s.HeaderColor.ToColor());
                copy.Priority = s.Priority;
                st.DefaultStates.Add(copy);
            }

            // 行为
            st.CustomActions = new List<ShapeAction>();
            foreach (ShapeAction a in _actions)
            {
                ShapeAction copy = new ShapeAction();
                copy.Name = a.Name;
                copy.ActionType = a.ActionType;
                copy.TargetState = a.TargetState;
                copy.CallbackName = a.CallbackName;
                copy.IconName = a.IconName;
                st.CustomActions.Add(copy);
            }

            _resultShapeType = st;
        }

        #endregion
    }
}

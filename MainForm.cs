using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Controls;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner
{
    public class MainForm : Form
    {
        private DrawingCanvas _canvas;
        private ToolboxPanel _toolbox;
        private PropertyGrid _propertyGrid;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _zoomLabel;
        private SplitContainer _mainSplit;
        private SplitContainer _rightSplit;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "云原生可视化设计器";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;
        }

        private void InitializeComponent()
        {
            _mainSplit = new SplitContainer();
            _mainSplit.Dock = DockStyle.Fill;
            _mainSplit.Orientation = Orientation.Vertical;
            _mainSplit.SplitterDistance = 200;
            _mainSplit.FixedPanel = FixedPanel.Panel1;
            _mainSplit.BackColor = Color.FromArgb(240, 240, 240);

            _rightSplit = new SplitContainer();
            _rightSplit.Dock = DockStyle.Fill;
            _rightSplit.Orientation = Orientation.Vertical;
            _rightSplit.SplitterDistance = 900;
            _rightSplit.FixedPanel = FixedPanel.Panel2;

            _toolbox = new ToolboxPanel();
            _toolbox.Dock = DockStyle.Fill;

            _canvas = new DrawingCanvas();
            _canvas.Dock = DockStyle.Fill;
            _canvas.BackColor = Color.White;

            _propertyGrid = new PropertyGrid();
            _propertyGrid.Dock = DockStyle.Fill;
            _propertyGrid.ToolbarVisible = true;
            _propertyGrid.HelpVisible = true;
            _propertyGrid.PropertySort = PropertySort.CategorizedAlphabetical;

            _toolStrip = new ToolStrip();
            InitializeToolStrip();

            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("就绪");
            _zoomLabel = new ToolStripStatusLabel("缩放: 100%");
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripStatusLabel(""));
            _statusStrip.Items.Add(_zoomLabel);

            _mainSplit.Panel1.Controls.Add(_toolbox);
            _rightSplit.Panel1.Controls.Add(_canvas);
            _rightSplit.Panel2.Controls.Add(_propertyGrid);
            _mainSplit.Panel2.Controls.Add(_rightSplit);

            _toolStrip.Dock = DockStyle.Top;
            _statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_toolStrip);
            this.Controls.Add(_mainSplit);

            _canvas.SelectionChanged += new EventHandler(OnCanvasSelectionChanged);
            _canvas.DocumentModified += new EventHandler(OnDocumentModified);
            _canvas.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            _canvas.MouseClick += new MouseEventHandler(OnCanvasMouseClick);
            _propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);

            InitializeSampleData();
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _canvas.Invalidate();
        }

        private void InitializeToolStrip()
        {
            ToolStripButton btnSelect = new ToolStripButton("选择", null, new EventHandler(OnSelectTool));
            btnSelect.CheckOnClick = true;
            btnSelect.Checked = true;
            btnSelect.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnConnect = new ToolStripButton("连线", null, new EventHandler(OnConnectTool));
            btnConnect.CheckOnClick = true;
            btnConnect.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripSeparator sep1 = new ToolStripSeparator();

            ToolStripButton btnStraight = new ToolStripButton("直线", null, new EventHandler(OnStraightMode));
            btnStraight.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnCurve = new ToolStripButton("曲线", null, new EventHandler(OnCurveMode));
            btnCurve.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnOrtho = new ToolStripButton("折线", null, new EventHandler(OnOrthoMode));
            btnOrtho.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripSeparator sep2 = new ToolStripSeparator();

            ToolStripButton btnZoomIn = new ToolStripButton("放大", null, new EventHandler(OnZoomIn));
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnZoomOut = new ToolStripButton("缩小", null, new EventHandler(OnZoomOut));
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnFit = new ToolStripButton("适应", null, new EventHandler(OnZoomFit));
            btnFit.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripSeparator sep3 = new ToolStripSeparator();

            ToolStripButton btnFront = new ToolStripButton("置顶", null, new EventHandler(OnBringToFront));
            btnFront.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnBack = new ToolStripButton("置底", null, new EventHandler(OnSendToBack));
            btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnDelete = new ToolStripButton("删除", null, new EventHandler(OnDeleteSelected));
            btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripSeparator sep4 = new ToolStripSeparator();

            ToolStripButton btnGrid = new ToolStripButton("网格", null, new EventHandler(OnToggleGrid));
            btnGrid.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnGrid.Checked = GlobalConfig.Instance.ShowGrid;
            btnGrid.CheckOnClick = true;

            ToolStripButton btnSnap = new ToolStripButton("对齐", null, new EventHandler(OnToggleSnap));
            btnSnap.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSnap.Checked = GlobalConfig.Instance.SnapToGrid;
            btnSnap.CheckOnClick = true;

            _toolStrip.Items.Add(btnSelect);
            _toolStrip.Items.Add(btnConnect);
            _toolStrip.Items.Add(sep1);
            _toolStrip.Items.Add(btnStraight);
            _toolStrip.Items.Add(btnCurve);
            _toolStrip.Items.Add(btnOrtho);
            _toolStrip.Items.Add(sep2);
            _toolStrip.Items.Add(btnZoomIn);
            _toolStrip.Items.Add(btnZoomOut);
            _toolStrip.Items.Add(btnFit);
            _toolStrip.Items.Add(sep3);
            _toolStrip.Items.Add(btnFront);
            _toolStrip.Items.Add(btnBack);
            _toolStrip.Items.Add(btnDelete);
            _toolStrip.Items.Add(sep4);
            _toolStrip.Items.Add(btnGrid);
            _toolStrip.Items.Add(btnSnap);
        }

        private void OnSelectTool(object sender, EventArgs e)
        {
            _canvas.CurrentTool = CanvasTool.Select;
            UpdateToolState();
        }

        private void OnConnectTool(object sender, EventArgs e)
        {
            _canvas.CurrentTool = CanvasTool.Connect;
            UpdateToolState();
        }

        private void OnStraightMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Straight);
        }

        private void OnCurveMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Curve);
        }

        private void OnOrthoMode(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Orthogonal);
        }

        private void OnZoomIn(object sender, EventArgs e)
        {
            _canvas.Zoom *= 1.2f;
            UpdateZoomLabel();
        }

        private void OnZoomOut(object sender, EventArgs e)
        {
            _canvas.Zoom /= 1.2f;
            UpdateZoomLabel();
        }

        private void OnZoomFit(object sender, EventArgs e)
        {
            _canvas.Zoom = 1.0f;
            _canvas.Offset = new PointF(0, 0);
            UpdateZoomLabel();
        }

        private void OnBringToFront(object sender, EventArgs e)
        {
            _canvas.BringToFront();
        }

        private void OnSendToBack(object sender, EventArgs e)
        {
            _canvas.SendToBack();
        }

        private void OnDeleteSelected(object sender, EventArgs e)
        {
            _canvas.DeleteSelected();
        }

        private void OnToggleGrid(object sender, EventArgs e)
        {
            GlobalConfig.Instance.ShowGrid = !GlobalConfig.Instance.ShowGrid;
        }

        private void OnToggleSnap(object sender, EventArgs e)
        {
            GlobalConfig.Instance.SnapToGrid = !GlobalConfig.Instance.SnapToGrid;
        }

        private void UpdateToolState()
        {
            foreach (ToolStripItem item in _toolStrip.Items)
            {
                ToolStripButton btn = item as ToolStripButton;
                if (btn != null && (btn.Text == "选择" || btn.Text == "连线"))
                {
                    btn.Checked = (btn.Text == "选择" && _canvas.CurrentTool == CanvasTool.Select) ||
                                  (btn.Text == "连线" && _canvas.CurrentTool == CanvasTool.Connect);
                }
            }
        }

        private void SetConnectionMode(ConnectionMode mode)
        {
            GlobalConfig.Instance.DefaultConnectionMode = mode;
            List<Connection> conns = _canvas.Document.GetSelectedConnections();
            foreach (Connection conn in conns)
            {
                conn.Mode = mode;
            }
            _canvas.Invalidate();
        }

        private void UpdateZoomLabel()
        {
            _zoomLabel.Text = string.Format("缩放: {0:0}%", _canvas.Zoom * 100);
        }

        private void OnCanvasSelectionChanged(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            List<Connection> conns = _canvas.Document.GetSelectedConnections();

            if (shapes.Count == 1 && conns.Count == 0)
            {
                _propertyGrid.SelectedObject = shapes[0];
            }
            else if (shapes.Count == 0 && conns.Count == 1)
            {
                _propertyGrid.SelectedObject = conns[0];
            }
            else if (shapes.Count > 1)
            {
                object[] arr = new object[shapes.Count];
                for (int i = 0; i < shapes.Count; i++)
                    arr[i] = shapes[i];
                _propertyGrid.SelectedObjects = arr;
            }
            else
            {
                _propertyGrid.SelectedObject = GlobalConfig.Instance;
            }

            _statusLabel.Text = string.Format("选中: {0} 个实体, {1} 条连线", shapes.Count, conns.Count);
        }

        private void OnDocumentModified(object sender, EventArgs e)
        {
            _statusLabel.Text = "文档已修改";
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            PointF world = _canvas.ScreenToWorld(e.Location);
            _zoomLabel.Text = string.Format("缩放: {0:0}% | 坐标: ({1:0}, {2:0})", _canvas.Zoom * 100, world.X, world.Y);
        }

        private void OnCanvasMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu(e.Location);
            }
        }

        private void ShowContextMenu(Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem itemDelete = new ToolStripMenuItem("删除", null, new EventHandler(OnCtxDelete));
            itemDelete.ShortcutKeyDisplayString = "Delete";

            ToolStripMenuItem itemFront = new ToolStripMenuItem("置于顶层", null, new EventHandler(OnCtxFront));
            ToolStripMenuItem itemBack = new ToolStripMenuItem("置于底层", null, new EventHandler(OnCtxBack));
            ToolStripSeparator itemSep = new ToolStripSeparator();
            ToolStripMenuItem itemProps = new ToolStripMenuItem("属性...", null, new EventHandler(OnCtxProps));

            menu.Items.Add(itemDelete);
            menu.Items.Add(itemFront);
            menu.Items.Add(itemBack);
            menu.Items.Add(itemSep);
            menu.Items.Add(itemProps);
            menu.Show(_canvas, location);
        }

        private void OnCtxDelete(object sender, EventArgs e) { _canvas.DeleteSelected(); }
        private void OnCtxFront(object sender, EventArgs e) { _canvas.BringToFront(); }
        private void OnCtxBack(object sender, EventArgs e) { _canvas.SendToBack(); }
        private void OnCtxProps(object sender, EventArgs e) { _propertyGrid.Focus(); }

        private void InitializeSampleData()
        {
            ContainerShape container = new ContainerShape();
            container.Name = "部署环境";
            container.HeaderText = "生产环境集群";
            container.X = 100;
            container.Y = 80;
            container.Width = 500;
            container.Height = 350;
            container.FillColor = Color.FromArgb(250, 250, 252);
            container.BorderColor = Color.FromArgb(100, 100, 130);

            RectangleShape rect1 = new RectangleShape();
            rect1.Name = "API网关";
            rect1.X = 140;
            rect1.Y = 140;
            rect1.FillColor = Color.FromArgb(230, 245, 255);
            rect1.BorderColor = Color.FromArgb(60, 130, 200);

            RectangleShape rect2 = new RectangleShape();
            rect2.Name = "用户服务";
            rect2.X = 340;
            rect2.Y = 140;
            rect2.FillColor = Color.FromArgb(230, 255, 240);
            rect2.BorderColor = Color.FromArgb(60, 180, 100);

            EllipseShape ellipse1 = new EllipseShape();
            ellipse1.Name = "数据库";
            ellipse1.X = 340;
            ellipse1.Y = 260;
            ellipse1.FillColor = Color.FromArgb(255, 240, 230);
            ellipse1.BorderColor = Color.FromArgb(200, 130, 60);

            DiamondShape diamond1 = new DiamondShape();
            diamond1.Name = "负载均衡";
            diamond1.X = 160;
            diamond1.Y = 260;
            diamond1.FillColor = Color.FromArgb(255, 240, 255);
            diamond1.BorderColor = Color.FromArgb(180, 60, 180);

            _canvas.Document.AddShape(container);
            _canvas.Document.AddShape(rect1);
            _canvas.Document.AddShape(rect2);
            _canvas.Document.AddShape(ellipse1);
            _canvas.Document.AddShape(diamond1);

            container.AddChild(rect1);
            container.AddChild(rect2);
            container.AddChild(ellipse1);
            container.AddChild(diamond1);

            Connection conn1 = new Connection();
            conn1.FromShape = rect1;
            conn1.ToShape = rect2;
            conn1.Mode = ConnectionMode.Straight;
            conn1.Label = "REST";

            Connection conn2 = new Connection();
            conn2.FromShape = rect2;
            conn2.ToShape = ellipse1;
            conn2.Mode = ConnectionMode.Orthogonal;
            conn2.Label = "SQL";

            Connection conn3 = new Connection();
            conn3.FromShape = diamond1;
            conn3.ToShape = rect1;
            conn3.Mode = ConnectionMode.Curve;

            _canvas.Document.AddConnection(conn1);
            _canvas.Document.AddConnection(conn2);
            _canvas.Document.AddConnection(conn3);

            _propertyGrid.SelectedObject = GlobalConfig.Instance;
            _canvas.Invalidate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Serialization;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Controls
{
    public partial class DiagramEditor : UserControl
    {
        private SplitContainer _mainSplit;
        private SplitContainer _rightSplit;
        private DrawingCanvas _canvas;
        private ToolboxPanel _toolbox;
        private PropertyGrid _propertyGrid;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _zoomLabel;
        private string _currentFilePath = "";
        private bool _contextMenuEnabled = true;

        // 菜单项字段
        private ToolStripMenuItem _menuFile;
        private ToolStripMenuItem _menuEdit;
        private ToolStripMenuItem _menuView;
        private ToolStripMenuItem _menuTools;
        private ToolStripMenuItem _menuFileNew;
        private ToolStripMenuItem _menuFileOpen;
        private ToolStripMenuItem _menuFileSave;
        private ToolStripMenuItem _menuFileSaveAs;
        private ToolStripMenuItem _menuFileExit;
        private ToolStripMenuItem _menuEditDelete;
        private ToolStripMenuItem _menuEditSelectAll;
        private ToolStripMenuItem _menuViewGrid;
        private ToolStripMenuItem _menuViewSnap;
        private ToolStripMenuItem _menuViewZoomIn;
        private ToolStripMenuItem _menuViewZoomOut;
        private ToolStripMenuItem _menuViewReset;
        private ToolStripMenuItem _menuViewToolbar;
        private ToolStripMenuItem _menuViewProperty;
        private ToolStripMenuItem _menuViewToolbox;
        private ToolStripMenuItem _menuViewContextMenu;
        private ToolStripMenuItem _menuToolSelect;
        private ToolStripMenuItem _menuToolConnect;
        private ToolStripMenuItem _menuToolStraight;
        private ToolStripMenuItem _menuToolCurve;
        private ToolStripMenuItem _menuToolOrtho;

        // 工具栏按钮字段
        private ToolStripButton _btnSelect;
        private ToolStripButton _btnConnect;
        private ToolStripButton _btnStraight;
        private ToolStripButton _btnCurve;
        private ToolStripButton _btnOrtho;
        private ToolStripButton _btnZoomIn;
        private ToolStripButton _btnZoomOut;
        private ToolStripButton _btnFit;
        private ToolStripButton _btnFront;
        private ToolStripButton _btnBack;
        private ToolStripButton _btnDelete;

        public DiagramEditor()
        {
            InitializeComponent();
            BuildLayout();
            BuildMenuStrip();
            BuildToolStrip();
            BuildStatusBar();
            WireEvents();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

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
        }

        private void BuildLayout()
        {
            _mainSplit.Panel1.Controls.Add(_toolbox);
            _rightSplit.Panel1.Controls.Add(_canvas);
            _rightSplit.Panel2.Controls.Add(_propertyGrid);
            _mainSplit.Panel2.Controls.Add(_rightSplit);

            _toolStrip.Dock = DockStyle.Top;
            _statusStrip.Dock = DockStyle.Bottom;

            this.Controls.Add(_statusStrip);
            this.Controls.Add(_toolStrip);
            this.Controls.Add(_menuStrip);
            this.Controls.Add(_mainSplit);
            this.MainMenuStrip = _menuStrip;
        }

        private void BuildMenuStrip()
        {
            _menuStrip = new MenuStrip();

            _menuFile = new ToolStripMenuItem("文件(&F)");
            _menuFileNew = new ToolStripMenuItem("新建", null, new EventHandler(OnFileNew));
            _menuFileOpen = new ToolStripMenuItem("打开...", null, new EventHandler(OnFileOpen));
            _menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            _menuFileSave = new ToolStripMenuItem("保存", null, new EventHandler(OnFileSave));
            _menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            _menuFileSaveAs = new ToolStripMenuItem("另存为...", null, new EventHandler(OnFileSaveAs));
            _menuFileExit = new ToolStripMenuItem("退出", null, new EventHandler(OnFileExit));
            _menuFile.DropDownItems.Add(_menuFileNew);
            _menuFile.DropDownItems.Add(_menuFileOpen);
            _menuFile.DropDownItems.Add(_menuFileSave);
            _menuFile.DropDownItems.Add(_menuFileSaveAs);
            _menuFile.DropDownItems.Add(new ToolStripSeparator());
            _menuFile.DropDownItems.Add(_menuFileExit);

            _menuEdit = new ToolStripMenuItem("编辑(&E)");
            _menuEditDelete = new ToolStripMenuItem("删除", null, new EventHandler(OnEditDelete));
            _menuEditDelete.ShortcutKeys = Keys.Delete;
            _menuEditSelectAll = new ToolStripMenuItem("全选", null, new EventHandler(OnEditSelectAll));
            _menuEditSelectAll.ShortcutKeys = Keys.Control | Keys.A;
            _menuEdit.DropDownItems.Add(_menuEditDelete);
            _menuEdit.DropDownItems.Add(_menuEditSelectAll);

            _menuView = new ToolStripMenuItem("视图(&V)");
            _menuViewGrid = new ToolStripMenuItem("网格", null, new EventHandler(OnViewGrid));
            _menuViewGrid.Checked = GlobalConfig.Instance.ShowGrid;
            _menuViewSnap = new ToolStripMenuItem("对齐", null, new EventHandler(OnViewSnap));
            _menuViewSnap.Checked = GlobalConfig.Instance.SnapToGrid;
            _menuViewToolbar = new ToolStripMenuItem("工具栏", null, new EventHandler(OnViewToolbar));
            _menuViewToolbar.Checked = true;
            _menuViewProperty = new ToolStripMenuItem("属性栏", null, new EventHandler(OnViewProperty));
            _menuViewProperty.Checked = true;
            _menuViewToolbox = new ToolStripMenuItem("工具箱", null, new EventHandler(OnViewToolbox));
            _menuViewToolbox.Checked = true;
            _menuViewContextMenu = new ToolStripMenuItem("右键菜单", null, new EventHandler(OnViewContextMenu));
            _menuViewContextMenu.Checked = true;
            _menuViewZoomIn = new ToolStripMenuItem("放大", null, new EventHandler(OnViewZoomIn));
            _menuViewZoomOut = new ToolStripMenuItem("缩小", null, new EventHandler(OnViewZoomOut));
            _menuViewReset = new ToolStripMenuItem("重置视图", null, new EventHandler(OnViewReset));
            _menuView.DropDownItems.Add(_menuViewGrid);
            _menuView.DropDownItems.Add(_menuViewSnap);
            _menuView.DropDownItems.Add(new ToolStripSeparator());
            _menuView.DropDownItems.Add(_menuViewToolbar);
            _menuView.DropDownItems.Add(_menuViewProperty);
            _menuView.DropDownItems.Add(_menuViewToolbox);
            _menuView.DropDownItems.Add(_menuViewContextMenu);
            _menuView.DropDownItems.Add(new ToolStripSeparator());
            _menuView.DropDownItems.Add(_menuViewZoomIn);
            _menuView.DropDownItems.Add(_menuViewZoomOut);
            _menuView.DropDownItems.Add(_menuViewReset);

            _menuTools = new ToolStripMenuItem("工具(&T)");
            _menuToolSelect = new ToolStripMenuItem("选择工具", null, new EventHandler(OnToolSelect));
            _menuToolConnect = new ToolStripMenuItem("连线工具", null, new EventHandler(OnToolConnect));
            _menuToolStraight = new ToolStripMenuItem("直线模式", null, new EventHandler(OnToolStraight));
            _menuToolCurve = new ToolStripMenuItem("曲线模式", null, new EventHandler(OnToolCurve));
            _menuToolOrtho = new ToolStripMenuItem("折线模式", null, new EventHandler(OnToolOrtho));
            _menuTools.DropDownItems.Add(_menuToolSelect);
            _menuTools.DropDownItems.Add(_menuToolConnect);
            _menuTools.DropDownItems.Add(new ToolStripSeparator());
            _menuTools.DropDownItems.Add(_menuToolStraight);
            _menuTools.DropDownItems.Add(_menuToolCurve);
            _menuTools.DropDownItems.Add(_menuToolOrtho);

            _menuStrip.Items.Add(_menuFile);
            _menuStrip.Items.Add(_menuEdit);
            _menuStrip.Items.Add(_menuView);
            _menuStrip.Items.Add(_menuTools);
        }

        private void BuildToolStrip()
        {
            _toolStrip = new ToolStrip();

            _btnSelect = new ToolStripButton("选择", null, new EventHandler(OnSelectTool));
            _btnSelect.CheckOnClick = true;
            _btnSelect.Checked = true;
            _btnSelect.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnConnect = new ToolStripButton("连线", null, new EventHandler(OnConnectTool));
            _btnConnect.CheckOnClick = true;
            _btnConnect.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnStraight = new ToolStripButton("直线", null, new EventHandler(OnStraightMode));
            _btnStraight.CheckOnClick = true;
            _btnStraight.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnCurve = new ToolStripButton("曲线", null, new EventHandler(OnCurveMode));
            _btnCurve.CheckOnClick = true;
            _btnCurve.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnOrtho = new ToolStripButton("折线", null, new EventHandler(OnOrthoMode));
            _btnOrtho.CheckOnClick = true;
            _btnOrtho.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnZoomIn = new ToolStripButton("放大", null, new EventHandler(OnZoomIn));
            _btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnZoomOut = new ToolStripButton("缩小", null, new EventHandler(OnZoomOut));
            _btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnFit = new ToolStripButton("适应", null, new EventHandler(OnZoomFit));
            _btnFit.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnFront = new ToolStripButton("置顶", null, new EventHandler(OnBringToFront));
            _btnFront.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnBack = new ToolStripButton("置底", null, new EventHandler(OnSendToBack));
            _btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _btnDelete = new ToolStripButton("删除", null, new EventHandler(OnDeleteSelected));
            _btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;

            _toolStrip.Items.Add(_btnSelect);
            _toolStrip.Items.Add(_btnConnect);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnStraight);
            _toolStrip.Items.Add(_btnCurve);
            _toolStrip.Items.Add(_btnOrtho);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnZoomIn);
            _toolStrip.Items.Add(_btnZoomOut);
            _toolStrip.Items.Add(_btnFit);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnFront);
            _toolStrip.Items.Add(_btnBack);
            _toolStrip.Items.Add(_btnDelete);
        }

        private void BuildStatusBar()
        {
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("就绪");
            _zoomLabel = new ToolStripStatusLabel("缩放: 100%");
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripStatusLabel(""));
            _statusStrip.Items.Add(_zoomLabel);
        }

        private void WireEvents()
        {
            _canvas.SelectionChanged += new EventHandler(OnCanvasSelectionChanged);
            _canvas.DocumentModified += new EventHandler(OnDocumentModified);
            _canvas.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            _canvas.MouseClick += new MouseEventHandler(OnCanvasMouseClick);
            _propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);
        }

        // ===== 公共属性 =====

        public DrawingCanvas Canvas
        {
            get { return _canvas; }
        }

        public DrawingDocument Document
        {
            get { return _canvas.Document; }
        }

        public string CurrentFilePath
        {
            get { return _currentFilePath; }
            set { _currentFilePath = value; }
        }

        [Category("面板")]
        [Description("是否显示工具栏")]
        public bool ShowToolbar
        {
            get { return _toolStrip.Visible; }
            set
            {
                _toolStrip.Visible = value;
                _menuViewToolbar.Checked = value;
            }
        }

        [Category("面板")]
        [Description("是否显示属性栏")]
        public bool ShowPropertyPanel
        {
            get { return !_rightSplit.Panel2Collapsed; }
            set
            {
                _rightSplit.Panel2Collapsed = !value;
                _menuViewProperty.Checked = value;
            }
        }

        [Category("面板")]
        [Description("是否显示工具箱面板")]
        public bool ShowToolboxPanel
        {
            get { return !_mainSplit.Panel1Collapsed; }
            set
            {
                _mainSplit.Panel1Collapsed = !value;
                _menuViewToolbox.Checked = value;
            }
        }

        [Category("面板")]
        [Description("是否显示菜单栏")]
        public bool ShowMenuStrip
        {
            get { return _menuStrip.Visible; }
            set { _menuStrip.Visible = value; }
        }

        [Category("面板")]
        [Description("是否显示状态栏")]
        public bool ShowStatusBar
        {
            get { return _statusStrip.Visible; }
            set { _statusStrip.Visible = value; }
        }

        [Category("行为")]
        [Description("是否启用右键菜单")]
        public bool ShowContextMenu
        {
            get { return _contextMenuEnabled; }
            set
            {
                _contextMenuEnabled = value;
                _menuViewContextMenu.Checked = value;
            }
        }

        // ===== 公共快捷方法 =====

        public void ZoomIn()
        {
            _canvas.Zoom *= 1.2f;
            UpdateZoomLabel();
        }

        public void ZoomOut()
        {
            _canvas.Zoom /= 1.2f;
            UpdateZoomLabel();
        }

        public void ZoomReset()
        {
            _canvas.Zoom = 1.0f;
            _canvas.Offset = new PointF(0, 0);
            UpdateZoomLabel();
        }

        public void SetTool(CanvasTool tool)
        {
            _canvas.CurrentTool = tool;
            UpdateToolState();
        }

        public void SetConnectionMode(ConnectionMode mode)
        {
            GlobalConfig.Instance.DefaultConnectionMode = mode;

            List<Connection> conns = _canvas.Document.GetSelectedConnections();
            foreach (Connection conn in conns)
            {
                conn.Mode = mode;
            }

            if (_btnStraight != null)
                _btnStraight.Checked = (mode == ConnectionMode.Straight);
            if (_btnCurve != null)
                _btnCurve.Checked = (mode == ConnectionMode.Curve);
            if (_btnOrtho != null)
                _btnOrtho.Checked = (mode == ConnectionMode.Orthogonal);

            _canvas.Invalidate();
        }

        public void DeleteSelected()
        {
            _canvas.DeleteSelected();
        }

        public void SelectAll()
        {
            foreach (ShapeBase s in _canvas.Document.Shapes)
                s.Selected = true;
            _canvas.Invalidate();
        }

        public void BringToFront()
        {
            _canvas.BringToFront();
        }

        public void SendToBack()
        {
            _canvas.SendToBack();
        }

        public void NewDocument()
        {
            _canvas.Document.Clear();
            _currentFilePath = "";
            _propertyGrid.SelectedObject = GlobalConfig.Instance;
            _statusLabel.Text = "新建文档";
            _canvas.Invalidate();
        }

        public void SaveDocument(string filePath)
        {
            if (filePath == null || filePath.Length == 0)
                return;
            XmlShapeSerializer.Save(filePath, _canvas.Document);
            _currentFilePath = filePath;
            _statusLabel.Text = "已保存: " + filePath;
        }

        public void LoadDocument(string filePath)
        {
            if (filePath == null || filePath.Length == 0)
                return;
            DrawingDocument doc = XmlShapeSerializer.Load(filePath);
            _canvas.Document.Clear();

            foreach (ShapeBase shape in doc.Shapes)
                _canvas.Document.AddShape(shape);
            foreach (Connection conn in doc.Connections)
                _canvas.Document.AddConnection(conn);

            _currentFilePath = filePath;
            _statusLabel.Text = "已打开: " + filePath;
            _canvas.Invalidate();
        }

        // ===== 事件 =====

        public event EventHandler DocumentSaved;
        public event EventHandler DocumentLoaded;
        public event EventHandler NewDocumentCreated;

        protected virtual void OnDocSaved(EventArgs e)
        {
            if (DocumentSaved != null)
                DocumentSaved(this, e);
        }

        protected virtual void OnDocLoaded(EventArgs e)
        {
            if (DocumentLoaded != null)
                DocumentLoaded(this, e);
        }

        protected virtual void OnDocNew(EventArgs e)
        {
            if (NewDocumentCreated != null)
                NewDocumentCreated(this, e);
        }

        // ===== 内部辅助 =====

        private void UpdateToolState()
        {
            if (_btnSelect != null)
                _btnSelect.Checked = (_canvas.CurrentTool == CanvasTool.Select);
            if (_btnConnect != null)
                _btnConnect.Checked = (_canvas.CurrentTool == CanvasTool.Connect);
        }

        private void UpdateZoomLabel()
        {
            if (_zoomLabel != null)
            {
                _zoomLabel.Text = string.Format("缩放: {0:0}%", _canvas.Zoom * 100);
            }
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _canvas.Invalidate();
        }
    }
}

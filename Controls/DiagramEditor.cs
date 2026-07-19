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
        private bool _showToolbarText = false;
        private int _toolbarRenderMode = 0; // 0=Image, 1=ImageAndText, 2=Text

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
        private ToolStripMenuItem _menuViewToolbarText;
        private ToolStripMenuItem _menuViewProperty;
        private ToolStripMenuItem _menuViewToolbox;
        private ToolStripMenuItem _menuViewContextMenu;
        private ToolStripMenuItem _menuViewThemeLight;
        private ToolStripMenuItem _menuViewThemeDark;
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
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;

            _mainSplit = new SplitContainer();
            _mainSplit.Dock = DockStyle.Fill;
            _mainSplit.Orientation = Orientation.Vertical;
            _mainSplit.SplitterDistance = 220;
            _mainSplit.FixedPanel = FixedPanel.Panel1;
            _mainSplit.BackColor = GlobalConfig.Instance.ToolPanelBackColor;
            _mainSplit.Panel1MinSize = 120;
            _mainSplit.Panel2MinSize = 200;

            _rightSplit = new SplitContainer();
            _rightSplit.Dock = DockStyle.Fill;
            _rightSplit.Orientation = Orientation.Vertical;
            _rightSplit.FixedPanel = FixedPanel.Panel2;
            _rightSplit.Panel2MinSize = 180;

            _toolbox = new ToolboxPanel();
            _toolbox.Dock = DockStyle.Fill;

            _canvas = new DrawingCanvas();
            _canvas.Dock = DockStyle.Fill;
            _canvas.BackColor = GlobalConfig.Instance.CanvasBackground;

            _propertyGrid = new PropertyGrid();
            _propertyGrid.Dock = DockStyle.Fill;
            _propertyGrid.ToolbarVisible = true;
            _propertyGrid.HelpVisible = true;
            _propertyGrid.PropertySort = PropertySort.CategorizedAlphabetical;
            _propertyGrid.SelectedObject = GlobalConfig.Instance;
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

            // 延迟设置右侧分栏距离，需要等控件布局完成
            this.Load += new EventHandler(OnFirstLoad);
        }

        private void OnFirstLoad(object sender, EventArgs e)
        {
            _rightSplit.SplitterDistance = _rightSplit.Width - 280;
            this.Load -= new EventHandler(OnFirstLoad);
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
            _menuViewToolbarText = new ToolStripMenuItem("工具栏文字", null, new EventHandler(OnViewToolbarText));
            _menuViewToolbarText.Checked = false;
            _menuViewProperty = new ToolStripMenuItem("属性栏", null, new EventHandler(OnViewProperty));
            _menuViewProperty.Checked = true;
            _menuViewToolbox = new ToolStripMenuItem("工具箱", null, new EventHandler(OnViewToolbox));
            _menuViewToolbox.Checked = true;
            _menuViewContextMenu = new ToolStripMenuItem("右键菜单", null, new EventHandler(OnViewContextMenu));
            _menuViewContextMenu.Checked = true;
            _menuViewThemeLight = new ToolStripMenuItem("亮色主题", null, new EventHandler(OnThemeLight));
            _menuViewThemeDark = new ToolStripMenuItem("暗色主题", null, new EventHandler(OnThemeDark));
            _menuViewZoomIn = new ToolStripMenuItem("放大", null, new EventHandler(OnViewZoomIn));
            _menuViewZoomOut = new ToolStripMenuItem("缩小", null, new EventHandler(OnViewZoomOut));
            _menuViewReset = new ToolStripMenuItem("重置视图", null, new EventHandler(OnViewReset));
            _menuView.DropDownItems.Add(_menuViewGrid);
            _menuView.DropDownItems.Add(_menuViewSnap);
            _menuView.DropDownItems.Add(new ToolStripSeparator());
            _menuView.DropDownItems.Add(_menuViewToolbar);
            _menuView.DropDownItems.Add(_menuViewToolbarText);
            _menuView.DropDownItems.Add(_menuViewProperty);
            _menuView.DropDownItems.Add(_menuViewToolbox);
            _menuView.DropDownItems.Add(_menuViewContextMenu);
            _menuView.DropDownItems.Add(new ToolStripSeparator());
            _menuView.DropDownItems.Add(_menuViewThemeLight);
            _menuView.DropDownItems.Add(_menuViewThemeDark);
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
            _toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            _toolStrip.Padding = new Padding(2, 0, 2, 0);
            _toolStrip.ImageScalingSize = new Size(20, 20);

            _btnSelect = new ToolStripButton(SystemIcons.WinLogo, "选择工具");
            _btnSelect.CheckOnClick = true;
            _btnSelect.Checked = true;
            _btnSelect.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnSelect.ToolTipText = "选择工具 (V)";

            _btnConnect = new ToolStripButton(SystemIcons.Warning, "连线工具");
            _btnConnect.CheckOnClick = true;
            _btnConnect.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnConnect.ToolTipText = "连线工具 (L)";

            _btnStraight = new ToolStripButton(CreateLineIcon(false), "直线模式");
            _btnStraight.CheckOnClick = true;
            _btnStraight.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnStraight.ToolTipText = "直线模式";

            _btnCurve = new ToolStripButton(CreateCurveIcon(), "曲线模式");
            _btnCurve.CheckOnClick = true;
            _btnCurve.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnCurve.ToolTipText = "曲线模式";

            _btnOrtho = new ToolStripButton(CreateOrthoIcon(), "折线模式");
            _btnOrtho.CheckOnClick = true;
            _btnOrtho.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnOrtho.ToolTipText = "折线模式";

            _toolStrip.Items.Add(_btnSelect);
            _toolStrip.Items.Add(_btnConnect);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(_btnStraight);
            _toolStrip.Items.Add(_btnCurve);
            _toolStrip.Items.Add(_btnOrtho);
            _toolStrip.Items.Add(new ToolStripSeparator());

            _btnZoomIn = new ToolStripButton(CreateZoomInIcon(), "放大");
            _btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnZoomIn.ToolTipText = "放大 (+)";

            _btnZoomOut = new ToolStripButton(CreateZoomOutIcon(), "缩小");
            _btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnZoomOut.ToolTipText = "缩小 (-)";

            _btnFit = new ToolStripButton(CreateFitIcon(), "适应视图");
            _btnFit.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnFit.ToolTipText = "重置视图";

            _toolStrip.Items.Add(_btnZoomIn);
            _toolStrip.Items.Add(_btnZoomOut);
            _toolStrip.Items.Add(_btnFit);
            _toolStrip.Items.Add(new ToolStripSeparator());

            _btnFront = new ToolStripButton(CreateTopIcon(), "置顶");
            _btnFront.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnFront.ToolTipText = "置于顶层";

            _btnBack = new ToolStripButton(CreateBottomIcon(), "置底");
            _btnBack.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnBack.ToolTipText = "置于底层";

            _toolStrip.Items.Add(_btnFront);
            _toolStrip.Items.Add(_btnBack);
            _toolStrip.Items.Add(new ToolStripSeparator());

            _btnDelete = new ToolStripButton(SystemIcons.Delete, "删除");
            _btnDelete.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _btnDelete.ToolTipText = "删除选中 (Delete)";

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

        // ===== 图标创建方法（纯代码绘制16x16图标）=====

        private Icon CreateLineIcon(bool curved)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (curved)
                {
                    g.DrawBezier(pen, 2, 13, 5, 2, 11, 2, 14, 13);
                }
                else
                {
                    g.DrawLine(pen, 2, 13, 14, 2);
                }
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateCurveIcon()
        {
            return CreateLineIcon(true);
        }

        private Icon CreateOrthoIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLines(pen, new PointF[] {
                    new PointF(2, 13), new PointF(2, 3),
                    new PointF(14, 3), new PointF(14, 13)
                });
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateZoomInIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawEllipse(pen, 3, 3, 9, 9);
                g.DrawLine(pen, 10, 10, 14, 14);
                g.DrawLine(pen, 6, 7, 9, 7);
                g.DrawLine(pen, 7.5f, 6, 7.5f, 9);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateZoomOutIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawEllipse(pen, 3, 3, 9, 9);
                g.DrawLine(pen, 10, 10, 14, 14);
                g.DrawLine(pen, 6, 7, 9, 7);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateFitIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawRectangle(pen, 3, 5, 10, 6);
                g.DrawRectangle(pen, 5, 3, 6, 10);
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateTopIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            using (Brush brush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPolygon(pen, new PointF[] {
                    new PointF(3, 11), new PointF(8, 4), new PointF(13, 11)
                });
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        private Icon CreateBottomIcon()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(Color.FromArgb(80, 80, 80), 1.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPolygon(pen, new PointF[] {
                    new PointF(3, 4), new PointF(8, 11), new PointF(13, 4)
                });
            }
            return Icon.FromHandle(bmp.GetHicon());
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

        [Category("外观")]
        [Description("工具栏是否显示文字标签")]
        public bool ShowToolbarText
        {
            get { return _showToolbarText; }
            set
            {
                _showToolbarText = value;
                _menuViewToolbarText.Checked = value;
                ApplyToolbarRenderMode();
            }
        }

        [Category("外观")]
        [Description("编辑器配色主题")]
        public EditorTheme Theme
        {
            get { return GlobalConfig.Instance.Theme; }
            set
            {
                GlobalConfig.Instance.Theme = value;
                ApplyTheme();
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
        public event EventHandler ThemeChanged;

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

        protected virtual void OnThemeChanged(EventArgs e)
        {
            if (ThemeChanged != null)
                ThemeChanged(this, e);
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

        private void ApplyToolbarRenderMode()
        {
            ToolStripItemDisplayStyle style = _showToolbarText
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Image;

            if (_btnSelect != null) _btnSelect.DisplayStyle = style;
            if (_btnConnect != null) _btnConnect.DisplayStyle = style;
            if (_btnStraight != null) _btnStraight.DisplayStyle = style;
            if (_btnCurve != null) _btnCurve.DisplayStyle = style;
            if (_btnOrtho != null) _btnOrtho.DisplayStyle = style;
            if (_btnZoomIn != null) _btnZoomIn.DisplayStyle = style;
            if (_btnZoomOut != null) _btnZoomOut.DisplayStyle = style;
            if (_btnFit != null) _btnFit.DisplayStyle = style;
            if (_btnFront != null) _btnFront.DisplayStyle = style;
            if (_btnBack != null) _btnBack.DisplayStyle = style;
            if (_btnDelete != null) _btnDelete.DisplayStyle = style;
        }

        public void ApplyTheme()
        {
            EditorTheme theme = GlobalConfig.Instance.Theme;

            _mainSplit.BackColor = GlobalConfig.Instance.ToolPanelBackColor;
            _canvas.BackColor = GlobalConfig.Instance.CanvasBackground;

            bool isDark = (theme == EditorTheme.Dark);

            _menuViewThemeLight.Checked = !isDark;
            _menuViewThemeDark.Checked = isDark;

            Color backColor = isDark ? Color.FromArgb(30, 35, 48) : Color.FromArgb(245, 248, 252);
            Color foreColor = isDark ? Color.FromArgb(210, 220, 235) : Color.FromArgb(30, 30, 30);

            _toolStrip.BackColor = backColor;
            _toolStrip.ForeColor = foreColor;
            _toolStrip.Renderer = new ToolStripProfessionalRenderer(new MyColorTable(isDark));

            _menuStrip.BackColor = backColor;
            _menuStrip.ForeColor = foreColor;
            _menuStrip.Renderer = new ToolStripProfessionalRenderer(new MyColorTable(isDark));

            _statusStrip.BackColor = isDark ? Color.FromArgb(25, 30, 42) : Color.FromArgb(240, 242, 248);
            _statusStrip.ForeColor = foreColor;

            this.Invalidate(true);
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _canvas.Invalidate();
        }
    }

    // ===== 主题色彩表 =====

    internal class MyColorTable : ProfessionalColorTable
    {
        private bool _dark;

        public MyColorTable(bool dark)
        {
            _dark = dark;
        }

        public override Color ToolStripGradientBegin
        {
            get { return _dark ? Color.FromArgb(45, 50, 65) : Color.FromArgb(250, 252, 255); }
        }

        public override Color ToolStripGradientMiddle
        {
            get { return _dark ? Color.FromArgb(38, 43, 58) : Color.FromArgb(245, 247, 252); }
        }

        public override Color ToolStripGradientEnd
        {
            get { return _dark ? Color.FromArgb(32, 37, 50) : Color.FromArgb(240, 242, 248); }
        }

        public override Color MenuStripGradientBegin
        {
            get { return _dark ? Color.FromArgb(45, 50, 65) : Color.FromArgb(250, 252, 255); }
        }

        public override Color MenuStripGradientEnd
        {
            get { return _dark ? Color.FromArgb(38, 43, 58) : Color.FromArgb(245, 247, 252); }
        }

        public override Color MenuItemSelected
        {
            get { return _dark ? Color.FromArgb(60, 120, 200) : Color.FromArgb(220, 235, 250); }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return _dark ? Color.FromArgb(50, 110, 190) : Color.FromArgb(210, 230, 248); }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return _dark ? Color.FromArgb(45, 100, 175) : Color.FromArgb(200, 225, 245); }
        }

        public override Color SeparatorDark
        {
            get { return _dark ? Color.FromArgb(55, 60, 78) : Color.FromArgb(210, 215, 225); }
        }

        public override Color SeparatorLight
        {
            get { return _dark ? Color.FromArgb(70, 75, 92) : Color.FromArgb(230, 233, 240); }
        }

        public override Color ImageMarginGradientBegin
        {
            get { return _dark ? Color.FromArgb(42, 47, 62) : Color.FromArgb(248, 250, 255); }
        }

        public override Color ImageMarginGradientMiddle
        {
            get { return _dark ? Color.FromArgb(38, 43, 58) : Color.FromArgb(245, 247, 252); }
        }

        public override Color ImageMarginGradientEnd
        {
            get { return _dark ? Color.FromArgb(35, 40, 52) : Color.FromArgb(242, 244, 250); }
        }

        public override Color ButtonSelectedHighlight
        {
            get { return _dark ? Color.FromArgb(70, 140, 220) : Color.FromArgb(200, 225, 248); }
        }

        public override Color ButtonPressedHighlight
        {
            get { return _dark ? Color.FromArgb(60, 120, 200) : Color.FromArgb(190, 218, 242); }
        }

        public override Color ButtonSelectedBorder
        {
            get { return _dark ? Color.FromArgb(90, 160, 240) : Color.FromArgb(160, 200, 235); }
        }

        public override Color StatusStripGradientBegin
        {
            get { return _dark ? Color.FromArgb(28, 32, 45) : Color.FromArgb(242, 244, 250); }
        }

        public override Color StatusStripGradientEnd
        {
            get { return _dark ? Color.FromArgb(25, 28, 40) : Color.FromArgb(235, 238, 245); }
        }
    }
}

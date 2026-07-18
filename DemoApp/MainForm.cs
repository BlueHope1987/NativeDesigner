using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Config;
using CloudNativeDesigner.Controls;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Serialization;
using CloudNativeDesigner.Shapes;

namespace DemoApp
{
    public class MainForm : Form
    {
        private DrawingCanvas _canvas;
        private ToolboxPanel _toolbox;
        private PropertyGrid _propertyGrid;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _zoomLabel;
        private SplitContainer _mainSplit;
        private SplitContainer _rightSplit;
        private string _currentFilePath = "";
        private bool _contextMenuEnabled = true;

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

        public MainForm()
        {
            InitializeComponent();
            this.Text = "云原生可视化设计器 - 演示应用";
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

            InitializeMenuStrip();
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
            this.Controls.Add(_menuStrip);
            this.Controls.Add(_mainSplit);
            this.MainMenuStrip = _menuStrip;

            _canvas.SelectionChanged += new EventHandler(OnCanvasSelectionChanged);
            _canvas.DocumentModified += new EventHandler(OnDocumentModified);
            _canvas.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            _canvas.MouseClick += new MouseEventHandler(OnCanvasMouseClick);
            _propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(OnPropertyValueChanged);

            InitializeShapeTypes();
            _toolbox.ReloadFromRegistry();
            InitializeSampleData();
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _canvas.Invalidate();
        }

        private void InitializeMenuStrip()
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

        private void InitializeToolStrip()
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

        #region Shape Type Registration

        private void InitializeShapeTypes()
        {
            ShapeTypeRegistry.Instance.Clear();

            RegisterRectangleType();
            RegisterEllipseType();
            RegisterDiamondType();
            RegisterHexagonType();
            RegisterCloudType();
            RegisterDatabaseType();
            RegisterStartEndType();
            RegisterDocumentType();
            RegisterUserType();
            RegisterNoteType();
            RegisterComponentType();
            RegisterContainerType();
            RegisterClassType();
        }

        private void RegisterRectangleType()
        {
            ShapeType t = new ShapeType();
            t.Name = "矩形";
            t.Category = "基本图形";
            t.Description = "圆角矩形";
            t.DefaultWidth = 140;
            t.DefaultHeight = 90;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(230, 245, 255));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 130, 200));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.RoundedRect;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.CornerRadius = 6f;
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand txt = new RenderCommand();
            txt.CommandType = RenderCommandType.Text;
            txt.X = 0f; txt.Y = 0f;
            txt.Width = 1f; txt.Height = 0.35f;
            txt.Text = "";
            txt.TextAlign = "center";
            txt.FontSize = 10f;
            txt.IsBold = false;
            txt.UseShapeColors = true;
            t.RenderCommands.Add(txt);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterEllipseType()
        {
            ShapeType t = new ShapeType();
            t.Name = "椭圆";
            t.Category = "基本图形";
            t.Description = "椭圆形";
            t.DefaultWidth = 120;
            t.DefaultHeight = 100;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(255, 240, 230));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(200, 130, 60));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Ellipse;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand txt = new RenderCommand();
            txt.CommandType = RenderCommandType.Text;
            txt.X = 0f; txt.Y = 0f;
            txt.Width = 1f; txt.Height = 0.35f;
            txt.Text = "";
            txt.TextAlign = "center";
            txt.FontSize = 10f;
            txt.UseShapeColors = true;
            t.RenderCommands.Add(txt);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterDiamondType()
        {
            ShapeType t = new ShapeType();
            t.Name = "菱形";
            t.Category = "基本图形";
            t.Description = "菱形决策节点";
            t.DefaultWidth = 120;
            t.DefaultHeight = 100;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(230, 255, 240));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 180, 100));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Polygon;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.PolygonPoints = new PointF[] {
                new PointF(0.5f, 0f),
                new PointF(1f, 0.5f),
                new PointF(0.5f, 1f),
                new PointF(0f, 0.5f)
            };
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand txt = new RenderCommand();
            txt.CommandType = RenderCommandType.Text;
            txt.X = 0f; txt.Y = 0f;
            txt.Width = 1f; txt.Height = 0.35f;
            txt.Text = "";
            txt.TextAlign = "center";
            txt.FontSize = 10f;
            txt.UseShapeColors = true;
            t.RenderCommands.Add(txt);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterHexagonType()
        {
            ShapeType t = new ShapeType();
            t.Name = "六边形";
            t.Category = "基本图形";
            t.Description = "正六边形";
            t.DefaultWidth = 130;
            t.DefaultHeight = 110;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(240, 230, 255));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(130, 60, 200));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Polygon;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.PolygonPoints = new PointF[] {
                new PointF(0.25f, 0f),
                new PointF(0.75f, 0f),
                new PointF(1f, 0.5f),
                new PointF(0.75f, 1f),
                new PointF(0.25f, 1f),
                new PointF(0f, 0.5f)
            };
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterCloudType()
        {
            ShapeType t = new ShapeType();
            t.Name = "云";
            t.Category = "云原生";
            t.Description = "云计算节点";
            t.DefaultWidth = 160;
            t.DefaultHeight = 100;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(240, 248, 255));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(0, 120, 215));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Polygon;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.PolygonPoints = new PointF[] {
                new PointF(0.20f, 0.65f),
                new PointF(0.08f, 0.55f),
                new PointF(0.12f, 0.35f),
                new PointF(0.30f, 0.25f),
                new PointF(0.45f, 0.15f),
                new PointF(0.65f, 0.15f),
                new PointF(0.80f, 0.25f),
                new PointF(0.92f, 0.35f),
                new PointF(0.90f, 0.55f),
                new PointF(0.78f, 0.65f),
                new PointF(0.65f, 0.70f),
                new PointF(0.35f, 0.70f)
            };
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterDatabaseType()
        {
            ShapeType t = new ShapeType();
            t.Name = "数据库";
            t.Category = "云原生";
            t.Description = "圆柱体数据库";
            t.DefaultWidth = 130;
            t.DefaultHeight = 120;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(230, 245, 255));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 100, 180));
            t.SupportsMembers = true;

            float topY = 0.08f;
            float midY = 0.55f;
            float botY = 0.92f;

            RenderCommand body = new RenderCommand();
            body.CommandType = RenderCommandType.Polygon;
            body.X = 0f; body.Y = 0f;
            body.Width = 1f; body.Height = 1f;
            body.PolygonPoints = new PointF[] {
                new PointF(0.10f, topY),
                new PointF(0.90f, topY),
                new PointF(0.90f, botY),
                new PointF(0.10f, botY)
            };
            body.UseShapeColors = true;
            body.Fill = true;
            body.Stroke = false;
            t.RenderCommands.Add(body);

            RenderCommand topCap = new RenderCommand();
            topCap.CommandType = RenderCommandType.Ellipse;
            topCap.X = 0.10f; topCap.Y = 0f;
            topCap.Width = 0.80f; topCap.Height = topY * 2;
            topCap.UseShapeColors = true;
            topCap.Fill = true;
            topCap.Stroke = true;
            topCap.StrokeWidth = 1.5f;
            t.RenderCommands.Add(topCap);

            RenderCommand midLine = new RenderCommand();
            midLine.CommandType = RenderCommandType.Line;
            midLine.X = 0.10f; midLine.Y = midY;
            midLine.Width = 0.80f; midLine.Height = 0f;
            midLine.StrokeColor = new XmlColor(Color.FromArgb(100, 100, 160));
            midLine.StrokeWidth = 1f;
            midLine.UseShapeColors = false;
            t.RenderCommands.Add(midLine);

            RenderCommand botCap = new RenderCommand();
            botCap.CommandType = RenderCommandType.Ellipse;
            botCap.X = 0.10f; botCap.Y = botY - topY;
            botCap.Width = 0.80f; botCap.Height = topY * 2;
            botCap.UseShapeColors = true;
            botCap.Fill = false;
            botCap.Stroke = true;
            botCap.StrokeWidth = 1.5f;
            t.RenderCommands.Add(botCap);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterStartEndType()
        {
            ShapeType t = new ShapeType();
            t.Name = "起止";
            t.Category = "流程图";
            t.Description = "流程图起止节点";
            t.DefaultWidth = 140;
            t.DefaultHeight = 70;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(255, 235, 235));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(200, 80, 80));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.RoundedRect;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.CornerRadius = 18f;
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterDocumentType()
        {
            ShapeType t = new ShapeType();
            t.Name = "文档";
            t.Category = "流程图";
            t.Description = "带折角的文档";
            t.DefaultWidth = 130;
            t.DefaultHeight = 110;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(255, 250, 240));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(180, 140, 80));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Polygon;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.PolygonPoints = new PointF[] {
                new PointF(0f, 0f),
                new PointF(0.75f, 0f),
                new PointF(1f, 0.22f),
                new PointF(1f, 1f),
                new PointF(0f, 1f)
            };
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand fold = new RenderCommand();
            fold.CommandType = RenderCommandType.Line;
            fold.X = 0.75f; fold.Y = 0f;
            fold.Width = 0f; fold.Height = 0.22f;
            fold.StrokeColor = new XmlColor(Color.FromArgb(180, 140, 80));
            fold.StrokeWidth = 1f;
            fold.UseShapeColors = false;
            t.RenderCommands.Add(fold);

            RenderCommand fold2 = new RenderCommand();
            fold2.CommandType = RenderCommandType.Line;
            fold2.X = 0.75f; fold2.Y = 0.22f;
            fold2.Width = 0.25f; fold2.Height = 0f;
            fold2.StrokeColor = new XmlColor(Color.FromArgb(180, 140, 80));
            fold2.StrokeWidth = 1f;
            fold2.UseShapeColors = false;
            t.RenderCommands.Add(fold2);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterUserType()
        {
            ShapeType t = new ShapeType();
            t.Name = "用户";
            t.Category = "UML";
            t.Description = "参与者/用户";
            t.DefaultWidth = 100;
            t.DefaultHeight = 130;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(255, 240, 230));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(160, 100, 60));
            t.SupportsMembers = true;

            RenderCommand head = new RenderCommand();
            head.CommandType = RenderCommandType.Ellipse;
            head.X = 0.25f; head.Y = 0f;
            head.Width = 0.50f; head.Height = 0.28f;
            head.UseShapeColors = true;
            head.Fill = true;
            head.Stroke = true;
            head.StrokeWidth = 1.5f;
            t.RenderCommands.Add(head);

            RenderCommand body = new RenderCommand();
            body.CommandType = RenderCommandType.Polygon;
            body.X = 0f; body.Y = 0f;
            body.Width = 1f; body.Height = 1f;
            body.PolygonPoints = new PointF[] {
                new PointF(0.50f, 0.28f),
                new PointF(0.85f, 0.48f),
                new PointF(0.95f, 1f),
                new PointF(0.05f, 1f),
                new PointF(0.15f, 0.48f)
            };
            body.UseShapeColors = true;
            body.Fill = true;
            body.Stroke = true;
            body.StrokeWidth = 1.5f;
            t.RenderCommands.Add(body);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterNoteType()
        {
            ShapeType t = new ShapeType();
            t.Name = "注释";
            t.Category = "流程图";
            t.Description = "便签注释";
            t.DefaultWidth = 150;
            t.DefaultHeight = 110;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(255, 255, 200));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(200, 180, 60));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.Polygon;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.PolygonPoints = new PointF[] {
                new PointF(0f, 0f),
                new PointF(0.85f, 0f),
                new PointF(1f, 0.15f),
                new PointF(1f, 1f),
                new PointF(0f, 1f)
            };
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand fold = new RenderCommand();
            fold.CommandType = RenderCommandType.Polygon;
            fold.X = 0f; fold.Y = 0f;
            fold.Width = 1f; fold.Height = 1f;
            fold.PolygonPoints = new PointF[] {
                new PointF(0.85f, 0f),
                new PointF(0.85f, 0.15f),
                new PointF(1f, 0.15f)
            };
            fold.StrokeColor = new XmlColor(Color.FromArgb(200, 180, 60));
            fold.StrokeWidth = 1f;
            fold.UseShapeColors = false;
            fold.Fill = false;
            fold.Stroke = true;
            t.RenderCommands.Add(fold);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterComponentType()
        {
            ShapeType t = new ShapeType();
            t.Name = "组件";
            t.Category = "云原生";
            t.Description = "微服务组件";
            t.DefaultWidth = 160;
            t.DefaultHeight = 110;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(240, 255, 240));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 160, 80));
            t.SupportsMembers = true;

            RenderCommand main = new RenderCommand();
            main.CommandType = RenderCommandType.RoundedRect;
            main.X = 0.10f; main.Y = 0.15f;
            main.Width = 0.80f; main.Height = 0.70f;
            main.CornerRadius = 4f;
            main.UseShapeColors = true;
            main.Fill = true;
            main.Stroke = true;
            main.StrokeWidth = 1.5f;
            t.RenderCommands.Add(main);

            RenderCommand plug1 = new RenderCommand();
            plug1.CommandType = RenderCommandType.RoundedRect;
            plug1.X = 0f; plug1.Y = 0.30f;
            plug1.Width = 0.15f; plug1.Height = 0.12f;
            plug1.CornerRadius = 2f;
            plug1.UseShapeColors = true;
            plug1.Fill = true;
            plug1.Stroke = true;
            plug1.StrokeWidth = 1f;
            t.RenderCommands.Add(plug1);

            RenderCommand plug2 = new RenderCommand();
            plug2.CommandType = RenderCommandType.RoundedRect;
            plug2.X = 0.85f; plug2.Y = 0.55f;
            plug2.Width = 0.15f; plug2.Height = 0.12f;
            plug2.CornerRadius = 2f;
            plug2.UseShapeColors = true;
            plug2.Fill = true;
            plug2.Stroke = true;
            plug2.StrokeWidth = 1f;
            t.RenderCommands.Add(plug2);

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterContainerType()
        {
            ShapeType t = new ShapeType();
            t.Name = "容器";
            t.Category = "容器";
            t.Description = "可嵌套容器";
            t.DefaultWidth = 300;
            t.DefaultHeight = 220;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(250, 250, 250));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(100, 100, 120));
            t.IsContainer = true;

            ShapeTypeRegistry.Instance.Register(t);
        }

        private void RegisterClassType()
        {
            ShapeType t = new ShapeType();
            t.Name = "类";
            t.Category = "UML";
            t.Description = "类图";
            t.DefaultWidth = 180;
            t.DefaultHeight = 160;
            t.DefaultFillColor = new XmlColor(Color.FromArgb(250, 248, 240));
            t.DefaultBorderColor = new XmlColor(Color.FromArgb(100, 100, 100));
            t.SupportsMembers = true;

            RenderCommand bg = new RenderCommand();
            bg.CommandType = RenderCommandType.RoundedRect;
            bg.X = 0f; bg.Y = 0f;
            bg.Width = 1f; bg.Height = 1f;
            bg.CornerRadius = 4f;
            bg.UseShapeColors = true;
            bg.Fill = true;
            bg.Stroke = true;
            bg.StrokeWidth = 1.5f;
            t.RenderCommands.Add(bg);

            RenderCommand title = new RenderCommand();
            title.CommandType = RenderCommandType.Text;
            title.X = 0f; title.Y = 0f;
            title.Width = 1f; title.Height = 0.22f;
            title.Text = "";
            title.TextAlign = "center";
            title.FontSize = 11f;
            title.IsBold = true;
            title.UseShapeColors = true;
            t.RenderCommands.Add(title);

            RenderCommand sep = new RenderCommand();
            sep.CommandType = RenderCommandType.Line;
            sep.X = 0f; sep.Y = 0.22f;
            sep.Width = 1f; sep.Height = 0f;
            sep.StrokeColor = new XmlColor(Color.FromArgb(180, 180, 180));
            sep.StrokeWidth = 1f;
            sep.UseShapeColors = false;
            t.RenderCommands.Add(sep);

            ShapeTypeRegistry.Instance.Register(t);
        }

        #endregion

        #region Menu Event Handlers

        private void OnFileNew(object sender, EventArgs e)
        {
            _canvas.Document.Clear();
            _currentFilePath = "";
            _propertyGrid.SelectedObject = GlobalConfig.Instance;
            _statusLabel.Text = "新建文档";
            _canvas.Invalidate();
        }

        private void OnFileOpen(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*";
            dlg.DefaultExt = "xml";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DrawingDocument doc = XmlShapeSerializer.Load(dlg.FileName);
                    _canvas.Document.Clear();

                    foreach (ShapeBase shape in doc.Shapes)
                        _canvas.Document.AddShape(shape);
                    foreach (Connection conn in doc.Connections)
                        _canvas.Document.AddConnection(conn);

                    _currentFilePath = dlg.FileName;
                    _statusLabel.Text = "已打开: " + dlg.FileName;
                    _canvas.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnFileSave(object sender, EventArgs e)
        {
            if (_currentFilePath.Length > 0)
            {
                try
                {
                    XmlShapeSerializer.Save(_currentFilePath, _canvas.Document);
                    _statusLabel.Text = "已保存: " + _currentFilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                OnFileSaveAs(sender, e);
            }
        }

        private void OnFileSaveAs(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML 文件 (*.xml)|*.xml|所有文件 (*.*)|*.*";
            dlg.DefaultExt = "xml";
            if (_currentFilePath.Length > 0)
                dlg.FileName = _currentFilePath;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlShapeSerializer.Save(dlg.FileName, _canvas.Document);
                    _currentFilePath = dlg.FileName;
                    _statusLabel.Text = "已保存: " + dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败: " + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnFileExit(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnEditDelete(object sender, EventArgs e)
        {
            _canvas.DeleteSelected();
        }

        private void OnEditSelectAll(object sender, EventArgs e)
        {
            foreach (ShapeBase s in _canvas.Document.Shapes)
                s.Selected = true;
            _canvas.Invalidate();
        }

        private void OnViewGrid(object sender, EventArgs e)
        {
            GlobalConfig.Instance.ShowGrid = !GlobalConfig.Instance.ShowGrid;
            _menuViewGrid.Checked = GlobalConfig.Instance.ShowGrid;
        }

        private void OnViewSnap(object sender, EventArgs e)
        {
            GlobalConfig.Instance.SnapToGrid = !GlobalConfig.Instance.SnapToGrid;
            _menuViewSnap.Checked = GlobalConfig.Instance.SnapToGrid;
        }

        private void OnViewZoomIn(object sender, EventArgs e)
        {
            _canvas.Zoom *= 1.2f;
            UpdateZoomLabel();
        }

        private void OnViewZoomOut(object sender, EventArgs e)
        {
            _canvas.Zoom /= 1.2f;
            UpdateZoomLabel();
        }

        private void OnViewReset(object sender, EventArgs e)
        {
            _canvas.Zoom = 1.0f;
            _canvas.Offset = new PointF(0, 0);
            UpdateZoomLabel();
        }

        private void OnViewToolbar(object sender, EventArgs e)
        {
            _toolStrip.Visible = !_toolStrip.Visible;
            _menuViewToolbar.Checked = _toolStrip.Visible;
        }

        private void OnViewProperty(object sender, EventArgs e)
        {
            if (_propertyGrid.Visible)
            {
                _propertyGrid.Visible = false;
                _rightSplit.Panel2Collapsed = true;
            }
            else
            {
                _rightSplit.Panel2Collapsed = false;
                _propertyGrid.Visible = true;
            }
            _menuViewProperty.Checked = _propertyGrid.Visible;
        }

        private void OnViewToolbox(object sender, EventArgs e)
        {
            if (_toolbox.Visible)
            {
                _toolbox.Visible = false;
                _mainSplit.Panel1Collapsed = true;
            }
            else
            {
                _mainSplit.Panel1Collapsed = false;
                _toolbox.Visible = true;
            }
            _menuViewToolbox.Checked = _toolbox.Visible;
        }

        private void OnViewContextMenu(object sender, EventArgs e)
        {
            _contextMenuEnabled = !_contextMenuEnabled;
            _menuViewContextMenu.Checked = _contextMenuEnabled;
        }

        private void OnToolSelect(object sender, EventArgs e)
        {
            _canvas.CurrentTool = CanvasTool.Select;
            UpdateToolState();
        }

        private void OnToolConnect(object sender, EventArgs e)
        {
            _canvas.CurrentTool = CanvasTool.Connect;
            UpdateToolState();
        }

        private void OnToolStraight(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Straight);
        }

        private void OnToolCurve(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Curve);
        }

        private void OnToolOrtho(object sender, EventArgs e)
        {
            SetConnectionMode(ConnectionMode.Orthogonal);
        }

        #endregion

        #region Toolbar Event Handlers

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

        #endregion

        #region Helper Methods

        private void UpdateToolState()
        {
            if (_btnSelect != null)
            {
                _btnSelect.Checked = (_canvas.CurrentTool == CanvasTool.Select);
            }
            if (_btnConnect != null)
            {
                _btnConnect.Checked = (_canvas.CurrentTool == CanvasTool.Connect);
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

            if (_btnStraight != null)
                _btnStraight.Checked = (mode == ConnectionMode.Straight);
            if (_btnCurve != null)
                _btnCurve.Checked = (mode == ConnectionMode.Curve);
            if (_btnOrtho != null)
                _btnOrtho.Checked = (mode == ConnectionMode.Orthogonal);

            _canvas.Invalidate();
        }

        private void UpdateZoomLabel()
        {
            if (_zoomLabel != null)
            {
                _zoomLabel.Text = string.Format("缩放: {0:0}%", _canvas.Zoom * 100);
            }
        }

        #endregion

        #region Canvas Event Handlers

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

            _statusLabel.Text = string.Format("选中: {0} 个实体, {1} 条连线",
                shapes.Count, conns.Count);
        }

        private void OnDocumentModified(object sender, EventArgs e)
        {
            _statusLabel.Text = "文档已修改";
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            PointF world = _canvas.ScreenToWorld(e.Location);
            _zoomLabel.Text = string.Format("缩放: {0:0}% | 坐标: ({1:0}, {2:0})",
                _canvas.Zoom * 100, world.X, world.Y);
        }

        private void OnCanvasMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _contextMenuEnabled)
            {
                ShowContextMenu(e.Location);
            }
        }

        #endregion

        #region Context Menu

        private void ShowContextMenu(Point location)
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            List<ShapeBase> selectedShapes = _canvas.Document.GetSelectedShapes();
            bool hasSingleGenericShape = false;
            bool supportsMembers = false;
            GenericShape gs = null;

            if (selectedShapes.Count == 1)
            {
                gs = selectedShapes[0] as GenericShape;
                if (gs != null)
                {
                    hasSingleGenericShape = true;
                    ShapeType st = ShapeTypeRegistry.Instance.GetShapeType(gs.ShapeTypeName);
                    if (st != null && st.SupportsMembers)
                    {
                        supportsMembers = true;
                    }
                }
            }

            if (hasSingleGenericShape && supportsMembers)
            {
                ToolStripMenuItem itemAddMember = new ToolStripMenuItem("添加成员",
                    null, new EventHandler(OnCtxAddMember));
                ToolStripMenuItem itemSwitchState = new ToolStripMenuItem("切换状态",
                    null, new EventHandler(OnCtxSwitchState));
                menu.Items.Add(itemAddMember);
                menu.Items.Add(itemSwitchState);
                menu.Items.Add(new ToolStripSeparator());
            }

            ToolStripMenuItem itemDelete = new ToolStripMenuItem("删除",
                null, new EventHandler(OnCtxDelete));
            itemDelete.ShortcutKeyDisplayString = "Delete";
            menu.Items.Add(itemDelete);

            if (selectedShapes.Count > 1)
            {
                ToolStripMenuItem itemFront = new ToolStripMenuItem("置顶",
                    null, new EventHandler(OnCtxFront));
                ToolStripMenuItem itemBack = new ToolStripMenuItem("置底",
                    null, new EventHandler(OnCtxBack));
                menu.Items.Add(itemFront);
                menu.Items.Add(itemBack);
            }

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem itemProps = new ToolStripMenuItem("属性...",
                null, new EventHandler(OnCtxProps));
            menu.Items.Add(itemProps);

            menu.Show(_canvas, location);
        }

        private void OnCtxDelete(object sender, EventArgs e)
        {
            _canvas.DeleteSelected();
        }

        private void OnCtxFront(object sender, EventArgs e)
        {
            _canvas.BringToFront();
        }

        private void OnCtxBack(object sender, EventArgs e)
        {
            _canvas.SendToBack();
        }

        private void OnCtxProps(object sender, EventArgs e)
        {
            _propertyGrid.Focus();
        }

        private void OnCtxAddMember(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape genericShape = shapes[0] as GenericShape;
                if (genericShape != null)
                {
                    ShapeMember m = new ShapeMember();
                    m.Name = "NewMember";
                    m.TypeName = "string";
                    genericShape.Members.Add(m);
                    _canvas.Invalidate();
                }
            }
        }

        private void OnCtxSwitchState(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape genericShape = shapes[0] as GenericShape;
                if (genericShape != null && genericShape.States.Count > 1)
                {
                    int idx = 0;
                    for (int i = 0; i < genericShape.States.Count; i++)
                    {
                        if (genericShape.States[i].Name == genericShape.CurrentStateName)
                        {
                            idx = i;
                            break;
                        }
                    }
                    idx = (idx + 1) % genericShape.States.Count;
                    genericShape.CurrentStateName = genericShape.States[idx].Name;
                    _canvas.Invalidate();
                }
            }
        }

        #endregion

        #region Sample Data

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

            GenericShape rect1 = new GenericShape();
            rect1.ShapeTypeName = "矩形";
            rect1.Name = "API网关";
            rect1.X = 140;
            rect1.Y = 140;
            rect1.FillColor = Color.FromArgb(230, 245, 255);
            rect1.BorderColor = Color.FromArgb(60, 130, 200);

            GenericShape rect2 = new GenericShape();
            rect2.ShapeTypeName = "矩形";
            rect2.Name = "用户服务";
            rect2.X = 340;
            rect2.Y = 140;
            rect2.FillColor = Color.FromArgb(230, 255, 240);
            rect2.BorderColor = Color.FromArgb(60, 180, 100);

            GenericShape ellipse1 = new GenericShape();
            ellipse1.ShapeTypeName = "椭圆";
            ellipse1.Name = "数据库";
            ellipse1.X = 340;
            ellipse1.Y = 260;
            ellipse1.FillColor = Color.FromArgb(255, 240, 230);
            ellipse1.BorderColor = Color.FromArgb(200, 130, 60);

            GenericShape diamond1 = new GenericShape();
            diamond1.ShapeTypeName = "菱形";
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

            GenericShape classShape = new GenericShape();
            classShape.ShapeTypeName = "类";
            classShape.Name = "UserService";
            classShape.X = 600;
            classShape.Y = 120;
            classShape.Width = 180;
            classShape.Height = 160;

            ShapeMember m1 = new ShapeMember();
            m1.MemberType = MemberType.Property;
            m1.Name = "Id";
            m1.TypeName = "Guid";
            m1.Visibility = Visibility.Public;
            classShape.Members.Add(m1);

            ShapeMember m2 = new ShapeMember();
            m2.MemberType = MemberType.Property;
            m2.Name = "Name";
            m2.TypeName = "string";
            m2.Visibility = Visibility.Public;
            classShape.Members.Add(m2);

            ShapeMember m3 = new ShapeMember();
            m3.MemberType = MemberType.Method;
            m3.Name = "GetUser";
            m3.TypeName = "User";
            m3.Visibility = Visibility.Public;
            ShapeMemberParameter p1 = new ShapeMemberParameter();
            p1.Name = "id";
            p1.TypeName = "Guid";
            m3.Parameters.Add(p1);
            classShape.Members.Add(m3);

            ShapeMember m4 = new ShapeMember();
            m4.MemberType = MemberType.Method;
            m4.Name = "Save";
            m4.TypeName = "bool";
            m4.Visibility = Visibility.Public;
            classShape.Members.Add(m4);

            ShapeState normalState = new ShapeState();
            normalState.Name = "Normal";
            normalState.FillColor = new XmlColor(Color.FromArgb(250, 248, 240));
            normalState.BorderColor = new XmlColor(Color.FromArgb(100, 100, 100));
            normalState.TextColor = new XmlColor(Color.FromArgb(40, 40, 40));
            classShape.AddState(normalState);

            ShapeState warningState = new ShapeState();
            warningState.Name = "Warning";
            warningState.FillColor = new XmlColor(Color.FromArgb(255, 248, 220));
            warningState.BorderColor = new XmlColor(Color.FromArgb(255, 140, 0));
            warningState.TextColor = new XmlColor(Color.FromArgb(180, 90, 0));
            classShape.AddState(warningState);

            ShapeState errorState = new ShapeState();
            errorState.Name = "Error";
            errorState.FillColor = new XmlColor(Color.FromArgb(255, 235, 235));
            errorState.BorderColor = new XmlColor(Color.FromArgb(220, 50, 50));
            errorState.TextColor = new XmlColor(Color.FromArgb(180, 30, 30));
            classShape.AddState(errorState);

            _canvas.Document.AddShape(classShape);

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

            Connection conn4 = new Connection();
            conn4.FromShape = rect2;
            conn4.ToShape = classShape;
            conn4.Mode = ConnectionMode.Straight;
            conn4.Label = "gRPC";

            _canvas.Document.AddConnection(conn1);
            _canvas.Document.AddConnection(conn2);
            _canvas.Document.AddConnection(conn3);
            _canvas.Document.AddConnection(conn4);

            _propertyGrid.SelectedObject = GlobalConfig.Instance;
            _canvas.Invalidate();
        }

        #endregion
    }
}

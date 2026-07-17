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
        private string _currentFilePath = "";

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

            InitializeShapeTypes();
            _toolbox.ReloadFromRegistry();
            InitializeSampleData();
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            _canvas.Invalidate();
        }

        private void InitializeShapeTypes()
        {
            ShapeTypeRegistry.Instance.Clear();

            ShapeType rectType = new ShapeType();
            rectType.Name = "矩形";
            rectType.Category = "基本图形";
            rectType.Description = "圆角矩形";
            rectType.DefaultWidth = 140;
            rectType.DefaultHeight = 90;
            rectType.DefaultFillColor = new XmlColor(Color.FromArgb(230, 245, 255));
            rectType.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 130, 200));
            rectType.SupportsMembers = true;

            RenderCommand rectBg = new RenderCommand();
            rectBg.CommandType = RenderCommandType.RoundedRect;
            rectBg.X = 0f; rectBg.Y = 0f;
            rectBg.Width = 1f; rectBg.Height = 1f;
            rectBg.CornerRadius = 6f;
            rectBg.UseShapeColors = true;
            rectBg.Fill = true;
            rectBg.Stroke = true;
            rectBg.StrokeWidth = 1.5f;
            rectType.RenderCommands.Add(rectBg);

            RenderCommand rectText = new RenderCommand();
            rectText.CommandType = RenderCommandType.Text;
            rectText.X = 0f; rectText.Y = 0f;
            rectText.Width = 1f; rectText.Height = 0.35f;
            rectText.Text = "";
            rectText.TextAlign = "center";
            rectText.FontSize = 10f;
            rectText.IsBold = false;
            rectText.UseShapeColors = true;
            rectType.RenderCommands.Add(rectText);

            ShapeTypeRegistry.Instance.Register(rectType);

            ShapeType ellipseType = new ShapeType();
            ellipseType.Name = "椭圆";
            ellipseType.Category = "基本图形";
            ellipseType.Description = "椭圆形";
            ellipseType.DefaultWidth = 120;
            ellipseType.DefaultHeight = 100;
            ellipseType.DefaultFillColor = new XmlColor(Color.FromArgb(255, 240, 230));
            ellipseType.DefaultBorderColor = new XmlColor(Color.FromArgb(200, 130, 60));
            ellipseType.SupportsMembers = true;

            RenderCommand ellipseBg = new RenderCommand();
            ellipseBg.CommandType = RenderCommandType.Ellipse;
            ellipseBg.X = 0f; ellipseBg.Y = 0f;
            ellipseBg.Width = 1f; ellipseBg.Height = 1f;
            ellipseBg.UseShapeColors = true;
            ellipseBg.Fill = true;
            ellipseBg.Stroke = true;
            ellipseBg.StrokeWidth = 1.5f;
            ellipseType.RenderCommands.Add(ellipseBg);

            RenderCommand ellipseText = new RenderCommand();
            ellipseText.CommandType = RenderCommandType.Text;
            ellipseText.X = 0f; ellipseText.Y = 0f;
            ellipseText.Width = 1f; ellipseText.Height = 0.35f;
            ellipseText.Text = "";
            ellipseText.TextAlign = "center";
            ellipseText.FontSize = 10f;
            ellipseText.UseShapeColors = true;
            ellipseType.RenderCommands.Add(ellipseText);

            ShapeTypeRegistry.Instance.Register(ellipseType);

            ShapeType diamondType = new ShapeType();
            diamondType.Name = "菱形";
            diamondType.Category = "基本图形";
            diamondType.Description = "菱形";
            diamondType.DefaultWidth = 120;
            diamondType.DefaultHeight = 100;
            diamondType.DefaultFillColor = new XmlColor(Color.FromArgb(230, 255, 240));
            diamondType.DefaultBorderColor = new XmlColor(Color.FromArgb(60, 180, 100));
            diamondType.SupportsMembers = true;

            RenderCommand diamondBg = new RenderCommand();
            diamondBg.CommandType = RenderCommandType.Polygon;
            diamondBg.X = 0f; diamondBg.Y = 0f;
            diamondBg.Width = 1f; diamondBg.Height = 1f;
            diamondBg.PolygonPoints = new PointF[] {
                new PointF(0.5f, 0f),
                new PointF(1f, 0.5f),
                new PointF(0.5f, 1f),
                new PointF(0f, 0.5f)
            };
            diamondBg.UseShapeColors = true;
            diamondBg.Fill = true;
            diamondBg.Stroke = true;
            diamondBg.StrokeWidth = 1.5f;
            diamondType.RenderCommands.Add(diamondBg);

            RenderCommand diamondText = new RenderCommand();
            diamondText.CommandType = RenderCommandType.Text;
            diamondText.X = 0f; diamondText.Y = 0f;
            diamondText.Width = 1f; diamondText.Height = 0.35f;
            diamondText.Text = "";
            diamondText.TextAlign = "center";
            diamondText.FontSize = 10f;
            diamondText.UseShapeColors = true;
            diamondType.RenderCommands.Add(diamondText);

            ShapeTypeRegistry.Instance.Register(diamondType);

            ShapeType containerType = new ShapeType();
            containerType.Name = "容器";
            containerType.Category = "容器";
            containerType.Description = "可嵌套容器";
            containerType.DefaultWidth = 300;
            containerType.DefaultHeight = 220;
            containerType.DefaultFillColor = new XmlColor(Color.FromArgb(250, 250, 250));
            containerType.DefaultBorderColor = new XmlColor(Color.FromArgb(100, 100, 120));
            containerType.IsContainer = true;

            ShapeTypeRegistry.Instance.Register(containerType);

            ShapeType classType = new ShapeType();
            classType.Name = "类";
            classType.Category = "UML";
            classType.Description = "类图";
            classType.DefaultWidth = 180;
            classType.DefaultHeight = 160;
            classType.DefaultFillColor = new XmlColor(Color.FromArgb(250, 248, 240));
            classType.DefaultBorderColor = new XmlColor(Color.FromArgb(100, 100, 100));
            classType.SupportsMembers = true;

            RenderCommand classBg = new RenderCommand();
            classBg.CommandType = RenderCommandType.RoundedRect;
            classBg.X = 0f; classBg.Y = 0f;
            classBg.Width = 1f; classBg.Height = 1f;
            classBg.CornerRadius = 4f;
            classBg.UseShapeColors = true;
            classBg.Fill = true;
            classBg.Stroke = true;
            classBg.StrokeWidth = 1.5f;
            classType.RenderCommands.Add(classBg);

            RenderCommand classTitle = new RenderCommand();
            classTitle.CommandType = RenderCommandType.Text;
            classTitle.X = 0f; classTitle.Y = 0f;
            classTitle.Width = 1f; classTitle.Height = 0.22f;
            classTitle.Text = "";
            classTitle.TextAlign = "center";
            classTitle.FontSize = 11f;
            classTitle.IsBold = true;
            classTitle.UseShapeColors = true;
            classType.RenderCommands.Add(classTitle);

            RenderCommand classSep = new RenderCommand();
            classSep.CommandType = RenderCommandType.Line;
            classSep.X = 0f; classSep.Y = 0.22f;
            classSep.Width = 1f; classSep.Height = 0f;
            classSep.StrokeColor = new XmlColor(Color.FromArgb(180, 180, 180));
            classSep.StrokeWidth = 1f;
            classSep.UseShapeColors = false;
            classType.RenderCommands.Add(classSep);

            ShapeTypeRegistry.Instance.Register(classType);
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

            ToolStripButton btnSave = new ToolStripButton("保存", null, new EventHandler(OnSave));
            btnSave.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripButton btnLoad = new ToolStripButton("打开", null, new EventHandler(OnLoad));
            btnLoad.DisplayStyle = ToolStripItemDisplayStyle.Text;

            ToolStripSeparator sep5 = new ToolStripSeparator();

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
            _toolStrip.Items.Add(btnSave);
            _toolStrip.Items.Add(btnLoad);
            _toolStrip.Items.Add(sep5);
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

        private void OnSave(object sender, EventArgs e)
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
                    MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
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
                    MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

            ToolStripMenuItem itemAddMember = new ToolStripMenuItem("添加成员", null, new EventHandler(OnCtxAddMember));
            ToolStripMenuItem itemState = new ToolStripMenuItem("切换状态", null, new EventHandler(OnCtxSwitchState));

            ToolStripSeparator itemSep = new ToolStripSeparator();
            ToolStripMenuItem itemProps = new ToolStripMenuItem("属性...", null, new EventHandler(OnCtxProps));

            menu.Items.Add(itemAddMember);
            menu.Items.Add(itemState);
            menu.Items.Add(new ToolStripSeparator());
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

        private void OnCtxAddMember(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape gs = shapes[0] as GenericShape;
                if (gs != null)
                {
                    ShapeMember m = new ShapeMember();
                    m.Name = "NewMember";
                    m.TypeName = "string";
                    gs.Members.Add(m);
                    _canvas.Invalidate();
                }
            }
        }

        private void OnCtxSwitchState(object sender, EventArgs e)
        {
            List<ShapeBase> shapes = _canvas.Document.GetSelectedShapes();
            if (shapes.Count == 1)
            {
                GenericShape gs = shapes[0] as GenericShape;
                if (gs != null && gs.States.Count > 1)
                {
                    int idx = 0;
                    for (int i = 0; i < gs.States.Count; i++)
                    {
                        if (gs.States[i].Name == gs.CurrentStateName)
                        {
                            idx = i;
                            break;
                        }
                    }
                    idx = (idx + 1) % gs.States.Count;
                    gs.CurrentStateName = gs.States[idx].Name;
                    _canvas.Invalidate();
                }
            }
        }

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
    }
}

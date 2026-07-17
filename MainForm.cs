using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
            // 主分割器
            _mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 200,
                FixedPanel = FixedPanel.Panel1,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            _rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 900,
                FixedPanel = FixedPanel.Panel2
            };

            // 工具箱
            _toolbox = new ToolboxPanel
            {
                Dock = DockStyle.Fill
            };

            // 画布
            _canvas = new DrawingCanvas
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // 属性面板
            _propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                ToolbarVisible = true,
                HelpVisible = true,
                PropertySort = PropertySort.CategorizedAlphabetical
            };

            // 工具栏
            _toolStrip = new ToolStrip();
            InitializeToolStrip();

            // 状态栏
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("就绪");
            _zoomLabel = new ToolStripStatusLabel("缩放: 100%");
            _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, new ToolStripStatusLabel(""), _zoomLabel });

            // 组装布局
            _mainSplit.Panel1.Controls.Add(_toolbox);
            _rightSplit.Panel1.Controls.Add(_canvas);
            _rightSplit.Panel2.Controls.Add(_propertyGrid);
            _mainSplit.Panel2.Controls.Add(_rightSplit);

            _toolStrip.Dock = DockStyle.Top;
            _statusStrip.Dock = DockStyle.Bottom;
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_toolStrip);
            this.Controls.Add(_mainSplit);

            // 事件绑定
            _canvas.SelectionChanged += OnCanvasSelectionChanged;
            _canvas.DocumentModified += OnDocumentModified;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseClick += OnCanvasMouseClick;
            _propertyGrid.PropertyValueChanged += (s, e) => _canvas.Invalidate();

            InitializeSampleData();
        }

        private void InitializeToolStrip()
        {
            var btnSelect = new ToolStripButton("选择", null, (s, e) => { _canvas.CurrentTool = CanvasTool.Select; UpdateToolState(); })
            {
                CheckOnClick = true,
                Checked = true,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            var btnConnect = new ToolStripButton("连线", null, (s, e) => { _canvas.CurrentTool = CanvasTool.Connect; UpdateToolState(); })
            {
                CheckOnClick = true,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            var btnSep1 = new ToolStripSeparator();

            var btnStraight = new ToolStripButton("直线", null, (s, e) => SetConnectionMode(ConnectionMode.Straight))
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnCurve = new ToolStripButton("曲线", null, (s, e) => SetConnectionMode(ConnectionMode.Curve))
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnOrtho = new ToolStripButton("折线", null, (s, e) => SetConnectionMode(ConnectionMode.Orthogonal))
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            var btnSep2 = new ToolStripSeparator();

            var btnZoomIn = new ToolStripButton("放大", null, (s, e) => { _canvas.Zoom *= 1.2f; UpdateZoomLabel(); })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnZoomOut = new ToolStripButton("缩小", null, (s, e) => { _canvas.Zoom /= 1.2f; UpdateZoomLabel(); })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnFit = new ToolStripButton("适应", null, (s, e) => { _canvas.Zoom = 1.0f; _canvas.Offset = new PointF(0, 0); UpdateZoomLabel(); })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            var btnSep3 = new ToolStripSeparator();

            var btnFront = new ToolStripButton("置顶", null, (s, e) => _canvas.BringToFront())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnBack = new ToolStripButton("置底", null, (s, e) => _canvas.SendToBack())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var btnDelete = new ToolStripButton("删除", null, (s, e) => _canvas.DeleteSelected())
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            var btnSep4 = new ToolStripSeparator();

            var btnGrid = new ToolStripButton("网格", null, (s, e) =>
            {
                GlobalConfig.Instance.ShowGrid = !GlobalConfig.Instance.ShowGrid;
                ((ToolStripButton)s).Checked = GlobalConfig.Instance.ShowGrid;
            })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Checked = GlobalConfig.Instance.ShowGrid,
                CheckOnClick = true
            };

            var btnSnap = new ToolStripButton("对齐", null, (s, e) =>
            {
                GlobalConfig.Instance.SnapToGrid = !GlobalConfig.Instance.SnapToGrid;
                ((ToolStripButton)s).Checked = GlobalConfig.Instance.SnapToGrid;
            })
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Checked = GlobalConfig.Instance.SnapToGrid,
                CheckOnClick = true
            };

            _toolStrip.Items.AddRange(new ToolStripItem[]
            {
                btnSelect, btnConnect, btnSep1,
                btnStraight, btnCurve, btnOrtho, btnSep2,
                btnZoomIn, btnZoomOut, btnFit, btnSep3,
                btnFront, btnBack, btnDelete, btnSep4,
                btnGrid, btnSnap
            });
        }

        private void UpdateToolState()
        {
            foreach (ToolStripItem item in _toolStrip.Items)
            {
                if (item is ToolStripButton btn && (btn.Text == "选择" || btn.Text == "连线"))
                {
                    btn.Checked = (btn.Text == "选择" && _canvas.CurrentTool == CanvasTool.Select) ||
                                  (btn.Text == "连线" && _canvas.CurrentTool == CanvasTool.Connect);
                }
            }
        }

        private void SetConnectionMode(ConnectionMode mode)
        {
            GlobalConfig.Instance.DefaultConnectionMode = mode;
            foreach (var conn in _canvas.Document.GetSelectedConnections())
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
            var shapes = _canvas.Document.GetSelectedShapes();
            var conns = _canvas.Document.GetSelectedConnections();

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
                _propertyGrid.SelectedObjects = shapes.ToArray();
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
            var world = _canvas.ScreenToWorld(e.Location);
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
            var menu = new ContextMenuStrip();

            var itemDelete = new ToolStripMenuItem("删除", null, (s, e) => _canvas.DeleteSelected())
            {
                ShortcutKeyDisplayString = "Delete"
            };
            var itemFront = new ToolStripMenuItem("置于顶层", null, (s, e) => _canvas.BringToFront());
            var itemBack = new ToolStripMenuItem("置于底层", null, (s, e) => _canvas.SendToBack());
            var itemSep = new ToolStripSeparator();
            var itemProps = new ToolStripMenuItem("属性...", null, (s, e) =>
            {
                _propertyGrid.Focus();
            });

            menu.Items.AddRange(new ToolStripItem[] { itemDelete, itemFront, itemBack, itemSep, itemProps });
            menu.Show(_canvas, location);
        }

        private void InitializeSampleData()
        {
            var container = new CloudNativeDesigner.Shapes.ContainerShape
            {
                Name = "部署环境",
                HeaderText = "生产环境集群",
                X = 100,
                Y = 80,
                Width = 500,
                Height = 350,
                FillColor = Color.FromArgb(250, 250, 252),
                BorderColor = Color.FromArgb(100, 100, 130)
            };

            var rect1 = new CloudNativeDesigner.Shapes.RectangleShape
            {
                Name = "API网关",
                X = 140,
                Y = 140,
                FillColor = Color.FromArgb(230, 245, 255),
                BorderColor = Color.FromArgb(60, 130, 200)
            };

            var rect2 = new CloudNativeDesigner.Shapes.RectangleShape
            {
                Name = "用户服务",
                X = 340,
                Y = 140,
                FillColor = Color.FromArgb(230, 255, 240),
                BorderColor = Color.FromArgb(60, 180, 100)
            };

            var ellipse1 = new CloudNativeDesigner.Shapes.EllipseShape
            {
                Name = "数据库",
                X = 340,
                Y = 260,
                FillColor = Color.FromArgb(255, 240, 230),
                BorderColor = Color.FromArgb(200, 130, 60)
            };

            var diamond1 = new CloudNativeDesigner.Shapes.DiamondShape
            {
                Name = "负载均衡",
                X = 160,
                Y = 260,
                FillColor = Color.FromArgb(255, 240, 255),
                BorderColor = Color.FromArgb(180, 60, 180)
            };

            _canvas.Document.AddShape(container);
            _canvas.Document.AddShape(rect1);
            _canvas.Document.AddShape(rect2);
            _canvas.Document.AddShape(ellipse1);
            _canvas.Document.AddShape(diamond1);

            container.AddChild(rect1);
            container.AddChild(rect2);
            container.AddChild(ellipse1);
            container.AddChild(diamond1);

            var conn1 = new Connection
            {
                FromShape = rect1,
                ToShape = rect2,
                Mode = ConnectionMode.Straight,
                Label = "REST"
            };
            var conn2 = new Connection
            {
                FromShape = rect2,
                ToShape = ellipse1,
                Mode = ConnectionMode.Orthogonal,
                Label = "SQL"
            };
            var conn3 = new Connection
            {
                FromShape = diamond1,
                ToShape = rect1,
                Mode = ConnectionMode.Curve
            };

            _canvas.Document.AddConnection(conn1);
            _canvas.Document.AddConnection(conn2);
            _canvas.Document.AddConnection(conn3);

            _propertyGrid.SelectedObject = GlobalConfig.Instance;
            _canvas.Invalidate();
        }
    }
}

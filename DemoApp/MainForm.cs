using System;
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
        private DiagramEditor _editor;
        private MenuStrip _menuStrip;

        public MainForm()
        {
            // 创建宿主菜单栏
            _menuStrip = new MenuStrip();
            this.MainMenuStrip = _menuStrip;
            this.Controls.Add(_menuStrip);

            // 创建编辑器控件
            _editor = new DiagramEditor();
            _editor.Dock = DockStyle.Fill;
            this.Controls.Add(_editor);

            // 将控件功能菜单注入宿主菜单栏
            _editor.ConfigureMenu(_menuStrip);
            _editor.ConfigureHostForm(this);

            this.Text = "云原生可视化设计器 - 演示应用";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application;

            InitializeShapeTypes();
            _editor.Canvas.Document.DocumentChanged += new EventHandler(OnDocumentChanged);
            InitializeSampleData();
        }

        private void OnDocumentChanged(object sender, EventArgs e)
        {
            // 可选：响应文档变更，例如更新标题栏
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

            _editor.Toolbox.ReloadFromRegistry();
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

            _editor.Document.AddShape(container);
            _editor.Document.AddShape(rect1);
            _editor.Document.AddShape(rect2);
            _editor.Document.AddShape(ellipse1);
            _editor.Document.AddShape(diamond1);

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

            _editor.Document.AddShape(classShape);

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

            _editor.Document.AddConnection(conn1);
            _editor.Document.AddConnection(conn2);
            _editor.Document.AddConnection(conn3);
            _editor.Document.AddConnection(conn4);

            _editor.Canvas.Invalidate();
        }

        #endregion
    }
}

# CloudNativeDesigner 架构规格说明书

## 1. 系统概述

### 1.1 项目名称

CloudNativeDesigner（云原生可视化设计器）

### 1.2 项目目标

CloudNativeDesigner 是一个可复用的 WinForms 可视化设计器控件库，兼容 .NET Framework 2.0（Visual Studio 2005+）。它提供了完整的图形绘制、连线、容器嵌套、序列化等功能，可被宿主应用程序直接拖放使用或编程集成，适用于构建各类可视化图表编辑器（如云原生架构图、流程图、UML 类图等）。

### 1.3 解决的问题

- **可视化设计器的重复开发**：提供开箱即用的画布、工具箱、属性栏、工具栏、状态栏、菜单等完整 UI 组件，避免每个项目从零搭建。
- **图形类型的灵活扩展**：通过 RenderCommand 数据驱动 + ShapeTypeRegistry 运行时注册机制，支持不修改控件库代码即可新增图形类型。
- **控件与宿主的解耦**：控件库不包含任何图形类型定义，图形注册由宿主完成；菜单通过注入机制集成，不强制宿主创建特定结构的菜单。
- **低版本 .NET 兼容**：严格使用 .NET 2.0 API（不使用 var、lambda、LINQ 等高版本语法），可在老旧环境中部署。

---

## 2. 解决方案结构

### 2.1 解决方案组成

| 项目 | 类型 | 输出 | 说明 |
|------|------|------|------|
| CloudNativeDesigner | Library | CloudNativeDesigner.dll | 控件库，包含所有核心、控件、序列化代码 |
| DemoApp | WinExe | DemoApp.exe | 演示应用，展示宿主集成与图形类型注册 |

### 2.2 解决方案文件

| 文件 | 说明 |
|------|------|
| `CloudNativeDesigner.sln` | Visual Studio 2005 格式解决方案文件 |
| `CloudNativeDesigner.csproj` | 控件库项目文件（OutputType=Library, TargetFrameworkVersion=v2.0） |
| `DemoApp/DemoApp.csproj` | 演示应用项目文件（OutputType=WinExe, TargetFrameworkVersion=v2.0） |

### 2.3 源文件清单

#### Core 层（`Core/`）

| 文件 | 职责 |
|------|------|
| `ShapeBase.cs` | 所有图形的抽象基类，定义属性、HitTest、Clone、选中绘制、ResizeHandle |
| `Connection.cs` | 连线模型，支持 Straight/Curve/Orthogonal 三种模式，含标签、箭头绘制 |
| `DrawingDocument.cs` | 文档模型，管理形状集合与连线集合，提供选中管理、ZOrder 排序、HitTest |
| `RenderCommand.cs` | 渲染命令，7 种类型（Rectangle/Ellipse/RoundedRect/Polygon/Line/Text/MemberArea），数据驱动绘制 |
| `ShapeType.cs` | 图形类型描述，包含名称、分类、RenderCommand 列表、默认尺寸/颜色、状态列表，可创建实例 |
| `ShapeTypeRegistry.cs` | 图形类型运行时注册表（单例），提供按名称/分类查询、注册/注销功能 |
| `ShapeMember.cs` | 类图成员定义（Property/Method/Event/Constraint/Field），含可见性、签名生成 |
| `ShapeMemberParameter.cs` | 成员参数定义（名称、类型、默认值） |
| `ShapeState.cs` | 图形状态定义，包含状态名及各部位颜色（填充/边框/文字/标题栏） |
| `GraphicsUtility.cs` | 公共图形工具类，提供圆角矩形路径创建与绘制 |
| `XmlColor.cs` | 可序列化颜色包装类，使用 int ARGB 值替代 Color 以支持 XML 序列化 |

#### Shapes 层（`Shapes/`）

| 文件 | 职责 |
|------|------|
| `GenericShape.cs` | 通用图形，通过 ShapeTypeRegistry 查找 RenderCommand 列表动态绘制，支持成员管理、状态切换、椭圆/多边形连线点 |
| `ContainerShape.cs` | 容器图形，支持嵌套子元素、HeaderHeight 标题栏、裁剪绘制子图形、联动移动子元素 |

#### Controls 层（`Controls/`）

| 文件 | 职责 |
|------|------|
| `DrawingCanvas.cs` | 画布控件（交互部分）：双缓冲、缩放/平移、选择/拖拽/连线/框选/调整尺寸、键盘快捷键、拖放接收 |
| `DrawingCanvas.Rendering.cs` | 画布控件（渲染部分）：OnPaint 渲染管线、背景渐变、网格、形状绘制、连线绘制、橡皮筋线、框选矩形 |
| `ToolboxPanel.cs` | 工具箱面板：按分类显示已注册图形类型，支持拖放创建图形实例，自动生成图标 |
| `DiagramEditor.cs` | 复合控件（Facade）：封装画布+工具箱+属性栏+工具栏+状态栏+右键菜单+菜单注入+主题系统 |
| `DiagramEditor.Commands.cs` | 复合控件的命令/事件处理部分：文件操作、编辑操作、视图切换、工具切换、右键菜单、上下文感知菜单 |

#### Config 层（`Config/`）

| 文件 | 职责 |
|------|------|
| `GlobalConfig.cs` | 全局配置单例：连线参数、网格参数、画布参数、主题枚举、14 个主题感知颜色属性 |

#### Serialization 层（`Serialization/`）

| 文件 | 职责 |
|------|------|
| `DocumentData.cs` | 数据传输对象（DTO）：DocumentData/ShapeData/ConnectionData/MemberData/ParameterData/StateData |
| `XmlShapeSerializer.cs` | XML 序列化/反序列化器，实现 DrawingDocument 与 DocumentData 之间的双向转换 |

#### DemoApp（`DemoApp/`）

| 文件 | 职责 |
|------|------|
| `Program.cs` | 应用程序入口点（STAThread, EnableVisualStyles） |
| `MainForm.cs` | 宿主窗体：创建 DiagramEditor、注册 13 种图形类型、注入菜单、初始化示例数据 |

---

## 3. 核心架构设计

### 3.1 复合控件模式（Facade）

`DiagramEditor` 是整个控件库的唯一公共入口（Facade），它封装了所有子组件：

```
DiagramEditor (UserControl)
  +-- ToolStrip（工具栏：选择/连线/直线/曲线/折线/缩放/置顶/置底/删除）
  +-- StatusStrip（状态栏：状态文本 + 缩放/坐标显示）
  +-- SplitContainer (_mainSplit)
  |     +-- Panel1: ToolboxPanel（工具箱）
  |     +-- Panel2: SplitContainer (_rightSplit)
  |           +-- Panel1: DrawingCanvas（画布）
  |           +-- Panel2: PropertyGrid（属性栏）
  +-- ContextMenuStrip（右键上下文菜单，运行时动态创建）
  +-- MenuStrip（宿主菜单注入）
```

宿主窗体只需创建一个 `DiagramEditor` 实例，即可获得完整的可视化编辑器功能。

### 3.2 控件-宿主协议

控件库通过两个公共方法与宿主窗体集成：

- **`ConfigureHostForm(Form parentForm)`**：获取宿主窗体的 `MainMenuStrip`，将其作为菜单注入目标。
- **`ConfigureMenu(MenuStrip hostMenu)`**：直接指定要注入的 MenuStrip，并立即执行菜单注入（`InjectMenus`）。

两个方法均为可选调用，控件在没有宿主集成的情况下仍可正常工作（仅无菜单功能）。

### 3.3 RenderCommand 驱动渲染

图形绘制采用数据驱动的 RenderCommand 模式：

1. `ShapeType` 包含一个 `List<RenderCommand>`，描述图形的绘制指令序列。
2. `GenericShape.Draw()` 从 `ShapeTypeRegistry` 获取对应的 `ShapeType`，然后依次执行每个 `RenderCommand.Execute()`。
3. 每个 `RenderCommand` 使用相对坐标（0~1 范围），在 `Execute` 时根据形状实际 Bounds 映射为绝对坐标。

这实现了**图形外观与控件库代码的完全分离**——新增图形类型只需注册新的 `ShapeType`，无需修改任何控件库代码。

### 3.4 容器-子元素模型

`ContainerShape` 继承 `ShapeBase`，维护一个 `Children` 列表：

- 子元素的 `Parent` 指向容器。
- 容器移动时，所有子元素联动移动（`Move` 方法重写）。
- 容器绘制时，使用 `Graphics.Clip` 裁剪区域，子元素只在容器 body 区域内绘制。
- 容器内的连线在容器绘制阶段单独渲染，也使用裁剪区域。
- `DrawingDocument.RemoveShape` 会级联移除子元素和相关连线。

### 3.5 序列化架构

采用 DTO（Data Transfer Object）模式：

```
DrawingDocument (运行时模型)
       |  ^
  ConvertToData / ConvertFromData
       |  |
DocumentData (XML 可序列化 DTO)
```

- `XmlShapeSerializer` 提供 `Save`/`Load` 静态方法，负责 DocumentData 的 XML 读写。
- `ConvertToData`/`ConvertFromData` 负责运行时对象与 DTO 之间的双向映射。
- 颜色通过 `XmlColor`（int ARGB）在 DTO 中存储，连接模式/可见性等枚举以字符串形式存储，反序列化时通过 `Enum.Parse` 恢复。

---

## 4. 模块详细设计

### 4.1 Core 层

#### 4.1.1 ShapeBase（抽象基类）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：所有图形（GenericShape、ContainerShape）的抽象基类。

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `Guid` | 唯一标识符，`[Browsable(false)]` |
| `Name` | `string` | 显示名称，`[Category("外观")]` |
| `Description` | `string` | 详细描述，`[Category("外观")]` |
| `Bounds` | `RectangleF` | 位置与尺寸，`[Browsable(false)]` |
| `X` / `Y` / `Width` / `Height` | `float` | 独立的布局属性，修改后调用 `NotifyChanged()`，`[Category("布局")]` |
| `FillColor` | `Color` | 填充颜色，`[Category("外观")]` |
| `BorderColor` | `Color` | 边框颜色，`[Category("外观")]` |
| `TextColor` | `Color` | 文字颜色，`[Category("外观")]` |
| `BorderWidth` | `float` | 边框宽度，`[Category("外观")]` |
| `Selected` | `bool` | 是否选中，`[Browsable(false)]` |
| `Hovered` | `bool` | 是否悬停，`[Browsable(false)]` |
| `IsContainer` | `bool` | 是否为容器，`[Browsable(false)]` |
| `Parent` | `ShapeBase` | 父容器引用，`[Browsable(false)]` |
| `ZOrder` | `int` | 绘制层级，`[Browsable(false)]` |
| `Visible` | `bool` | 是否可见，`[Browsable(false)]` |
| `Resizable` | `bool` | 是否允许调整大小，`[Category("行为")]` |
| `MinWidth` / `MinHeight` | `float` | 调整大小的最小限制，`[Category("行为")]` |
| `Center` | `PointF` | 只读，几何中心点，`[Browsable(false)]` |

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `Move` | `virtual void Move(float dx, float dy)` | 移动图形 |
| `HitTest` | `virtual bool HitTest(PointF pt)` | 点命中测试（检查 Visible） |
| `HitTest` | `virtual bool HitTest(RectangleF rect)` | 矩形相交测试 |
| `GetNearestConnectionPoint` | `virtual PointF GetNearestConnectionPoint(PointF from)` | 计算从外部点到图形边缘的最近连接点（基于矩形边界等比缩放） |
| `HitTestResizeHandle` | `ResizeHandle HitTestResizeHandle(PointF pt, float tolerance)` | 检测鼠标是否命中调整手柄（8 个方向 + 4 条边），仅在 `Resizable && Selected` 时有效 |
| `GetResizeCursor` | `static Cursor GetResizeCursor(ResizeHandle handle)` | 根据手柄方向返回对应鼠标光标 |
| `Draw` | `abstract void Draw(Graphics g, float scale)` | 抽象绘制方法 |
| `Clone` | `abstract ShapeBase Clone()` | 抽象克隆方法 |
| `DrawSelection` | `virtual void DrawSelection(Graphics g, float scale)` | 绘制选中/悬停指示器（虚线框 + 手柄方块），选中时蓝色虚线，悬停时灰色虚线 |

**ResizeHandle 枚举**：`None / TopLeft / TopCenter / TopRight / MiddleLeft / MiddleRight / BottomLeft / BottomCenter / BottomRight`

**事件**：`Changed` — 属性变更时触发。

#### 4.1.2 Connection（连线）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：表示两个图形之间的连线。

**ConnectionMode 枚举**：`Straight / Curve / Orthogonal`

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `Guid` | 唯一标识符 |
| `FromShape` / `ToShape` | `ShapeBase` | 起止图形引用 |
| `FromPoint` / `ToPoint` | `PointF` | 起止坐标 |
| `Mode` | `ConnectionMode` | 连线模式，`[Category("外观")]` |
| `LineColor` | `Color` | 线条颜色，`[Category("外观")]` |
| `LineWidth` | `float` | 线条宽度，`[Category("外观")]` |
| `DashStyle` | `DashStyle` | 线型（Solid/Dash/Dot 等），`[Category("外观")]` |
| `ArrowAtEnd` | `bool` | 末端箭头，`[Category("外观")]` |
| `Label` | `string` | 标签文本，`[Category("外观")]` |
| `Selected` | `bool` | 是否选中 |
| `AllowIntersection` | `bool` | 是否允许连线相交 |

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `UpdateEndpoints` | `void UpdateEndpoints()` | 根据 FromShape/ToShape 的当前位置重新计算连接点 |
| `Draw` | `void Draw(Graphics g, float scale)` | 绘制连线（支持 Straight 直线、Curve 贝塞尔曲线、Orthogonal 折线），含箭头和标签 |
| `HitTest` | `bool HitTest(PointF pt, float tolerance)` | 点到连线各段的距离测试 |

**内部方法**：
- `GetDrawPoints()` — 根据模式返回控制点数组（Straight 2 点, Curve 4 点贝塞尔, Orthogonal 4 点折线）
- `DrawArrow()` — 在末端绘制三角形箭头
- `GetLabelPosition()` — 计算标签位置（直线取中点，曲线取中间两控制点中点）
- `DistanceToSegment()` / `Distance()` — 点到线段的距离计算

#### 4.1.3 DrawingDocument（文档模型）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：管理所有图形和连线，提供文档操作方法。

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Name` | `string` | 文档名称 |
| `PageSize` | `SizeF` | 页面尺寸（默认 2000x1500） |
| `Shapes` | `List<ShapeBase>` | 图形集合（按 ZOrder 排序） |
| `Connections` | `List<Connection>` | 连线集合 |

**关键方法**：

| 方法 | 说明 |
|------|------|
| `AddShape(ShapeBase)` | 添加图形，自动设置 ZOrder |
| `RemoveShape(ShapeBase)` | 移除图形（级联移除关联连线、从容器中移出、递归移除容器子元素） |
| `AddConnection(Connection)` | 添加连线 |
| `RemoveConnection(Connection)` | 移除连线 |
| `ClearSelection()` | 清除所有图形和连线的选中状态 |
| `GetSelectedShapes()` | 返回所有选中图形列表 |
| `GetSelectedConnections()` | 返回所有选中连线列表 |
| `BringToFront(ShapeBase)` | 置顶图形（ZOrder 重新计算并排序） |
| `SendToBack(ShapeBase)` | 置底图形（ZOrder 重新计算并排序） |
| `HitTestShape(PointF)` | 从顶层到底层进行点命中测试，返回首个命中的图形 |
| `HitTestConnection(PointF, float)` | 遍历连线进行命中测试 |
| `GetShapesInRect(RectangleF)` | 返回与矩形相交的所有图形 |
| `Clear()` | 清空文档 |

**内部类**：
- `ZOrderComparer` — 实现 `IComparer<ShapeBase>`，按 ZOrder 升序排列。

**事件**：`DocumentChanged` — 文档内容变更时触发。

#### 4.1.4 RenderCommand（渲染命令）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：数据驱动的图形绘制指令。

**RenderCommandType 枚举**：`Rectangle / Ellipse / RoundedRect / Polygon / Line / Text / MemberArea`

**关键属性**：

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `CommandType` | `RenderCommandType` | `Rectangle` | 命令类型 |
| `X` / `Y` / `Width` / `Height` | `float` | 0/0/1/1 | 相对坐标（0~1），在 Execute 时映射到实际 Bounds |
| `CornerRadius` | `float` | 0 | 圆角半径（RoundedRect） |
| `FillColor` / `StrokeColor` | `XmlColor` | Transparent/Black | 自定义颜色 |
| `StrokeWidth` | `float` | 1 | 描边宽度 |
| `Text` | `string` | "" | 文本内容 |
| `TextAlign` | `string` | "center" | 对齐方式（"center"/"left"/"right"） |
| `FontSize` | `float` | 10 | 字号 |
| `IsBold` | `bool` | false | 是否加粗 |
| `PolygonPoints` | `PointF[]` | 空数组 | 多边形顶点（相对坐标 0~1） |
| `UseShapeColors` | `bool` | true | 是否使用图形自身颜色（优先级高于 FillColor/StrokeColor） |
| `Fill` / `Stroke` | `bool` | true | 是否执行填充/描边 |

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `Execute` | `void Execute(Graphics g, RectangleF bounds, ShapeColors colors, float scale)` | 执行渲染命令，将相对坐标映射到绝对坐标后分派到具体绘制方法 |

**ShapeColors 辅助类**：
```csharp
public class ShapeColors
{
    public Color FillColor;      // 填充色
    public Color BorderColor;    // 边框色
    public Color TextColor;      // 文字色
    public Color HeaderColor;    // 标题栏色
}
```

#### 4.1.5 ShapeType（图形类型描述）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：描述一种图形类型的元数据，包含渲染指令、默认外观和行为标志。

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Name` | `string` | 类型名称（唯一标识） |
| `Category` | `string` | 分类名（默认 "基本图形"），用于工具箱分组 |
| `Description` | `string` | 类型描述 |
| `IconName` | `string` | 图标名称 |
| `RenderCommands` | `List<RenderCommand>` | 渲染命令列表 |
| `IsContainer` | `bool` | 是否为容器类型 |
| `SupportsMembers` | `bool` | 是否支持类图成员系统 |
| `DefaultWidth` / `DefaultHeight` | `float` | 默认尺寸 |
| `DefaultFillColor` / `DefaultBorderColor` / `DefaultTextColor` | `XmlColor` | 默认颜色 |
| `DefaultStates` | `List<ShapeState>` | 默认状态列表 |

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `CreateInstance` | `ShapeBase CreateInstance()` | 根据类型创建图形实例。IsContainer=true 创建 ContainerShape，否则创建 GenericShape，并应用默认颜色和状态 |

#### 4.1.6 ShapeTypeRegistry（图形类型注册表）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：全局单例，管理所有已注册的图形类型。

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `Instance` | `static ShapeTypeRegistry Instance` | 单例访问 |
| `Register` | `void Register(ShapeType)` | 注册图形类型（以 Name 为键） |
| `Unregister` | `void Unregister(string name)` | 注销图形类型 |
| `GetShapeType` | `ShapeType GetShapeType(string name)` | 按名称查找 |
| `GetAllTypes` | `List<ShapeType> GetAllTypes()` | 获取所有类型 |
| `GetCategories` | `List<string> GetCategories()` | 获取所有分类名 |
| `GetTypesByCategory` | `List<ShapeType> GetTypesByCategory(string)` | 按分类获取类型列表 |
| `Clear` | `void Clear()` | 清空注册表 |
| `Contains` | `bool Contains(string name)` | 检查是否已注册 |

#### 4.1.7 ShapeMember / ShapeMemberParameter / ShapeState

**ShapeMember（类图成员）**

**命名空间**：`CloudNativeDesigner.Core`

**MemberType 枚举**：`Property / Method / Event / Constraint / Field`
**Visibility 枚举**：`Public / Private / Protected / Internal`

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `MemberType` | `MemberType` | 成员类型 |
| `Name` | `string` | 成员名称 |
| `TypeName` | `string` | 数据类型名 |
| `Visibility` | `Visibility` | 可见性 |
| `IsStatic` | `bool` | 是否静态 |
| `IsAbstract` | `bool` | 是否抽象 |
| `DefaultValue` | `string` | 默认值 |
| `Parameters` | `List<ShapeMemberParameter>` | 方法参数列表 |

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `GetSignature` | `string GetSignature()` | 生成 UML 风格签名（如 `+ GetName() : string`） |

可见性符号：Public=`+`, Private=`-`, Protected=`#`, Internal=`~`

---

**ShapeMemberParameter（方法参数）**

**命名空间**：`CloudNativeDesigner.Core`

**属性**：`Name`、`TypeName`、`DefaultValue`

`ToString()` 方法返回格式：`"TypeName Name"` 或 `"TypeName Name = DefaultValue"`

---

**ShapeState（图形状态）**

**命名空间**：`CloudNativeDesigner.Core`

**职责**：定义图形在特定状态下的颜色方案。

**属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Name` | `string` | 状态名（默认 "Normal"） |
| `FillColor` | `XmlColor` | 该状态的填充颜色 |
| `BorderColor` | `XmlColor` | 该状态的边框颜色 |
| `TextColor` | `XmlColor` | 该状态的文字颜色 |
| `HeaderColor` | `XmlColor` | 该状态的标题栏颜色 |
| `Priority` | `int` | 优先级 |

#### 4.1.8 GraphicsUtility（公共工具）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：消除圆角矩形绘制重复代码。

**关键方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `CreateRoundedRectPath` | `static GraphicsPath CreateRoundedRectPath(float x, float y, float width, float height, float radius)` | 创建圆角矩形路径，自动钳制 radius 不超过宽/高的一半 |
| `CreateRoundedRectPath` | `static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius)` | 重载，接受 RectangleF |
| `DrawRoundedRectOutline` | `static void DrawRoundedRectOutline(Graphics g, Pen pen, RectangleF rect, float radius)` | 绘制圆角矩形描边 |

#### 4.1.9 XmlColor（可序列化颜色）

**命名空间**：`CloudNativeDesigner.Core`

**职责**：将 `System.Drawing.Color` 包装为可 XML 序列化的形式。

**属性**：`Argb`（int）— 存储颜色的 ARGB 值。

**方法**：
- `XmlColor(Color color)` — 构造函数，从 Color 创建
- `Color ToColor()` — 转换回 Color
- `implicit operator Color(XmlColor)` — 隐式转换为 Color
- `implicit operator XmlColor(Color)` — 隐式从 Color 转换

---

### 4.2 Shapes 层

#### 4.2.1 GenericShape（通用图形）

**命名空间**：`CloudNativeDesigner.Shapes`

**职责**：通过 RenderCommand 列表驱动绘制的通用图形，支持成员管理和状态切换。

**继承**：`ShapeBase`

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `ShapeTypeName` | `string` | 关联的 ShapeType 名称，用于从 Registry 查找渲染指令 |
| `Members` | `List<ShapeMember>` | 类图成员列表 |
| `States` | `List<ShapeState>` | 状态列表 |
| `CurrentStateName` | `string` | 当前状态名，设置后自动应用该状态颜色 |
| `MemberAreaTop` | `float` | 成员区域起始位置（相对比例，默认 0.35） |

**关键方法**：

| 方法 | 说明 |
|------|------|
| `AddState(ShapeState)` / `RemoveState(string)` | 添加/移除状态 |
| `GetCurrentState()` | 按名称查找当前状态 |
| `Draw(Graphics, float)` | 渲染流程：查找 ShapeType -> 执行 RenderCommand 列表 -> 绘制名称 -> 绘制成员 -> 绘制选中 |
| `DrawFallback` | 当 ShapeType 未找到时的后备绘制（简单矩形） |
| `GetNearestConnectionPoint` | 重写：椭圆类型使用椭圆边缘计算，多边形类型回退到矩形算法 |
| `Clone` | 深拷贝（新 Guid），复制所有成员和状态 |

**绘制细节**：
- `DrawName`：名称绘制在 Bounds 上部 `MemberAreaTop` 比例区域内，使用 `StringTrimming.EllipsisCharacter` 截断
- `DrawMembers`：成员列表从 `Bounds.Y + Height * MemberAreaTop` 开始逐行绘制，行高由 `_memberLineHeight`（16f）控制，绘制前先画分隔线

#### 4.2.2 ContainerShape（容器图形）

**命名空间**：`CloudNativeDesigner.Shapes`

**职责**：可嵌套子元素的容器，带标题栏和裁剪绘制。

**继承**：`ShapeBase`

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `HeaderHeight` | `float` | 标题栏高度（默认 30f），`[Category("外观")]` |
| `HeaderColor` | `Color` | 标题栏颜色，`[Category("外观")]` |
| `HeaderText` | `string` | 标题文字，`[Category("外观")]` |
| `Children` | `List<ShapeBase>` | 子元素列表 |

**关键方法**：

| 方法 | 说明 |
|------|------|
| `AddChild(ShapeBase)` / `RemoveChild(ShapeBase)` | 添加/移除子元素（防重复添加，防自引用） |
| `Move(float dx, float dy)` | 重写：移动自身并联动移动所有子元素 |
| `HitTestHeader(PointF)` | 检测点是否命中标题栏区域 |
| `Draw(Graphics, float)` | 绘制流程：圆角矩形背景 -> 标题栏（圆角上半部分 + 标题文字） -> body 分隔线 -> 裁剪区域绘制子元素 -> 选中指示器 |
| `Clone` | 深拷贝（不复制子元素） |

**内部类**：`ZOrderComparer` — 子元素按 ZOrder 排序后绘制。

---

### 4.3 Controls 层

#### 4.3.1 DrawingCanvas（画布控件）

**命名空间**：`CloudNativeDesigner.Controls`

**类型**：`partial class DrawingCanvas : Control`

文件拆分：
- `DrawingCanvas.cs` — 交互逻辑（鼠标事件、键盘事件、拖放、坐标转换）
- `DrawingCanvas.Rendering.cs` — 渲染逻辑（OnPaint、DrawBackground、DrawGrid、DrawShapes、DrawConnections 等）

**CanvasTool 枚举**：`Select / Connect`

**关键属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `Document` | `DrawingDocument` | 文档模型 |
| `CurrentTool` | `CanvasTool` | 当前工具（Select/Connect） |
| `Zoom` | `float` | 缩放比例（0.1~5.0） |
| `Offset` | `PointF` | 平移偏移量 |

**事件**：`SelectionChanged`、`DocumentModified`

**构造函数初始化**：
- 设置 `ControlStyles.AllPaintingInWmPaint | UserPaint | OptimizedDoubleBuffer | ResizeRedraw`
- 初始化 `BufferedGraphicsContext`，最大缓冲 4096x4096
- 订阅 `GlobalConfig.Instance.Changed` 和 `Document.DocumentChanged` 事件

**交互方法**：

| 方法 | 说明 |
|------|------|
| `OnMouseDown` | 分发鼠标按下：中键平移、左键连线/选择/拖拽/框选/调整尺寸 |
| `OnMouseMove` | 分发鼠标移动：中键平移、拖拽移动（支持网格吸附）、连线橡皮筋、框选矩形、悬停高亮、光标更新 |
| `OnMouseUp` | 分发鼠标抬起：结束拖拽（含容器成员更新）、结束连线（创建 Connection）、结束框选（选中范围内图形） |
| `OnMouseWheel` | 滚轮缩放（以鼠标位置为中心点） |
| `OnDragOver` / `OnDragDrop` | 接收工具箱拖放（创建图形实例、吸附网格、容器成员检测） |
| `OnKeyDown` | 键盘快捷键：Delete 删除、Escape 取消、Ctrl+A 全选 |
| `DeleteSelected` | 删除选中的图形和连线 |
| `BringToFront` / `SendToBack` | 置顶/置底选中图形 |
| `ScreenToWorld` / `WorldToScreen` | 屏幕坐标与世界坐标转换 |

**容器成员自动管理**：
- `UpdateContainerMembership`：拖拽结束后检查被移动的图形是否完全落入某个容器 body 区域，自动添加/移除父子关系。
- `IsFullyInsideContainer`：检查图形是否在容器 body 内（留 4px 边距）。
- `ExpandWithChildren`：拖拽时自动扩展选中集，包含容器内所有子元素。

**资源释放**：`Dispose` 中释放 `BufferedGraphics` 和 `BufferedGraphicsContext`。

---

**DrawingCanvas.Rendering.cs（渲染部分）**

**渲染管线**（`OnPaint`）：

```
OnPaint(PaintEventArgs e)
  |
  +-- 分配/复用 BufferedGraphics
  +-- DrawBackground(g)          // 绘制背景（不受缩放影响）
  +-- 设置 SmoothingMode / TextRenderingHint
  +-- g.TranslateTransform(offset)
  +-- g.ScaleTransform(zoom)
  +-- DrawGrid(g)                // 绘制网格
  +-- DrawShapes(g)              // 绘制图形（含容器子元素和容器内连线）
  +-- DrawConnections(g)         // 绘制全局连线（排除同一容器内的连线）
  +-- DrawRubberBand(g)          // 绘制连线橡皮筋
  +-- DrawSelectionRect(g)      // 绘制框选矩形
  +-- g.ResetTransform()
  +-- _bufferedGraphics.Render(e.Graphics)   // 输出到屏幕
```

**DrawBackground（背景绘制）**：
1. 底层纯色填充（`CanvasBackground`）
2. 水平线性渐变叠加（从右侧 `GradientCenterColor` 到左侧 `CanvasBackground`）
3. 垂直线性渐变半透明叠加（从下方亮到上方暗，alpha=100）
4. 中心椭圆 PathGradientBrush 柔化高光（位于右下角，半径为控件尺寸 50%，SetSigmaBellShape(0.6, 1.0)）

**DrawGrid（网格绘制）**：
- 仅在 `GlobalConfig.Instance.ShowGrid` 为 true 时绘制
- 计算可视区域的世界坐标范围，从起始网格对齐点开始绘制水平和垂直线
- 网格颜色使用 `GlobalConfig.Instance.GridColor`，线宽 0.5f/scale

**DrawShapes（图形绘制）**：
- 遍历所有 `Parent == null` 且可见的图形进行绘制
- 对于 `ContainerShape`，绘制完容器后额外调用 `DrawConnectionsForContainer` 绘制容器内连线

**DrawConnections（全局连线绘制）**：
- 遍历所有连线，跳过 `FromShape.Parent == ToShape.Parent` 的容器内连线（这些在容器绘制阶段渲染）

**DrawConnectionsForContainer（容器内连线裁剪绘制）**：
- 使用 `g.Save()` / `g.Clip = new Region(bodyRect)` / `g.Restore(state)` 裁剪容器 body 区域
- 只绘制两端都在同一容器内的连线

**DrawRubberBand（连线橡皮筋）**：
- 虚线样式，颜色使用 `RubberBandColor`

**DrawSelectionRect（框选矩形）**：
- 半透明填充 + 虚线边框

#### 4.3.2 ToolboxPanel（工具箱面板）

**命名空间**：`CloudNativeDesigner.Controls`

**类型**：`ToolboxPanel : Panel`

**职责**：按分类显示已注册的图形类型，支持拖放创建图形实例。

**关键类型**：
- `CreateShapeHandler` 委托：`delegate ShapeBase CreateShapeHandler()`
- `ToolboxItem` 类：`Name`、`Category`、`Icon`（Bitmap）、`CreateShape`（委托）

**关键属性**：`SelectedItem`（当前选中的工具项）

**事件**：`ItemSelected`

**关键方法**：

| 方法 | 说明 |
|------|------|
| `ReloadFromRegistry()` | 从 ShapeTypeRegistry 重新加载所有图形类型，创建 ToolboxItem，自动生成图标 |
| `AddItem(ToolboxItem)` | 手动添加工具项 |
| `ClearItems()` | 清空工具箱 |
| `OnPaint` | 自定义绘制：分类标题（加粗）+ 工具项（图标+名称），选中项蓝色高亮，悬停项浅蓝背景 |
| `OnMouseDown` | 点击选中工具项并启动 `DoDragDrop` |

**图标生成**：
- `CreateIconFromType(ShapeType)` — 通过 `ShapeType.CreateInstance()` 创建临时图形实例，绘制到 28x28 Bitmap
- `CreateDefaultIcon(ToolboxItem)` — 后备方案，绘制简单灰色矩形

#### 4.3.3 DiagramEditor（复合控件）

**命名空间**：`CloudNativeDesigner.Controls`

**类型**：`partial class DiagramEditor : UserControl`

文件拆分：
- `DiagramEditor.cs` — 组件初始化、布局、公共属性/方法、菜单注入、主题系统、图标创建
- `DiagramEditor.Commands.cs` — 菜单/工具栏/画布事件处理、右键菜单、上下文感知菜单

**公共属性**：

| 属性 | 类型 | Category | 说明 |
|------|------|----------|------|
| `Canvas` | `DrawingCanvas` | -- | 画布控件（只读） |
| `Toolbox` | `ToolboxPanel` | -- | 工具箱（只读） |
| `ToolStrip` | `ToolStrip` | -- | 工具栏（只读） |
| `StatusBar` | `StatusStrip` | -- | 状态栏（只读） |
| `Document` | `DrawingDocument` | -- | 文档模型（只读，等同于 Canvas.Document） |
| `CurrentFilePath` | `string` | -- | 当前文件路径 |
| `ShowToolbar` | `bool` | 面板 | 是否显示工具栏 |
| `ShowPropertyPanel` | `bool` | 面板 | 是否显示属性栏 |
| `ShowToolboxPanel` | `bool` | 面板 | 是否显示工具箱面板 |
| `ShowMenuStrip` | `bool` | 面板 | 是否显示菜单栏 |
| `ShowStatusBar` | `bool` | 面板 | 是否显示状态栏 |
| `ShowContextMenu` | `bool` | 行为 | 是否启用右键菜单 |
| `ShowToolbarText` | `bool` | 外观 | 工具栏是否显示文字标签 |
| `Theme` | `EditorTheme` | 外观 | 编辑器配色主题（Light/Dark） |

**公共方法**：

| 方法 | 说明 |
|------|------|
| `ZoomIn()` / `ZoomOut()` / `ZoomReset()` | 缩放操作（1.2 倍步进） |
| `SetTool(CanvasTool)` | 切换当前工具（Select/Connect） |
| `SetConnectionMode(ConnectionMode)` | 设置连线模式（Straight/Curve/Orthogonal），同步更新选中连线和工具栏状态 |
| `DeleteSelected()` | 删除选中 |
| `SelectAll()` | 全选 |
| `BringToFront()` / `SendToBack()` | 层级调整 |
| `NewDocument()` | 新建文档 |
| `SaveDocument(string filePath)` | 保存文档到 XML 文件 |
| `LoadDocument(string filePath)` | 从 XML 文件加载文档 |
| `ConfigureHostForm(Form)` | 宿主集成（获取 MainMenuStrip） |
| `ConfigureMenu(MenuStrip)` | 菜单注入 |
| `ApplyTheme()` | 应用主题（工具栏、菜单栏、状态栏颜色） |

**公共事件**：`DocumentSaved`、`DocumentLoaded`、`NewDocumentCreated`、`ThemeChanged`

---

**菜单注入机制**：

1. `ConfigureMenu(MenuStrip)` → 保存引用 → `InjectMenus()`
2. `InjectMenus()` 通过 `FindOrCreateMenu` 查找或创建以下顶级菜单：
   - **文件(&F)**：新建、打开(Ctrl+O)、保存(Ctrl+S)、另存为、退出
   - **编辑(&E)**：删除(Delete)、全选(Ctrl+A)
   - **视图(&V)**：网格（Check）、对齐（Check）、工具栏（Check）、工具栏文字（Check）、属性栏（Check）、工具箱（Check）、右键菜单（Check）、状态栏（Check）、亮色主题（Check）、暗色主题（Check）、放大、缩小、重置视图
   - **工具(&T)**：选择工具、连线工具、直线模式、曲线模式、折线模式
   - **图形(&S)**：添加成员、切换状态、置顶、置底

3. `FindOrCreateMenu(MenuStrip, string text)` — 按文本匹配查找已有菜单项，不存在则创建新的。

---

**上下文感知菜单（UpdateMenuAvailability）**：

根据当前选中状态动态启用/禁用菜单项：
- `_menuEditDelete` / `_menuShapeToFront` / `_menuShapeToBack`：有选中时启用
- `_menuShapeAddMember`：选中单个 GenericShape 且其 ShapeType.SupportsMembers=true 时启用
- `_menuShapeSwitchState`：选中单个 GenericShape 且有多个 State 时启用

---

**右键菜单（ShowCanvasContextMenu）**：

运行时动态创建 `ContextMenuStrip`，根据上下文显示不同菜单项：
- 选中单个支持成员的 GenericShape：显示 "添加成员" / "切换状态"
- 始终显示："删除"
- 选中多个图形时：显示 "置顶" / "置底"
- 始终显示："属性..."

---

**Layout 延迟初始化**：

构造函数中不设置 `SplitterDistance`，而是注册 `Layout` 事件：
```csharp
this.Layout += new LayoutEventHandler(OnEditorLayout);
```
`OnEditorLayout` 在控件首次获得有效尺寸（Width > 50 && Height > 50）时调用 `ApplySplitterDistances`，然后注销事件。

`ApplySplitterDistances` 调用 `SafeSetSplitterDistance`：
- `_mainSplit`：工具箱面板 220px，不够时回退到 20%
- `_rightSplit`：属性栏 280px（从右侧算），不够时回退到 66%

`SafeSetSplitterDistance`：
- 检查 `FixedPanel` 方向，根据 Panel1MinSize/Panel2MinSize 钳制范围
- 超出范围时按 `fallbackRatio` 比例回退
- 整个操作包裹在 try-catch 中，静默忽略异常

---

**主题系统（MyColorTable）**：

`MyColorTable` 继承 `ProfessionalColorTable`，根据 `_dark` 标志覆盖 20+ 颜色属性：

| 覆盖属性 | 暗色值 | 亮色值 |
|----------|--------|--------|
| `ToolStripGradientBegin` | (45,50,65) | (250,252,255) |
| `ToolStripGradientMiddle` | (38,43,58) | (245,247,252) |
| `ToolStripGradientEnd` | (32,37,50) | (240,242,248) |
| `MenuStripGradientBegin` | (45,50,65) | (250,252,255) |
| `MenuStripGradientEnd` | (38,43,58) | (245,247,252) |
| `MenuItemSelected` | (60,120,200) | (220,235,250) |
| `MenuItemSelectedGradientBegin` | (50,110,190) | (210,230,248) |
| `MenuItemSelectedGradientEnd` | (45,100,175) | (200,225,245) |
| `SeparatorDark` | (55,60,78) | (210,215,225) |
| `SeparatorLight` | (70,75,92) | (230,233,240) |
| `ImageMarginGradientBegin` | (42,47,62) | (248,250,255) |
| `ImageMarginGradientMiddle` | (38,43,58) | (245,247,252) |
| `ImageMarginGradientEnd` | (35,40,52) | (242,244,250) |
| `ButtonSelectedHighlight` | (70,140,220) | (200,225,248) |
| `ButtonPressedHighlight` | (60,120,200) | (190,218,242) |
| `ButtonSelectedBorder` | (90,160,240) | (160,200,235) |
| `StatusStripGradientBegin` | (28,32,45) | (242,244,250) |
| `StatusStripGradientEnd` | (25,28,40) | (235,238,245) |

`ApplyTheme()` 同时设置 ToolStrip、MenuStrip、StatusStrip 的 BackColor/ForeColor/Renderer。

---

### 4.4 Config 层

#### 4.4.1 GlobalConfig（全局配置）

**命名空间**：`CloudNativeDesigner.Config`

**职责**：全局配置单例，管理连线、网格、画布和主题相关参数。

**EditorTheme 枚举**：`Light / Dark`

**可配置属性**（PropertyGrid 可编辑）：

| Category | 属性 | 类型 | 默认值 | 说明 |
|----------|------|------|--------|------|
| 连线 | `DefaultConnectionMode` | `ConnectionMode` | `Straight` | 默认连线模式 |
| 连线 | `AllowConnectionIntersection` | `bool` | `false` | 允许连线相交 |
| 连线 | `ShowConnectionArcOnIntersect` | `bool` | `true` | 相交时显示圆弧 |
| 连线 | `IntersectionArcColor` | `Color` | (255,80,80) | 相交圆弧颜色 |
| 连线 | `IntersectionArcRadius` | `float` | 6 | 相交圆弧半径 |
| 网格 | `SnapToGrid` | `bool` | `true` | 对齐网格 |
| 网格 | `GridSize` | `float` | 20 | 网格大小 |
| 网格 | `ShowGrid` | `bool` | `true` | 显示网格 |
| 画布 | `AntiAlias` | `bool` | `true` | 抗锯齿 |
| 画布 | `Theme` | `EditorTheme` | `Light` | 主题 |

**主题感知只读属性**（`[Browsable(false)]`）：

| 属性 | 暗色值 | 亮色值 |
|------|--------|--------|
| `CanvasBackground` | (25,30,50) | (235,242,250) |
| `GridColor` | (45,55,80) | (210,220,235) |
| `GradientCenterColor` | (30,38,65) | (245,250,255) |
| `SelectionColor` | (80,160,255) | (0,120,215) |
| `ToolPanelBackColor` | (35,40,55) | (245,247,250) |
| `ToolPanelCategoryColor` | (60,70,95) | (225,230,240) |
| `ToolPanelTextColor` | (200,210,230) | (50,55,65) |
| `ToolPanelBorderColor` | (55,65,85) | (210,215,225) |
| `RubberBandColor` | (80,160,255) | (0,120,215) |

**事件**：`Changed` — 配置变更时触发（画布订阅此事件以重绘）。

---

### 4.5 Serialization 层

#### 4.5.1 DocumentData（数据 DTO）

**命名空间**：`CloudNativeDesigner.Serialization`

**职责**：XML 序列化的数据传输对象，包含以下嵌套类：

| DTO 类 | 用途 | 关键字段 |
|--------|------|----------|
| `DocumentData` | 文档根数据 | Name, PageWidth, PageHeight, Shapes[], Connections[] |
| `ShapeData` | 图形数据 | ShapeClass, Id, Name, Description, X/Y/Width/Height, ArgbFillColor/BorderColor/TextColor, BorderWidth, ZOrder, Visible, ParentId, ShapeTypeName, IsContainer, HeaderText, HeaderHeight, ArgbHeaderColor, Members[], States[], CurrentStateName, MemberAreaTop |
| `MemberData` | 成员数据 | MemberType, Name, TypeName, Visibility, IsStatic, IsAbstract, DefaultValue, Parameters[] |
| `ParameterData` | 参数数据 | Name, TypeName, DefaultValue |
| `StateData` | 状态数据 | Name, ArgbFillColor, ArgbBorderColor, ArgbTextColor, ArgbHeaderColor, Priority |
| `ConnectionData` | 连线数据 | Id, FromShapeId, ToShapeId, Mode, ArgbLineColor, LineWidth, DashStyle, ArrowAtEnd, Label |

所有 DTO 类标记为 `[Serializable]`。

#### 4.5.2 XmlShapeSerializer（XML 序列化/反序列化）

**命名空间**：`CloudNativeDesigner.Serialization`

**职责**：实现 DrawingDocument（运行时模型）与 DocumentData（XML DTO）之间的双向转换。

**公共方法**：

| 方法 | 签名 | 说明 |
|------|------|------|
| `Save` | `static void Save(string filePath, DrawingDocument document)` | 将文档序列化为 XML 文件 |
| `Load` | `static DrawingDocument Load(string filePath)` | 从 XML 文件反序列化文档 |
| `ConvertToData` | `static DocumentData ConvertToData(DrawingDocument document)` | 运行时模型 → DTO |
| `ConvertFromData` | `static DrawingDocument ConvertFromData(DocumentData data)` | DTO → 运行时模型 |

**序列化流程**（`ConvertToData`）：
1. 创建 DocumentData，复制 Name/PageSize
2. 遍历所有 Shape，转换为 ShapeData（建立 idMap）
3. 回填 ParentId（通过 idMap 查找父元素 ID）
4. 遍历所有 Connection，转换为 ConnectionData

**反序列化流程**（`ConvertFromData`）：
1. 创建 DrawingDocument，设置 Name/PageSize
2. 遍历 ShapeData，转换为 ShapeBase（GenericShape 或 ContainerShape），建立 shapeMap
3. 建立父子关系（通过 parentMap）
4. 遍历 ConnectionData，转换为 Connection（通过 shapeMap 恢复 FromShape/ToShape 引用），Enum.Parse 带 try-catch 回退

---

## 5. 渲染管线

### 5.1 完整渲染流程

```
OnPaint(PaintEventArgs e)
    |
    +-- 1. 分配/复用 BufferedGraphics
    |     if (bufferedGraphics == null || bufferSize != ClientSize)
    |         重新分配
    |
    +-- 2. DrawBackground(g)
    |     纯色填充 → 水平渐变 → 垂直半透明渐变 → 椭圆柔化高光
    |     (不受 TranslateTransform/ScaleTransform 影响)
    |
    +-- 3. 设置 SmoothingMode.AntiAlias (如 GlobalConfig.AntiAlias)
    |
    +-- 4. g.TranslateTransform(offset.X, offset.Y)
    |     g.ScaleTransform(zoom, zoom)
    |
    +-- 5. DrawGrid(g)
    |     遍历可视区域，按 GridSize 绘制水平/垂直线
    |
    +-- 6. DrawShapes(g)
    |     遍历 Parent==null 且 Visible 的图形：
    |       shape.Draw(g, zoom)
    |       if (ContainerShape) → DrawConnectionsForContainer (裁剪绘制)
    |
    +-- 7. DrawConnections(g)
    |     遍历全局连线，跳过同一容器内的连线
    |
    +-- 8. DrawRubberBand(g)
    |     if (isConnecting) → 虚线连接橡皮筋
    |
    +-- 9. DrawSelectionRect(g)
    |     if (isSelecting) → 半透明填充 + 虚线框
    |
    +-- 10. g.ResetTransform()
    |
    +-- 11. _bufferedGraphics.Render(e.Graphics)
    |     将缓冲区输出到屏幕
```

### 5.2 背景渐变实现

`DrawBackground` 实现放射渐变效果（从右下到左上）：
1. **底层纯色**：`CanvasBackground` 全屏填充
2. **水平渐变叠加**：从右侧 `GradientCenterColor` 到左侧 `CanvasBackground` 的 `LinearGradientBrush`（WrapMode.TileFlipXY）
3. **垂直渐变半透明叠加**：从下方半透明 `centerColor`（alpha=100）到上方全透明的 `LinearGradientBrush`
4. **中心高光椭圆**：在右下角（85%, 85% 位置）使用 `PathGradientBrush`（SetSigmaBellShape(0.6, 1.0)）柔化高光

### 5.3 容器内连线裁剪绘制

为解决容器内连线溢出容器边界的问题，采用分层绘制策略：
- **DrawShapes** 阶段：绘制容器时，额外对容器内连线使用 `g.Clip = new Region(bodyRect)` 裁剪后绘制
- **DrawConnections** 阶段：全局连线遍历时跳过 `FromShape.Parent == ToShape.Parent` 的连线（避免重复绘制）

---

## 6. 交互管线

### 6.1 CanvasTool 枚举

| 工具 | 说明 |
|------|------|
| `Select` | 选择/拖拽/框选/调整尺寸模式，默认光标 |
| `Connect` | 连线创建模式，十字光标 |

### 6.2 选择流程

1. `OnMouseDown` → 获取世界坐标 → `HitTestShape(worldPos)`
2. 命中图形：
   - 未按 Ctrl → 先 `ClearSelection()`
   - 设置 `shape.Selected = true`
   - 检查 `HitTestResizeHandle` → 如果命中手柄，进入调整尺寸模式
   - 否则进入拖拽模式（`_isDragging = true`），记录拖拽起始位置和原始位置
   - `ExpandWithChildren` 自动扩展选中集（包含容器子元素）
3. 命中连线：清除其他选中，设置 `conn.Selected = true`
4. 未命中任何对象：清除选中，进入框选模式

### 6.3 拖拽流程

1. `OnMouseMove` → 计算位移 dx/dy
2. 如 `SnapToGrid`：将坐标对齐到网格
3. 更新所有 `_draggingShapes` 的位置
4. `OnMouseUp` → `_isDragging = false` → `UpdateContainerMembership` 检查容器归属

### 6.4 框选流程

1. `OnMouseDown`（空白区域） → `_isSelecting = true`，记录起始点
2. `OnMouseMove` → 计算矩形范围 `_selectionRect`
3. `OnMouseUp` → `GetShapesInRect(_selectionRect)` → 批量选中

### 6.5 连线创建流程

1. `CurrentTool = Connect`
2. `OnMouseDown` → `StartConnection`：找到起始图形，计算连接点
3. `OnMouseMove` → 更新橡皮筋终点 `_connectCurrentPoint`
4. `OnMouseUp` → `EndConnection`：找到目标图形（不能与起始图形相同），创建 Connection 并添加到文档

### 6.6 调整尺寸流程

1. `OnMouseDown` → 检测 `HitTestResizeHandle` → 记录原始 Bounds 和起始点
2. `OnMouseMove` → 根据 ResizeHandle 类型计算新的 X/Y/Width/Height
3. 应用 MinWidth/MinHeight 限制
4. 如 `SnapToGrid`：对齐到网格
5. `OnMouseUp` → 结束调整

### 6.7 鼠标事件分发

`OnMouseDown` 按优先级判断：中键 > Connect 工具 > ResizeHandle > Shape > Connection > 框选

`OnMouseMove` 按优先级判断：中键平移 > 调整尺寸 > 拖拽 > 连线橡皮筋 > 框选 > 悬停检测/光标更新

---

## 7. 主题系统

### 7.1 EditorTheme 枚举

```csharp
public enum EditorTheme
{
    Light,  // 亮色主题
    Dark    // 暗色主题
}
```

### 7.2 GlobalConfig 主题感知属性

`GlobalConfig` 定义了 9 个主题感知只读属性（`[Browsable(false)]`），根据 `_theme` 值返回不同颜色：

- `CanvasBackground`
- `GridColor`
- `GradientCenterColor`
- `SelectionColor`
- `ToolPanelBackColor`
- `ToolPanelCategoryColor`
- `ToolPanelTextColor`
- `ToolPanelBorderColor`
- `RubberBandColor`

### 7.3 MyColorTable

继承 `ProfessionalColorTable`，覆盖 20 个颜色属性以支持亮色/暗色主题。

### 7.4 ApplyTheme 方法

`DiagramEditor.ApplyTheme()` 在以下场景调用：
- 构造函数中初始化
- `Theme` 属性设置时
- `OnThemeLight` / `OnThemeDark` 菜单事件

应用范围：
- `_toolStrip`：BackColor、ForeColor、Renderer（MyColorTable）
- `_hostMenu`：BackColor、ForeColor、Renderer（MyColorTable）
- `_statusStrip`：BackColor、ForeColor
- `_mainSplit`：BackColor
- 更新主题菜单 CheckState
- 触发 `Invalidate(true)` 重绘

---

## 8. 图形类型注册机制

### 8.1 ShapeType 描述

每个图形类型通过 `ShapeType` 类描述：
- 名称（唯一标识）
- 分类（用于工具箱分组）
- RenderCommand 列表（定义绘制方式）
- 默认尺寸和颜色
- 行为标志（IsContainer、SupportsMembers）
- 默认状态列表

### 8.2 ShapeTypeRegistry 单例

全局注册表，以 Name 为键存储 ShapeType，提供注册/注销/查询功能。

### 8.3 预注册图形类型列表

DemoApp 中注册了 13 种图形类型：

| 分类 | 名称 | 描述 | 图形类型 | 主要 RenderCommand |
|------|------|------|----------|-------------------|
| 基本图形 | 矩形 | 圆角矩形 | GenericShape | RoundedRect + Text |
| 基本图形 | 椭圆 | 椭圆形 | GenericShape | Ellipse + Text |
| 基本图形 | 菱形 | 菱形决策节点 | GenericShape | Polygon(4点) + Text |
| 基本图形 | 六边形 | 正六边形 | GenericShape | Polygon(6点) |
| 云原生 | 云 | 云计算节点 | GenericShape | Polygon(12点) |
| 云原生 | 数据库 | 圆柱体数据库 | GenericShape | Polygon(body) + Ellipse(top) + Line + Ellipse(bottom) |
| 云原生 | 组件 | 微服务组件 | GenericShape | RoundedRect(main) + RoundedRect(plug1) + RoundedRect(plug2) |
| 流程图 | 起止 | 流程图起止节点 | GenericShape | RoundedRect(r=18) |
| 流程图 | 文档 | 带折角的文档 | GenericShape | Polygon(5点) + Line(fold) + Line(fold2) |
| 流程图 | 注释 | 便签注释 | GenericShape | Polygon(5点) + Polygon(fold triangle) |
| 容器 | 容器 | 可嵌套容器 | ContainerShape | (无 RenderCommand，容器自带绘制逻辑) |
| UML | 用户 | 参与者/用户 | GenericShape | Ellipse(head) + Polygon(body) |
| UML | 类 | 类图 | GenericShape | RoundedRect + Text(bold) + Line(sep) |

### 8.4 注册流程

1. 宿主在初始化时调用 `ShapeTypeRegistry.Instance.Clear()` 清空注册表
2. 对每种图形类型：
   - 创建 `ShapeType` 实例
   - 设置名称、分类、描述、默认尺寸/颜色
   - 创建 `RenderCommand` 列表（使用相对坐标 0~1）
   - 调用 `ShapeTypeRegistry.Instance.Register(t)` 注册
3. 调用 `_editor.Toolbox.ReloadFromRegistry()` 刷新工具箱显示

### 8.5 控件剥离原则

控件库本身不注册任何图形类型。所有图形类型由宿主应用在初始化时注册。这保证了：
- 控件库可以独立于具体图形类型使用
- 不同的宿主应用可以注册不同的图形类型
- 新增图形类型不需要修改控件库代码

---

## 9. 宿主集成指南

### 9.1 最简集成代码示例

```csharp
public class MyForm : Form
{
    private DiagramEditor _editor;
    private MenuStrip _menuStrip;

    public MyForm()
    {
        // 1. 创建宿主菜单栏
        _menuStrip = new MenuStrip();
        this.MainMenuStrip = _menuStrip;
        this.Controls.Add(_menuStrip);

        // 2. 创建编辑器控件
        _editor = new DiagramEditor();
        _editor.Dock = DockStyle.Fill;
        this.Controls.Add(_editor);

        // 3. 注入菜单（可选）
        _editor.ConfigureMenu(_menuStrip);

        // 4. 注册图形类型
        RegisterShapeTypes();
    }

    private void RegisterShapeTypes()
    {
        // 注册矩形
        ShapeType rect = new ShapeType();
        rect.Name = "矩形";
        rect.Category = "基本图形";
        rect.RenderCommands.Add(new RenderCommand()); // 配置渲染命令...
        ShapeTypeRegistry.Instance.Register(rect);

        // 刷新工具箱
        _editor.Toolbox.ReloadFromRegistry();
    }
}
```

### 9.2 ConfigureMenu / ConfigureHostForm 调用顺序

两种方式任选其一：

**方式一**：先创建 MenuStrip，后创建 DiagramEditor
```csharp
_menuStrip = new MenuStrip();
this.MainMenuStrip = _menuStrip;
this.Controls.Add(_menuStrip);

_editor = new DiagramEditor();
_editor.Dock = DockStyle.Fill;
this.Controls.Add(_editor);

_editor.ConfigureHostForm(this);  // 自动获取 MainMenuStrip
```

**方式二**：显式注入菜单
```csharp
_menuStrip = new MenuStrip();
this.MainMenuStrip = _menuStrip;
this.Controls.Add(_menuStrip);

_editor = new DiagramEditor();
_editor.Dock = DockStyle.Fill;
this.Controls.Add(_editor);

_editor.ConfigureMenu(_menuStrip);  // 显式注入
```

### 9.3 图形类型注册时机

图形类型注册必须在 DiagramEditor 创建**之后**、Toolbox 刷新**之前**完成：

```csharp
// 正确顺序
_editor = new DiagramEditor();        // 1. 创建控件
RegisterShapeTypes();                 // 2. 注册图形类型
_editor.Toolbox.ReloadFromRegistry(); // 3. 刷新工具箱
```

注意：`ConfigureMenu`/`ConfigureHostForm` 是可选调用，不调用也能正常工作（仅无菜单功能）。

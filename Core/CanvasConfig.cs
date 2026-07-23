using System;
using System.Collections.Generic;
using System.ComponentModel;
using CloudNativeDesigner.Config;

namespace CloudNativeDesigner.Core
{
    /// <summary>
    /// 画布配置。存储编辑器的 UI 状态和工具配置，
    /// 可由宿主定义并在加载画布时初始化编辑器状态。
    /// 作为 DrawingDocument 的一部分随文档序列化。
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CanvasConfig
    {
        private bool _showToolbar = true;
        private bool _showPropertyPanel = false;
        private bool _showToolboxPanel = true;
        private bool _showMenuStrip = true;
        private bool _showStatusBar = true;
        private bool _showContextMenu = true;
        private bool _showToolbarText = false;
        private string _theme = "Light";
        private ConnectionMode _connectionMode = ConnectionMode.Straight;
        private bool _designMode = true;
        private List<string> _enabledToolNames = new List<string>();
        private List<string> _shapeTypeNames = new List<string>();

        [Category("面板")]
        [Description("是否显示工具栏")]
        public bool ShowToolbar
        {
            get { return _showToolbar; }
            set { _showToolbar = value; }
        }

        [Category("面板")]
        [Description("是否显示属性面板")]
        public bool ShowPropertyPanel
        {
            get { return _showPropertyPanel; }
            set { _showPropertyPanel = value; }
        }

        [Category("面板")]
        [Description("是否显示工具箱面板")]
        public bool ShowToolboxPanel
        {
            get { return _showToolboxPanel; }
            set { _showToolboxPanel = value; }
        }

        [Category("面板")]
        [Description("是否显示菜单栏")]
        public bool ShowMenuStrip
        {
            get { return _showMenuStrip; }
            set { _showMenuStrip = value; }
        }

        [Category("面板")]
        [Description("是否显示状态栏")]
        public bool ShowStatusBar
        {
            get { return _showStatusBar; }
            set { _showStatusBar = value; }
        }

        [Category("行为")]
        [Description("是否启用右键菜单")]
        public bool ShowContextMenu
        {
            get { return _showContextMenu; }
            set { _showContextMenu = value; }
        }

        [Category("外观")]
        [Description("是否在工具栏按钮上显示文字")]
        public bool ShowToolbarText
        {
            get { return _showToolbarText; }
            set { _showToolbarText = value; }
        }

        [Category("外观")]
        [Description("编辑器主题：Light 或 Dark")]
        public string Theme
        {
            get { return _theme; }
            set { _theme = value; }
        }

        [Category("行为")]
        [Description("默认连线模式")]
        public ConnectionMode ConnectionMode
        {
            get { return _connectionMode; }
            set { _connectionMode = value; }
        }

        [Category("行为")]
        [Description("设计模式。false 时隐藏工具栏、工具箱、菜单栏等编辑 UI，仅保留运行时操作")]
        public bool DesignMode
        {
            get { return _designMode; }
            set { _designMode = value; }
        }

        [Category("工具")]
        [Description("启用的工具名列表")]
        public List<string> EnabledToolNames
        {
            get { return _enabledToolNames; }
            set { _enabledToolNames = value; }
        }

        [Category("图形")]
        [Description("可用的图形类型名列表")]
        public List<string> ShapeTypeNames
        {
            get { return _shapeTypeNames; }
            set { _shapeTypeNames = value; }
        }

        public CanvasConfig() { }

        public override string ToString()
        {
            return string.Format("{0}, DesignMode={1}", _theme, _designMode);
        }
    }
}

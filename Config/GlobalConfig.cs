using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Config
{
    public enum EditorTheme
    {
        Light,
        Dark
    }

    [Serializable]
    public class GlobalConfig
    {
        private static GlobalConfig _instance = new GlobalConfig();
        public static GlobalConfig Instance { get { return _instance; } }

        private ConnectionMode _defaultConnectionMode = ConnectionMode.Straight;
        private bool _allowConnectionIntersection = false;
        private bool _showConnectionArcOnIntersect = true;
        private Color _intersectionArcColor = Color.FromArgb(255, 80, 80);
        private float _intersectionArcRadius = 6f;
        private bool _snapToGrid = true;
        private float _gridSize = 20f;
        private bool _showGrid = true;
        private bool _antiAlias = true;
        private float _defaultZoom = 1.0f;
        private EditorTheme _theme = EditorTheme.Light;

        [Category("连线")]
        [DisplayName("默认连线模式")]
        public ConnectionMode DefaultConnectionMode
        {
            get { return _defaultConnectionMode; }
            set { _defaultConnectionMode = value; NotifyChanged(); }
        }

        [Category("连线")]
        [DisplayName("允许连线相交")]
        public bool AllowConnectionIntersection
        {
            get { return _allowConnectionIntersection; }
            set { _allowConnectionIntersection = value; NotifyChanged(); }
        }

        [Category("连线")]
        [DisplayName("相交时显示圆弧")]
        public bool ShowConnectionArcOnIntersect
        {
            get { return _showConnectionArcOnIntersect; }
            set { _showConnectionArcOnIntersect = value; NotifyChanged(); }
        }

        [Category("连线")]
        [DisplayName("相交圆弧颜色")]
        public Color IntersectionArcColor
        {
            get { return _intersectionArcColor; }
            set { _intersectionArcColor = value; NotifyChanged(); }
        }

        [Category("连线")]
        [DisplayName("相交圆弧半径")]
        public float IntersectionArcRadius
        {
            get { return _intersectionArcRadius; }
            set { _intersectionArcRadius = value; NotifyChanged(); }
        }

        [Category("网格")]
        [DisplayName("对齐网格")]
        public bool SnapToGrid
        {
            get { return _snapToGrid; }
            set { _snapToGrid = value; NotifyChanged(); }
        }

        [Category("网格")]
        [DisplayName("网格大小")]
        public float GridSize
        {
            get { return _gridSize; }
            set { _gridSize = value; NotifyChanged(); }
        }

        [Category("网格")]
        [DisplayName("显示网格")]
        public bool ShowGrid
        {
            get { return _showGrid; }
            set { _showGrid = value; NotifyChanged(); }
        }

        [Category("画布")]
        [DisplayName("抗锯齿")]
        public bool AntiAlias
        {
            get { return _antiAlias; }
            set { _antiAlias = value; NotifyChanged(); }
        }

        [Category("画布")]
        [DisplayName("主题")]
        [Description("编辑器配色主题")]
        public EditorTheme Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                ApplyThemeColors();
                NotifyChanged();
            }
        }

        [Browsable(false)]
        public float DefaultZoom
        {
            get { return _defaultZoom; }
            set { _defaultZoom = value; }
        }

        [Browsable(false)]
        public Color CanvasBackground
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(25, 30, 50);
                return Color.FromArgb(235, 242, 250);
            }
        }

        [Browsable(false)]
        public Color GridColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(45, 55, 80);
                return Color.FromArgb(210, 220, 235);
            }
        }

        [Browsable(false)]
        public Color GradientCenterColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(30, 38, 65);
                return Color.FromArgb(245, 250, 255);
            }
        }

        [Browsable(false)]
        public Color SelectionColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(80, 160, 255);
                return Color.FromArgb(0, 120, 215);
            }
        }

        [Browsable(false)]
        public Color ToolPanelBackColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(35, 40, 55);
                return Color.FromArgb(245, 247, 250);
            }
        }

        [Browsable(false)]
        public Color ToolPanelCategoryColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(60, 70, 95);
                return Color.FromArgb(225, 230, 240);
            }
        }

        [Browsable(false)]
        public Color ToolPanelTextColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(200, 210, 230);
                return Color.FromArgb(50, 55, 65);
            }
        }

        [Browsable(false)]
        public Color ToolPanelBorderColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(55, 65, 85);
                return Color.FromArgb(210, 215, 225);
            }
        }

        [Browsable(false)]
        public Color RubberBandColor
        {
            get
            {
                if (_theme == EditorTheme.Dark)
                    return Color.FromArgb(80, 160, 255);
                return Color.FromArgb(0, 120, 215);
            }
        }

        public void ApplyThemeColors()
        {
        }

        public event EventHandler Changed;

        private void NotifyChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }
    }
}

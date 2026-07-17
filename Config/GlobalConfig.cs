using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Config
{
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
        private Color _gridColor = Color.FromArgb(220, 220, 220);
        private Color _canvasBackground = Color.White;
        private bool _antiAlias = true;
        private float _defaultZoom = 1.0f;

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

        [Category("网格")]
        [DisplayName("网格颜色")]
        public Color GridColor
        {
            get { return _gridColor; }
            set { _gridColor = value; NotifyChanged(); }
        }

        [Category("画布")]
        [DisplayName("背景颜色")]
        public Color CanvasBackground
        {
            get { return _canvasBackground; }
            set { _canvasBackground = value; NotifyChanged(); }
        }

        [Category("画布")]
        [DisplayName("抗锯齿")]
        public bool AntiAlias
        {
            get { return _antiAlias; }
            set { _antiAlias = value; NotifyChanged(); }
        }

        [Browsable(false)]
        public float DefaultZoom
        {
            get { return _defaultZoom; }
            set { _defaultZoom = value; }
        }

        public event EventHandler Changed;

        private void NotifyChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }
    }
}

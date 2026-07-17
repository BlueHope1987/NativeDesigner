using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CloudNativeDesigner.Core
{
    [Serializable]
    public abstract class ShapeBase
    {
        private Guid _id = Guid.NewGuid();
        private RectangleF _bounds = new RectangleF(0, 0, 120, 80);
        private string _name = "Shape";
        private string _description = "";
        private Color _fillColor = Color.FromArgb(230, 240, 255);
        private Color _borderColor = Color.FromArgb(80, 120, 180);
        private Color _textColor = Color.FromArgb(40, 40, 40);
        private float _borderWidth = 1.5f;
        private bool _selected = false;
        private bool _hovered = false;
        private bool _isContainer = false;
        private ShapeBase _parent = null;
        private int _zOrder = 0;
        private bool _visible = true;

        [Browsable(false)]
        public Guid Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [Category("外观")]
        [DisplayName("名称")]
        [Description("实体的显示名称")]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null)
                    _name = "";
                else
                    _name = value;
                NotifyChanged();
            }
        }

        [Category("外观")]
        [DisplayName("描述")]
        [Description("实体的详细描述")]
        public string Description
        {
            get { return _description; }
            set
            {
                if (value == null)
                    _description = "";
                else
                    _description = value;
                NotifyChanged();
            }
        }

        [Browsable(false)]
        public RectangleF Bounds
        {
            get { return _bounds; }
            set { _bounds = value; NotifyChanged(); }
        }

        [Category("布局")]
        [DisplayName("X")]
        public float X
        {
            get { return _bounds.X; }
            set { _bounds.X = value; NotifyChanged(); }
        }

        [Category("布局")]
        [DisplayName("Y")]
        public float Y
        {
            get { return _bounds.Y; }
            set { _bounds.Y = value; NotifyChanged(); }
        }

        [Category("布局")]
        [DisplayName("宽度")]
        public float Width
        {
            get { return _bounds.Width; }
            set
            {
                if (value > 10)
                {
                    _bounds.Width = value;
                    NotifyChanged();
                }
            }
        }

        [Category("布局")]
        [DisplayName("高度")]
        public float Height
        {
            get { return _bounds.Height; }
            set
            {
                if (value > 10)
                {
                    _bounds.Height = value;
                    NotifyChanged();
                }
            }
        }

        [Category("外观")]
        [DisplayName("填充颜色")]
        public Color FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; NotifyChanged(); }
        }

        [Category("外观")]
        [DisplayName("边框颜色")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; NotifyChanged(); }
        }

        [Category("外观")]
        [DisplayName("文字颜色")]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; NotifyChanged(); }
        }

        [Category("外观")]
        [DisplayName("边框宽度")]
        public float BorderWidth
        {
            get { return _borderWidth; }
            set { _borderWidth = value; NotifyChanged(); }
        }

        [Browsable(false)]
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; NotifyChanged(); }
        }

        [Browsable(false)]
        public bool Hovered
        {
            get { return _hovered; }
            set { _hovered = value; }
        }

        [Browsable(false)]
        public bool IsContainer
        {
            get { return _isContainer; }
            set { _isContainer = value; }
        }

        [Browsable(false)]
        public ShapeBase Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        [Browsable(false)]
        public int ZOrder
        {
            get { return _zOrder; }
            set { _zOrder = value; }
        }

        [Browsable(false)]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        [Browsable(false)]
        public PointF Center
        {
            get { return new PointF(_bounds.X + _bounds.Width / 2f, _bounds.Y + _bounds.Height / 2f); }
        }

        public virtual void Move(float dx, float dy)
        {
            _bounds.X += dx;
            _bounds.Y += dy;
            NotifyChanged();
        }

        public virtual bool HitTest(PointF pt)
        {
            if (!Visible)
                return false;
            return _bounds.Contains(pt);
        }

        public virtual bool HitTest(RectangleF rect)
        {
            if (!Visible)
                return false;
            return _bounds.IntersectsWith(rect);
        }

        public virtual PointF GetNearestConnectionPoint(PointF from)
        {
            PointF center = Center;
            float halfW = _bounds.Width / 2f;
            float halfH = _bounds.Height / 2f;

            float dx = from.X - center.X;
            float dy = from.Y - center.Y;

            if (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f)
                return new PointF(center.X + halfW, center.Y);

            float absDx = Math.Abs(dx);
            float absDy = Math.Abs(dy);
            float scale;

            if (absDx * halfH > absDy * halfW)
                scale = halfW / absDx;
            else
                scale = halfH / absDy;

            return new PointF(center.X + dx * scale, center.Y + dy * scale);
        }

        public abstract void Draw(Graphics g, float scale);
        public abstract ShapeBase Clone();

        public event EventHandler Changed;

        protected void NotifyChanged()
        {
            if (Changed != null)
                Changed(this, EventArgs.Empty);
        }

        protected virtual void DrawSelection(Graphics g, float scale)
        {
            if (!Selected && !Hovered)
                return;

            RectangleF rect = _bounds;
            float offset = 4f / scale;
            rect.Inflate(offset, offset);

            Color penColor = Selected ? Color.FromArgb(0, 120, 215) : Color.Gray;
            using (Pen pen = new Pen(penColor))
            {
                pen.DashStyle = DashStyle.Dash;
                pen.Width = 1f / scale;
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

            if (Selected)
            {
                float handleSize = 6f / scale;
                using (Brush brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    DrawHandle(g, brush, rect.Left, rect.Top, handleSize);
                    DrawHandle(g, brush, rect.Right, rect.Top, handleSize);
                    DrawHandle(g, brush, rect.Left, rect.Bottom, handleSize);
                    DrawHandle(g, brush, rect.Right, rect.Bottom, handleSize);
                }
            }
        }

        private void DrawHandle(Graphics g, Brush brush, float x, float y, float size)
        {
            g.FillRectangle(brush, x - size / 2f, y - size / 2f, size, size);
        }
    }
}

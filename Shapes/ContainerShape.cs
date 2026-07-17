using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Shapes
{
    [Serializable]
    public class ContainerShape : ShapeBase
    {
        private List<ShapeBase> _children = new List<ShapeBase>();
        private float _headerHeight = 30f;
        private Color _headerColor = Color.FromArgb(80, 130, 180);
        private string _headerText = "容器";

        public ContainerShape()
        {
            Name = "容器";
            IsContainer = true;
            Bounds = new RectangleF(0, 0, 300, 220);
            FillColor = Color.FromArgb(250, 250, 250);
            BorderColor = Color.FromArgb(100, 100, 120);
            HeaderColor = Color.FromArgb(80, 130, 180);
        }

        [Category("外观")]
        [DisplayName("标题栏高度")]
        public float HeaderHeight
        {
            get { return _headerHeight; }
            set { _headerHeight = value; NotifyChanged(); }
        }

        [Category("外观")]
        [DisplayName("标题栏颜色")]
        public Color HeaderColor
        {
            get { return _headerColor; }
            set { _headerColor = value; NotifyChanged(); }
        }

        [Category("外观")]
        [DisplayName("标题文字")]
        public string HeaderText
        {
            get { return _headerText; }
            set { _headerText = value ?? ""; NotifyChanged(); }
        }

        public List<ShapeBase> Children { get { return _children; } }

        public void AddChild(ShapeBase child)
        {
            if (child == null || child == this) return;
            if (!_children.Contains(child))
            {
                _children.Add(child);
                child.Parent = this;
                NotifyChanged();
            }
        }

        public void RemoveChild(ShapeBase child)
        {
            if (child == null) return;
            _children.Remove(child);
            child.Parent = null;
            NotifyChanged();
        }

        public override void Move(float dx, float dy)
        {
            base.Move(dx, dy);
            foreach (var child in _children)
            {
                child.Move(dx, dy);
            }
        }

        public override bool HitTest(PointF pt)
        {
            if (!Visible) return false;
            return Bounds.Contains(pt);
        }

        public bool HitTestHeader(PointF pt)
        {
            RectangleF header = Bounds;
            header.Height = _headerHeight;
            return header.Contains(pt);
        }

        public override void Draw(Graphics g, float scale)
        {
            RectangleF rect = Bounds;
            RectangleF headerRect = rect;
            headerRect.Height = _headerHeight;
            RectangleF bodyRect = rect;
            bodyRect.Y += _headerHeight;
            bodyRect.Height -= _headerHeight;

            using (GraphicsPath path = CreateRoundedRect(rect, 6f))
            {
                using (Brush brush = new SolidBrush(FillColor))
                {
                    g.FillPath(brush, path);
                }
            }

            using (Brush headerBrush = new SolidBrush(_headerColor))
            {
                GraphicsPath headerPath = new GraphicsPath();
                float r = 6f;
                headerPath.AddArc(headerRect.X, headerRect.Y, r * 2, r * 2, 180, 90);
                headerPath.AddArc(headerRect.Right - r * 2, headerRect.Y, r * 2, r * 2, 270, 90);
                headerPath.AddLine(headerRect.Right, headerRect.Bottom, headerRect.X, headerRect.Bottom);
                headerPath.CloseFigure();
                g.FillPath(headerBrush, headerPath);
                headerPath.Dispose();
            }

            using (Pen pen = new Pen(Selected ? Color.FromArgb(0, 120, 215) : BorderColor, BorderWidth / scale))
            {
                g.DrawRoundedRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height, 6f);
                g.DrawLine(pen, rect.X, rect.Y + _headerHeight, rect.Right, rect.Y + _headerHeight);
            }

            DrawHeaderText(g, scale);

            GraphicsState state = g.Save();
            g.Clip = new Region(bodyRect);
            foreach (var child in _children.Where(c => c.Visible).OrderBy(c => c.ZOrder))
            {
                child.Draw(g, scale);
            }
            g.Restore(state);

            DrawSelection(g, scale);
        }

        private void DrawHeaderText(Graphics g, float scale)
        {
            using (Font font = new Font("Microsoft YaHei", 10f / scale, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.White))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                RectangleF textRect = Bounds;
                textRect.Height = _headerHeight;
                textRect.Inflate(-8 / scale, 0);
                g.DrawString(_headerText, font, brush, textRect, sf);
            }
        }

        private GraphicsPath CreateRoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r = Math.Min(radius, Math.Min(rect.Width / 2f, rect.Height / 2f));
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        public override ShapeBase Clone()
        {
            return new ContainerShape
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Bounds = this.Bounds,
                FillColor = this.FillColor,
                BorderColor = this.BorderColor,
                TextColor = this.TextColor,
                BorderWidth = this.BorderWidth,
                HeaderHeight = this.HeaderHeight,
                HeaderColor = this.HeaderColor,
                HeaderText = this.HeaderText
            };
        }
    }

    public static class GraphicsExtensions
    {
        public static void DrawRoundedRectangle(this Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                float r = Math.Min(radius, Math.Min(width / 2f, height / 2f));
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + width - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + width - r * 2, y + height - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + height - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                g.DrawPath(pen, path);
            }
        }
    }
}

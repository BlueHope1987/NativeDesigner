using System;
using System.Drawing;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Shapes
{
    [Serializable]
    public class EllipseShape : ShapeBase
    {
        public EllipseShape()
        {
            Name = "椭圆";
            Bounds = new RectangleF(0, 0, 120, 100);
            FillColor = Color.FromArgb(255, 240, 230);
            BorderColor = Color.FromArgb(200, 130, 60);
        }

        public override void Draw(Graphics g, float scale)
        {
            RectangleF rect = Bounds;

            using (Brush brush = new SolidBrush(FillColor))
            {
                g.FillEllipse(brush, rect);
            }

            using (Pen pen = new Pen(Selected ? Color.FromArgb(0, 120, 215) : BorderColor, BorderWidth / scale))
            {
                g.DrawEllipse(pen, rect);
            }

            DrawText(g, scale);
            DrawSelection(g, scale);
        }

        private void DrawText(Graphics g, float scale)
        {
            if (string.IsNullOrEmpty(Name))
                return;

            using (Font font = new Font("Microsoft YaHei", 10f / scale, FontStyle.Regular))
            using (Brush brush = new SolidBrush(TextColor))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.Trimming = StringTrimming.EllipsisCharacter;

                RectangleF textRect = Bounds;
                textRect.Inflate(-10 / scale, -10 / scale);
                g.DrawString(Name, font, brush, textRect, sf);
            }
        }

        public override PointF GetNearestConnectionPoint(PointF from)
        {
            PointF center = Center;
            float rx = Bounds.Width / 2f;
            float ry = Bounds.Height / 2f;
            float dx = from.X - center.X;
            float dy = from.Y - center.Y;

            if (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f)
                return new PointF(center.X + rx, center.Y);

            float angle = (float)Math.Atan2(dy, dx);
            return new PointF(center.X + rx * (float)Math.Cos(angle), center.Y + ry * (float)Math.Sin(angle));
        }

        public override ShapeBase Clone()
        {
            EllipseShape clone = new EllipseShape();
            clone.Id = Guid.NewGuid();
            clone.Name = this.Name;
            clone.Description = this.Description;
            clone.Bounds = this.Bounds;
            clone.FillColor = this.FillColor;
            clone.BorderColor = this.BorderColor;
            clone.TextColor = this.TextColor;
            clone.BorderWidth = this.BorderWidth;
            return clone;
        }
    }
}

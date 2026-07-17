using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Shapes
{
    [Serializable]
    public class DiamondShape : ShapeBase
    {
        public DiamondShape()
        {
            Name = "菱形";
            Bounds = new RectangleF(0, 0, 120, 100);
            FillColor = Color.FromArgb(230, 255, 240);
            BorderColor = Color.FromArgb(60, 180, 100);
        }

        public override void Draw(Graphics g, float scale)
        {
            RectangleF rect = Bounds;
            PointF[] points = new PointF[]
            {
                new PointF(rect.X + rect.Width / 2f, rect.Y),
                new PointF(rect.Right, rect.Y + rect.Height / 2f),
                new PointF(rect.X + rect.Width / 2f, rect.Bottom),
                new PointF(rect.X, rect.Y + rect.Height / 2f)
            };

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(points);
                using (Brush brush = new SolidBrush(FillColor))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Selected ? Color.FromArgb(0, 120, 215) : BorderColor, BorderWidth / scale))
                {
                    g.DrawPath(pen, path);
                }
            }

            DrawText(g, scale);
            DrawSelection(g, scale);
        }

        private void DrawText(Graphics g, float scale)
        {
            if (string.IsNullOrEmpty(Name)) return;

            using (Font font = new Font("Microsoft YaHei", 10f / scale, FontStyle.Regular))
            using (Brush brush = new SolidBrush(TextColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(Name, font, brush, Center.X, Center.Y, sf);
            }
        }

        public override PointF GetNearestConnectionPoint(PointF from)
        {
            PointF center = Center;
            float dx = from.X - center.X;
            float dy = from.Y - center.Y;
            float halfW = Bounds.Width / 2f;
            float halfH = Bounds.Height / 2f;

            if (Math.Abs(dx) < 0.001f && Math.Abs(dy) < 0.001f)
                return new PointF(center.X + halfW, center.Y);

            float absDx = Math.Abs(dx);
            float absDy = Math.Abs(dy);
            float slopeX = halfH * absDx;
            float slopeY = halfW * absDy;

            if (slopeX > slopeY)
            {
                float scale = halfW / absDx;
                return new PointF(center.X + dx * scale, center.Y + dy * scale);
            }
            else
            {
                float scale = halfH / absDy;
                return new PointF(center.X + dx * scale, center.Y + dy * scale);
            }
        }

        public override ShapeBase Clone()
        {
            return new DiamondShape
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Bounds = this.Bounds,
                FillColor = this.FillColor,
                BorderColor = this.BorderColor,
                TextColor = this.TextColor,
                BorderWidth = this.BorderWidth
            };
        }
    }
}

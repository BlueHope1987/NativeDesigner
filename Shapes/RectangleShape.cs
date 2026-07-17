using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Shapes
{
    [Serializable]
    public class RectangleShape : ShapeBase
    {
        private float _cornerRadius = 6f;

        public float CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; NotifyChanged(); }
        }

        public RectangleShape()
        {
            Name = "矩形";
            Bounds = new RectangleF(0, 0, 140, 90);
            FillColor = Color.FromArgb(230, 245, 255);
            BorderColor = Color.FromArgb(60, 130, 200);
        }

        public override void Draw(Graphics g, float scale)
        {
            RectangleF rect = Bounds;

            using (GraphicsPath path = CreateRoundedRect(rect, _cornerRadius))
            {
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

                RectangleF textRect = Bounds;
                textRect.Inflate(-6 / scale, -6 / scale);
                g.DrawString(Name, font, brush, textRect, sf);
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
            return new RectangleShape
            {
                Id = Guid.NewGuid(),
                Name = this.Name,
                Description = this.Description,
                Bounds = this.Bounds,
                FillColor = this.FillColor,
                BorderColor = this.BorderColor,
                TextColor = this.TextColor,
                BorderWidth = this.BorderWidth,
                CornerRadius = this.CornerRadius
            };
        }
    }
}

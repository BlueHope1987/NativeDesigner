using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CloudNativeDesigner.Core;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Controls
{
    public class ToolboxItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public Bitmap Icon { get; set; }
        public Func<ShapeBase> CreateShape { get; set; }
    }

    public class ToolboxPanel : Panel
    {
        private List<ToolboxItem> _items = new List<ToolboxItem>();
        private ToolboxItem _selectedItem = null;
        private int _itemHeight = 36;
        private int _iconSize = 28;
        private int _padding = 4;

        public ToolboxPanel()
        {
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.AutoScroll = true;
            this.DoubleBuffered = true;

            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            AddItem(new ToolboxItem
            {
                Name = "矩形",
                Category = "基本图形",
                CreateShape = () => new RectangleShape()
            });

            AddItem(new ToolboxItem
            {
                Name = "椭圆",
                Category = "基本图形",
                CreateShape = () => new EllipseShape()
            });

            AddItem(new ToolboxItem
            {
                Name = "菱形",
                Category = "基本图形",
                CreateShape = () => new DiamondShape()
            });

            AddItem(new ToolboxItem
            {
                Name = "容器",
                Category = "容器",
                CreateShape = () => new ContainerShape()
            });
        }

        public void AddItem(ToolboxItem item)
        {
            if (item == null) return;
            item.Icon = item.Icon ?? CreateDefaultIcon(item);
            _items.Add(item);
            Invalidate();
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedItem = null;
            Invalidate();
        }

        public ToolboxItem SelectedItem { get { return _selectedItem; } }

        public event EventHandler<ToolboxItem> ItemSelected;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float y = _padding + AutoScrollPosition.Y;
            string currentCategory = null;

            foreach (var item in _items)
            {
                if (item.Category != currentCategory)
                {
                    currentCategory = item.Category;
                    y += _padding;
                    using (Font font = new Font("Microsoft YaHei", 9f, FontStyle.Bold))
                    using (Brush brush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                    {
                        g.DrawString(currentCategory, font, brush, _padding, y);
                    }
                    y += 20;
                }

                RectangleF itemRect = new RectangleF(_padding, y, ClientSize.Width - _padding * 2, _itemHeight);
                bool isSelected = (_selectedItem == item);
                bool isHovered = itemRect.Contains(PointToClient(Cursor.Position));

                if (isSelected)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(220, 240, 255)))
                    {
                        g.FillRectangle(brush, itemRect);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(0, 120, 215)))
                    {
                        g.DrawRectangle(pen, itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
                    }
                }
                else if (isHovered)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(240, 248, 255)))
                    {
                        g.FillRectangle(brush, itemRect);
                    }
                }

                if (item.Icon != null)
                {
                    g.DrawImage(item.Icon, itemRect.X + 4, itemRect.Y + (_itemHeight - _iconSize) / 2, _iconSize, _iconSize);
                }

                using (Font font = new Font("Microsoft YaHei", 9f, FontStyle.Regular))
                using (Brush brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    RectangleF textRect = itemRect;
                    textRect.X += _iconSize + 10;
                    textRect.Width -= _iconSize + 14;
                    g.DrawString(item.Name, font, brush, textRect, sf);
                }

                y += _itemHeight + 2;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            float y = _padding + AutoScrollPosition.Y;
            string currentCategory = null;

            foreach (var item in _items)
            {
                if (item.Category != currentCategory)
                {
                    currentCategory = item.Category;
                    y += _padding + 20;
                }

                RectangleF itemRect = new RectangleF(_padding, y, ClientSize.Width - _padding * 2, _itemHeight);
                if (itemRect.Contains(e.Location))
                {
                    _selectedItem = item;
                    ItemSelected?.Invoke(this, item);
                    Invalidate();
                    DoDragDrop(item, DragDropEffects.Copy);
                    return;
                }

                y += _itemHeight + 2;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Invalidate();
        }

        private Bitmap CreateDefaultIcon(ToolboxItem item)
        {
            Bitmap bmp = new Bitmap(_iconSize, _iconSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Rectangle rect = new Rectangle(2, 2, _iconSize - 4, _iconSize - 4);

                if (item.CreateShape != null)
                {
                    try
                    {
                        var shape = item.CreateShape();
                        shape.Bounds = new RectangleF(2, 2, _iconSize - 4, _iconSize - 4);
                        shape.Draw(g, 1.0f);
                    }
                    catch
                    {
                        DrawFallbackIcon(g, rect, item.Name);
                    }
                }
                else
                {
                    DrawFallbackIcon(g, rect, item.Name);
                }
            }
            return bmp;
        }

        private void DrawFallbackIcon(Graphics g, Rectangle rect, string name)
        {
            using (Brush brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
            using (Pen pen = new Pen(Color.FromArgb(120, 120, 120)))
            {
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
            }
        }
    }
}

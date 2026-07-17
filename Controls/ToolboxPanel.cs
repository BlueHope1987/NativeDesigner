using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CloudNativeDesigner.Core;

namespace CloudNativeDesigner.Controls
{
    public delegate ShapeBase CreateShapeHandler();

    public class ToolboxItem
    {
        private string _name;
        private string _category;
        private Bitmap _icon;
        private CreateShapeHandler _createShape;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public Bitmap Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public CreateShapeHandler CreateShape
        {
            get { return _createShape; }
            set { _createShape = value; }
        }
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
        }

        public void ReloadFromRegistry()
        {
            _items.Clear();
            _selectedItem = null;

            List<string> categories = ShapeTypeRegistry.Instance.GetCategories();
            foreach (string category in categories)
            {
                List<ShapeType> types = ShapeTypeRegistry.Instance.GetTypesByCategory(category);
                foreach (ShapeType type in types)
                {
                    ToolboxItem item = new ToolboxItem();
                    item.Name = type.Name;
                    item.Category = type.Category;
                    item.CreateShape = new CreateShapeHandler(type.CreateInstance);
                    item.Icon = CreateIconFromType(type);
                    _items.Add(item);
                }
            }
            Invalidate();
        }

        public void AddItem(ToolboxItem item)
        {
            if (item == null)
                return;
            if (item.Icon == null)
                item.Icon = CreateDefaultIcon(item);
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

        public event EventHandler ItemSelected;

        protected virtual void OnItemSelected(ToolboxItem item)
        {
            if (ItemSelected != null)
                ItemSelected(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float y = _padding + AutoScrollPosition.Y;
            string currentCategory = null;

            foreach (ToolboxItem item in _items)
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
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;

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

            if (e.Button != MouseButtons.Left)
                return;

            float y = _padding + AutoScrollPosition.Y;
            string currentCategory = null;

            foreach (ToolboxItem item in _items)
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
                    OnItemSelected(item);
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

        private Bitmap CreateIconFromType(ShapeType shapeType)
        {
            Bitmap bmp = new Bitmap(_iconSize, _iconSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                try
                {
                    ShapeBase shape = shapeType.CreateInstance();
                    shape.Bounds = new RectangleF(2, 2, _iconSize - 4, _iconSize - 4);
                    shape.Draw(g, 1.0f);
                }
                catch
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                    using (Pen pen = new Pen(Color.FromArgb(120, 120, 120)))
                    {
                        g.FillRectangle(brush, 2, 2, _iconSize - 4, _iconSize - 4);
                        g.DrawRectangle(pen, 2, 2, _iconSize - 4, _iconSize - 4);
                    }
                }
            }
            return bmp;
        }

        private Bitmap CreateDefaultIcon(ToolboxItem item)
        {
            Bitmap bmp = new Bitmap(_iconSize, _iconSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Rectangle rect = new Rectangle(2, 2, _iconSize - 4, _iconSize - 4);

                if (item.CreateShape != null)
                {
                    try
                    {
                        ShapeBase shape = item.CreateShape();
                        shape.Bounds = new RectangleF(2, 2, _iconSize - 4, _iconSize - 4);
                        shape.Draw(g, 1.0f);
                    }
                    catch
                    {
                        DrawFallbackIcon(g, rect);
                    }
                }
                else
                {
                    DrawFallbackIcon(g, rect);
                }
            }
            return bmp;
        }

        private void DrawFallbackIcon(Graphics g, Rectangle rect)
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

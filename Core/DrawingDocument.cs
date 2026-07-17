using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CloudNativeDesigner.Core
{
    [Serializable]
    public class DrawingDocument
    {
        private List<ShapeBase> _shapes = new List<ShapeBase>();
        private List<Connection> _connections = new List<Connection>();
        private string _name = "未命名";
        private SizeF _pageSize = new SizeF(2000, 1500);

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public SizeF PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public List<ShapeBase> Shapes { get { return _shapes; } }
        public List<Connection> Connections { get { return _connections; } }

        public void AddShape(ShapeBase shape)
        {
            if (shape == null) return;
            _shapes.Add(shape);
            shape.ZOrder = _shapes.Count;
            OnDocumentChanged(EventArgs.Empty);
        }

        public void RemoveShape(ShapeBase shape)
        {
            if (shape == null) return;

            var toRemove = _connections.Where(c => c.FromShape == shape || c.ToShape == shape).ToList();
            foreach (var conn in toRemove)
                _connections.Remove(conn);

            if (shape.Parent is CloudNativeDesigner.Shapes.ContainerShape container)
            {
                container.RemoveChild(shape);
            }

            foreach (var child in shape is CloudNativeDesigner.Shapes.ContainerShape c ? c.Children.ToList() : new List<ShapeBase>())
            {
                child.Parent = null;
            }

            _shapes.Remove(shape);
            OnDocumentChanged(EventArgs.Empty);
        }

        public void AddConnection(Connection conn)
        {
            if (conn == null) return;
            _connections.Add(conn);
            OnDocumentChanged(EventArgs.Empty);
        }

        public void RemoveConnection(Connection conn)
        {
            if (conn == null) return;
            _connections.Remove(conn);
            OnDocumentChanged(EventArgs.Empty);
        }

        public void ClearSelection()
        {
            foreach (var shape in _shapes)
                shape.Selected = false;
            foreach (var conn in _connections)
                conn.Selected = false;
        }

        public List<ShapeBase> GetSelectedShapes()
        {
            return _shapes.Where(s => s.Selected).ToList();
        }

        public List<Connection> GetSelectedConnections()
        {
            return _connections.Where(c => c.Selected).ToList();
        }

        public void BringToFront(ShapeBase shape)
        {
            if (shape == null || !_shapes.Contains(shape)) return;
            int maxZ = _shapes.Count > 0 ? _shapes.Max(s => s.ZOrder) : 0;
            shape.ZOrder = maxZ + 1;
            SortByZOrder();
            OnDocumentChanged(EventArgs.Empty);
        }

        public void SendToBack(ShapeBase shape)
        {
            if (shape == null || !_shapes.Contains(shape)) return;
            int minZ = _shapes.Count > 0 ? _shapes.Min(s => s.ZOrder) : 0;
            shape.ZOrder = minZ - 1;
            SortByZOrder();
            OnDocumentChanged(EventArgs.Empty);
        }

        private void SortByZOrder()
        {
            _shapes.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
        }

        public ShapeBase HitTestShape(PointF pt)
        {
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].HitTest(pt))
                    return _shapes[i];
            }
            return null;
        }

        public Connection HitTestConnection(PointF pt, float tolerance)
        {
            foreach (var conn in _connections)
            {
                if (conn.HitTest(pt, tolerance))
                    return conn;
            }
            return null;
        }

        public List<ShapeBase> GetShapesInRect(RectangleF rect)
        {
            return _shapes.Where(s => s.HitTest(rect)).ToList();
        }

        public void Clear()
        {
            _shapes.Clear();
            _connections.Clear();
            OnDocumentChanged(EventArgs.Empty);
        }

        public event EventHandler DocumentChanged;
        protected void OnDocumentChanged(EventArgs e)
        {
            DocumentChanged?.Invoke(this, e);
        }
    }
}

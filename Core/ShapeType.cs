using System;
using System.Collections.Generic;
using System.Drawing;
using CloudNativeDesigner.Shapes;

namespace CloudNativeDesigner.Core
{
    [Serializable]
    public class ShapeType
    {
        private string _name = "";
        private string _category = "基本图形";
        private string _description = "";
        private string _iconName = "";
        private List<RenderCommand> _renderCommands = new List<RenderCommand>();
        private bool _isContainer = false;
        private bool _supportsMembers = false;
        private float _defaultWidth = 120f;
        private float _defaultHeight = 80f;
        private XmlColor _defaultFillColor = new XmlColor(Color.FromArgb(230, 240, 255));
        private XmlColor _defaultBorderColor = new XmlColor(Color.FromArgb(80, 120, 180));
        private XmlColor _defaultTextColor = new XmlColor(Color.FromArgb(40, 40, 40));
        private List<ShapeState> _defaultStates = new List<ShapeState>();

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

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string IconName
        {
            get { return _iconName; }
            set { _iconName = value; }
        }

        public List<RenderCommand> RenderCommands
        {
            get { return _renderCommands; }
            set { _renderCommands = value; }
        }

        public bool IsContainer
        {
            get { return _isContainer; }
            set { _isContainer = value; }
        }

        public bool SupportsMembers
        {
            get { return _supportsMembers; }
            set { _supportsMembers = value; }
        }

        public float DefaultWidth
        {
            get { return _defaultWidth; }
            set { _defaultWidth = value; }
        }

        public float DefaultHeight
        {
            get { return _defaultHeight; }
            set { _defaultHeight = value; }
        }

        public XmlColor DefaultFillColor
        {
            get { return _defaultFillColor; }
            set { _defaultFillColor = value; }
        }

        public XmlColor DefaultBorderColor
        {
            get { return _defaultBorderColor; }
            set { _defaultBorderColor = value; }
        }

        public XmlColor DefaultTextColor
        {
            get { return _defaultTextColor; }
            set { _defaultTextColor = value; }
        }

        public List<ShapeState> DefaultStates
        {
            get { return _defaultStates; }
            set { _defaultStates = value; }
        }

        public ShapeType() { }

        public ShapeBase CreateInstance()
        {
            if (_isContainer)
            {
                ContainerShape shape = new ContainerShape();
                shape.Name = _name;
                shape.Bounds = new RectangleF(0, 0, _defaultWidth, _defaultHeight);
                shape.FillColor = _defaultFillColor.ToColor();
                shape.BorderColor = _defaultBorderColor.ToColor();
                shape.TextColor = _defaultTextColor.ToColor();
                return shape;
            }
            else
            {
                GenericShape shape = new GenericShape();
                shape.ShapeTypeName = _name;
                shape.Name = _name;
                shape.Bounds = new RectangleF(0, 0, _defaultWidth, _defaultHeight);
                shape.FillColor = _defaultFillColor.ToColor();
                shape.BorderColor = _defaultBorderColor.ToColor();
                shape.TextColor = _defaultTextColor.ToColor();

                foreach (ShapeState state in _defaultStates)
                {
                    shape.AddState(state);
                }
                if (_defaultStates.Count > 0)
                {
                    shape.CurrentStateName = _defaultStates[0].Name;
                }

                return shape;
            }
        }
    }
}

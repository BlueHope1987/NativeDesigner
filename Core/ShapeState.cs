using System;
using System.Drawing;

namespace CloudNativeDesigner.Core
{
    [Serializable]
    public class ShapeState
    {
        private string _name = "Normal";
        private XmlColor _fillColor = new XmlColor(Color.FromArgb(230, 240, 255));
        private XmlColor _borderColor = new XmlColor(Color.FromArgb(80, 120, 180));
        private XmlColor _textColor = new XmlColor(Color.FromArgb(40, 40, 40));
        private XmlColor _headerColor = new XmlColor(Color.FromArgb(80, 130, 180));
        private int _priority = 0;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public XmlColor FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        public XmlColor BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        public XmlColor TextColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        public XmlColor HeaderColor
        {
            get { return _headerColor; }
            set { _headerColor = value; }
        }

        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public ShapeState() { }

        public ShapeState(string name, Color fill, Color border, Color text)
        {
            _name = name;
            _fillColor = new XmlColor(fill);
            _borderColor = new XmlColor(border);
            _textColor = new XmlColor(text);
        }
    }
}

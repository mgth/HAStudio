using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HAStudio
{
    public delegate void ChangedSelectedWidget(Widget sender, EventArgs e);
    public delegate void ChangedSizePanel(Panel sender, EventArgs e);
    public delegate void ChangedWidgetsPanel(Panel sender, EventArgs e);
    public class Panel
    {
        public event ChangedSelectedWidget SelectionChanged;
        public event ChangedSizePanel SizeChanged;
        public event ChangedWidgetsPanel WidgetsChanged;

        public void OnWidgetSelected(Widget sender, EventArgs e)
        {
            if (SelectionChanged != null) SelectionChanged(sender, e);
        }
        public void OnSizeChanged()
        {
            if (SizeChanged != null) SizeChanged(this, new EventArgs());
        }
        public void OnWidgetsChanged()
        {
            if (WidgetsChanged != null) WidgetsChanged(this, new EventArgs());
        }

        public Panel()
        {
            Widgets.
        }

        [XmlElement("button", typeof(PanelButton))]
        public List<Widget> Widgets = new List<Widget>();

        [XmlAttribute]
        public int _width = 2;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnSizeChanged();
            }
        }
        private int _height = 2;
        [XmlAttribute]
        public int Height
        {
            get  { return _height; }
            set
            {
                _height = value;
                OnSizeChanged();
            }
        }
        public int MaxWidth
        {
            get
            {
                int width = 0;
                foreach (Widget w in Widgets)
                {
                    width = (int)Math.Max(width, w.pos.Right + 1);
                }
                return width;
            }
        }
        public int MaxHeight
        {
            get
            {
                int height = 0;
                foreach (Widget w in Widgets)
                {
                    height = (int)Math.Max(height, w.pos.Bottom + 1);
                }
                return height;
            }
        }

        public void AddRow() { Height++; }
        public void AddColumn() { Width++; }

        public void RemoveRow()
        {
            if (Height <= MaxHeight) return;
            Height--;
        }
        public void RemoveColumn()
        {
            if (Width <= MaxWidth) return;
            Width--;
        }

        public void AddWidget(Widget widget)
        {
            widget.Selected += Widget_Selected;
            Widgets.Add(widget);
            OnWidgetsChanged();
        }

        private void Widget_Selected(Widget sender, EventArgs e)
        {
            OnWidgetSelected(sender, e);
        }
    }
}

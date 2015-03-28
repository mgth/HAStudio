using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace HAStudio
{
    public abstract class Widget : INotifyPropertyChanged
    {
        public event ChangedSelectedWidget Selected;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private UIElement _control;
        [XmlIgnore]
        public UIElement Control
        {
            get {
                if (_control == null) setControl();
                return _control;
            }
            set
            {
                _control = value;
            }
        }

        protected abstract void setControl();


        //        [XmlAttribute]
        [XmlIgnore]
        public Rect pos { get { return new Rect(Left, Top, Width, Height); } set { Left = value.Left; Top = value.Top; Width = value.Width; Height = value.Height; } }
        [XmlAttribute]
        public double Left { get; set; } = 0;
        [XmlAttribute]
        public double Top { get; set; } = 0;
        [XmlAttribute]
        public double Width { get; set; } = 1;
        [XmlAttribute]
        public double Height { get; set; } = 1;

        private String _topic = "";
        [Category("Widget"), XmlAttribute]
        public String Topic
        {
            get { return _topic; }
            set { _topic = value; OnPropertyChanged("Topic"); }
        }

        private String _caption = "";
        [Category("Widget"), XmlAttribute]
        public String Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }


        protected virtual void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        protected bool _selected = false;
        public virtual void Select()
        {
            _selected = true;
        }
        public virtual void Unselect()
        {
            _selected = false;
        }


        public void setPosition(Rect r)
        {
            pos = r;
        }
    }
}

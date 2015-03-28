using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Windows;

namespace HAStudio
{
    public class PanelButton : Widget
    {
        private PanelButtonGui _gui { get { return Control as PanelButtonGui; } }
        [XmlAttribute]
        public String ValueOn { get; set; } = "1";
        [XmlAttribute]
        public String ValueOff { get; set; } = "0";

        public override void Select()
        {
            base.Select();
            if (_icon != null)
                _icon.ClickIcon += _icon_ClickIcon;
        }

        public override void Unselect()
        {
            if (_icon != null)
                _icon.ClickIcon -= _icon_ClickIcon;
            base.Unselect();
        }

        private void _icon_ClickIcon(object sender, int X, int Y)
        {
            IconIndexX = X;
            IconIndexY = Y;
        }

        private Icon _icon = new Icon();
        [Editor(typeof(ImageProperty), typeof(ImageProperty))]
        public Icon Icon
        {
            get { return _icon; }
            set
            {

                if (_icon != null)
                    _icon.ClickIcon -= _icon_ClickIcon;

                _icon = value;

                if (_icon != null)
                {
                    if (_selected)
                        _icon.ClickIcon += _icon_ClickIcon;
                    _icon.BitmapChanged += _icon_BitmapChanged;
                }
            }
        }

        private void _icon_BitmapChanged(Icon sender)
        {
            _gui.Image.Source = sender.RenderedImage(IconIndexX, IconIndexY);
        }


        [XmlAttribute]
        public int IconCountX
        {
            get { return Icon.CountX; }
            set
            {
                Icon.CountX = value;
                OnPropertyChanged("CountX");
            }
        }
        [XmlAttribute]
        public int IconCountY
        {
            get { return Icon.CountY; }
            set
            {
                Icon.CountY = value;
                OnPropertyChanged("CountY");
            }
        }

        private int _iconIndexX = 0;
        [XmlAttribute]
        public int IconIndexX
        {
            get { return _iconIndexX; }
            set
            {
                if (value < Icon.CountX && value >= 0)
                {
                    _iconIndexX = value;
                    _icon_BitmapChanged(Icon);
                }
            }
        }



        private int _iconIndexY = 0;
        [XmlAttribute]
        public int IconIndexY
        {
            get { return _iconIndexY; }
            set
            {
                if (value < Icon.CountY && value >= 0)
                {
                    _iconIndexY = value;
                    _icon_BitmapChanged(Icon);
                }
            }
        }

        public PanelButton()
        {
        }

        protected override void setControl()
        {
            PanelButtonGui b = new PanelButtonGui();
            b.Button.Click += new RoutedEventHandler(PanelButton_click);

            Binding bindingCaption = new Binding("Caption") { Source = this };
            b.Caption.SetBinding(Label.ContentProperty, bindingCaption);

            Control = b;
        }

        Color _color;
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (_gui != null) { _gui.Brush.Color = _color; }
            }
        }

        protected void PanelButton_click(object sender, EventArgs e)
        {
            OnSelected(new EventArgs());
        }

    }
}

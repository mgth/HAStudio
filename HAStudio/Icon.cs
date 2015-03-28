using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace HAStudio
{
    public delegate void IconBitmapChanged(Icon sender);
    public delegate void IconClickEventHandler(Object sender, int X, int Y);
    public class Icon : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event IconClickEventHandler ClickIcon;
        public void OnClickIcon(int X, int Y)
        {
            if (ClickIcon != null)
            {
                ClickIcon(this, X, Y);
            }
        }

        public event IconBitmapChanged BitmapChanged;
        protected void OnBitmapChanged()
        {
            if (BitmapChanged != null)
            {
                BitmapChanged(this);
            }
        }


        private int _countX = 1;
        [XmlAttribute]
        public int CountX { get { return _countX; }
            set {
                _countX =value;
                OnPropertyChanged("CountX");
            }
        }
        private int _countY = 1;
        [XmlAttribute]
        public int CountY { get { return _countY; }
            set {
                    _countY = value;
                OnPropertyChanged("CountY");
            }
        }

        private BitmapSource _bitmap;
        [XmlIgnore]
        public BitmapSource Bitmap
        {
            get { return _bitmap; }
            set
            {

                FormatConvertedBitmap img = new FormatConvertedBitmap();
                {
                    img.BeginInit();
                    img.Source = value;
                    img.DestinationFormat = PixelFormats.Bgra32;
                    img.EndInit();

                    _bitmap = img;
                }

                OnBitmapChanged();
                OnPropertyChanged("Bitmap");
            }
        }

        [XmlElement]
        public String Base64
        {
            get
            {
                BitmapSource img = _bitmap;
                if (img != null)
                {
                    int stride = img.PixelWidth * 4;
                    int size = img.PixelHeight * stride;
                    byte[] pixels = new byte[size];
                    img.CopyPixels(pixels, stride, 0);

                    MemoryStream uncompressed = new MemoryStream(pixels);

                    using (var compressed = new MemoryStream())
                    {
                        GZipStream gz = new GZipStream(compressed, CompressionMode.Compress);
                        uncompressed.CopyTo(gz);
                        return Convert.ToBase64String(compressed.ToArray());
                    }
                }
                return null;
            }
            set
            {

            }
        }

        public BitmapSource RenderedImage(int X, int Y)
        {
                if (_bitmap == null) return null;
                CroppedBitmap img = new CroppedBitmap(
                    _bitmap,
                    new Int32Rect(
                        (_bitmap.PixelWidth / CountX) * X,
                        (_bitmap.PixelHeight / CountY) * Y,
                        _bitmap.PixelWidth / CountX,
                        _bitmap.PixelHeight / CountY
                        ));

            BitmapEditor be = new BitmapEditor(img);
            be.SetTransparency(be.SearchTransparency());
            return be.Bitmap;
        }

    }
}

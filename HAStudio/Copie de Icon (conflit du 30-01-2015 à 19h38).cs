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
                _bitmap = value;

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
                CroppedBitmap cb = new CroppedBitmap(
                    _bitmap,
                    new Int32Rect(
                        (_bitmap.PixelWidth / CountX) * X,
                        (_bitmap.PixelHeight / CountY) * Y,
                        _bitmap.PixelWidth / CountX,
                        _bitmap.PixelHeight / CountY
                        ));

                FormatConvertedBitmap img = new FormatConvertedBitmap();
                img.BeginInit();
                img.Source = cb;
                img.DestinationFormat = PixelFormats.Bgra32;
                img.EndInit();

                int stride = img.PixelWidth * 4;
                int size = img.PixelHeight * stride;
                byte[] pixels = new byte[size];
                img.CopyPixels(pixels, stride, 0);

                for (int y = 0; y < img.PixelHeight; y++)
                    for (int x = 0; x < img.PixelWidth; x++)
                    {
                        int index = y * stride + 4 * x;

                        byte red = pixels[index];
                        byte green = pixels[index + 1];
                        byte blue = pixels[index + 2];
                        if (red == 255 && green == 255 && blue == 255)
                            pixels[index + 3] = 0;
                    }

                return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, img.Format, img.Palette, pixels, stride);
        }

    }
}

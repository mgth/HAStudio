using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
namespace HAStudio
{
    public class BitmapEditor
    {
        private byte[] _pixels;
        private int _stride;
        private int _width, _height;
        private int _dpiX, _dpiY;
        public BitmapEditor(BitmapSource bitmap)
        {
            _stride = (_width = bitmap.PixelWidth) * 4;
            int size = (_height = bitmap.PixelHeight) * _stride;
            _pixels = new byte[size];

            FormatConvertedBitmap img = new FormatConvertedBitmap();
            {
                img.BeginInit();
                img.Source = bitmap;
                img.DestinationFormat = PixelFormats.Bgra32;
                img.EndInit();
                img.CopyPixels(_pixels, _stride, 0);
            }
        }

        public BitmapSource Bitmap
        {
            get
            {
                return BitmapSource.Create(_width, _height, _dpiX, _dpiY, PixelFormats.Bgra32, null, _pixels, _stride);
            }
        }

        private int index(int X, int Y) { return Y * _stride + 4 * X; } 
        public void SetPixel(int X, int Y, Color c)
        {
            int i = index(X, Y);
            _pixels[i++] = c.B;
            _pixels[i++] = c.G;
            _pixels[i++] = c.R;
            _pixels[i++] = c.A;
        }

        public Color GetPixel(int X, int Y)
        {
            int i = index(X, Y);
            return new Color
            {
                B = _pixels[i],
                G = _pixels[i+1],
                R = _pixels[i+2],
                A = _pixels[i+3]
            };
        }

        public void SetTransparency(Color t)
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                {
                    Color c = GetPixel(x, y);

                    if (c.B == t.B && c.G == t.G && c.R == t.R)
                    {
                        c.A = 0;
                        SetPixel(x, y, c);
                    }
                }

        }

        private void pushColor(Dictionary<Color,int> colors, Color c)
        {
                if (colors.ContainsKey(c)) colors[c]++;
                else colors.Add(c, 1);
        }
        public Color SearchTransparency()
        {
            Dictionary<Color,int> colors =new Dictionary<Color, int>();
            for (int X = 0; X < _width;X++)
            {
                pushColor(colors,GetPixel(X, 0));
                pushColor(colors, GetPixel(X, _height-1));
            }
            for (int Y = 0; Y < _height; Y++)
            {
                pushColor(colors, GetPixel(0, Y));
                pushColor(colors, GetPixel(_width - 1, Y));
            }
            int max = 0;
            Color maxc = new Color();
            foreach(Color c in colors.Keys)
            {
                if (colors[c]>max)
                {
                    max = colors[c];
                    maxc = c;
                }
            }

            return maxc;
        }
    }
}

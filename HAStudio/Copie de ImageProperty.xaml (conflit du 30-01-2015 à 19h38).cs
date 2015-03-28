using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace HAStudio
{
    /// <summary>
    /// Interaction logic for ImageProperty.xaml
    /// </summary>
    public partial class ImageProperty : UserControl, ITypeEditor//, INotifyPropertyChanged
    {
        public ImageProperty()
        {
            InitializeComponent();
        }
//        public static DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(BitmapImage), typeof(ImageProperty),
//                                                                                                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private Icon _icon;

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            if (propertyItem.Value!=null)
            {
                _icon = propertyItem.Value as Icon;
            }
            else
            {
                _icon = new Icon();
                propertyItem.Value = _icon; 
            }
            if (_icon == null) throw new Exception("Is not an Icon");

            image.Source = _icon.Bitmap;
            return this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void cmdBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openfile = new Microsoft.Win32.OpenFileDialog();
            openfile.DefaultExt = "*";
            openfile.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.ico";
            Nullable<bool> result = openfile.ShowDialog();
            if (result == true)
            {
                    image.Source = _icon.Bitmap = new BitmapImage(new Uri(openfile.FileName));
            }

        }
        private readonly MouseHookListener _MouseHookManager = new MouseHookListener(new GlobalHooker());
        private void cmdScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _MouseHookManager.Enabled = true ;
            _MouseHookManager.MouseDownExt += HookManager_MouseDownExt;
            //_MouseHookManager.MouseMoveExt += HookManager_MouseMoveExt;

        }


        private Point StartScreenshot;
        private Point StopScreenshot;

        private Window selArea;

        private void HookManager_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            StartScreenshot = new Point(e.X,e.Y);
            _MouseHookManager.MouseDownExt -= HookManager_MouseDownExt;
            _MouseHookManager.MouseDownExt += HookManager_MouseDown2Ext;
            _MouseHookManager.MouseMoveExt += HookManager_MouseMoveExt;

            e.Handled = true;
        }

        private void HookManager_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            if (selArea==null)
            {
                selArea = new Window() {
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    Background = new SolidColorBrush { Opacity = 0 },
                    Content = new Border { BorderThickness = new Thickness(2), BorderBrush = Brushes.Blue }
                };
            }

            //e.Handled = true;
            Rect r = new Rect(StartScreenshot, new Point(e.X, e.Y));
            selArea.Left = r.Left;
            selArea.Width = Math.Max(1, r.Width);
            selArea.Top = r.Top;
            selArea.Height = Math.Max(1, r.Height);

            selArea.Show();
        }

        private void HookManager_MouseDown2Ext(object sender, MouseEventExtArgs e)
        {
            e.Handled = true;
            selArea.Close();
            selArea = null;
            //if (e.IsMouseKeyDown) return;

            StopScreenshot = new Point(e.X, e.Y);
            _MouseHookManager.MouseDownExt -= HookManager_MouseDown2Ext;
            _MouseHookManager.MouseMoveExt -= HookManager_MouseMoveExt;
            _MouseHookManager.Enabled = false;
            _icon.Bitmap = Screenshot(new Rect(StartScreenshot, StopScreenshot));
        }

        public BitmapSource Screenshot(Rect area)
        {
            using (var screenBmp = new System.Drawing.Bitmap( (int)area.Width, (int)area.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen((int)area.TopLeft.X, (int)area.TopLeft.Y, 0, 0, screenBmp.Size);


                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }


            /*            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)area.Width, (int)area.Height);
                        System.Drawing.Graphics gr1 = System.Drawing.Graphics.FromImage(bitmap);
                        IntPtr dc1 = gr1.GetHdc();
                        IntPtr dc2 = NativeMethods.GetWindowDC(NativeMethods.GetForegroundWindow());
                        NativeMethods.BitBlt(dc1, (int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, dc2, 0, 0, 13369376);
                        gr1.ReleaseHdc(dc1);

                        using (MemoryStream memory = new MemoryStream())
                        {
                            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            memory.Position = 0;
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            return bitmapImage;
                        }*/
        }
    }

    internal class NativeMethods
    {
        [DllImport("user32.dll")]
        public extern static IntPtr GetDesktopWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern UInt64 BitBlt
             (IntPtr hDestDC,
             int x,
             int y,
             int nWidth,
             int nHeight,
             IntPtr hSrcDC,
             int xSrc,
             int ySrc,
             System.Int32 dwRop);
    }


}

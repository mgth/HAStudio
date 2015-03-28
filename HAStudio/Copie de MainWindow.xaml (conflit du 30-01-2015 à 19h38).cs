using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using System.Xml;
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Xml.Schema;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace HAStudio
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)]
        public UInt32 ColorBGRA;
        // 8 bit components
        [FieldOffset(0)]
        public byte Blue;
        [FieldOffset(1)]
        public byte Green;
        [FieldOffset(2)]
        public byte Red;
        [FieldOffset(3)]
        public byte Alpha;
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Panel panel = new Panel();
        public MainWindow()
        {
            InitializeComponent();
            panel.SelectionChanged += Panel_SelectionChanged;
        }

        private void Panel_SelectionChanged(Widget sender, EventArgs e)
        {
            setSelection(sender);
        }

        public void setGrid(int x, int y)
        {
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < x; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Clear();
            for (int i = 0; i < y; i++) grid.RowDefinitions.Add(new RowDefinition());
        }

        private void cmdAddColumn_Click(object sender, RoutedEventArgs e)
        {
            panel.AddColumn();
            setGrid(panel.Width, panel.Height);
        }

        private void cmdRemoveCollumn_Click(object sender, RoutedEventArgs e)
        {
            panel.RemoveColumn();
            setGrid(panel.Width, panel.Height);
        }

        private void cmdAddRow_Click(object sender, RoutedEventArgs e)
        {
            panel.AddRow();
            setGrid(panel.Width, panel.Height);
        }

        private void cmdRemoveRow_Click(object sender, RoutedEventArgs e)
        {
            panel.RemoveRow();
            setGrid(panel.Width, panel.Height);
        }

        private Point _selectStart = new Point(0, 0);
        private Point _selectEnd = new Point(0, 0);

        public Point selectStart { get { return _selectStart; }
            set {
                _selectStart = value;
                setSelection();
            }
        }
        public Point selectEnd
        {
            get { return _selectEnd; }
            set
            {
                _selectEnd = value;
                setSelection();
            }
        }

        public Rect selection
        {
            get
            {
                return new Rect(selectStart, selectEnd);
            }
            set
            {
                selectStart = value.TopLeft;
                selectEnd = value.BottomRight;
                setSelection();
            }
            
         }

        private void setSelection()
        {
            Rect s = selection;

            Grid.SetColumn(border, (int)s.Left);
            Grid.SetRow(border, (int)s.Top);
            Grid.SetColumnSpan(border, (int)s.Width+1);
            Grid.SetRowSpan(border, (int)s.Height+1);
        }

        private Point CellInGrid(Grid grid,Point point)
        {
            return new Point(
                Math.Truncate(point.X / (grid.ActualWidth / panel.Width)),
                Math.Truncate(point.Y / (grid.ActualHeight / panel.Height))
                );
        }

        private void grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //           Grid grid = sender as Grid;
            selection = new Rect(CellInGrid(grid , e.GetPosition(grid)),new Size(0,0));
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selectEnd = CellInGrid(grid, e.GetPosition(grid));
        }

        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;

            Point p = e.GetPosition(grid);
            selectEnd = CellInGrid(grid, e.GetPosition(grid));
        }

        private void cmdAddButton_Click(object sender, RoutedEventArgs e)
        {
            PanelButton pb = new PanelButton();
            pb.setPosition(selection);
            panel.AddWidget(pb);
            panel.setControls(grid);
            setSelection(pb);
        }

        private void setSelection(Widget w)
        {
            PropertyPanel.SelectedObject = w;
            selection = w.pos;
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "XML documents (.xml)|*.xml"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                XmlSerializer xs = new XmlSerializer(typeof(Panel));
                FileStream file = new FileStream(_filename, FileMode.Open);
                file.Position = 0;
                panel = (Panel)xs.Deserialize(file);
                file.Close();
            }
        }

        private String _filename = null;
        private void SaveAsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = "XML documents (.xml)|*.xml"
            };
 
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                _filename = dlg.FileName;
                SaveCmdExecuted(sender, e);
            }
        }

        private void SaveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (_filename == null)
                SaveAsCmdExecuted(sender, e);
            else
            {
                XmlSerializer xs = new XmlSerializer(panel.GetType());
                FileStream file = new FileStream(_filename,FileMode.Create);
                xs.Serialize(file, panel);
                file.Close();
            }
        }

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }

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

        protected UIElement _control;


        //        [XmlAttribute]
        [XmlIgnore]
        public Rect pos { get { return new Rect(Left, Top, Width, Height); } set { Left = value.Left; Top = value.Top; Width = value.Width; Height = value.Height; } }
        [XmlAttribute]
        public double Left { get; set; }=0;
        [XmlAttribute]
        public double Top { get; set; }=0;
        [XmlAttribute]
        public double Width { get; set; }=1;
        [XmlAttribute]
        public double Height { get; set; }=1;

        private String _topic="";
        [Category("Widget"), XmlAttribute]
        public String Topic
        {
            get { return _topic; }
            set { _topic = value; OnPropertyChanged("Topic"); }
        }

        private String _caption="";
        [Category("Widget"), XmlAttribute]
        public String Caption
        {
            get { return _caption; }
            set {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }

        protected virtual void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public void setPosition(Rect r)
        {
            pos = r;
        }
        public void setControl(Grid grid)
        {
            Grid.SetColumn(_control, (int)pos.X);
            Grid.SetRow(_control, (int)pos.Y);
            Grid.SetColumnSpan(_control, (int)pos.Width+1);
            Grid.SetRowSpan(_control, (int)pos.Height+1);

            if (grid.Children.Contains(_control)) return;
            grid.Children.Add(_control);
        }
    }

    public class PanelButton : Widget
    {
        private PanelButtonGui _gui { get { return _control as PanelButtonGui; } }
        [XmlAttribute]
        public String ValueOn { get; set; } = "1";
        [XmlAttribute]
        public String ValueOff { get; set; }  = "0";

        private Icon _icon = new Icon();
        [Editor(typeof(ImageProperty), typeof(ImageProperty))]
        public Icon Icon { get { return _icon; }
            set {
                _icon = value;
                Binding binding = new Binding
                {
                    Path = new PropertyPath("_gui.Image.Source"),
                    Mode = BindingMode.OneWay,
                    Source = _icon
                 };
            }
        }

        private void _icon_BitmapChanged(Icon sender)
        {
            _gui.Image.Source = Icon.RenderedImage(IconIndexX,IconIndexY);
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
        public int IconIndexX { get { return _iconIndexX; }
            set {
                if (value< Icon.CountX && value >=0)
                {
                    _iconIndexX = value;
                    _gui.Image.Source = Icon.RenderedImage(IconIndexX,IconIndexY);
                }
            }
        }



        private int _iconIndexY = 0;
        [XmlAttribute]
        public int IconIndexY { get { return _iconIndexY; }
            set {
                if (value < Icon.CountY && value >= 0)
                {
                    _iconIndexY = value;
                    _gui.Image.Source = Icon.RenderedImage(IconIndexX, IconIndexY);
                }
            }
        }

        public PanelButton()
        {
            PanelButtonGui b = new PanelButtonGui();
            b.Button.Click += new RoutedEventHandler(PanelButton_click);

            Binding bindingCaption = new Binding("Caption") { Source = this };
            Label caption = new Label();
            caption.SetBinding(Label.ContentProperty, bindingCaption);

            Binding bindingTopic = new Binding("Topic") { Source = this };
            Label topic = new Label();
            topic.SetBinding(Label.ContentProperty, bindingTopic);

            _control = b;
        }

        protected void PanelButton_click(object sender, EventArgs e)
        {
            OnSelected(new EventArgs());
        }

    }
    /*
        public class ToggleButon : PanelButton
        {

        }

        public class Slider : Widget
        {
            public int minValue = 0;
            public int maxValue = 100;
            public int currentValue = 0;
        }

        public class ColorPicker : Widget
        {

        }
        */
    public delegate void ChangedSelectedWidget(Widget sender, EventArgs e);
    public class Panel 
    {
        public event ChangedSelectedWidget SelectionChanged;

        [XmlElement("button", typeof(PanelButton))]
        public List<Widget> Widgets = new List<Widget>();

        [XmlAttribute]
        public int Width = 2;
        [XmlAttribute]
        public int Height = 2;
        public int MaxWidth
        {
            get
            {
                int width = 0;
                foreach (Widget w in Widgets)
                {
                    width = (int)Math.Max(width, w.pos.Right+1);
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
                    height = (int)Math.Max(height, w.pos.Bottom+1);
                }
                return height;
            }
        }

        public void AddRow() { Height++; }
        public void AddColumn() { Width++; }

        public void RemoveRow() {
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
        }

        private void Widget_Selected(Widget sender, EventArgs e)
        {
            if(SelectionChanged!=null) SelectionChanged(sender,e);
        }

        public void setControls(Grid grid)
        {
            foreach(Widget w in Widgets)
            {
                w.setControl(grid);
            }
        }
    }

}


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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Panel = new Panel();
            InitializeComponent();
        }

        private Panel _panel;
        public Panel Panel
        {
            get { return _panel; }
            set {
                _panel = value;
                _panel.SelectionChanged += Panel_SelectionChanged;
                _panel.SizeChanged += Panel_SizeChanged;
                _panel.WidgetsChanged += Panel_WidgetsChanged;
            }
        }

        private void Panel_WidgetsChanged(Panel sender, EventArgs e)
        {
            grid.Children.Clear();
            foreach (Widget w in sender.Widgets)
            {
                Grid.SetColumn(w.Control, (int)w.pos.X);
                Grid.SetRow(w.Control, (int)w.pos.Y);
                Grid.SetColumnSpan(w.Control, (int)w.pos.Width + 1);
                Grid.SetRowSpan(w.Control, (int)w.pos.Height + 1);

                if (grid.Children.Contains(w.Control)) return;
                grid.Children.Add(w.Control);
            }
        }

        private void Panel_SizeChanged(Panel sender, EventArgs e)
        {
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < sender.Width; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Clear();
            for (int i = 0; i < sender.Height; i++) grid.RowDefinitions.Add(new RowDefinition());
        }
        private void Panel_SelectionChanged(Widget sender, EventArgs e)
        {
            setSelection(sender);
        }


        //

        private void cmdAddColumn_Click(object sender, RoutedEventArgs e) { Panel.AddColumn(); }
        private void cmdRemoveCollumn_Click(object sender, RoutedEventArgs e){ Panel.RemoveColumn();}
        private void cmdAddRow_Click(object sender, RoutedEventArgs e) { Panel.AddRow(); }
        private void cmdRemoveRow_Click(object sender, RoutedEventArgs e) { Panel.RemoveRow(); }

        // 

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

            if (!grid.Children.Contains(border)) grid.Children.Add(border);

            Grid.SetColumn(border, (int)s.Left);
            Grid.SetRow(border, (int)s.Top);
            Grid.SetColumnSpan(border, (int)s.Width+1);
            Grid.SetRowSpan(border, (int)s.Height+1);
        }

        private Point CellInGrid(Grid grid,Point point)
        {
            return new Point(
                Math.Truncate(point.X / (grid.ActualWidth / _panel.Width)),
                Math.Truncate(point.Y / (grid.ActualHeight / _panel.Height))
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
            _panel.AddWidget(pb);
            setSelection(pb);
        }

        private void setSelection(Widget w)
        {
            Widget oldWidget = PropertyPanel.SelectedObject as Widget;
            if (oldWidget != null)
            {
                oldWidget.Unselect();
            }
            PropertyPanel.SelectedObject = w;
            w.Select();
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
                _filename = dlg.FileName;

                XmlSerializer xs = new XmlSerializer(typeof(Panel));
                FileStream file = new FileStream(_filename, FileMode.Open);
                file.Position = 0;
                Panel = (Panel)xs.Deserialize(file);
                Panel.OnSizeChanged();
                Panel.OnWidgetsChanged();

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
                XmlSerializer xs = new XmlSerializer(Panel.GetType());
                FileStream file = new FileStream(_filename,FileMode.Create);
                xs.Serialize(file, Panel);
                file.Close();
            }
        }

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
    }

}


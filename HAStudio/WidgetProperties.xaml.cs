using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace HAStudio
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class WidgetPropertiesWindow : Window
    {
        public WidgetPropertiesWindow()
        {
            InitializeComponent();
        }
/*
        public void AddProperty(String caption, UIElement ui)
        {
            Label l = new Label { Content = caption };

            int pos = GridProperties.RowDefinitions.Count();
            GridProperties.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

            Grid.SetRow(l, pos);
            Grid.SetRow(ui, pos);
            Grid.SetColumn(l, 0);
            Grid.SetColumn(ui, 1);

            GridProperties.Children.Add(l);
            GridProperties.Children.Add(ui);
        }
        */
        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

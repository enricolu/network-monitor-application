using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NetworkMonitorApplication
{
    /// <summary>
    /// Interaction logic for TrafficGadget.xaml
    /// </summary>
    public partial class TrafficGadget : Window
    {
        public TrafficGadget(NetworkMonitor.NetworkMonitor monitor)
        {
            InitializeComponent();

            this.Left = SystemParameters.PrimaryScreenWidth - this.Width;
            this.Top = SystemParameters.PrimaryScreenHeight - (this.Height + 50);

            this.Monitor = monitor;
            this.DataContext = monitor;
        }

        public NetworkMonitor.NetworkMonitor Monitor { get; set; }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

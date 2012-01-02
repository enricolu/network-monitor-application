using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NetworkMonitor;

namespace NetworkMonitorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private NetworkMonitor.NetworkMonitor monitor;
		
        public MainWindow()
        {
            InitializeComponent();
			monitor = new NetworkMonitor.NetworkMonitor();
			monitor.PacketReceived += new PacketReceivedEventHandler(monitor_PacketReceived);
            dataGridPackets.ItemsSource = monitor.Packets;
            statusBar.DataContext = monitor;

            Thread t = new Thread(() =>
                {
                    monitor.StartListening();                  
                });

            t.Start();
        }
		
		private void monitor_PacketReceived(object sender, Packet p)
		{
            DispatcherOperation disOp =
                dataGridPackets.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal, new Action(() =>
                        {
                            dataGridPackets.Items.Refresh();
                        }));

            Thread.Sleep(10);
		}
    }
}

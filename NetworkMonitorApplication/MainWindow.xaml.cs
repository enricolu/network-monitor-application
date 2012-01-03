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
        private bool isActivated = false;
		
        public MainWindow()
        {
            InitializeComponent();
			monitor = new NetworkMonitor.NetworkMonitor();
			monitor.PacketReceived += new PacketReceivedEventHandler(monitor_PacketReceived);
            dataGridPackets.ItemsSource = monitor.Packets;
            statusBar.DataContext = monitor;
        }
		
		private void monitor_PacketReceived(object sender, Packet p)
		{
            if (isActivated)
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(() =>
            {
                monitor.StartListening();
            });

            t.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            monitor.PauseListening();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if(!isActivated)
            {
                isActivated = true;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            isActivated = false;
        }
    }
}

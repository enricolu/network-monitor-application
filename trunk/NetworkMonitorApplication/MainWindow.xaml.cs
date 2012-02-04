using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.Common;
using NetworkMonitor;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Image = System.Drawing.Image;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace NetworkMonitorApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private NetworkMonitor.NetworkMonitor monitor;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private BackgroundWorker worker = new BackgroundWorker();
		
        public MainWindow()
        {
            InitializeComponent();
			monitor = new NetworkMonitor.NetworkMonitor();
            dataGridPackets.ItemsSource = monitor.Packets;
            statusBar.DataContext = monitor;

            InitializeIcon();
        }

        private void InitializeIcon()
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.Icon = new System.Drawing.Icon("Images/network.ico");
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new MouseEventHandler(notifyIcon_MouseDoubleClick);

            ContextMenuStrip iconMenu = new ContextMenuStrip();
            iconMenu.Items.Add(new ToolStripMenuItem("Start", new System.Drawing.Bitmap("Images/Play.png"),
                                                     (sender, e) => { btnStart_Click(this, null); }));
            iconMenu.Items.Add(new ToolStripMenuItem("Pause", new System.Drawing.Bitmap("Images/Pause.png"), 
                                                     (sender, e) => { btnPause_Click(this, null); }));
            iconMenu.Items.Add(new ToolStripMenuItem("Exit", new System.Drawing.Bitmap("Images/Exit.png"), 
                                                     (sender, e) => { Close(); }));

            this.notifyIcon.ContextMenuStrip = iconMenu;
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            worker.DoWork += new DoWorkEventHandler((s, a) =>
            {
                monitor.StartListening();
            });

            worker.RunWorkerAsync();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += new EventHandler((s, a) =>
                {
                    dataGridPackets.Items.Refresh();
                });
            timer.Start();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            worker.WorkerSupportsCancellation = true;
            worker.CancelAsync();
            worker.Dispose();
            monitor.PauseListening();
        }

        private void dataGridPackets_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Packet selectedPacket = this.monitor.Packets[this.dataGridPackets.SelectedIndex];
            panelPacketInformation.DataContext = selectedPacket;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            this.monitor.Filter.Protocol = (IpProtocol)comboProtocols.SelectedValue;
            this.monitor.Filter.Direction = (PacketDirection) comboDirection.SelectedValue;
            if(!string.IsNullOrEmpty(this.txtHost.Text))
            {
                this.monitor.Filter.Host = this.txtHost.Text;
            }

            this.filteringProgress.Visibility = Visibility.Visible;
            this.filteringProgress.IsIndeterminate = true;
            BackgroundWorker filterWorker = new BackgroundWorker();
            filterWorker.DoWork += new DoWorkEventHandler((a, b) =>
            {
                this.monitor.FilterPackets();
                this.filteringProgress.Value++;
            });
            
            filterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((a, b) =>
            {
                this.dataGridPackets.ItemsSource = this.monitor.Packets;
                this.lblFoundPackets.Content = this.monitor.Packets.Count.ToString();
                this.filteringProgress.IsIndeterminate = false;
                this.filteringProgress.Visibility = Visibility.Collapsed;
            });
            
            filterWorker.RunWorkerAsync();
        }

        private void btnShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            BackgroundWorker getStatsWorker = new BackgroundWorker();
            byte[] minutes = null;
            double[] traffic = null;
            getStatsWorker.DoWork += new DoWorkEventHandler((a, b) =>
            {
                TrafficStatistics statistics = new TrafficStatistics(monitor.DeserializeAllPackets());
                minutes = statistics.GetDownloadedByMinutes().Keys.ToArray();
                traffic = statistics.GetDownloadedByMinutes().Values.Select(x => (double)x).ToArray();
            });

            getStatsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((a, b) =>
            {
                ObservableDataSource<byte> minutesSource = new ObservableDataSource<byte>(minutes);
                minutesSource.SetXMapping(x => x);
                ObservableDataSource<double> trafficSource = new ObservableDataSource<double>(traffic);
                trafficSource.SetYMapping(y => y);

                plotter.Visible = new DataRect(-1, -1, 62, 10000000);

                CompositeDataSource compositeDataSource = new CompositeDataSource(minutesSource, trafficSource);
                this.plotter.AddLineGraph(compositeDataSource, new Pen(Brushes.Blue, 2),
                                          new CirclePointMarker { Size = 1.0, Fill = Brushes.Red },
                                          new PenDescription("Downloaded"));
                plotter.Children.RemoveAll<IPlotterElement>(plotter.MouseNavigation.GetType());
                plotter.Children.RemoveAll<IPlotterElement>(plotter.KeyboardNavigation.GetType());
                //plotter.Viewport.FitToView();                                                                                                                                     
            });

            getStatsWorker.RunWorkerAsync();
        }
    }
}

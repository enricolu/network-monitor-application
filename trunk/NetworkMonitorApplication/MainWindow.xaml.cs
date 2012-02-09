using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
//using Microsoft.Research.DynamicDataDisplay;
//using Microsoft.Research.DynamicDataDisplay.DataSources;
//using Microsoft.Research.DynamicDataDisplay.PointMarkers;

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
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 20 });

			monitor = new NetworkMonitor.NetworkMonitor();
            dataGridPackets.ItemsSource = monitor.Packets;
            statusBar.DataContext = monitor;

            this.columnChart.Series.Clear();
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

            this.progressBarFiltering.Visibility = Visibility.Visible;

            BackgroundWorker filterWorker = new BackgroundWorker();
            filterWorker.DoWork += new DoWorkEventHandler((a, b) =>
            {
                this.monitor.FilterPackets();
            });
            
            filterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((a, b) =>
            {
                this.dataGridPackets.ItemsSource = this.monitor.Packets;
                this.lblFoundPackets.Content = this.monitor.Packets.Count.ToString();
                this.progressBarFiltering.Visibility = Visibility.Collapsed;
            });
            
            filterWorker.RunWorkerAsync();
        }

        private void btnShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            this.progressBarStatistics.Visibility = System.Windows.Visibility.Visible;
            BackgroundWorker getStatsWorker = new BackgroundWorker();

            ObservableCollection<KeyValuePair<byte, ulong>> downloaded = new ObservableCollection<KeyValuePair<byte, ulong>>();
            Series downSeries = new LineSeries()
            {
                Title = "Downloaded",
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = downloaded,
                DataPointStyle = (Style)FindResource("downLine")
            };
            ObservableCollection<KeyValuePair<byte, ulong>> uploaded = new ObservableCollection<KeyValuePair<byte, ulong>>();
            Series upSeries = new LineSeries()
            {
                Title = "Uploaded",
                DependentValuePath = "Value",
                IndependentValuePath = "Key",
                ItemsSource = uploaded,
                DataPointStyle = (Style)FindResource("upLine")
            };   

            this.columnChart.Series.Add(downSeries);
            this.columnChart.Series.Add(upSeries);                 

            getStatsWorker.DoWork += new DoWorkEventHandler((a, b) =>
            {
                List<Packet> allPackets = monitor.DeserializeAllPackets();
                TrafficStatistics statistics = new TrafficStatistics(allPackets);

                var downloadedByMinutes = statistics.GetDownloadedByMinutes();
                var uploadedByMinutes = statistics.GetUploadedByMinutes();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (var key in downloadedByMinutes.Keys)
                    {
                        downloaded.Add(new KeyValuePair<byte, ulong>(key, downloadedByMinutes[key]));
                    }

                    foreach (var key in uploadedByMinutes.Keys)
                    {
                        uploaded.Add(new KeyValuePair<byte, ulong>(key, uploadedByMinutes[key]));
                    }
                }));
                
                
                allPackets.Clear();
                allPackets = null;
            });

            getStatsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((a, b) =>
            {
                this.progressBarStatistics.Visibility =System.Windows.Visibility.Collapsed;
            });

            getStatsWorker.RunWorkerAsync();
        }
    }
}

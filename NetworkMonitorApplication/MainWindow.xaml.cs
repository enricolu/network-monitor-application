using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
using NetworkMonitor;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Image = System.Drawing.Image;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;

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
            iconMenu.Items.Add(new ToolStripMenuItem("Start", new Bitmap("Images/Play.png"),
                                                     (sender, e) => { btnStart_Click(this, null); }));
            iconMenu.Items.Add(new ToolStripMenuItem("Pause", new Bitmap("Images/Pause.png"), 
                                                     (sender, e) => { btnPause_Click(this, null); }));
            iconMenu.Items.Add(new ToolStripMenuItem("Exit", new Bitmap("Images/Exit.png"), 
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
            int packetCount = monitor.DeserializeAllPackets().Count;
            MessageBox.Show(packetCount.ToString());
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

            this.monitor.FilterPackets();
        }
    }
}

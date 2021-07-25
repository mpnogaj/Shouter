using Radio.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using S = Radio.Properties.Settings;

namespace Radio.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;
        private bool _force = false;
        EventHandler onShow;
        EventHandler onClose;
        private NotifyIcon icon;

        public MainWindow()
        {
            InitializeComponent();
            onShow += (sender, args) => ShowWindow();
            onClose += (sender, args) => ForceClose();
            WindowState = S.Default.Maximized ? WindowState.Maximized : WindowState.Normal;
            Left = S.Default.Position.X;
            Top = S.Default.Position.Y;
            Width = S.Default.Size.Width;
            Height = S.Default.Size.Height;


            icon = new NotifyIcon();
            icon.Icon = Properties.Resources.icon;
            icon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                new MenuItem("Pokaż", onShow),
                new MenuItem("-"),
                new MenuItem("Zamknij", onClose),
            });
            
            icon.DoubleClick += (sender, args) =>
            {
                ShowWindow();
            };
            
            vm = new MainWindowViewModel();
            DataContext = vm;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (S.Default.MinimizeToTray && !_force)
            {
                e.Cancel = true;
                ToTray();
            }
            else
            {
                vm.OnClose(e);
            }
            _force = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            S.Default.Maximized = WindowState == WindowState.Maximized;
            S.Default.Position = new System.Drawing.Point((int)Left, (int)Top);
            S.Default.Size = new System.Drawing.Size((int)Width, (int)Height);
            S.Default.Save();
            base.OnClosed(e);
        }

        private void ToTray()
        {
            icon.Visible = true;
            Hide();
        }

        private void ShowWindow()
        {
            icon.Visible = false;
            Show();
        }
        
        private void ForceClose()
        {
            _force = true;
            Close();
        }

        private void MinimizeTray_Click(object sender, RoutedEventArgs e) => ToTray();

        private void ForceClose_Click(object sender, RoutedEventArgs e) => ForceClose();

        private void Licenses_Click(object sender, RoutedEventArgs e) => new Licenses { Owner = this }.ShowDialog();

        private void About_Click(object sender, RoutedEventArgs e) => new About { Owner = this }.ShowDialog();
    }
}

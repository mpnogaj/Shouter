using System.Reflection;
using System.Windows;

namespace Radio.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Version.Text = string.Format($"{version.Major}.{version.Minor}.{version.Revision}");
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}

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
            Version.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}

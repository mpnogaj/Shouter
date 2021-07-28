using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Shouter.Views
{
    /// <summary>
    /// Interaction logic for Licenses.xaml
    /// </summary>
    public partial class Licenses : Window
    {
        private const string ModernWpfDir = @"Licenses\modernwpf.txt";
        private const string IconsDir = @"Licenses\icons.txt";

        private readonly string ModernWpfText;
        private readonly string IconsText;

        public Licenses()
        {
            using(StreamReader sr = new StreamReader(ModernWpfDir))
            {
                ModernWpfText = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(IconsDir))
            {
                IconsText = sr.ReadToEnd();
            }

            InitializeComponent();

            CurrentLicense.Text = ModernWpfText;
        }

        private void UpdateLicense(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null || CurrentLicense == null) return;
            var cb = (ComboBox)sender;
            switch (cb.SelectedIndex)
            {
                case 0:
                    CurrentLicense.Text = ModernWpfText;
                    break;
                case 1:
                    CurrentLicense.Text = IconsText;
                    break;
                default:
                    CurrentLicense.Text = string.Empty;
                    break;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => Close();
    }
}

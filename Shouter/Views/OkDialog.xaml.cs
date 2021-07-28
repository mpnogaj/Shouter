using FontAwesome5;
using System.Windows;

namespace Shouter.Views
{
    /// <summary>
    /// Interaction logic for OkDialog.xaml
    /// </summary>
    public partial class OkDialog : Window
    {
        public OkDialog()
        {
            InitializeComponent();
        }

        public static void ShowDialog(string message, string title, EFontAwesomeIcon icon, Window owner = null)
        {
            OkDialog dialog = new OkDialog();
            dialog.Owner = owner == null ? Application.Current.MainWindow : owner;
            dialog.Message.Text = message;
            dialog.Title = title;
            dialog.DialogIcon.Icon = icon;
            dialog.ShowDialog();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

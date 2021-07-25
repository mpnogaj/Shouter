using FontAwesome5;
using System.Windows;

namespace Radio.Views
{
    /// <summary>
    /// Interaction logic for YesNoDialog.xaml
    /// </summary>
    public partial class YesNoDialog : Window
    {
        public bool YesClicked { get; private set; }

        public YesNoDialog()
        {
            InitializeComponent();
        }

        public static bool ShowDialog(string message, string title, EFontAwesomeIcon icon, Window owner = null)
        {
            YesNoDialog dialog = new YesNoDialog
            {
                Owner = owner ?? Application.Current.MainWindow,
                Title = title
            };
            dialog.Message.Text = message;
            dialog.Title = title;
            dialog.DialogIcon.Icon = icon;
            dialog.ShowDialog();
            return dialog.YesClicked;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            YesClicked = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            YesClicked = false;
            Close();
        }
    }
}

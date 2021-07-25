using System.Windows;

namespace Radio.Views
{
    /// <summary>
    /// Interaction logic for AddEditDialog.xaml
    /// </summary>
    public partial class AddEditDialog : Window
    {
        public AddEditDialog()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
        }
    }
}

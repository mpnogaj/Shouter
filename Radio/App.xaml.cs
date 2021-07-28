using System;
using System.IO;
using System.Windows;
using Radio.Properties;

namespace Radio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string directory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Radio";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(App).Assembly.Location));

            Settings.Default.Upgrade();
        }
    }
}

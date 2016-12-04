using System.Windows;

namespace MusicSink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnExit(object sender, ExitEventArgs e)
        {
            MusicSink.Properties.Settings.Default.Save();
        }
    }
}

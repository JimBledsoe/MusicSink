using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace MusicSink
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //protected List<DriveInfo> driveList;        // decide which to use later
        protected List<string> removableList;       // decide which to use later
        //protected IEnumerable<string> removableDrives;       // decide which to use later

        public MainWindow()
        {
            InitializeComponent();

            // Set sane defaults
            remoteMasterPath.Text = null;
            localMasterPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            //driveList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).ToList();
            removableList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).Select(s => s.ToString()).ToList();
            //removableDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).Select(s => s.ToString());
            if (removableList.Count() == 0) 
            {
                removableList.Add("<insert removable media and select it>");
            }
            removableDriveCombo.ItemsSource = removableList;
            removableDriveCombo.SelectedIndex = 0;

            // Set last used settings
        }

        private void remoteMasterBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select the folder on the network where the remote master music files are stored.";
            dialog.SelectedPath = remoteMasterPath.Text;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                remoteMasterPath.Text = dialog.SelectedPath;
            }
        }

        private void localMasterBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select the folder on this computer where the local master music files are stored.";
            dialog.SelectedPath = localMasterPath.Text;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                localMasterPath.Text = dialog.SelectedPath;
            }
        }

        private void removableDriveCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void scanMasterRemoteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

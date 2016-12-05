using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UsbDriveEvents;


static class Constants
{
    public const string ManifestFilename = "MusicSink_manifest.json";
}


namespace MusicSink
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected List<string> removableList;
        private UsbDriveDetector usbDriveDetector = null;

        public MainWindow()
        {
            InitializeComponent();

            usbDriveDetector = new UsbDriveDetector();
            usbDriveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            usbDriveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            //driveDetector.QueryRemove += new DriveDetectorEventHandler(OnQueryRemove);

            // Set last used settings
        }

        // Remote Master Widgets
        private void remoteMasterPath_Loaded(object sender, RoutedEventArgs e)
        {
            // empty is appropriate for the remote master
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

        private void remoteMasterPath_Changed(object sender, RoutedEventArgs e)
        {
            FileUtils.validatePathTextbox(remoteMasterPath, Properties.Settings.Default["remoteMasterPath"].ToString());
        }

        private void remoteMasterPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).RaiseEvent(new RoutedEventArgs(TextBox.LostFocusEvent));
            }
        }

        // Local Master Widgets
        private void localMasterPath_Loaded(object sender, RoutedEventArgs e)
        {
            // Use the MyMusic folder by default if no local master is defined yet
            if (localMasterPath.Text.Length == 0)
            {
                localMasterPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
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

        private void localMasterPath_Changed(object sender, RoutedEventArgs e)
        {
            FileUtils.validatePathTextbox(localMasterPath, Properties.Settings.Default["localMasterPath"].ToString());
        }

        private void localMasterPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).RaiseEvent(new RoutedEventArgs(TextBox.LostFocusEvent));
            }
        }

        // Removable Media Widgets
        private void enumerateRemovableDriveCombo()
        {
            removableList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).Select(s => s.ToString()).ToList();
            if (removableList.Count() == 0)
            {
                removableList.Add("<insert removable media and select it>");
            }
            removableDriveCombo.ItemsSource = removableList;
            removableDriveCombo.SelectedIndex = 0;
        }

        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
            e.HookQueryRemove = true;  // be given a chance to cancel the remove
            enumerateRemovableDriveCombo();
        }

        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            enumerateRemovableDriveCombo();
        }


        //// Called by DriveDetector when removable drive is about to be removed
        //private void OnQueryRemove(object sender, DriveDetectorEventArgs e)
        //{
        //    // Should we allow the drive to be unplugged?
        //    if (System.Windows.Forms.MessageBox.Show("Allow remove?", "Query remove",
        //        MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
        //            DialogResult.Yes)
        //        e.Cancel = false;        // Allow removal
        //    else
        //        e.Cancel = true;         // Cancel the removal of the device
        //}

        private void removableDriveCombo_Loaded(object sender, RoutedEventArgs e)

        {
            enumerateRemovableDriveCombo();
        }

        private void removableDriveCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        // Begin the scan process widgets
        private void scanMasterRemoteButton_Click(object sender, RoutedEventArgs e)
        {
            MusicFolder.scanMusicFolder(localMasterPath.Text);
              
        }

    }
}

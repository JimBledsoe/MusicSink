using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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
        public List<MusicFile> workingList = new List<MusicFile>();
        //public string MediaPlaying {
        //    get { return mediaPlayer.Source.ToString(); }
        //    set { mediaPlayer.Source = new Uri(value); }
        //}

        public MainWindow()
        {
            InitializeComponent();

            usbDriveDetector = new UsbDriveDetector();
            usbDriveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            usbDriveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            //driveDetector.QueryRemove += new DriveDetectorEventHandler(OnQueryRemove);

            // Link music file list to CollectionViewSource for the datagrid
            CollectionViewSource itemCollectionViewSource;
            itemCollectionViewSource = (CollectionViewSource)(FindResource("ItemCollectionViewSource"));
            itemCollectionViewSource.Source = workingList;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Remote Master Widgets
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void remoteMasterPath_Loaded(object sender, RoutedEventArgs e)
        {
            // empty is appropriate for the remote master
            remoteMasterScanButton.IsEnabled = FileUtils.validatePathTextbox(remoteMasterPath, Properties.Settings.Default["remoteMasterPath"].ToString());
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
            remoteMasterScanButton.IsEnabled = FileUtils.validatePathTextbox(remoteMasterPath, Properties.Settings.Default["remoteMasterPath"].ToString());
        }

        private void remoteMasterPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).RaiseEvent(new RoutedEventArgs(TextBox.LostFocusEvent));
            }
        }

        private void remoteMasterScanButton_Click(object sender, RoutedEventArgs e)
        {
            mediaStop();
            workingList.Clear();
            filesGrid.Items.Refresh();  // Can't make the grid appear blank while loading yet....
            lblMessage.Text = MusicFolder.scanMusicFolder(remoteMasterPath.Text, workingList);
            filesGrid.Items.Refresh();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Local Master Widgets
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void localMasterPath_Loaded(object sender, RoutedEventArgs e)
        {
            // Use the MyMusic folder by default if no local master is defined yet
            if (localMasterPath.Text.Length == 0)
            {
                localMasterPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            }
            localMasterScanButton.IsEnabled = FileUtils.validatePathTextbox(localMasterPath, Properties.Settings.Default["localMasterPath"].ToString());
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
            localMasterScanButton.IsEnabled = FileUtils.validatePathTextbox(localMasterPath, Properties.Settings.Default["localMasterPath"].ToString());
        }

        private void localMasterPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).RaiseEvent(new RoutedEventArgs(TextBox.LostFocusEvent));
            }
        }

        private void localMasterScanButton_Click(object sender, RoutedEventArgs e)
        {
            mediaStop();
            workingList.Clear();
            filesGrid.Items.Refresh();  // Can't make the grid appear blank while loading yet....
            lblMessage.Text = MusicFolder.scanMusicFolder(localMasterPath.Text, workingList);
            filesGrid.Items.Refresh();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Removable Media Widgets
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void enumerateRemovableDriveCombo()
        {
            removableList = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable).Select(s => s.ToString()).ToList();
            removableList.Insert(0, "<No removable media drive selected>");
            removableDriveCombo.ItemsSource = removableList;
            removableDriveCombo.SelectedIndex = (removableList.Count > 1) ? 1 : 0;  // Select first valid drive (skip bogus first entry)
            removableScanButton.IsEnabled = FileUtils.validatePathCombobox(removableDriveCombo);
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
            removableScanButton.IsEnabled = FileUtils.validatePathCombobox(removableDriveCombo);
        }

        private void removableScanButton_Click(object sender, RoutedEventArgs e)
        {
            mediaStop();
            workingList.Clear();
            filesGrid.Items.Refresh();  // Can't make the grid appear blank while loading yet....
            lblMessage.Text = MusicFolder.scanMusicFolder(removableDriveCombo.Text, workingList);
            filesGrid.Items.Refresh();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Data Grid Widgets
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void filesClearButton_Click(object sender, RoutedEventArgs e)
        {
            mediaStop();
            workingList.Clear();
            filesGrid.Items.Refresh();
        }

        static private ToggleButton lastPlayButton = null;
        private void onRowPlayPause(object sender, RoutedEventArgs e)
        {
            MusicFile selectedFile = (MusicFile)filesGrid.SelectedItem;
            bool currentIsChecked = ((sender as ToggleButton).IsChecked == true);

            Uri newMusic = new Uri(selectedFile.fileName);
            // If we have clicked on a new row, then load up the file
            if (newMusic != mediaPlayer.Source)
            {
                mediaPlayer.Source = new Uri(selectedFile.fileName);
                // uncheck the last row that was checked (radio style toggles)
                if (lastPlayButton != null)
                {
                    lastPlayButton.IsChecked = false;
                    lastPlayButton.UpdateLayout();
                }

            }
            lastPlayButton = (sender as ToggleButton);

            if (currentIsChecked)
            {
                mediaPlayer.Play();
            }
            else
            {
                mediaPlayer.Pause();
            }
        }

        private void onRowCopy(object sender, RoutedEventArgs e)
        {
        }

        private void onRowHide(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility =
                      row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Helper Methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void mediaStop()
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Source = null;
            }
        }
    }
}

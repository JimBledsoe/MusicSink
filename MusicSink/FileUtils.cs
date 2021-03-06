﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace MusicSink
{
    class FileUtils
    {
        // Validate the path entered into a path textbox in the UI
        static public bool validatePathTextbox(TextBox tb, string revertTo)
        {
            if (tb.Text.Length > 0)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(tb.Text));
                if (!dir.Exists)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The folder '" + tb.Text + "' does not exist.  Reverting back to previous value of '" + revertTo + "'.",
                        "Path validation failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
                    if (result == MessageBoxResult.OK)
                    {
                        tb.Text = revertTo;
                        return false;
                    }
                }
            }
            MusicSink.Properties.Settings.Default.Save();
            return (tb.Text.Length > 0);
        }

        // Validate the path entered into a path combobox in the UI
        static public bool validatePathCombobox(ComboBox cb)
        {
            try
            {
                string path = cb.SelectedItem.ToString();
                path = path.Substring(0, path.IndexOf(" "));
                if (path.Length > 0)
                {
                    DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
                    return (dir.Exists);
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        // Read all of the files under a path into a list of MusicFolder class
        static public MusicFolder EnumerateMusicFolder(string sDir, BackgroundWorker worker, DoWorkEventArgs e, Func<BackgroundWorker, DoWorkEventArgs, bool> checkForCancel)
        {
            List<MusicFile> musicFiles = null;
            musicFiles = EnumerateFilesInPath(sDir, musicFiles, worker, e, checkForCancel);
            MusicFolder musicFolder = new MusicFolder(sDir, musicFiles);
            return (musicFolder);
        }

        // Read all of the files under a path into a list of MusicFile class
        static private List<MusicFile> EnumerateFilesInPath(string sDir, List<MusicFile> musicFiles, BackgroundWorker worker, DoWorkEventArgs e, Func<BackgroundWorker, DoWorkEventArgs, bool>checkForCancel)
        {
            List<MusicFile> outFiles = new List<MusicFile>();
            if (musicFiles != null)
            {
                foreach (MusicFile file in musicFiles)
                {
                    outFiles.Add(file);
                }
            }

            try
            {
                // Get all the files in this folder
                foreach (string f in Directory.GetFiles(sDir))
                {
                    // skip the manifest file
                    if (!f.Contains(Constants.ManifestFilename))
                    {
                        MusicFile mf = new MusicFile(f);
                        outFiles.Add(mf);
                        //Console.WriteLine(f);
                        worker.ReportProgress(-1, "Read " + f);
                        if (checkForCancel(worker, e)) return null;
                    }
                }
                // Recurse into the subdirs in this folder
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    outFiles = EnumerateFilesInPath(d, outFiles, worker, e, checkForCancel);
                }
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show(
                        "An error occurred trying to read all the files in the folder '" + sDir + "'.",
                        "Failed to read files in path",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
            }

            return (outFiles);
        }
    }
}

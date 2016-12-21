using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace MusicSink
{

    class MusicFolder
    {
        public string folderName;
        public List<MusicFile> musicFiles;

        public MusicFolder(string folder, List<MusicFile> mf)
        {
            folderName = folder;
            musicFiles = mf;
        }

        // Begin the scan process widgets
        static public string scanMusicFolder(string folderName, List<MusicFile> workingFiles, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int addedCount = 0;
            int deletedCount = 0;
            FileInfo masterManifest = new FileInfo(Path.GetFullPath(folderName) + "\\" + Constants.ManifestFilename);
            MusicFolder scannedMasterMusic = null;
            MusicFolder manifestMasterMusic = null;
            int percentComplete = 0;

            // Scan the master folder and build a fresh list
            Console.WriteLine("Scan the folder for music files....");
            percentComplete = 20;
            worker.ReportProgress(percentComplete, "Scan the folder for music files....");
            scannedMasterMusic = FileUtils.EnumerateMusicFolder(folderName, worker, e, checkForCancel);

            // If we have a previous manifest on the master folder, read it in
            Console.WriteLine("Look for a manifest file in the folder....");
            percentComplete = 40;
            worker.ReportProgress(percentComplete, "Look for a manifest file in the folder....");
            if (checkForCancel(worker, e)) return "Cancelled.";
            if (masterManifest.Exists)
            {
                string json = File.ReadAllText(masterManifest.FullName);
                manifestMasterMusic = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicFolder>(json);
            }
            else
            {
                manifestMasterMusic = new MusicFolder(folderName, null);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(scannedMasterMusic, Newtonsoft.Json.Formatting.Indented);
                try
                {
                    File.WriteAllText(masterManifest.FullName, json);
                }
                catch
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Could not write the manifest file to " + masterManifest.FullName,
                        "Cannot create music manifest",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);

                }
            }

            // Go through all the recently scanned files to see if any are not in the disk list (new files)
            Console.WriteLine("Chew through all the files we just scanned....");
            percentComplete = 60;
            worker.ReportProgress(percentComplete, "Chew through all the files we just scanned....");
            foreach (MusicFile currentFile in scannedMasterMusic.musicFiles)
            {
                MusicFile searchFile = null;
                worker.ReportProgress(percentComplete, "Process " + currentFile.FileName);
                if (checkForCancel(worker, e)) return "Cancelled.";

                if (manifestMasterMusic.musicFiles != null)
                {
                    searchFile = manifestMasterMusic.musicFiles.Find(srch => srch.FileName == currentFile.FileName);
                }
                currentFile.isProcessed = true;

                // If the file exists in the disk manifest, then we are good
                if (searchFile != null)
                {
                    searchFile.isProcessed = true;
                    currentFile.status = MusicFile.MusicFileStatus.Old;
                    searchFile.status = MusicFile.MusicFileStatus.Old;
                }
                else  // Not in the disk manifest means a new file, add to our working list
                {
                    MusicFile newFile = new MusicFile(currentFile.FileName);
                    currentFile.status = MusicFile.MusicFileStatus.New;
                    newFile.status = MusicFile.MusicFileStatus.New;
                    addedCount++;
                    workingFiles.Add(newFile);
                }
            }

            // Now go through the disk list and mark any unprocessed as deleted from the master
            Console.WriteLine("Now walk all the manifest files looking for deleted ones....");
            percentComplete = 80;
            worker.ReportProgress(percentComplete, "Now walk all the manifest files looking for deleted ones....");
            if (manifestMasterMusic.musicFiles != null)
            {
                foreach (MusicFile currentFile in manifestMasterMusic.musicFiles)
                {
                    worker.ReportProgress(percentComplete, "Scrub " + currentFile.FileName);
                    if (checkForCancel(worker, e)) return "Cancelled.";
                    if (currentFile.isProcessed == false)
                    {
                        currentFile.isProcessed = true;
                        MusicFile newFile = new MusicFile(currentFile.FileName);
                        currentFile.status = MusicFile.MusicFileStatus.Deleted;
                        newFile.status = MusicFile.MusicFileStatus.Deleted;
                        deletedCount++;
                        //workingFiles.Add(newFile);
                    }
                }
            }

            Console.WriteLine("Scanning process is complete....");
            percentComplete = 100;
            worker.ReportProgress(percentComplete, "Scanning process is complete....");
            return (manifestMasterMusic.musicFiles.Count + " files in library, " + 
                    scannedMasterMusic.musicFiles.Count + " files scanned, " + 
                    addedCount + " files added, " + 
                    deletedCount + " files deleted, " +
                    workingFiles.Count + " files to process.");
        }

        static private bool checkForCancel(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            return e.Cancel;
        }
    }

    public class MusicFile
    {
        public string FileName { get; set; }
        public DateTime timeStamp;
        public long size;
        public string md5;
        public bool isProcessed;
        public bool isIgnored;
        public MusicFileStatus status { get; set; }
        public string Id3Title { get; set; }
        public string Id3Album { get; set; }
        public string Id3Artist { get; set; }
        public string Id3Genre { get; set; }
        public string id3Length;

        public enum MusicFileStatus { Unknown, New, Old, Deleted, Hidden }

        // Constructor from a raw filename
        public MusicFile(string fname)
        {
            FileName = "";
            // timeStamp = ??? what to initialize to;
            size = -1;
            md5 = null;
            isProcessed = false;
            isIgnored = false;
            status = MusicFileStatus.Deleted;

            if (fname != null)
            {
                try
                {
                    FileInfo fi = new FileInfo(fname);
                    if (fi.Exists)
                    {
                        FileName = fi.FullName;
                        timeStamp = fi.LastWriteTime;
                        size = fi.Length;
                        md5 = null;  // do in a separate pass calculateFileMD5(fileName);
                        status = MusicFileStatus.Unknown;
                        readID3Tags(fi.FullName);
                    }
                }
                catch
                {
                }
            }
        }

        private void readID3Tags(string filename)
        {
            TagLib.File file = TagLib.File.Create(filename);
            Id3Title = file.Tag.Title;
            Id3Album = file.Tag.Album;
            Id3Artist = file.Tag.JoinedAlbumArtists;
            id3Length = file.Properties.Duration.ToString();
        }

        // Calculate the MD5 checksum of a file
        private string calculateFileMD5(string filename)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return Encoding.Default.GetString(md5.ComputeHash(stream));
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

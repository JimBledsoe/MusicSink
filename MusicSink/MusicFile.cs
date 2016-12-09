using System;
using System.Collections.Generic;
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
        static public void scanMusicFolder(string folderName, List<MusicFile> workingFiles)
        {
            FileInfo masterManifest = new FileInfo(Path.GetFullPath(folderName) + "\\" + Constants.ManifestFilename);
            MusicFolder currentMasterMusic, diskMasterMusic;

            currentMasterMusic = FileUtils.EnumerateMusicFolder(folderName);

            if (masterManifest.Exists)
            {
                string json = File.ReadAllText(masterManifest.FullName);
                diskMasterMusic = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicFolder>(json);
            }
            else
            {
                diskMasterMusic = new MusicFolder(folderName, null);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(currentMasterMusic);
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
            foreach (MusicFile currentFile in currentMasterMusic.musicFiles)
            {
                MusicFile searchFile = diskMasterMusic.musicFiles.Find(srch => srch.fileName == currentFile.fileName);
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
                    MusicFile newFile = new MusicFile(currentFile.fileName);
                    currentFile.status = MusicFile.MusicFileStatus.New;
                    newFile.status = MusicFile.MusicFileStatus.New;
                    workingFiles.Add(newFile);
                }
            }

            // Now go through the disk list and mark any unprocessed as deleted from the master
            foreach (MusicFile currentFile in diskMasterMusic.musicFiles)
            {
                if (currentFile.isProcessed == false)
                {
                    currentFile.isProcessed = true;
                    MusicFile newFile = new MusicFile(currentFile.fileName);
                    currentFile.status = MusicFile.MusicFileStatus.Deleted;
                    newFile.status = MusicFile.MusicFileStatus.Deleted;
                    workingFiles.Add(newFile);
                }
            }

            return;
        }
    }

    public class MusicFile
    {
        public string fileName { get; set; }
        public DateTime timeStamp;
        public long size;
        public string md5;
        public bool isProcessed;
        public bool isIgnored;
        public MusicFileStatus status { get; set; }

        public enum MusicFileStatus { Unknown, New, Old, Deleted, Hidden }

        // Constructor from a raw filename
        public MusicFile(string fname)
        {
            try
            {
                FileInfo fi = new FileInfo(fname);
                fileName = fi.FullName;
                timeStamp = fi.LastWriteTime;
                size = fi.Length;
                md5 = null; // calculateFileMD5(fileName);
                isProcessed = false;
                isIgnored = false;
                status = MusicFileStatus.Unknown;
            }
            catch
            {
                // I guess it will be left as null
            }
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

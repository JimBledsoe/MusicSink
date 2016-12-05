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
        static public void scanMusicFolder(string folderName)
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
        }
    }

    class MusicFile
    {
        public string fileName;
        public DateTime timeStamp;
        public long size;
        public string md5;

        //// Constructor from a FileInfo class
        //public MusicFile(System.IO.FileInfo fi)
        //{
        //    fileName = fi.FullName;
        //    timeStamp = fi.LastWriteTime;
        //    size = fi.Length;
        //    md5 = null; // calculateFileMD5(fileName);
        //}

        // Constructor from a raw data
        public MusicFile(string fname)
        {
            try
            {
                FileInfo fi = new FileInfo(fname);
                fileName = fi.FullName;
                timeStamp = fi.LastWriteTime;
                size = fi.Length;
                md5 = null; // calculateFileMD5(fileName);

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

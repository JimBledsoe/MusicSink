using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;


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

    }

    class MusicFile
    {
        public string fileName;
        public DateTime timeStamp;
        public long size;
        public string md5;

        // Constructor from a FileInfo class
        public MusicFile(System.IO.FileInfo fi)
        {
            fileName = fi.FullName;
            timeStamp = fi.LastWriteTime;
            size = fi.Length;
            md5 = calculateFileMD5(fileName);
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

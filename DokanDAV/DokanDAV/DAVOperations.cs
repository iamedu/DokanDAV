using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

using Dokan;

namespace DokanDAV
{
    class DAVOperations : DokanOperations
    {
        private MemFileSystem memfs;

        public DAVOperations()
        {
            memfs = new MemFileSystem();
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            memfs.CreateDirectory(webFilename);

            return 0;
        }

        

        public int CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            throw new NotImplementedException();
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            if (!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }
            if (memfs.Delete(webFilename))
            {
                return 0;
            }
            return -1;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            throw new NotImplementedException();
        }

        public int FindFiles(string filename, System.Collections.ArrayList files, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            if (!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }

            ICollection<MemFile> memFiles = memfs.List(webFilename);

            foreach(MemFile mf in memFiles) {
                FileInformation fi = new FileInformation();

                fi.FileName = mf.Name;
                fi.Length = mf.Length;
                fi.CreationTime = mf.DateCreated;
                fi.LastAccessTime = mf.LastAccessed;
                fi.LastWriteTime = mf.LastUpdated;

                if (mf.Type == WebdavClient.DAVType.File)
                {
                    fi.Attributes = FileAttributes.Normal;
                }
                else
                {
                    fi.Attributes = FileAttributes.Directory;
                }

                files.Add(fi);
            }

            return 0;
        }

        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return -1;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            return -1;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            if(!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }
            MemFile mf = memfs.Lookup(webFilename);

            fileinfo.Length = mf.Length;
            fileinfo.CreationTime = mf.DateCreated;
            fileinfo.LastAccessTime = mf.LastAccessed;
            fileinfo.LastWriteTime = mf.LastUpdated;

            if (mf.Type == WebdavClient.DAVType.File)
            {
                fileinfo.Attributes = FileAttributes.Normal;
            }
            else
            {
                fileinfo.Attributes = FileAttributes.Directory;
            }

            return 0;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            string newWebFilename = Normalize(newname);

            try
            {
                memfs.MoveFile(filename, newname);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }

            return 0;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            MemFile file;
            if (!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }

            file = memfs.Lookup(webFilename);
            if (file.Type == WebdavClient.DAVType.File)
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }
            else
            {
                return 0;
            }
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
            return -1;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int SetFileAttributes(string filename, System.IO.FileAttributes attr, DokanFileInfo info)
        {
            return -1;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            return -1;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {
            Debug.WriteLine("Unmounting fs");
            return 0;
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            return -1;
        }

        private string Normalize(string filename)
        {
            return filename.Replace('\\', '/');
        }

    }
}

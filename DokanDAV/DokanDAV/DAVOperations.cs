using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;

using Dokan;
using WebdavClient;

namespace DokanDAV
{
    public struct DAVSize {
        public ulong used;
        public ulong total;
    }

    public class DAVOperations : DokanOperations
    {
        private MemFileSystem memfs;
        private DAVClient client;
        private string username;
        private string password;

        public long CacheMillis { get; set; }

        private Func<string, DAVSize> sizeFunc;

        public DAVOperations(DAVProtocol protocol,
                             String host,
                             int port,
                             String basePath,
                             String username,
                             String password,
                             Func<string, DAVSize> sizeFunc)
        {
            string userHome = Environment.GetEnvironmentVariable("USERPROFILE");
            memfs = new MemFileSystem(userHome + "\\.davfs\\");

            client = new WebdavClient.DAVClient(protocol,
                                                host,
                                                port,
                                                basePath,
                                                username,
                                                password);

            CacheMillis = 30 * 1000;
            FillList("/");

            this.username = username;
            this.password = password;
            this.sizeFunc = sizeFunc;

            Console.WriteLine("This is a beautiful day");
        }

        public int Cleanup(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            return 0;
        }

        public int CloseFile(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            string localFilename = memfs.LocalFilename(webFilename);

            Console.WriteLine("Close " + webFilename);

            MemFile file = memfs.Lookup(webFilename);

            if (file != null)
            {
                if (file.LocallyModified)
                {
                    file.LocallyModified = false;
                    client.Upload(localFilename, webFilename);
                }
                file.Unlock();
            }

            return 0;
        }

        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            try
            {
                if (memfs.Exists(webFilename))
                {
                    return -DokanNet.ERROR_FILE_EXISTS;
                }
                if (client.CreateFolder(webFilename))
                {
                    memfs.CreateDirectory(webFilename);
                }
                else
                {
                    return -DokanNet.ERROR_ACCESS_DENIED;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }

            

            return 0;
        }

        

        public int CreateFile(string filename, System.IO.FileAccess access, System.IO.FileShare share, System.IO.FileMode mode, System.IO.FileOptions options, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            Console.WriteLine("Open " + webFilename + " " + access);

            MemFileContext context = new MemFileContext();
            MemFile file;

            context.Mode = mode;
            context.Access = access;
            context.Share = share;
            context.Options = options;

            info.Context = context;

            switch (mode)
            {
                case FileMode.Append:
                    if (!memfs.Exists(webFilename))
                    {
                        return -DokanNet.ERROR_FILE_NOT_FOUND;
                    }

                    file = memfs.Lookup(webFilename);

                    break;
                case FileMode.Create:
                    if (memfs.Exists(webFilename))
                    {
                        if (!memfs.Delete(webFilename))
                        {
                            return -DokanNet.ERROR_ACCESS_DENIED;
                        }
                    }

                    file = memfs.CreateFile(webFilename);

                    break;
                case FileMode.CreateNew:
                    

                    if (memfs.Exists(webFilename))
                    {
                        return -DokanNet.ERROR_ALREADY_EXISTS;
                    }

                    file = memfs.CreateFile(webFilename);

                    break;
                case FileMode.Open:
                    if (memfs.Exists(webFilename))
                    {
                        file = memfs.Lookup(webFilename);
                    }
                    else
                    {
                        return -DokanNet.ERROR_FILE_NOT_FOUND;
                    }

                    break;
                case FileMode.OpenOrCreate:

                    if (!memfs.Exists(webFilename))
                    {
                        file = memfs.CreateFile(webFilename);
                    }
                    else
                    {
                        file = memfs.Lookup(webFilename);
                    }

                    break;
                case FileMode.Truncate:
                    if (!memfs.Exists(webFilename))
                    {
                        return -DokanNet.ERROR_FILE_NOT_FOUND;
                    }

                    if (!memfs.Delete(webFilename))
                    {
                        return -DokanNet.ERROR_ACCESS_DENIED;
                    }

                    file = memfs.CreateFile(webFilename, 0);

                    break;
                default:
                    return -1;
            }

            Console.WriteLine("Opened " + webFilename + " " + access + " about to lock");

            if (!file.TryLock())
            {
                Console.WriteLine("NOT NOT NOT " + webFilename + " " + access + " about to lock");
                return DokanNet.ERROR_SHARING_VIOLATION;
            }



            file.LocallyModified = false;

            if (file.LastAccessed.AddMilliseconds(CacheMillis) < DateTime.Now)
            {
                if(File.Exists(memfs.LocalFilename(webFilename)))
                {
                    File.Delete(memfs.LocalFilename(webFilename));
                }
            }

            return 0;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            if (!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }

            try
            {
                if (client.Delete(webFilename))
                {
                    if (memfs.Delete(webFilename))
                    {
                        return 0;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }

            

            return -1;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            if (!memfs.Exists(webFilename))
            {
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }

            try
            {
                if (client.Delete(webFilename))
                {
                    if (memfs.Delete(webFilename))
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return -1;
        }

        public int FindFiles(string filename, System.Collections.ArrayList files, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            if (!memfs.Exists(webFilename))
            {
                //Need to improve this, if it wasn't found check in the internet
                try
                {
                    FillList(webFilename);
                    if (!memfs.Exists(webFilename))
                    {
                        return -DokanNet.ERROR_PATH_NOT_FOUND;
                    }
                }
                catch
                {
                    return -DokanNet.ERROR_PATH_NOT_FOUND;
                }
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            }

            MemFile parent = memfs.Lookup(webFilename);

            if (!parent.Listed)
            {
                FillList(webFilename);
            }

            if (parent.LastAccessed.AddMilliseconds(CacheMillis) < DateTime.Now)
            {
                FillList(webFilename);
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
            Debug.WriteLine("FlushFileBuffers " + filename);
            return -1;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            ulong usedBytes;

            DAVSize size;

            if (sizeFunc != null)
            {
                size = sizeFunc(username);
                totalBytes = size.total;
                usedBytes = size.used;
                freeBytesAvailable = (totalBytes - usedBytes);
                totalFreeBytes = (totalBytes - usedBytes);
            }
            else
            {
                freeBytesAvailable = 512 * 1024 * 1024;
                totalBytes = 1024 * 1024 * 1024;
                totalFreeBytes = 512 * 1024 * 1024;
            }

            return 0;
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
            Debug.WriteLine("LockFile");
            return -1;
        }

        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            string newWebFilename = Normalize(newname);

            try
            {
                if (client.Move(webFilename, newWebFilename))
                {
                    memfs.MoveFile(webFilename, newWebFilename);
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return -1;
            }
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
            string webFilename = Normalize(filename);
            string localFilename;

            localFilename = memfs.LocalFilename(webFilename);

            if (!File.Exists(localFilename))
            {
                try
                {
                    client.Download(localFilename, webFilename);
                }
                catch
                {
                    return -DokanNet.ERROR_FILE_NOT_FOUND;
                }
            }

            try
            {
                memfs.ReadFile(webFilename, buffer, ref readBytes, offset);
            }
            catch
            {
                return -1;
            }

            return 0;
        }

        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            try
            {
                memfs.SetEndOfFile(webFilename, length);
            }
            catch
            {
                return -1;
            }

            return 0;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);
            try
            {
                memfs.SetEndOfFile(webFilename, length);
            }
            catch
            {
                return -1;
            }

            return 0;
        }

        public int SetFileAttributes(string filename, System.IO.FileAttributes attr, DokanFileInfo info)
        {
            Debug.WriteLine("SetFileAttributes " + filename);
            return -1;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            Debug.WriteLine("SetFileTime " + filename);
            return -1;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            Debug.WriteLine("UnlockFile " + filename);
            return 0;
        }

        public int Unmount(DokanFileInfo info)
        {

            memfs.Umount();

            return 0;
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            string webFilename = Normalize(filename);

            try
            {
                memfs.WriteFile(webFilename, buffer, ref writtenBytes, offset);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
            
            return 0;
        }

        private void FillList(string filename)
        {
            string webFilename = Normalize(filename);
            MemFile parent = memfs.Lookup(webFilename);
            List<DAVFileInfo> davFileInfo = client.List(webFilename);

            parent.Clear();

            foreach(DAVFileInfo info in davFileInfo)
            {
                MemFile memFile = new MemFile(true);

                memFile.Length = info.Length;
                memFile.DateCreated = info.DateCreated;
                memFile.LastUpdated = info.LastModified;
                memFile.LastAccessed = DateTime.Now;
                memFile.Type = info.Type;

                parent[info.Name] = memFile;
            }

            parent.Listed = true;

        }

        private string Normalize(string filename)
        {
            return filename.Replace('\\', '/');
        }

    }
}

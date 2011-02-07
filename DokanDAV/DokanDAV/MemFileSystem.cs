using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using WebdavClient;
using System.IO;

namespace DokanDAV
{
    /*
     * Simple rule, for this to work every path must NOT
     * end with a slash
     */
    public class MemFileSystem
    {
        private MemFile root;

        public string BasePath { get; private set; }

        public MemFileSystem(string basePath)
        {
            root = new MemFile(false);
            root.DateCreated = DateTime.Now;
            root.LastAccessed = DateTime.Now;
            root.LastUpdated = DateTime.Now;
            root.Type = DAVType.Folder;

            BasePath = basePath;

            if (Directory.Exists(BasePath))
            {
                Directory.Delete(BasePath, true);
            }

            Directory.CreateDirectory(BasePath);

        }

        public bool Delete(string path)
        {
            string parent = Parent(path);
            string filename = Filename(path);


            if (!Exists(path))
            {
                return false;
            }

            MemFile parentNode = Lookup(parent);
            MemFile node = parentNode[filename];
            string localFilename = LocalFilename(path);

            parentNode.Remove(filename);
            node.Unlock();
            File.Delete(localFilename);

            return true;
        }

        public MemFile CreateDirectory(string path)
        {
            string parent = Parent(path);
            string filename = Filename(path);

            MemFile parentNode = Lookup(parent);
            MemFile childNode = new MemFile(true);

            childNode.DateCreated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
            childNode.LastAccessed = DateTime.Now;
            childNode.Type = DAVType.Folder;
            childNode.Length = 0;
            childNode.AbsolutePath = path;

            parentNode[filename] = childNode;

            return childNode;
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            string destinationParent = Parent(destinationPath);
            string destinationFilename = Filename(destinationPath);

            MemFile sourceNode = Lookup(sourcePath);
            MemFile destinationNode = new MemFile(sourceNode.Remote);
            MemFile destionationParent = Lookup(destinationParent);

            destinationNode.AbsolutePath = destinationPath;
            destinationNode.Type = sourceNode.Type;
            destinationNode.LastAccessed = DateTime.Now;
            destinationNode.LastUpdated = DateTime.Now;
            destinationNode.DateCreated = DateTime.Now;
            destinationNode.Length = sourceNode.Length;

            destionationParent[destinationFilename] = destinationNode;
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            string sourceParent = Parent(sourcePath);
            string sourceFilename = Filename(sourcePath);
            string destinationParent = Parent(destinationPath);
            string destinationFilename = Filename(destinationPath);

            string sourceLocal = LocalFilename(sourcePath);
            string destinationLocal = LocalFilename(destinationPath);

            MemFile sourceParentNode = Lookup(sourceParent);
            MemFile sourceNode = Lookup(sourcePath);
            MemFile destinationNode = Lookup(destinationParent);

            sourceNode.LastUpdated = DateTime.Now;
            sourceNode.LastAccessed = DateTime.Now;
            sourceNode.DateCreated = DateTime.Now;

            sourceNode.AbsolutePath = destinationPath;

            destinationNode[destinationFilename] = sourceNode;

            sourceParentNode.Remove(sourceFilename);

            if (File.Exists(sourceLocal))
            {
                File.Move(sourceLocal, destinationLocal);
            }

        }

        public MemFile CreateFile(string path, int length = 0, bool remote = true)
        {
            string parent = Parent(path);
            string filename = Filename(path);

            MemFile parentNode = Lookup(parent);
            MemFile childNode = new MemFile(remote);

            childNode.DateCreated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
            childNode.LastAccessed = DateTime.Now;
            childNode.Type = DAVType.File;
            childNode.Length = length;
            childNode.AbsolutePath = path;

            parentNode[filename] = childNode;

            return childNode;
        }

        public ICollection<MemFile> List(string path)
        {
            MemFile parent = Lookup(path);

            if (parent.Type != DAVType.Folder)
            {
                throw new Exception("Cannot list a file");
            }

            return parent.Values;
        }

        public bool Exists(string path)
        {
            MemFile file;

            try
            {
                file = Lookup(path);
            }
            catch
            {
                return false;
            }

            return file != null;
        }

        public string Parent(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        public string Filename(string path)
        {
            return path.Substring(path.LastIndexOf('/') + 1);
        }

        public MemFile Lookup(string path)
        {
            MemFile helper;
            string[] parts;

            helper = root;
            parts = path.Split('/');

            foreach (string part in parts)
            {
                if (part.Length == 0)
                {
                    continue;
                }
                if(!helper.ContainsKey(part)) {
                    return null;
                }
                helper = helper[part];
            }

            return helper;
        }

        public MemFile this[string key]
        {
            get
            {
                return Lookup(key);
            }
            
        }


        public void WriteFile(string webFilename, byte[] buffer, ref uint writtenBytes, long offset)
        {
            string localFilename = LocalFilename(webFilename);
            MemFile file;

            file = Lookup(webFilename);

            
                using (FileStream fs = File.Open(localFilename, FileMode.OpenOrCreate, FileAccess.Write))
                {


                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(buffer, 0, buffer.Length);

                    writtenBytes = (uint)buffer.Length;

                    file.Length = fs.Length;
                    file.LocallyModified = true;
                }
            

        }

        public void ReadFile(string webFilename, byte[] buffer, ref uint readBytes, long offset)
        {
            string localFilename = LocalFilename(webFilename);

            
                using (FileStream fs = File.Open(localFilename, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Read(buffer, 0, buffer.Length);
                    readBytes = (uint)buffer.Length;
                }
            


        }

        public string LocalFilename(string path)
        {
            return BasePath + SHA1(path);
        }

        public void Umount()
        {
            Directory.Delete(BasePath, true);
        }

        private string SHA1(string value)
        {
            SHA1CryptoServiceProvider sp = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(sp.ComputeHash(Encoding.Default.GetBytes(value))).Replace("-", "");
        }

        public void SetEndOfFile(string webFilename, long length)
        {
            string localFilename = LocalFilename(webFilename);
            using(FileStream fs = File.Open(localFilename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.SetLength(length);
            }
        }
    }
}

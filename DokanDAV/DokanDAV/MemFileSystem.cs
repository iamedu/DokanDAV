using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;
using WebdavClient;

namespace DokanDAV
{
    /*
     * Simple rule, for this to work every path must NOT
     * end with a slash
     */
    public class MemFileSystem
    {
        private MemFile root;

        public MemFileSystem()
        {
            root = new MemFile(false);
            root.DateCreated = DateTime.Now;
            root.LastAccessed = DateTime.Now;
            root.LastUpdated = DateTime.Now;
            root.Type = DAVType.Folder;
        }

        public MemFile CreateDirectory(string path)
        {
            string parent = Parent(path);
            string filename = Filename(path);

            MemFile parentNode = Lookup(parent);
            MemFile childNode = new MemFile(true);

            childNode.DateCreated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
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
            string destinationParent = Parent(destinationPath);
            string destinationFilename = Filename(destinationPath);

            MemFile sourceNode = Lookup(sourcePath);
            MemFile destinationNode = Lookup(destinationParent);

            sourceNode.LastUpdated = DateTime.Now;
            sourceNode.LastAccessed = DateTime.Now;
            sourceNode.AbsolutePath = destinationPath;

            destinationNode[destinationFilename] = sourceNode;

        }

        public MemFile CreateFile(string path, int length = 0, bool remote = true)
        {
            string parent = Parent(path);
            string filename = Filename(path);

            MemFile parentNode = Lookup(parent);
            MemFile childNode = new MemFile(remote);

            childNode.DateCreated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
            childNode.LastUpdated = DateTime.Now;
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


    }
}

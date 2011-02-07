using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WebdavClient;

using System.IO;

namespace DokanDAV
{
    public class MemFileContext
    {
        public FileMode Mode { get; set; }
        public FileAccess Access { get; set; }
        public FileShare Share { get; set; }
        public FileOptions Options { get; set; }
        public MemFile File { get; set; } 
    }

    public class MemFile : IDictionary<string, MemFile>
    {
        private Dictionary<string, MemFile> files;
        public string Name { get; private set; }
        public string AbsolutePath { get; set; }
        public long Length { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastAccessed { get; set; }
        public DAVType Type { get; set; }
        public bool Remote { get; set; }
        public bool Locked { get; private set; }
        public bool LocallyModified { get; set; }

        public readonly object WriteLock = new object();

        private readonly object _locker = new object();

        public bool TryLock()
        {
            bool result;
            lock (_locker)
            {
                if (Locked)
                {
                    result = false;
                }
                else
                {
                    Locked = true;
                    result = true;
                }
            }
            return result;
        }

        public void Unlock()
        {
            lock (_locker)
            {
                Locked = false;
            }
        }

        public MemFile(bool remote)
        {
            files = new Dictionary<string, MemFile>();
            Name = "/";
            Remote = remote;
        }

        public MemFile this[string key]
        {
            get
            {
                return files[key];
            }
            set
            {
                if (Type == DAVType.Folder && key.Length > 0)
                {
                    value.Name = key;
                    files[key] = value;
                }
            }
        }


        public void Add(string key, MemFile value)
        {
            this[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return files.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get {
                return files.Keys;
            }
        }

        public bool Remove(string key)
        {
            return files.Remove(key);
        }

        public bool TryGetValue(string key, out MemFile value)
        {
            return files.TryGetValue(key, out value);
        }

        public ICollection<MemFile> Values
        {
            get {
                return files.Values;
            }
        }

        public void Add(KeyValuePair<string, MemFile> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            files.Clear();
        }

        public bool Contains(KeyValuePair<string, MemFile> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, MemFile>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get {
                return files.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, MemFile> item)
        {
            return files.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, MemFile>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return files.GetEnumerator();
        }
    }
}

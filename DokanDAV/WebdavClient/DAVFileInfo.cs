using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebdavClient
{
    public class DAVFileInfo
    {
        public string Name
        {
            get;
            set;
        }

        public DateTime DateCreated
        {
            get;
            set;
        }

        public DateTime LastModified
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public DAVType Type
        {
            get;
            set;
        }

    }
}

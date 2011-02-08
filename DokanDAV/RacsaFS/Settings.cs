using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;   

namespace RacsaFS
{
    public class Settings
    {
        private readonly RegistryKey BaseKey = Registry.CurrentUser;
        private readonly static String SubKeyName = @"Software\Javanes\Storage";

        public string Hostname
        {
            get
            {
                return Read("hostname");
            }
        }

        public int Port
        {
            get
            {
                return int.Parse(Read("port"));
            }
        }

        public string Base
        {
            get
            {
                return Read("base");
            }
        }

        public string Service
        {
            get
            {
                return Read("service");
            }
        }

        public string Username
        {
            get
            {
                return Read("username");
            }
            set
            {
                Write("username", value);
            }
        }

        public string Password
        {
            get
            {
                return Read("password");
            }
            set
            {
                Write("password", value);
            }
        }

        public string Mount
        {
            get
            {
                return Read("Mount");
            }
            set
            {
                Write("Mount", value);
            }
        }

        public string Webservice
        {
            get
            {
                return "http://" + Hostname + ":" + Port + Base + Service;
            }
        }

        private void Write(string KeyName, object Value)
        {  
            RegistryKey rk = BaseKey;
            RegistryKey sk1 = rk.CreateSubKey(SubKeyName);

            sk1.SetValue(KeyName.ToUpper(), Value);

        }

        private string Read(string KeyName)
        {
            RegistryKey rk = BaseKey;
            RegistryKey sk1 = rk.OpenSubKey(SubKeyName);

            if (sk1 == null)
            {
                return null;
            }
            else
            {
                return (string)sk1.GetValue(KeyName.ToUpper());
            }

        }

    }
}

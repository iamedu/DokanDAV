using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

using Dokan;
using DokanDAV;

namespace DAVTest
{
    public class Class1
    {

        public static void Main()
        {
            DokanOptions opt = new DokanOptions();
            opt.MountPoint = "n:\\";
            opt.DebugMode = true;
            opt.VolumeLabel = "DDV";

            int status = DokanNet.DokanMain(opt, new DAVOperations());

            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    Debug.WriteLine("Drive letter error");
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    Debug.WriteLine("Drive install error");
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    Debug.WriteLine("Mount error");
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    Debug.WriteLine("Start error");
                    break;
                case DokanNet.DOKAN_ERROR:
                    Debug.WriteLine("Unknown error");
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    Debug.WriteLine("Success");
                    break;
                default:
                    Debug.WriteLine("Unknown status: %d", status);
                    break;
            }
        }

    }
}

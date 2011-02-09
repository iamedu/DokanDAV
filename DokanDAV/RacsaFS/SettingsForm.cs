using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using Dokan;

using DokanDAV;

namespace RacsaFS
{
    public partial class SettingsForm : Form
    {
        private Settings settings;
        private Func<string, DokanDAV.DAVSize> sizeFunc;
        private DAVOperations operations;

        public SettingsForm()
        {
            InitializeComponent();
            settings = new Settings();


            sizeFunc = delegate(string username)
            {
                DokanDAV.DAVSize size = new DokanDAV.DAVSize();

                using (AVService.DAVClient client = new AVService.DAVClient())
                {
                    size.total = (ulong)client.getTotalSpace(username);
                    size.used = (ulong)client.getUsedSpace(username);
                }

                return size;
            };
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            userTxt.Text = settings.Username;
            passwordTxt.Text = settings.Password;
            driveCombo.Text = settings.Mount;
        }

        private void mountButton_Click(object sender, EventArgs e)
        {
            settings.Username = userTxt.Text;
            settings.Password = passwordTxt.Text;
            settings.Mount = driveCombo.Text;

            if (Connect())
            {
                this.Hide();
            }
            else
            {
                MessageBox.Show("Ocurrio un error, por favor revise sus datos");
            }

        }

        private void StartDAV()
        {
            Dokan.DokanOptions options = new DokanOptions();

            options.MountPoint = settings.Mount;
            options.VolumeLabel = "AV de " + settings.Username;
            options.DebugMode = true;

            int status = DokanNet.DokanMain(options, operations);

            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    MessageBox.Show("Drive incorrecto");
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    MessageBox.Show("Error al instalar el drive");
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    MessageBox.Show("Error al montar el drive");
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    MessageBox.Show("Error al iniciar el drive");
                    break;
                case DokanNet.DOKAN_ERROR:
                    MessageBox.Show("Error desconocido");
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    MessageBox.Show("Ha desmontado su AV");
                    break;
                default:
                    MessageBox.Show("Status desconocido " + status);
                    break;
            }

        }

        private bool Connect()
        {
            string basePath;

            

            using (AVService.DAVClient client = new AVService.DAVClient())
            {
                basePath = client.getBasePath(settings.Username, settings.Password);
                if (basePath == null)
                {
                    return false;
                }
                basePath = settings.Base + basePath;
            }

            operations = new DokanDAV.DAVOperations(WebdavClient.DAVProtocol.HTTP,
                                                                           settings.Hostname,
                                                                           settings.Port,
                                                                           basePath,
                                                                           settings.Username,
                                                                           settings.Password,
                                                                           sizeFunc);
            Thread t = new Thread(new ThreadStart(StartDAV));


            t.Start();
            

            return true;
        }
        
    }
}

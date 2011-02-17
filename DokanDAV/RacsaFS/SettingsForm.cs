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
        private bool mounted;

        public SettingsForm()
        {
            InitializeComponent();
            settings = new Settings();
            mounted = false;

            sizeFunc = delegate(string username)
            {
                DokanDAV.DAVSize size = new DokanDAV.DAVSize();

                try
                {

                    using (AVService.DAVClient client = new AVService.DAVClient())
                    {
                        size.total = (ulong)client.getTotalSpace(username);
                        size.used = (ulong)client.getUsedSpace(username);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return size;
            };
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            userTxt.Text = settings.Username;
            passwordTxt.Text = settings.Password;
            driveCombo.Text = settings.Mount;

            try
            {
                if (!Connect())
                {
                    WindowState = FormWindowState.Normal;
                    this.Show();
                }
            }
            catch
            {
                WindowState = FormWindowState.Normal;
                this.Show();
            }

        }

        private void mountButton_Click(object sender, EventArgs e)
        {
            DokanNet.DokanUnmount(settings.Mount[0]);
            settings.Username = userTxt.Text;
            settings.Password = passwordTxt.Text;
            settings.Mount = driveCombo.Text;
            this.Hide();

            if (!Connect())
            {
                this.Show();
                MessageBox.Show("Ocurrió un error, por favor revise sus datos");
            }

        }

        private void StartDAV()
        {
            Dokan.DokanOptions options = new DokanOptions();

            options.MountPoint = settings.Mount;
            options.VolumeLabel = "AV de " + settings.Username;
            options.DebugMode = true;

            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipTitle = "Montando AV";
            notifyIcon.BalloonTipText = "Montando AV en " + options.MountPoint;
            notifyIcon.ShowBalloonTip(3000);

            mounted = true;
            mountToolStripMenuItem.Text = "Desmontar";

            int status = DokanNet.DokanMain(options, operations);

            mountToolStripMenuItem.Text = "Montar";

            mounted = false;

            switch (status)
            {
                case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error de letra de drive";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error al instalar el drive";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                case DokanNet.DOKAN_MOUNT_ERROR:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error al tratar de montar su disco";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                case DokanNet.DOKAN_START_ERROR:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error al inicializar el disco AV";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                case DokanNet.DOKAN_ERROR:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error desconocido";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                case DokanNet.DOKAN_SUCCESS:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.BalloonTipTitle = "Disco desmontado";
                    notifyIcon.BalloonTipText = "El drive de AV ha sido desmontado";
                    notifyIcon.ShowBalloonTip(3000);
                    break;
                default:
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon.BalloonTipTitle = "Error al montar AV";
                    notifyIcon.BalloonTipText = "Error desconocido, status: " + status;
                    notifyIcon.ShowBalloonTip(3000);
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
                                                                           basePath + "/" + settings.Username,
                                                                           settings.Username,
                                                                           settings.Password,
                                                                           sizeFunc);
            Thread t = new Thread(new ThreadStart(StartDAV));


            t.Start();
            

            return true;
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DokanNet.DokanUnmount(settings.Mount[0]);
            }
            catch
            {
                Console.WriteLine("Drive not mounted");
            }

            Application.Exit();
        }

        private void desmontarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mounted)
            {
                DokanNet.DokanUnmount(settings.Mount[0]);
                mounted = false;
            }
            else
            {
                this.Hide();
                if (!Connect())
                {
                    this.Show();
                    MessageBox.Show("Ocurrió un error, por favor revise sus datos");
                }
            }
        }

        private void configuraciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Show();
        }


        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Show();
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        
    }
}

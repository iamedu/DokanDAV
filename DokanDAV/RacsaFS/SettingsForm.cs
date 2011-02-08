using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RacsaFS
{
    public partial class SettingsForm : Form
    {
        private Settings settings;
        private Func<string, DokanDAV.DAVSize> sizeFunc;

        public SettingsForm()
        {
            InitializeComponent();
            settings = new Settings();


            sizeFunc = delegate(string username)
            {
                DokanDAV.DAVSize size = new DokanDAV.DAVSize();

                using (AVService.DAVClient client = new AVService.DAVClient("DAV", settings.Webservice))
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
        }

        
    }
}

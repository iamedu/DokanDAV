namespace RacsaFS
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.usernameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.driveCombo = new System.Windows.Forms.ComboBox();
            this.userTxt = new System.Windows.Forms.TextBox();
            this.passwordTxt = new System.Windows.Forms.TextBox();
            this.mountButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "notifyIcon1";
            this.notifyIcon.Visible = true;
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(10, 25);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(55, 13);
            this.usernameLabel.TabIndex = 0;
            this.usernameLabel.Text = "Username";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Drive";
            // 
            // driveCombo
            // 
            this.driveCombo.FormattingEnabled = true;
            this.driveCombo.Items.AddRange(new object[] {
            "M:\\",
            "N:\\",
            "O:\\",
            "P:\\",
            "Q:\\",
            "R:\\"});
            this.driveCombo.Location = new System.Drawing.Point(72, 85);
            this.driveCombo.Name = "driveCombo";
            this.driveCombo.Size = new System.Drawing.Size(117, 21);
            this.driveCombo.TabIndex = 3;
            this.driveCombo.Text = "N:\\";
            // 
            // userTxt
            // 
            this.userTxt.Location = new System.Drawing.Point(72, 25);
            this.userTxt.Name = "userTxt";
            this.userTxt.Size = new System.Drawing.Size(117, 20);
            this.userTxt.TabIndex = 4;
            // 
            // passwordTxt
            // 
            this.passwordTxt.Location = new System.Drawing.Point(72, 55);
            this.passwordTxt.Name = "passwordTxt";
            this.passwordTxt.PasswordChar = '*';
            this.passwordTxt.Size = new System.Drawing.Size(117, 20);
            this.passwordTxt.TabIndex = 5;
            // 
            // mountButton
            // 
            this.mountButton.Location = new System.Drawing.Point(114, 114);
            this.mountButton.Name = "mountButton";
            this.mountButton.Size = new System.Drawing.Size(75, 23);
            this.mountButton.TabIndex = 6;
            this.mountButton.Text = "Mount";
            this.mountButton.UseVisualStyleBackColor = true;
            this.mountButton.Click += new System.EventHandler(this.mountButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(211, 149);
            this.Controls.Add(this.mountButton);
            this.Controls.Add(this.passwordTxt);
            this.Controls.Add(this.userTxt);
            this.Controls.Add(this.driveCombo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.usernameLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox driveCombo;
        private System.Windows.Forms.TextBox userTxt;
        private System.Windows.Forms.TextBox passwordTxt;
        private System.Windows.Forms.Button mountButton;
    }
}


//    File:        ConnectionForm.Designer.cs
//    Copyright:   Copyright (C) 2012 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/Minecraft-Mod-Updater
//    Description: This is intended to help Minecraft server owners who use mods make the experience of adding new mods and updating old ones easier for everyone.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
namespace ModUpdater.Admin.GUI
{
    partial class ConnectionForm
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
            this.lsSrvs = new System.Windows.Forms.ListBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtSvr = new System.Windows.Forms.TextBox();
            this.txtUsernm = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtPasswd = new System.Windows.Forms.TextBox();
            this.lblSrvs = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSvr = new System.Windows.Forms.Label();
            this.lblUsernm = new System.Windows.Forms.Label();
            this.lblPasswd = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.updaterProgressBar1 = new ModUpdater.Controls.ProgressBar();
            this.SuspendLayout();
            // 
            // lsSrvs
            // 
            this.lsSrvs.FormattingEnabled = true;
            this.lsSrvs.Location = new System.Drawing.Point(0, 27);
            this.lsSrvs.Name = "lsSrvs";
            this.lsSrvs.Size = new System.Drawing.Size(120, 108);
            this.lsSrvs.TabIndex = 6;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(229, 109);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtSvr
            // 
            this.txtSvr.Location = new System.Drawing.Point(189, 31);
            this.txtSvr.Name = "txtSvr";
            this.txtSvr.Size = new System.Drawing.Size(100, 20);
            this.txtSvr.TabIndex = 0;
            // 
            // txtUsernm
            // 
            this.txtUsernm.Location = new System.Drawing.Point(189, 57);
            this.txtUsernm.Name = "txtUsernm";
            this.txtUsernm.Size = new System.Drawing.Size(100, 20);
            this.txtUsernm.TabIndex = 2;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(295, 31);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(58, 20);
            this.txtPort.TabIndex = 1;
            // 
            // txtPasswd
            // 
            this.txtPasswd.Location = new System.Drawing.Point(189, 83);
            this.txtPasswd.Name = "txtPasswd";
            this.txtPasswd.Size = new System.Drawing.Size(100, 20);
            this.txtPasswd.TabIndex = 3;
            this.txtPasswd.UseSystemPasswordChar = true;
            // 
            // lblSrvs
            // 
            this.lblSrvs.AutoSize = true;
            this.lblSrvs.Location = new System.Drawing.Point(12, 9);
            this.lblSrvs.Name = "lblSrvs";
            this.lblSrvs.Size = new System.Drawing.Size(81, 13);
            this.lblSrvs.TabIndex = 7;
            this.lblSrvs.Text = "Recent Servers";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(167, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(97, 13);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "Connect To Server";
            // 
            // lblSvr
            // 
            this.lblSvr.AutoSize = true;
            this.lblSvr.Location = new System.Drawing.Point(145, 34);
            this.lblSvr.Name = "lblSvr";
            this.lblSvr.Size = new System.Drawing.Size(38, 13);
            this.lblSvr.TabIndex = 9;
            this.lblSvr.Text = "Server";
            // 
            // lblUsernm
            // 
            this.lblUsernm.AutoSize = true;
            this.lblUsernm.Location = new System.Drawing.Point(130, 60);
            this.lblUsernm.Name = "lblUsernm";
            this.lblUsernm.Size = new System.Drawing.Size(55, 13);
            this.lblUsernm.TabIndex = 10;
            this.lblUsernm.Text = "Username";
            // 
            // lblPasswd
            // 
            this.lblPasswd.AutoSize = true;
            this.lblPasswd.Location = new System.Drawing.Point(132, 86);
            this.lblPasswd.Name = "lblPasswd";
            this.lblPasswd.Size = new System.Drawing.Size(53, 13);
            this.lblPasswd.TabIndex = 11;
            this.lblPasswd.Text = "Password";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(148, 109);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Location = new System.Drawing.Point(12, 138);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(94, 13);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Connection Status";
            // 
            // updaterProgressBar1
            // 
            this.updaterProgressBar1.BackColor = System.Drawing.Color.Transparent;
            this.updaterProgressBar1.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(211)))), ((int)(((byte)(0)))));
            this.updaterProgressBar1.Location = new System.Drawing.Point(0, 154);
            this.updaterProgressBar1.MaxValue = 120;
            this.updaterProgressBar1.Name = "updaterProgressBar1";
            this.updaterProgressBar1.Size = new System.Drawing.Size(383, 32);
            this.updaterProgressBar1.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(211)))), ((int)(((byte)(0)))));
            this.updaterProgressBar1.Step = 20;
            this.updaterProgressBar1.TabIndex = 8;
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 188);
            this.Controls.Add(this.updaterProgressBar1);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblPasswd);
            this.Controls.Add(this.lblUsernm);
            this.Controls.Add(this.lblSvr);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSrvs);
            this.Controls.Add(this.txtPasswd);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtUsernm);
            this.Controls.Add(this.txtSvr);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lsSrvs);
            this.Name = "ConnectionForm";
            this.Text = "Connect To Server";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lsSrvs;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtSvr;
        private System.Windows.Forms.TextBox txtUsernm;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtPasswd;
        private System.Windows.Forms.Label lblSrvs;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSvr;
        private System.Windows.Forms.Label lblUsernm;
        private System.Windows.Forms.Label lblPasswd;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
        private ModUpdater.Controls.ProgressBar updaterProgressBar1;
    }
}

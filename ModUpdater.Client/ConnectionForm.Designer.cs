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
namespace ModUpdater.Client
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionForm));
            this.txtServer = new System.Windows.Forms.TextBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMcPath = new System.Windows.Forms.TextBox();
            this.btnFindMc = new System.Windows.Forms.Button();
            this.mcpathfinder = new System.Windows.Forms.FolderBrowserDialog();
            this.chkStartMC = new System.Windows.Forms.CheckBox();
            this.chkAuUpdate = new System.Windows.Forms.CheckBox();
            this.tempPortTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(73, 32);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(100, 20);
            this.txtServer.TabIndex = 0;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(134, 85);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(100, 23);
            this.btnUpdate.TabIndex = 1;
            this.btnUpdate.Text = "Update Mods";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            this.btnUpdate.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ConnectionForm_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(91, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(12, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 26);
            this.label3.TabIndex = 4;
            this.label3.Text = "Minecraft \r\nPath";
            // 
            // txtMcPath
            // 
            this.txtMcPath.Location = new System.Drawing.Point(73, 58);
            this.txtMcPath.Name = "txtMcPath";
            this.txtMcPath.Size = new System.Drawing.Size(100, 20);
            this.txtMcPath.TabIndex = 5;
            // 
            // btnFindMc
            // 
            this.btnFindMc.Location = new System.Drawing.Point(179, 56);
            this.btnFindMc.Name = "btnFindMc";
            this.btnFindMc.Size = new System.Drawing.Size(55, 23);
            this.btnFindMc.TabIndex = 6;
            this.btnFindMc.Text = "Browse";
            this.btnFindMc.UseVisualStyleBackColor = true;
            this.btnFindMc.Click += new System.EventHandler(this.btnFindMc_Click);
            // 
            // chkStartMC
            // 
            this.chkStartMC.AutoSize = true;
            this.chkStartMC.BackColor = System.Drawing.Color.Transparent;
            this.chkStartMC.ForeColor = System.Drawing.Color.White;
            this.chkStartMC.Location = new System.Drawing.Point(15, 89);
            this.chkStartMC.Name = "chkStartMC";
            this.chkStartMC.Size = new System.Drawing.Size(101, 17);
            this.chkStartMC.TabIndex = 7;
            this.chkStartMC.Text = "Start Minecraft?";
            this.chkStartMC.UseVisualStyleBackColor = false;
            // 
            // chkAuUpdate
            // 
            this.chkAuUpdate.AutoSize = true;
            this.chkAuUpdate.BackColor = System.Drawing.Color.Transparent;
            this.chkAuUpdate.ForeColor = System.Drawing.Color.White;
            this.chkAuUpdate.Location = new System.Drawing.Point(15, 112);
            this.chkAuUpdate.Name = "chkAuUpdate";
            this.chkAuUpdate.Size = new System.Drawing.Size(128, 17);
            this.chkAuUpdate.TabIndex = 8;
            this.chkAuUpdate.Text = "Enable Auto-Update?";
            this.chkAuUpdate.UseVisualStyleBackColor = false;
            this.chkAuUpdate.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // tempPortTxt
            // 
            this.tempPortTxt.Location = new System.Drawing.Point(179, 32);
            this.tempPortTxt.Name = "tempPortTxt";
            this.tempPortTxt.Size = new System.Drawing.Size(55, 20);
            this.tempPortTxt.TabIndex = 9;
            this.tempPortTxt.Text = "4713";
            // 
            // ConnectionForm
            // 
            this.AcceptButton = this.btnUpdate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(246, 132);
            this.Controls.Add(this.tempPortTxt);
            this.Controls.Add(this.chkAuUpdate);
            this.Controls.Add(this.chkStartMC);
            this.Controls.Add(this.btnFindMc);
            this.Controls.Add(this.txtMcPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.txtServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ConnectionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to update server";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMcPath;
        private System.Windows.Forms.Button btnFindMc;
        private System.Windows.Forms.FolderBrowserDialog mcpathfinder;
        private System.Windows.Forms.CheckBox chkStartMC;
        private System.Windows.Forms.CheckBox chkAuUpdate;
        private System.Windows.Forms.TextBox tempPortTxt;
    }
}

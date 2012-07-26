//    File:        ControlEditMod.Designer.cs
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
    partial class ControlEditMod
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.txtWhitelist = new System.Windows.Forms.TextBox();
            this.txtBlacklist = new System.Windows.Forms.TextBox();
            this.txtPostDownload = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.modSelector = new System.Windows.Forms.OpenFileDialog();
            this.modPlacer = new System.Windows.Forms.FolderBrowserDialog();
            this.chkOptional = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.lsRequired = new System.Windows.Forms.CheckedListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(86, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(141, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Add/Edit Mod";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(252, 327);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Save Mod";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(171, 327);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(223, 64);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Browse";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(52, 14);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(165, 20);
            this.txtName.TabIndex = 4;
            // 
            // txtAuthor
            // 
            this.txtAuthor.Location = new System.Drawing.Point(52, 41);
            this.txtAuthor.Name = "txtAuthor";
            this.txtAuthor.Size = new System.Drawing.Size(165, 20);
            this.txtAuthor.TabIndex = 5;
            // 
            // txtFile
            // 
            this.txtFile.BackColor = System.Drawing.SystemColors.Window;
            this.txtFile.Location = new System.Drawing.Point(52, 67);
            this.txtFile.Name = "txtFile";
            this.txtFile.Size = new System.Drawing.Size(165, 20);
            this.txtFile.TabIndex = 6;
            this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
            // 
            // txtWhitelist
            // 
            this.txtWhitelist.Location = new System.Drawing.Point(161, 19);
            this.txtWhitelist.Multiline = true;
            this.txtWhitelist.Name = "txtWhitelist";
            this.txtWhitelist.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtWhitelist.Size = new System.Drawing.Size(139, 245);
            this.txtWhitelist.TabIndex = 7;
            // 
            // txtBlacklist
            // 
            this.txtBlacklist.Location = new System.Drawing.Point(6, 19);
            this.txtBlacklist.Multiline = true;
            this.txtBlacklist.Name = "txtBlacklist";
            this.txtBlacklist.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBlacklist.Size = new System.Drawing.Size(135, 242);
            this.txtBlacklist.TabIndex = 8;
            // 
            // txtPostDownload
            // 
            this.txtPostDownload.Location = new System.Drawing.Point(161, 21);
            this.txtPostDownload.Multiline = true;
            this.txtPostDownload.Name = "txtPostDownload";
            this.txtPostDownload.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPostDownload.Size = new System.Drawing.Size(142, 242);
            this.txtPostDownload.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Author";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "File";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(184, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Whitelisted Users";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Blacklisted Users";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(193, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Post Download";
            // 
            // modSelector
            // 
            this.modSelector.Filter = "Zip Files|*.zip|Jar Files|*.jar|Config Files|*.cfg";
            // 
            // chkOptional
            // 
            this.chkOptional.AutoSize = true;
            this.chkOptional.Location = new System.Drawing.Point(35, 21);
            this.chkOptional.Name = "chkOptional";
            this.chkOptional.Size = new System.Drawing.Size(65, 17);
            this.chkOptional.TabIndex = 16;
            this.chkOptional.Text = "Optional";
            this.chkOptional.UseVisualStyleBackColor = true;
            this.chkOptional.CheckedChanged += new System.EventHandler(this.chkOptional_CheckedChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(6, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(314, 293);
            this.tabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.txtName);
            this.tabPage1.Controls.Add(this.txtAuthor);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.button3);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.txtFile);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(306, 267);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General Options";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.txtBlacklist);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.txtWhitelist);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(306, 267);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "User Management";
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.lsRequired);
            this.tabPage3.Controls.Add(this.txtPostDownload);
            this.tabPage3.Controls.Add(this.chkOptional);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(306, 267);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Misc";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Requirements";
            // 
            // lsRequired
            // 
            this.lsRequired.FormattingEnabled = true;
            this.lsRequired.Location = new System.Drawing.Point(3, 64);
            this.lsRequired.Name = "lsRequired";
            this.lsRequired.Size = new System.Drawing.Size(152, 199);
            this.lsRequired.TabIndex = 17;
            // 
            // ControlEditMod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.tabControl1);
            this.Name = "ControlEditMod";
            this.Size = new System.Drawing.Size(330, 350);
            this.Load += new System.EventHandler(this.ControlEditMod_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.OpenFileDialog modSelector;
        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.TextBox txtName;
        public System.Windows.Forms.TextBox txtAuthor;
        public System.Windows.Forms.TextBox txtFile;
        public System.Windows.Forms.TextBox txtWhitelist;
        public System.Windows.Forms.TextBox txtBlacklist;
        public System.Windows.Forms.TextBox txtPostDownload;
        internal System.Windows.Forms.Button button1;
        internal System.Windows.Forms.Button button2;
        private System.Windows.Forms.FolderBrowserDialog modPlacer;
        private System.Windows.Forms.CheckBox chkOptional;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox lsRequired;
    }
}

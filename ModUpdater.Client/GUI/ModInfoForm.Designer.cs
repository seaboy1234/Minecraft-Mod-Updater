//    File:        ModInfoForm.Designer.cs
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
namespace ModUpdater.Client.GUI
{
    partial class ModInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModInfoForm));
            this.lblWinName = new System.Windows.Forms.Label();
            this.lblNa = new System.Windows.Forms.Label();
            this.lblAu = new System.Windows.Forms.Label();
            this.lblFn = new System.Windows.Forms.Label();
            this.lblFs = new System.Windows.Forms.Label();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblFileN = new System.Windows.Forms.Label();
            this.lblFileS = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblWinName
            // 
            this.lblWinName.AutoSize = true;
            this.lblWinName.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWinName.Location = new System.Drawing.Point(99, 9);
            this.lblWinName.Name = "lblWinName";
            this.lblWinName.Size = new System.Drawing.Size(99, 29);
            this.lblWinName.TabIndex = 0;
            this.lblWinName.Text = "File Info";
            // 
            // lblNa
            // 
            this.lblNa.AutoSize = true;
            this.lblNa.Location = new System.Drawing.Point(106, 66);
            this.lblNa.Name = "lblNa";
            this.lblNa.Size = new System.Drawing.Size(35, 13);
            this.lblNa.TabIndex = 1;
            this.lblNa.Text = "Name";
            // 
            // lblAu
            // 
            this.lblAu.AutoSize = true;
            this.lblAu.Location = new System.Drawing.Point(106, 79);
            this.lblAu.Name = "lblAu";
            this.lblAu.Size = new System.Drawing.Size(38, 13);
            this.lblAu.TabIndex = 2;
            this.lblAu.Text = "Author";
            // 
            // lblFn
            // 
            this.lblFn.AutoSize = true;
            this.lblFn.Location = new System.Drawing.Point(106, 92);
            this.lblFn.Name = "lblFn";
            this.lblFn.Size = new System.Drawing.Size(54, 13);
            this.lblFn.TabIndex = 3;
            this.lblFn.Text = "File Name";
            // 
            // lblFs
            // 
            this.lblFs.AutoSize = true;
            this.lblFs.Location = new System.Drawing.Point(106, 105);
            this.lblFs.Name = "lblFs";
            this.lblFs.Size = new System.Drawing.Size(46, 13);
            this.lblFs.TabIndex = 4;
            this.lblFs.Text = "File Size";
            // 
            // txtDesc
            // 
            this.txtDesc.BackColor = System.Drawing.Color.White;
            this.txtDesc.Location = new System.Drawing.Point(12, 176);
            this.txtDesc.Multiline = true;
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.ReadOnly = true;
            this.txtDesc.Size = new System.Drawing.Size(300, 96);
            this.txtDesc.TabIndex = 5;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(163, 66);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "label6";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(163, 79);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(35, 13);
            this.lblAuthor.TabIndex = 7;
            this.lblAuthor.Text = "label7";
            // 
            // lblFileN
            // 
            this.lblFileN.AutoSize = true;
            this.lblFileN.Location = new System.Drawing.Point(163, 92);
            this.lblFileN.Name = "lblFileN";
            this.lblFileN.Size = new System.Drawing.Size(35, 13);
            this.lblFileN.TabIndex = 8;
            this.lblFileN.Text = "label8";
            // 
            // lblFileS
            // 
            this.lblFileS.AutoSize = true;
            this.lblFileS.Location = new System.Drawing.Point(163, 105);
            this.lblFileS.Name = "lblFileS";
            this.lblFileS.Size = new System.Drawing.Size(35, 13);
            this.lblFileS.TabIndex = 9;
            this.lblFileS.Text = "label9";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(123, 160);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Description";
            // 
            // ModInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 284);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblFileS);
            this.Controls.Add(this.lblFileN);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.lblFs);
            this.Controls.Add(this.lblFn);
            this.Controls.Add(this.lblAu);
            this.Controls.Add(this.lblNa);
            this.Controls.Add(this.lblWinName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModInfoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModInfoForm";
            this.Load += new System.EventHandler(this.ModInfoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblWinName;
        private System.Windows.Forms.Label lblNa;
        private System.Windows.Forms.Label lblAu;
        private System.Windows.Forms.Label lblFn;
        private System.Windows.Forms.Label lblFs;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblFileN;
        private System.Windows.Forms.Label lblFileS;
        private System.Windows.Forms.Label label10;
    }
}

//    File:        ControlDownloadProgress.Designer.cs
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
namespace ModUpdater.Admin.Controls
{
    partial class ControlDownloadProgress
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
            this.progress = new ModUpdater.Controls.ProgressBar();
            this.title = new System.Windows.Forms.Label();
            this.message = new System.Windows.Forms.Label();
            this.percent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progress
            // 
            this.progress.BackColor = System.Drawing.Color.Transparent;
            this.progress.Location = new System.Drawing.Point(3, 91);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(324, 32);
            this.progress.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(211)))), ((int)(((byte)(40)))));
            this.progress.Step = 0;
            this.progress.TabIndex = 0;
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(58, 11);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(217, 37);
            this.title.TabIndex = 1;
            this.title.Text = "Syncing Mods";
            // 
            // message
            // 
            this.message.AutoSize = true;
            this.message.Location = new System.Drawing.Point(11, 75);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(84, 13);
            this.message.TabIndex = 2;
            this.message.Text = "Mod Name - File";
            // 
            // percent
            // 
            this.percent.AutoSize = true;
            this.percent.Location = new System.Drawing.Point(292, 75);
            this.percent.Name = "percent";
            this.percent.Size = new System.Drawing.Size(33, 13);
            this.percent.TabIndex = 3;
            this.percent.Text = "100%";
            // 
            // ControlDownloadProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.percent);
            this.Controls.Add(this.message);
            this.Controls.Add(this.title);
            this.Controls.Add(this.progress);
            this.Name = "ControlDownloadProgress";
            this.Size = new System.Drawing.Size(330, 350);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        internal ModUpdater.Controls.ProgressBar progress;
        internal System.Windows.Forms.Label message;
        internal System.Windows.Forms.Label percent;
    }
}

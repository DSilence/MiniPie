namespace Uninstall
{
    partial class UninstallForm
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
            this.Continue = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.PreserveCache = new System.Windows.Forms.CheckBox();
            this.PreserveSettings = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // Continue
            // 
            this.Continue.Location = new System.Drawing.Point(116, 227);
            this.Continue.Name = "Continue";
            this.Continue.Size = new System.Drawing.Size(75, 23);
            this.Continue.TabIndex = 0;
            this.Continue.Text = "Continue";
            this.Continue.UseVisualStyleBackColor = true;
            this.Continue.Click += new System.EventHandler(this.Continue_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(197, 227);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // InfoLabel
            // 
            this.InfoLabel.AutoSize = true;
            this.InfoLabel.Location = new System.Drawing.Point(35, 30);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(0, 13);
            this.InfoLabel.TabIndex = 2;
            // 
            // PreserveCache
            // 
            this.PreserveCache.AutoSize = true;
            this.PreserveCache.Checked = true;
            this.PreserveCache.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PreserveCache.Location = new System.Drawing.Point(38, 85);
            this.PreserveCache.Name = "PreserveCache";
            this.PreserveCache.Size = new System.Drawing.Size(15, 14);
            this.PreserveCache.TabIndex = 3;
            this.PreserveCache.UseVisualStyleBackColor = true;
            // 
            // PreserveSettings
            // 
            this.PreserveSettings.AutoSize = true;
            this.PreserveSettings.Checked = true;
            this.PreserveSettings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.PreserveSettings.Location = new System.Drawing.Point(38, 109);
            this.PreserveSettings.Name = "PreserveSettings";
            this.PreserveSettings.Size = new System.Drawing.Size(15, 14);
            this.PreserveSettings.TabIndex = 4;
            this.PreserveSettings.UseVisualStyleBackColor = true;
            // 
            // UninstallForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 262);
            this.Controls.Add(this.PreserveSettings);
            this.Controls.Add(this.PreserveCache);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Continue);
            this.Name = "UninstallForm";
            this.Text = "Uninstall MiniPie";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Continue;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.CheckBox PreserveCache;
        private System.Windows.Forms.CheckBox PreserveSettings;
    }
}
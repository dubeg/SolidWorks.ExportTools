namespace Dubeg.Sw.ExportTools
{
    partial class FrmLogs
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._richTextBoxPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _richTextBoxPanel
            // 
            this._richTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._richTextBoxPanel.Location = new System.Drawing.Point(0, 0);
            this._richTextBoxPanel.Name = "_richTextBoxPanel";
            this._richTextBoxPanel.Size = new System.Drawing.Size(896, 598);
            this._richTextBoxPanel.TabIndex = 1;
            // 
            // FrmLogs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(896, 598);
            this.Controls.Add(this._richTextBoxPanel);
            this.Name = "FrmLogs";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel _richTextBoxPanel;
    }
}
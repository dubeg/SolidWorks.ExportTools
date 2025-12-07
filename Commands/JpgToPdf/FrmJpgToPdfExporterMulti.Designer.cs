namespace Dubeg.Sw.ExportTools.Commands.JpgToPdf;

partial class FrmJpgToPdfExporterMulti {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
            this._btnPickFile = new System.Windows.Forms.Button();
            this._lblPickFile = new System.Windows.Forms.Label();
            this._tbFilePath = new System.Windows.Forms.TextBox();
            this.lblOutputPath = new System.Windows.Forms.Label();
            this.tbOutputPath = new System.Windows.Forms.TextBox();
            this.btnOpenOutputPath = new System.Windows.Forms.Button();
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnExportToPDF = new System.Windows.Forms.Button();
            this._gridItems = new BrightIdeasSoftware.ObjectListView();
            this._wipSpinner = new Spinner();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnLogs = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._gridItems)).BeginInit();
            this.SuspendLayout();
            // 
            // _btnPickFile
            // 
            this._btnPickFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnPickFile.Location = new System.Drawing.Point(439, 34);
            this._btnPickFile.Name = "_btnPickFile";
            this._btnPickFile.Size = new System.Drawing.Size(107, 33);
            this._btnPickFile.TabIndex = 6;
            this._btnPickFile.Text = "Choisir";
            this._btnPickFile.UseVisualStyleBackColor = true;
            this._btnPickFile.Click += new System.EventHandler(this.btnPickFile_Click);
            // 
            // _lblPickFile
            // 
            this._lblPickFile.AutoSize = true;
            this._lblPickFile.Location = new System.Drawing.Point(9, 10);
            this._lblPickFile.Name = "_lblPickFile";
            this._lblPickFile.Size = new System.Drawing.Size(356, 16);
            this._lblPickFile.TabIndex = 5;
            this._lblPickFile.Text = "Choisir un fichier texte contenant une liste de fichiers .slddrw";
            // 
            // _tbFilePath
            // 
            this._tbFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tbFilePath.Location = new System.Drawing.Point(12, 39);
            this._tbFilePath.Name = "_tbFilePath";
            this._tbFilePath.ReadOnly = true;
            this._tbFilePath.Size = new System.Drawing.Size(421, 22);
            this._tbFilePath.TabIndex = 4;
            // 
            // lblOutputPath
            // 
            this.lblOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputPath.AutoSize = true;
            this.lblOutputPath.Location = new System.Drawing.Point(12, 441);
            this.lblOutputPath.Name = "lblOutputPath";
            this.lblOutputPath.Size = new System.Drawing.Size(142, 16);
            this.lblOutputPath.TabIndex = 17;
            this.lblOutputPath.Text = "Dossier de sortie (.pdf)";
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOutputPath.Location = new System.Drawing.Point(12, 462);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(349, 22);
            this.tbOutputPath.TabIndex = 16;
            // 
            // btnOpenOutputPath
            // 
            this.btnOpenOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenOutputPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenOutputPath.Location = new System.Drawing.Point(367, 455);
            this.btnOpenOutputPath.Name = "btnOpenOutputPath";
            this.btnOpenOutputPath.Size = new System.Drawing.Size(179, 37);
            this.btnOpenOutputPath.TabIndex = 15;
            this.btnOpenOutputPath.Text = "Ouvrir dossier de sortie";
            this.btnOpenOutputPath.UseVisualStyleBackColor = true;
            // 
            // btnExportToPDF
            // 
            this.btnExportToPDF.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportToPDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportToPDF.Location = new System.Drawing.Point(12, 355);
            this.btnExportToPDF.Name = "btnExportToPDF";
            this.btnExportToPDF.Size = new System.Drawing.Size(437, 46);
            this.btnExportToPDF.TabIndex = 18;
            this.btnExportToPDF.Text = "Exporter en PDF";
            this.btnExportToPDF.UseVisualStyleBackColor = true;
            this.btnExportToPDF.Click += new System.EventHandler(this.btnExportToPDF_Click);
            // 
            // _gridItems
            // 
            this._gridItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gridItems.CellEditUseWholeCell = false;
            this._gridItems.Cursor = System.Windows.Forms.Cursors.Default;
            this._gridItems.HideSelection = false;
            this._gridItems.Location = new System.Drawing.Point(12, 73);
            this._gridItems.Name = "_gridItems";
            this._gridItems.Size = new System.Drawing.Size(534, 276);
            this._gridItems.TabIndex = 0;
            this._gridItems.UseCompatibleStateImageBehavior = false;
            this._gridItems.View = System.Windows.Forms.View.Details;
            // 
            // _wipSpinner
            // 
            this._wipSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._wipSpinner.BackColor = System.Drawing.Color.Transparent;
            this._wipSpinner.Location = new System.Drawing.Point(15, 360);
            this._wipSpinner.Name = "_wipSpinner";
            this._wipSpinner.NodeBorderColor = System.Drawing.Color.White;
            this._wipSpinner.NodeBorderSize = 2;
            this._wipSpinner.NodeCount = 8;
            this._wipSpinner.NodeFillColor = System.Drawing.Color.DimGray;
            this._wipSpinner.NodeRadius = 0.8D;
            this._wipSpinner.NodeResizeRatio = 0.5D;
            this._wipSpinner.Size = new System.Drawing.Size(37, 37);
            this._wipSpinner.SpinnerRadius = 30;
            this._wipSpinner.TabIndex = 20;
            this._wipSpinner.Text = "spinner1";
            this._wipSpinner.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(455, 355);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 46);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnLogs
            // 
            this.btnLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLogs.Location = new System.Drawing.Point(12, 403);
            this.btnLogs.Name = "btnLogs";
            this.btnLogs.Size = new System.Drawing.Size(75, 27);
            this.btnLogs.TabIndex = 4;
            this.btnLogs.Text = "Logs";
            this.btnLogs.UseVisualStyleBackColor = true;
            this.btnLogs.Click += new System.EventHandler(this.btnLogs_Click);
            // 
            // FrmJpgToPdfExporterMulti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(558, 496);
            this.Controls.Add(this.btnLogs);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this._wipSpinner);
            this.Controls.Add(this._gridItems);
            this.Controls.Add(this.btnExportToPDF);
            this.Controls.Add(this.lblOutputPath);
            this.Controls.Add(this.tbOutputPath);
            this.Controls.Add(this.btnOpenOutputPath);
            this.Controls.Add(this._btnPickFile);
            this.Controls.Add(this._lblPickFile);
            this.Controls.Add(this._tbFilePath);
            this.Name = "FrmJpgToPdfExporterMulti";
            this.Text = "Exporter JPG → PDF";
            ((System.ComponentModel.ISupportInitialize)(this._gridItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button _btnPickFile;
    private System.Windows.Forms.Label _lblPickFile;
    private System.Windows.Forms.TextBox _tbFilePath;
    private System.Windows.Forms.Label lblOutputPath;
    private System.Windows.Forms.TextBox tbOutputPath;
    private System.Windows.Forms.Button btnOpenOutputPath;
    private System.Windows.Forms.OpenFileDialog _openFileDialog;
    private System.Windows.Forms.Button btnExportToPDF;
    private BrightIdeasSoftware.ObjectListView _gridItems;
    private Spinner _wipSpinner;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnLogs;
}
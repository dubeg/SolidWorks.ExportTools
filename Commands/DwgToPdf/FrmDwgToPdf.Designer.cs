namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf {
    partial class FrmDwgToPdf {
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
            this._btnImportExport = new System.Windows.Forms.Button();
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._tbFilePath = new System.Windows.Forms.TextBox();
            this._lblPickFile = new System.Windows.Forms.Label();
            this._btnPickFile = new System.Windows.Forms.Button();
            this._lblPickSheet = new System.Windows.Forms.Label();
            this._gridSheets = new System.Windows.Forms.DataGridView();
            this.cbRemoveSegmentsFromOverlappingViews = new System.Windows.Forms.CheckBox();
            this.ddlSelectMode = new System.Windows.Forms.ComboBox();
            this.lblSelectMode = new System.Windows.Forms.Label();
            this.cbFilterOnDominantLayer = new System.Windows.Forms.CheckBox();
            this._gridIgnoredLayers = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpenOutputPath = new System.Windows.Forms.Button();
            this.tbOutputPath = new System.Windows.Forms.TextBox();
            this.lblOutputPath = new System.Windows.Forms.Label();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCenterAndScale = new System.Windows.Forms.Button();
            this.btnSaveAsPDF = new System.Windows.Forms.Button();
            this.cbDisableGraphicUpdates = new System.Windows.Forms.CheckBox();
            this._wipSpinner = new Spinner();
            ((System.ComponentModel.ISupportInitialize)(this._gridSheets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridIgnoredLayers)).BeginInit();
            this.SuspendLayout();
            // 
            // _btnImportExport
            // 
            this._btnImportExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnImportExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._btnImportExport.Location = new System.Drawing.Point(601, 636);
            this._btnImportExport.Name = "_btnImportExport";
            this._btnImportExport.Size = new System.Drawing.Size(179, 37);
            this._btnImportExport.TabIndex = 0;
            this._btnImportExport.Text = "Exporter";
            this._btnImportExport.UseVisualStyleBackColor = true;
            this._btnImportExport.Click += new System.EventHandler(this.btnImportExport_Click);
            // 
            // _tbFilePath
            // 
            this._tbFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tbFilePath.Location = new System.Drawing.Point(33, 37);
            this._tbFilePath.Name = "_tbFilePath";
            this._tbFilePath.Size = new System.Drawing.Size(634, 22);
            this._tbFilePath.TabIndex = 1;
            // 
            // _lblPickFile
            // 
            this._lblPickFile.AutoSize = true;
            this._lblPickFile.Location = new System.Drawing.Point(30, 13);
            this._lblPickFile.Name = "_lblPickFile";
            this._lblPickFile.Size = new System.Drawing.Size(147, 16);
            this._lblPickFile.TabIndex = 2;
            this._lblPickFile.Text = "1. Choisir un fichier .dwg";
            // 
            // _btnPickFile
            // 
            this._btnPickFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnPickFile.Location = new System.Drawing.Point(673, 32);
            this._btnPickFile.Name = "_btnPickFile";
            this._btnPickFile.Size = new System.Drawing.Size(107, 33);
            this._btnPickFile.TabIndex = 3;
            this._btnPickFile.Text = "Choisir";
            this._btnPickFile.UseVisualStyleBackColor = true;
            this._btnPickFile.Click += new System.EventHandler(this.btnPickFile_Click);
            // 
            // _lblPickSheet
            // 
            this._lblPickSheet.AutoSize = true;
            this._lblPickSheet.Location = new System.Drawing.Point(30, 95);
            this._lblPickSheet.Name = "_lblPickSheet";
            this._lblPickSheet.Size = new System.Drawing.Size(187, 16);
            this._lblPickSheet.TabIndex = 4;
            this._lblPickSheet.Text = "2. Choisir une feuille à importer";
            // 
            // _gridSheets
            // 
            this._gridSheets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gridSheets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._gridSheets.Location = new System.Drawing.Point(33, 114);
            this._gridSheets.Name = "_gridSheets";
            this._gridSheets.RowHeadersWidth = 51;
            this._gridSheets.RowTemplate.Height = 24;
            this._gridSheets.Size = new System.Drawing.Size(747, 243);
            this._gridSheets.TabIndex = 5;
            // 
            // cbRemoveSegmentsFromOverlappingViews
            // 
            this.cbRemoveSegmentsFromOverlappingViews.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbRemoveSegmentsFromOverlappingViews.AutoSize = true;
            this.cbRemoveSegmentsFromOverlappingViews.Location = new System.Drawing.Point(257, 621);
            this.cbRemoveSegmentsFromOverlappingViews.Name = "cbRemoveSegmentsFromOverlappingViews";
            this.cbRemoveSegmentsFromOverlappingViews.Size = new System.Drawing.Size(284, 20);
            this.cbRemoveSegmentsFromOverlappingViews.TabIndex = 6;
            this.cbRemoveSegmentsFromOverlappingViews.Text = "Remove segments from overlapping views";
            this.cbRemoveSegmentsFromOverlappingViews.UseVisualStyleBackColor = true;
            // 
            // ddlSelectMode
            // 
            this.ddlSelectMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddlSelectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlSelectMode.FormattingEnabled = true;
            this.ddlSelectMode.Items.AddRange(new object[] {
            "BoxSelect",
            "OneByOne"});
            this.ddlSelectMode.Location = new System.Drawing.Point(33, 619);
            this.ddlSelectMode.Name = "ddlSelectMode";
            this.ddlSelectMode.Size = new System.Drawing.Size(209, 24);
            this.ddlSelectMode.TabIndex = 7;
            // 
            // lblSelectMode
            // 
            this.lblSelectMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSelectMode.AutoSize = true;
            this.lblSelectMode.Location = new System.Drawing.Point(33, 597);
            this.lblSelectMode.Name = "lblSelectMode";
            this.lblSelectMode.Size = new System.Drawing.Size(118, 16);
            this.lblSelectMode.TabIndex = 8;
            this.lblSelectMode.Text = "Mode de sélection";
            // 
            // cbFilterOnDominantLayer
            // 
            this.cbFilterOnDominantLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFilterOnDominantLayer.AutoSize = true;
            this.cbFilterOnDominantLayer.Location = new System.Drawing.Point(257, 645);
            this.cbFilterOnDominantLayer.Name = "cbFilterOnDominantLayer";
            this.cbFilterOnDominantLayer.Size = new System.Drawing.Size(167, 20);
            this.cbFilterOnDominantLayer.TabIndex = 9;
            this.cbFilterOnDominantLayer.Text = "Filter on dominant layer";
            this.cbFilterOnDominantLayer.UseVisualStyleBackColor = true;
            // 
            // _gridIgnoredLayers
            // 
            this._gridIgnoredLayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gridIgnoredLayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._gridIgnoredLayers.Location = new System.Drawing.Point(33, 388);
            this._gridIgnoredLayers.Name = "_gridIgnoredLayers";
            this._gridIgnoredLayers.RowHeadersWidth = 51;
            this._gridIgnoredLayers.RowTemplate.Height = 24;
            this._gridIgnoredLayers.Size = new System.Drawing.Size(747, 200);
            this._gridIgnoredLayers.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 367);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(455, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "3. Choisir des layers à ignorer (ie. layer est exclue si contient le mot spécifié" +
    ")";
            // 
            // btnOpenOutputPath
            // 
            this.btnOpenOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenOutputPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenOutputPath.Location = new System.Drawing.Point(601, 850);
            this.btnOpenOutputPath.Name = "btnOpenOutputPath";
            this.btnOpenOutputPath.Size = new System.Drawing.Size(179, 37);
            this.btnOpenOutputPath.TabIndex = 12;
            this.btnOpenOutputPath.Text = "Ouvrir dossier de sortie";
            this.btnOpenOutputPath.UseVisualStyleBackColor = true;
            // 
            // tbOutputPath
            // 
            this.tbOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbOutputPath.Location = new System.Drawing.Point(33, 857);
            this.tbOutputPath.Name = "tbOutputPath";
            this.tbOutputPath.ReadOnly = true;
            this.tbOutputPath.Size = new System.Drawing.Size(556, 22);
            this.tbOutputPath.TabIndex = 13;
            // 
            // lblOutputPath
            // 
            this.lblOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputPath.AutoSize = true;
            this.lblOutputPath.Location = new System.Drawing.Point(33, 836);
            this.lblOutputPath.Name = "lblOutputPath";
            this.lblOutputPath.Size = new System.Drawing.Size(142, 16);
            this.lblOutputPath.TabIndex = 14;
            this.lblOutputPath.Text = "Dossier de sortie (.pdf)";
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(645, 679);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(135, 37);
            this.btnCopy.TabIndex = 15;
            this.btnCopy.Text = "Copier vue";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnCenterAndScale
            // 
            this.btnCenterAndScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCenterAndScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCenterAndScale.Location = new System.Drawing.Point(645, 722);
            this.btnCenterAndScale.Name = "btnCenterAndScale";
            this.btnCenterAndScale.Size = new System.Drawing.Size(135, 37);
            this.btnCenterAndScale.TabIndex = 16;
            this.btnCenterAndScale.Text = "Centrer et aggrandir";
            this.btnCenterAndScale.UseVisualStyleBackColor = true;
            this.btnCenterAndScale.Click += new System.EventHandler(this.btnCenterAndScale_Click);
            // 
            // btnSaveAsPDF
            // 
            this.btnSaveAsPDF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAsPDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveAsPDF.Location = new System.Drawing.Point(645, 765);
            this.btnSaveAsPDF.Name = "btnSaveAsPDF";
            this.btnSaveAsPDF.Size = new System.Drawing.Size(135, 37);
            this.btnSaveAsPDF.TabIndex = 17;
            this.btnSaveAsPDF.Text = "Sauvegarder PDF";
            this.btnSaveAsPDF.UseVisualStyleBackColor = true;
            this.btnSaveAsPDF.Click += new System.EventHandler(this.btnSaveAsPDF_Click);
            // 
            // cbDisableGraphicUpdates
            // 
            this.cbDisableGraphicUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbDisableGraphicUpdates.AutoSize = true;
            this.cbDisableGraphicUpdates.Location = new System.Drawing.Point(257, 671);
            this.cbDisableGraphicUpdates.Name = "cbDisableGraphicUpdates";
            this.cbDisableGraphicUpdates.Size = new System.Drawing.Size(312, 20);
            this.cbDisableGraphicUpdates.TabIndex = 18;
            this.cbDisableGraphicUpdates.Text = "Désactiver les mises à jour graphique dans SW";
            this.cbDisableGraphicUpdates.UseVisualStyleBackColor = true;
            // 
            // _wipSpinner
            // 
            this._wipSpinner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._wipSpinner.BackColor = System.Drawing.Color.Transparent;
            this._wipSpinner.Location = new System.Drawing.Point(601, 636);
            this._wipSpinner.Name = "_wipSpinner";
            this._wipSpinner.NodeBorderColor = System.Drawing.Color.White;
            this._wipSpinner.NodeBorderSize = 2;
            this._wipSpinner.NodeCount = 8;
            this._wipSpinner.NodeFillColor = System.Drawing.Color.DimGray;
            this._wipSpinner.NodeRadius = 0.8D;
            this._wipSpinner.NodeResizeRatio = 0.5D;
            this._wipSpinner.Size = new System.Drawing.Size(37, 37);
            this._wipSpinner.SpinnerRadius = 30;
            this._wipSpinner.TabIndex = 19;
            this._wipSpinner.Text = "spinner1";
            this._wipSpinner.Visible = false;
            // 
            // FrmDwgToPdfExporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 899);
            this.Controls.Add(this._wipSpinner);
            this.Controls.Add(this.cbDisableGraphicUpdates);
            this.Controls.Add(this.btnSaveAsPDF);
            this.Controls.Add(this.btnCenterAndScale);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.lblOutputPath);
            this.Controls.Add(this.tbOutputPath);
            this.Controls.Add(this.btnOpenOutputPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._gridIgnoredLayers);
            this.Controls.Add(this.cbFilterOnDominantLayer);
            this.Controls.Add(this.lblSelectMode);
            this.Controls.Add(this.ddlSelectMode);
            this.Controls.Add(this.cbRemoveSegmentsFromOverlappingViews);
            this.Controls.Add(this._gridSheets);
            this.Controls.Add(this._lblPickSheet);
            this.Controls.Add(this._btnPickFile);
            this.Controls.Add(this._lblPickFile);
            this.Controls.Add(this._tbFilePath);
            this.Controls.Add(this._btnImportExport);
            this.Name = "FrmDwgToPdfExporter";
            this.Text = "Exporter DWG → PDF";
            ((System.ComponentModel.ISupportInitialize)(this._gridSheets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridIgnoredLayers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _btnImportExport;
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private System.Windows.Forms.TextBox _tbFilePath;
        private System.Windows.Forms.Label _lblPickFile;
        private System.Windows.Forms.Button _btnPickFile;
        private System.Windows.Forms.Label _lblPickSheet;
        private System.Windows.Forms.DataGridView _gridSheets;
        private System.Windows.Forms.CheckBox cbRemoveSegmentsFromOverlappingViews;
        private System.Windows.Forms.ComboBox ddlSelectMode;
        private System.Windows.Forms.Label lblSelectMode;
        private System.Windows.Forms.CheckBox cbFilterOnDominantLayer;
        private System.Windows.Forms.DataGridView _gridIgnoredLayers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOpenOutputPath;
        private System.Windows.Forms.TextBox tbOutputPath;
        private System.Windows.Forms.Label lblOutputPath;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCenterAndScale;
        private System.Windows.Forms.Button btnSaveAsPDF;
        private System.Windows.Forms.CheckBox cbDisableGraphicUpdates;
        private Spinner _wipSpinner;
    }
}


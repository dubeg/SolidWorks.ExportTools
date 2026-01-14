using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using Dubeg.Sw.ExportTools.Commands.Base;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands.DrawingToSvg {
    public class DrawingToSvgCommand : CommandBase<AppSettings> {
        public DrawingToSvgCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
            : base(uiMgr, swApp, appSettings, addin) {}

        private string PromptForOutputFolder() {
            var folderBrowserDialog = new FolderBrowserEx.FolderBrowserDialog {
                Title = "Select output folder for exported SVG files",
                AllowMultiSelect = false,
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                return folderBrowserDialog.SelectedFolder;
            }
            return null;
        }

        public string RunForCurrentSheet() {
            ModelDoc2 model = null;
            try {
                // 1. Prompt user to select output folder
                var outputFolderPath = PromptForOutputFolder();
                if (string.IsNullOrEmpty(outputFolderPath)) {
                    return null; // User cancelled
                }

                if (App.IActiveDoc2 == null) {
                    throw new InvalidOperationException("No active document.");
                }
                model = App.IActiveDoc2;

                // 2. Check if a drawing view is selected
                string selectedViewName = null;
                var selMgr = model.ISelectionManager;
                if (selMgr.GetSelectedObjectCount2(-1) == 1) {
                    var selType = (swSelectType_e)selMgr.GetSelectedObjectType3(1, -1);
                    if (selType == swSelectType_e.swSelDRAWINGVIEWS) {
                        var selectedView = (IView)selMgr.GetSelectedObject6(1, -1);
                        selectedViewName = selectedView.GetName2();
                    }
                }
                // App.UserControlBackground = true;
                // App.Visible = false;
                model.FeatureManager.EnableFeatureTree = false;
                model.FeatureManager.EnableFeatureTreeWindow = false;
                model.ConfigurationManager.EnableConfigurationTree = false;
                model.IActiveView.EnableGraphicsUpdate = false;
                model.SketchManager.DisplayWhenAdded = false;
                model.SketchManager.AddToDB = true;
                model.ISelectionManager.EnableContourSelection = false;
                model.ISelectionManager.EnableSelection = false;
                //model.SetBlockingState((int)swBlockingStates_e.swFullBlock);
                model.Lock();
                //App.EnableBackgroundProcessing = true;
                //var drawing = (DrawingDoc)model;
                //drawing.AutomaticViewUpdate = false;
                //drawing.BackgroundProcessingOption = (int)swBackgroundProcessOption_e.swBackgroundProcessing_Enabled;
                //App.UserControlBackground = true;
                //App.Visible = false;
                //App.DocumentVisible(Visible: false, (int)swDocumentTypes_e.swDocPART);
                //App.DocumentVisible(Visible: false, (int)swDocumentTypes_e.swDocASSEMBLY);
                //App.DocumentVisible(Visible: false, (int)swDocumentTypes_e.swDocDRAWING);
                
                // Ensure output directory exists
                if (!Directory.Exists(outputFolderPath)) {
                    Directory.CreateDirectory(outputFolderPath);
                }

                // Build output file path
                var baseFileName = Path.GetFileNameWithoutExtension(App.IActiveDoc2.GetPathName());
                var fileName = string.IsNullOrEmpty(selectedViewName) 
                    ? $"{baseFileName}.svg" 
                    : $"{baseFileName}_{selectedViewName}.svg";
                var outFilePath = Path.Combine(outputFolderPath, fileName);

                // Export SVG
                var exporter = new SvgExporter(App);
                var exportIndividualViews = string.IsNullOrEmpty(selectedViewName); // Only export individual views if no specific view selected
                var warnings = exporter.Export(outFilePath, fitToContent: true, includeBomMetadata: true, exportIndividualViews: exportIndividualViews, specificViewName: selectedViewName);

                System.Diagnostics.Process.Start("explorer", $""" "{outFilePath}" """);

                return outFilePath;
            }
            finally {
                //App.EnableBackgroundProcessing = false;
                //App.Visible = true;
                //App.UserControlBackground = false;
                if (model is not null) {
                    //var drawing = (DrawingDoc)model;
                    //drawing.BackgroundProcessingOption = (int)swBackgroundProcessOption_e.swBackgroundProcessing_Disabled;
                    //model.SetBlockingState((int)swBlockingStates_e.swNoBlock);
                    //App.DocumentVisible(Visible: true, (int)swDocumentTypes_e.swDocPART);
                    //App.DocumentVisible(Visible: true, (int)swDocumentTypes_e.swDocASSEMBLY);
                    //App.DocumentVisible(Visible: true, (int)swDocumentTypes_e.swDocDRAWING);
                    model.UnLock();
                    model.FeatureManager.EnableFeatureTree = true;
                    model.FeatureManager.EnableFeatureTreeWindow = true;
                    model.ConfigurationManager.EnableConfigurationTree = true;
                    model.IActiveView.EnableGraphicsUpdate = true;
                    model.SketchManager.DisplayWhenAdded = true;
                    model.SketchManager.AddToDB = false;
                    model.FeatureManager.UpdateFeatureTree();
                    model.ISelectionManager.EnableContourSelection = false;
                    model.ISelectionManager.EnableSelection = true;
                    // model.EditRebuild3();
                }
            }
        }
    }
}


using System;
using System.IO;
using Dubeg.Sw.ExportTools.Commands.Base;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks;

namespace Dubeg.Sw.ExportTools.Commands.DrawingToSvg {
    public class DrawingToSvgCommand : CommandBase<AppSettings> {
        public DrawingToSvgCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
            : base(uiMgr, swApp, appSettings, addin) {}

        public string RunForCurrentSheet() {
            ModelDoc2 model = null;
            try {
                if (App.IActiveDoc2 == null) {
                    throw new InvalidOperationException("No active document.");
                }
                model = App.IActiveDoc2;
                // App.UserControlBackground = true;
                // App.Visible = false;
                model.Lock();
                model.FeatureManager.EnableFeatureTree = false;
                model.ConfigurationManager.EnableConfigurationTree = false;
                model.IActiveView.EnableGraphicsUpdate = false;
                model.SketchManager.DisplayWhenAdded = false;
                model.SketchManager.AddToDB = true;

                EnsureOutputDirectoryExists();
                var outFilePath = GetOutputFilePath(App.IActiveDoc2.GetPathName(), "svg");

                var exporter = new SvgExporter(App);
                exporter.Export(outFilePath, fitToContent: true);

                System.Diagnostics.Process.Start("explorer", $""" "{outFilePath}" """);

                return outFilePath;
            }
            finally {
                if (model is not null) {
                    model.UnLock();
                    model.FeatureManager.EnableFeatureTree = true;
                    model.ConfigurationManager.EnableConfigurationTree = true;
                    model.IActiveView.EnableGraphicsUpdate = true;
                    model.SketchManager.DisplayWhenAdded = true;
                    model.SketchManager.AddToDB = false;
                    model.FeatureManager.UpdateFeatureTree();
                    model.EditRebuild3();
                }
            }
        }
    }
}


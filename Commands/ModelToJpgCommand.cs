using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dubeg.Sw.ExportTools.Commands.Base;
using Dubeg.Sw.ExportTools.Enums;
using Dubeg.Sw.ExportTools.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks;


namespace Dubeg.Sw.ExportTools.Commands;

public class ModelToJpgCommand : CommandBase<AppSettings> {
    public ModelToJpgCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin)
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public string RunForCurrentDocument(bool openAfterExport = false) {
        if (!SwApp.Documents.Any()) {
            throw new Exception("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new Exception("No document active.");
        }

        EnsureOutputDirectoryExists();
        var doc = SwApp.Documents.Active;
        var outFilePath = GetOutputFilePath(doc.Path, "jpg");

        var model = App.IActiveDoc2;
        if (model == null) {
            throw new Exception($"No file currently opened");
        }
        var modelView = model.ActiveView as IModelView;
        if (modelView == null) {
            throw new Exception("Failed to get active view");
        }

        model.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swViewDisplayHideAllTypes, true);
        model.ShowNamedView2(null, (int)swStandardViews_e.swIsometricView);
        model.ViewZoomtofit2();

        var config = (Configuration)model.GetActiveConfiguration();
        var scene = config.GetScene();
        scene.BackgroundType = (int)swSceneBackgroundType_e.swBackgroundType_Plain;
        scene.FloorReflections = false;
        scene.FloorShadows = false;

        model.ViewDisplayHiddenremoved();

        var (success, errors) = App.SaveAsJpg(
            outFilePath,
            AllOrCurrentSheet.CurrentSheet,
            swTiffImageType_e.swTiffImageGrayScale,
            ScreenOrPrintCapture.ScreenCapture,
            SheetOrPrintSize.SheetSize,
            jpgCompressionLevel: 1
        );
        if (!success) {
            throw new Exception($"Failed to export as jpg: {Path.GetFileName(outFilePath)}. Errors: {string.Join(", ", errors)}");
        }

        if (openAfterExport) {
            System.Diagnostics.Process.Start("explorer", $""" "{outFilePath}" """);
        }
        return outFilePath;
    }
}
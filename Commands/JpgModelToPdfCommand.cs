using System;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools.Commands.Base;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using System.IO;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using Dubeg.Sw.ExportTools.Utils;
using Xarial.XCad.Sketch;
using Xarial.XCad.Base;

namespace Dubeg.Sw.ExportTools.Commands;

public class JpgModelToPdfCommand : CommandBase<AppSettings> {
    public JpgModelToPdfCommand(AddinUiManager uiMgr, ISwApplication swApp, AppSettings appSettings, AddIn addin) 
        : base(uiMgr, swApp, appSettings, addin) {
    }

    public string RunForCurrentDocument(bool openPdf = false) {
        if (!SwApp.Documents.Any()) {
            throw new Exception("No documents opened.");
        }
        if (SwApp.Documents.Active is null) {
            throw new Exception("No document active.");
        }
        
        EnsureOutputDirectoryExists();
        var doc = SwApp.Documents.Active;
        var outFilePath = GetOutputFilePath(doc.Path);

        var model = App.IActiveDoc2;
        if (model == null) {
            throw new Exception($"No file currently opened");
        }
        var modelView = model.ActiveView as IModelView;
        if (modelView == null) {
            throw new Exception("Failed to get active view");
        }

        var hasPicture = SwApp.Documents.Active.Features.Any(x => x is IXSketchPicture);
        if (!hasPicture) {
            throw new Exception("No picture found in this model");
        }

        // TODO:
        // Some pictures have a border around them.
        // To get rid of it, we could try setting Visible=False on everything (IsUserFeature=true) 
        // except the picture & its sketch.

        model.ShowNamedView2(null, (int)swStandardViews_e.swFrontView);
        model.ViewZoomtofit2();

        var config = (Configuration)model.GetActiveConfiguration();
        var scene = config.GetScene();
        scene.BackgroundType = (int)swSceneBackgroundType_e.swBackgroundType_Plain;
        scene.FloorReflections = false;
        scene.FloorShadows = false;

        model.ViewDisplayHiddenremoved();

        var (success, errors) = App.SaveAsPdf(outFilePath, exportInColor: false);
        if (!success) {
            throw new Exception($"Failed to export as pdf: {Path.GetFileName(outFilePath)}. Errors: {string.Join(", ", errors)}");
        }

        if (openPdf) {
            System.Diagnostics.Process.Start("explorer", $""" "{outFilePath}" """);
        }
        return outFilePath;
    }

    public async Task<string> RunForFileAsync(string inputFilePath) => await Task.Run(() => RunForFile(inputFilePath));

    public string RunForFile(string inputFilePath) {
        var docType = Path.GetExtension(inputFilePath).ToLower() switch {
            ".sldprt" => swDocumentTypes_e.swDocPART,
            ".sldasm" => swDocumentTypes_e.swDocASSEMBLY,
            _ => throw new ArgumentException($"Unsupported file type: {inputFilePath}. Only .sldprt and .sldasm files are supported.")
        };
        var (success, errors) = App.OpenDocument(
            inputFilePath,
            docType
        );
        if (!success) {
            throw new Exception($"Failed to open file {Path.GetFileName(inputFilePath)}: {string.Join(", ", errors)}");
        }
        return RunForCurrentDocument();
    }
} 
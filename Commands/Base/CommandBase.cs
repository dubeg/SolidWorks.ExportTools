using System;
using System.IO;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools;
using SolidWorks.Interop.sldworks;

namespace Dubeg.Sw.ExportTools.Commands.Base;

public abstract class CommandBase<TAppSettings> where TAppSettings : AppSettings {
    public ISwApplication SwApp { get; private set; }
    public ISldWorks App => SwApp.Sw;
    public TAppSettings AppSettings { get; private set; }
    public AddinUiManager UiManager { get; private set; }
    public AddIn Addin { get; private set; }
    public string OutputFolderPath { get; set; }

    /// <summary>
    /// The SolidWorks command ID assigned at runtime. Null if not yet assigned.
    /// </summary>
    public int? CommandId { get; set; }

    protected CommandBase(AddinUiManager uiMgr, ISwApplication swApp, TAppSettings appSettings, AddIn addin) {
        SwApp = swApp;
        AppSettings = appSettings;
        UiManager = uiMgr;
        Addin = addin;
        OutputFolderPath = AppSettings.OutputFolderPath;
    }

    protected string GetOutputFilePath(string inputFilePath, string extension) {
        var outFileName = Path.GetFileNameWithoutExtension(inputFilePath);
        return Path.Combine(OutputFolderPath, $"{outFileName}.{extension}");
    }

    protected virtual void EnsureOutputDirectoryExists() {
        if (!Directory.Exists(OutputFolderPath)) {
            Directory.CreateDirectory(OutputFolderPath);
        }
    }

    public void CloseAllDocuments() => SwApp.Sw.CloseAllDocuments(true);
    public void CloseCurrentDocument() => SwApp.Documents.Active?.Close();
}
using System;
using System.IO;
using Xarial.XCad.SolidWorks;
using Dubeg.Sw.ExportTools;
using SolidWorks.Interop.sldworks;

namespace Dubeg.Sw.ExportTools.Commands.Base;

public abstract class CommandBase<TAppSettings> where TAppSettings: AppSettings {
    public ISwApplication SwApp { get; private set; }
    public ISldWorks App => SwApp.Sw;
    public TAppSettings AppSettings { get; private set; }
    public AddinUiManager UiManager { get; private set; }
    public AddIn Addin { get; private set; }

    protected CommandBase(AddinUiManager uiMgr, ISwApplication swApp, TAppSettings appSettings, AddIn addin) {
        SwApp = swApp;
        AppSettings = appSettings;
        UiManager = uiMgr;
        Addin = addin;
    }

    protected string GetOutputFilePath(string inputFilePath, string extension = "pdf") {
        var outFileName = Path.GetFileNameWithoutExtension(inputFilePath);
        return Path.Combine(AppSettings.OutputFolderPath, $"{outFileName}.{extension}");
    }

    protected void EnsureOutputDirectoryExists() {
        if (!Directory.Exists(AppSettings.OutputFolderPath)) {
            Directory.CreateDirectory(AppSettings.OutputFolderPath);
        }
    }
    
    public void CloseAllDocuments() => SwApp.Sw.CloseAllDocuments(true);
    public void CloseCurrentDocument() => SwApp.Documents.Active?.Close();
} 
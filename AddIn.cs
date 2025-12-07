using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Dubeg.Sw.ExportTools.Commands;
using Dubeg.Sw.ExportTools.Commands.DwgToPdf;
using Dubeg.Sw.ExportTools.Commands.JpgToPdf;
using Dubeg.Sw.ExportTools.Commands.ModelToPdf;
using Dubeg.Sw.ExportTools.Commands.DrawingToSvg;
using Dubeg.Sw.ExportTools.Commands.BomToCsv;
using Dubeg.Sw.ExportTools.Utils;
using Serilog;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using SolidWorks.Interop.sldworks;

namespace Dubeg.Sw.ExportTools;

[ComVisible(true)]
[Guid("77e25b86-f216-41fe-87df-0bd3a7139c2a")]
[Title("DUBEG Export Tools")]
[Description("")]
public class AddIn : SwAddInEx {
    public AddinUiManager UiManager { get; private set; }
    private FrmLogs _logsForm;
    private JpgToPdfCommand _jpgToPdfCommand;
    private ModelToPdfCommand _modelToPdfCommand;
    private DwgToPdfCommand _dwgToPdfCommand;
    private ViewToSheetCommand _viewToSheetCommand;
    private SheetToPdfCommand _sheetToPdfCommand;
    private ViewToPdfCommand _viewToPdfCommand;
    private CenterViewInSheetCommand _centerViewCommand;
    private ScaleViewInSheetCommand _scaleViewCommand;
    private OleObjectToSheetCommand _oleObjectToSheetCommand;
    private OleObjectToPdfCommand _oleObjectToPdfCommand;
    private JpgModelToPdfCommand _jpgModelToPdfCommand;
    private JpgOrModelToPdfCommand _jpgOrModelToPdfCommand;
    private OleOrJpgOrViewToPdfCommand _oleOrJpgOrViewToPdfCommand;
    private ModelToJpgCommand _modelToJpgCommand;
    private SheetToJpgCommand _sheetToJpgCommand;
    private DrawingToSvgCommand _drawingToSvgCommand;
    private BomToCsvCommand _bomToCsvCommand;
    private LoggingSettings _loggingSettings;

    public AddIn() {
        AppDomainLoader.Init();
    }

    public override void OnDisconnect() {
        base.OnDisconnect();
        UiManager.Stop();
    }

    public override void OnConnect() {
        try {
            base.OnConnect();
            var config = new ConfigurationBuilder()
                .SetBasePath(
                    Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location
                    )
                )
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            _loggingSettings = new LoggingSettings();
            config.GetSection("Logging").Bind(_loggingSettings);

            var logDir = _loggingSettings.LogDirPath;
            if (!Directory.Exists(logDir)) {
                Directory.CreateDirectory(logDir);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    _loggingSettings.LogFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                )
                .CreateLogger();

            UiManager = new AddinUiManager();
            UiManager.Start();

            var defaultSettings = new Commands.Base.AppSettings();
            var jpgToPdfSettings = new Commands.Base.AppSettings();
            var modelToPdfSettings = new Commands.Base.AppSettings();
            var dwgToPdfSettings = new Commands.DwgToPdf.DwgToPdfAppSettings();

            config.GetSection("Default").Bind(defaultSettings);
            config.GetSection("JpgToPdf").Bind(jpgToPdfSettings);
            config.GetSection("ModelToPdf").Bind(modelToPdfSettings);
            config.GetSection("DwgToPdf").Bind(dwgToPdfSettings);

            _jpgToPdfCommand = new JpgToPdfCommand(UiManager, Application, jpgToPdfSettings, this);
            _modelToPdfCommand = new ModelToPdfCommand(UiManager, Application, modelToPdfSettings, this);
            _dwgToPdfCommand = new DwgToPdfCommand(UiManager, Application, dwgToPdfSettings, this);
            _sheetToPdfCommand = new SheetToPdfCommand(UiManager, Application, defaultSettings, this);
            _viewToPdfCommand = new ViewToPdfCommand(UiManager, Application, defaultSettings, this);
            _viewToSheetCommand = new ViewToSheetCommand(UiManager, Application, defaultSettings, this);
            _centerViewCommand = new CenterViewInSheetCommand(UiManager, Application, defaultSettings, this);
            _scaleViewCommand = new ScaleViewInSheetCommand(UiManager, Application, defaultSettings, this);
            _oleObjectToSheetCommand = new OleObjectToSheetCommand(UiManager, Application, defaultSettings, this);
            _oleObjectToPdfCommand = new OleObjectToPdfCommand(UiManager, Application, defaultSettings, this);
            _jpgModelToPdfCommand = new JpgModelToPdfCommand(UiManager, Application, defaultSettings, this);
            _jpgOrModelToPdfCommand = new JpgOrModelToPdfCommand(UiManager, Application, defaultSettings, this);
            _oleOrJpgOrViewToPdfCommand = new OleOrJpgOrViewToPdfCommand(UiManager, Application, defaultSettings, this);
            _modelToJpgCommand = new ModelToJpgCommand(UiManager, Application, defaultSettings, this);
            _sheetToJpgCommand = new SheetToJpgCommand(UiManager, Application, defaultSettings, this);
            _drawingToSvgCommand = new DrawingToSvgCommand(UiManager, Application, defaultSettings, this);
            _bomToCsvCommand = new BomToCsvCommand(UiManager, Application, defaultSettings, this);

            UiManager.InitAndHideForm(() => _logsForm = new FrmLogs() {
                LogsFolderPath = _loggingSettings.LogDirPath
            });

            CommandManager.AddCommandGroup<AddinCommandTypes>().CommandClick += (cmd) => {
                try {
                    switch (cmd) {
                        case AddinCommandTypes.LogViewer: ShowLogViewer(); break;

                        case AddinCommandTypes.JpgToPdf: _jpgToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.JpgToPdfMulti: _jpgToPdfCommand.ShowMultiExportForm(); break;

                        case AddinCommandTypes.ModelToPdf: _modelToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.ModelToPdfMulti: _modelToPdfCommand.ShowMultiExportForm(); break;

                        case AddinCommandTypes.DwgToPdf: _dwgToPdfCommand.ExportCurrentDocument(); break;
                        case AddinCommandTypes.DwgToPdfMulti: _dwgToPdfCommand.ShowMultiExportForm(); break;

                        case AddinCommandTypes.ViewToPdf: _viewToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.SheetToPdf: _sheetToPdfCommand.RunForCurrentSheet(); break;

                        case AddinCommandTypes.ViewToSheet: _viewToSheetCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.CenterViewInSheet: _centerViewCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.ScaleViewInSheet: _scaleViewCommand.RunForCurrentDocument(); break;

                        case AddinCommandTypes.OleObjectToSheet: _oleObjectToSheetCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.OleObjectToPdf: _oleObjectToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.JpgModelToPdf: _jpgModelToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.JpgOrModelToPdf: _jpgOrModelToPdfCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.OleOrJpgOrViewToPdf: _oleOrJpgOrViewToPdfCommand.RunForCurrentDocument(); break;

                        case AddinCommandTypes.ModelToJpg: _modelToJpgCommand.RunForCurrentDocument(); break;
                        case AddinCommandTypes.DrawingToJpg: _sheetToJpgCommand.RunForCurrentSheet(); break;
                        case AddinCommandTypes.DrawingToSvg: _drawingToSvgCommand.RunForCurrentSheet(); break;
                        case AddinCommandTypes.BomToCsv: _bomToCsvCommand.RunForCurrentSheet(); break;
                    }
                }
                catch (Exception ex) {
                    var msg = "Error running command";
                    Log.Fatal(ex, msg);
                    MessageBox.Show($"{msg}: {ex.Message}", $"{typeof(AddIn).Namespace}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
        }
        catch (Exception ex) {
            var msg = "Error on addin connect";
            var msgLine = $"{msg}: {ex.Message}";
            Debug.WriteLine(msgLine);
            Log.Fatal(ex, msg);
            throw;
        }
    }

    public void ShowLogViewer() {
        _logsForm.Invoke(new Action(() => {
            _logsForm.Show();
            if (_logsForm.WindowState == FormWindowState.Minimized) {
                _logsForm.WindowState = FormWindowState.Normal;
            }
            _logsForm.Activate();
        }));
    }
}

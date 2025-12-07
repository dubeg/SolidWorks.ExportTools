using System;
using System.Threading;
using System.Windows.Forms;

namespace Dubeg.Sw.ExportTools;
public class AddinUiManager {
    private Thread _uiThread;
    private ApplicationContext _appContext;
    private SynchronizationContext _syncContext;

    public void Start() {
        _uiThread = new Thread(() => {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            _syncContext = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_syncContext);
            _appContext = new ApplicationContext();
            Application.Run(_appContext);
        });

        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.IsBackground = true;
        _uiThread.Start();

        // Wait until _syncContext is assigned (optional)
        while (_syncContext == null) Thread.Sleep(10);
    }

    public void InitAndShowForm<T>(Func<T> formGenerator) where T: Form {
        _syncContext.Post(_ => {
            var form = formGenerator.Invoke();
            form.Show();
            form.Activate();
        }, null);
    }

    public void InitAndHideForm<T>(Func<T> formGenerator) where T : Form {
        _syncContext.Post(_ => {
            var form = formGenerator.Invoke();
            form.Opacity = 0;
            form.Show();
            form.Hide();
            form.Opacity = 1;
        }, null);
    }

    public void Stop() {
        _appContext.ExitThread();
    }
}


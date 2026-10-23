using System;
using System.IO;
using System.Windows;

namespace KCD2SaveEditor.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            LogAndShowException(e.ExceptionObject as Exception, "AppDomain Unhandled Exception");
        };

        DispatcherUnhandledException += (s, e) =>
        {
            LogAndShowException(e.Exception, "Dispatcher Unhandled Exception");
            e.Handled = true;
        };
    }

    private static void LogAndShowException(Exception? ex, string title)
    {
        if (ex == null) return;
        string msg = $"{title}:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
        try
        {
            File.WriteAllText("crash_log.txt", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {title}\n{ex}\n");
        }
        catch { }
        MessageBox.Show(msg, "KCD2 Save Editor - Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

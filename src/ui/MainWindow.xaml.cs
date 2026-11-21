using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KCD2SaveEditor.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StateChanged += MainWindow_StateChanged;
        Loaded += MainWindow_Loaded;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        IntPtr handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(WndProc);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateWindowStateLayout();

        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.CheckFirstStartGameExePrompt();
        }
    }

    private void MainWindow_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                string droppedFile = files[0];
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    if (droppedFile.EndsWith(".whs", StringComparison.OrdinalIgnoreCase))
                    {
                        vm.LoadSaveFile(droppedFile);
                        string? folder = System.IO.Path.GetDirectoryName(droppedFile);
                        if (!string.IsNullOrEmpty(folder) && folder != vm.CurrentDirectoryPath)
                        {
                            vm.ScanDirectory(folder);
                        }
                    }
                    else
                    {
                        vm.StatusMessage = vm.IsGerman
                            ? "Fehler: Es können nur .whs Dateien geöffnet werden!"
                            : "Error: Only .whs files can be opened!";
                    }
                }
            }
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        UpdateWindowStateLayout();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        UpdateWindowStateLayout();
    }

    private void UpdateWindowStateLayout()
    {
        if (WindowState == WindowState.Maximized)
        {
            if (MaximizeIcon != null)
            {
                MaximizeIcon.Data = Geometry.Parse("M2,0 L8,0 L8,6 L6,6 L6,8 L0,8 L0,2 L2,2 Z M2,2 L6,2 L6,6 L2,6 Z");
            }
            if (BtnMaximize != null)
            {
                var vm = DataContext as ViewModels.MainViewModel;
                BtnMaximize.ToolTip = vm != null ? vm.Loc_WindowRestoreTip : "Restore";
            }
            if (RootBorder != null)
            {
                RootBorder.BorderThickness = new Thickness(0);
                RootBorder.Margin = new Thickness(8);
            }
        }
        else
        {
            if (MaximizeIcon != null)
            {
                MaximizeIcon.Data = Geometry.Parse("M0.5,0.5 L8.5,0.5 L8.5,8.5 L0.5,8.5 Z");
            }
            if (BtnMaximize != null)
            {
                var vm = DataContext as ViewModels.MainViewModel;
                BtnMaximize.ToolTip = vm != null ? vm.Loc_WindowMaxTip : "Maximize";
            }
            if (RootBorder != null)
            {
                RootBorder.BorderThickness = new Thickness(1);
                RootBorder.Margin = new Thickness(0);
            }
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            BtnMaximize_Click(sender, e);
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch
            {
                // Ignored if handled by WindowChrome
            }
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        UpdateWindowStateLayout();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    #region Win32 Message Hook for Borderless Maximization
    private const int WM_GETMINMAXINFO = 0x0024;
    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            WmGetMinMaxInfo(hwnd, lParam);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
        IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        if (hMonitor != IntPtr.Zero)
        {
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(hMonitor, ref mi))
            {
                int workWidth = Math.Abs(mi.rcWork.Right - mi.rcWork.Left);
                int workHeight = Math.Abs(mi.rcWork.Bottom - mi.rcWork.Top);

                mmi.ptMaxPosition.X = Math.Abs(mi.rcWork.Left - mi.rcMonitor.Left);
                mmi.ptMaxPosition.Y = Math.Abs(mi.rcWork.Top - mi.rcMonitor.Top);
                mmi.ptMaxSize.X = workWidth;
                mmi.ptMaxSize.Y = workHeight;
                mmi.ptMaxTrackSize.X = workWidth;
                mmi.ptMaxTrackSize.Y = workHeight;
            }
        }
        Marshal.StructureToPtr(mmi, lParam, true);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    #endregion
}

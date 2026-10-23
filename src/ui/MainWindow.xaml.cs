using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        string[] args = Environment.GetCommandLineArgs();
        int screenIdx = Array.IndexOf(args, "--capture-screenshots");
        if (screenIdx >= 0 && screenIdx + 1 < args.Length)
        {
            string outDir = args[screenIdx + 1];
            _ = RunScreenshotSequenceAsync(outDir);
            return;
        }

        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.CheckFirstStartGameExePrompt();
        }
    }

    private async Task RunScreenshotSequenceAsync(string outDir)
    {
        try
        {
            Directory.CreateDirectory(outDir);
            if (DataContext is not ViewModels.MainViewModel vm) return;

            WindowState = WindowState.Normal;
            Width = 1600;
            Height = 980;
            Left = 40;
            Top = 40;
            UpdateLayout();

            // Load save340.whs if not loaded
            string saveFile = @"D:\Projects\KCD_2_SaveGAme_Editor\SaveGames\save340.whs";
            if (File.Exists(saveFile) && !vm.IsSaveLoaded)
            {
                vm.LoadSaveFile(saveFile);
            }

            async Task CaptureAsync(string filename)
            {
                await Task.Delay(400);
                UpdateLayout();
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Render);
                await Task.Delay(200);

                int w = Math.Max(1, (int)ActualWidth);
                int h = Math.Max(1, (int)ActualHeight);
                var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(this);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                string targetPath = Path.Combine(outDir, filename);
                using (var fs = File.Create(targetPath))
                {
                    encoder.Save(fs);
                }
            }

            // 1. Inventory View with Henry Paperdoll & Defense Matrix
            vm.SwitchMainTabCommand.Execute("Inventar");
            vm.SelectedInventoryTarget = "Henry";
            await CaptureAsync("01_player_henry_paperdoll.png");

            // 2. Paperdoll Slot Picker Modal (Helmet slot)
            var helmSlot = vm.LeftPaperdollSlots.FirstOrDefault(s => s.Id == "slot_helm") ?? vm.LeftPaperdollSlots.FirstOrDefault();
            if (helmSlot != null)
            {
                helmSlot.ClickCommand?.Execute(null);
                await CaptureAsync("02_slot_equipment_modal.png");
                vm.CloseSlotPickerModalCommand.Execute(null);
            }

            // 3. Inventory View with Pebbles Horse Tack Slots & Tack Outfits I/II/III
            vm.SwitchMainTabCommand.Execute("Inventar");
            vm.SelectedInventoryTarget = "Pebbles";
            await CaptureAsync("03_horse_tack_inventory.png");

            // 4. Horse Tack Slot Equipment Modal (Saddle)
            var saddleSlot = vm.HorseEquipmentSlots.FirstOrDefault(s => s.Id == "horse_saddle") ?? vm.HorseEquipmentSlots.FirstOrDefault();
            if (saddleSlot != null)
            {
                saddleSlot.ClickCommand?.Execute(null);
                await CaptureAsync("04_horse_tack_modal.png");
                vm.CloseSlotPickerModalCommand.Execute(null);
            }

            // 5. Player Tab - Henry Character Attributes & Reputation
            vm.SwitchMainTabCommand.Execute("Spieler");
            vm.SwitchPlayerSubTabCommand.Execute("Henry");
            await CaptureAsync("05_player_attributes_reputation.png");

            // 6. Player Tab - Mutt Dog Companion (Skills & Obedience)
            vm.SwitchMainTabCommand.Execute("Spieler");
            vm.SwitchPlayerSubTabCommand.Execute("Dog");
            await CaptureAsync("06_companion_dog.png");

            // 7. Player Tab - Pebbles Mount Stats & Horsemanship
            vm.SwitchMainTabCommand.Execute("Spieler");
            vm.SwitchPlayerSubTabCommand.Execute("Horse");
            await CaptureAsync("07_companion_horse.png");

            // 8. Codex Item Spawner Dialog (5,604 Items Database)
            vm.SwitchMainTabCommand.Execute("Inventar");
            vm.OpenAddItemModalCommand.Execute(null);
            await CaptureAsync("08_codex_spawner_modal.png");
            vm.CloseAddItemModalCommand.Execute(null);

            // 9. Questlog Missions View (Active, Objectives, Categories)
            vm.SwitchMainTabCommand.Execute("Questlog");
            vm.SwitchQuestSubTabCommand.Execute("Missions");
            await CaptureAsync("09_questlog_missions.png");

            // 10. Key Game Characters & Fates (Radzig, Capon, Theresa, Revive)
            vm.SwitchMainTabCommand.Execute("Questlog");
            vm.SwitchQuestSubTabCommand.Execute("Fates");
            await CaptureAsync("10_questlog_fates.png");

            // 11. Crafting & Alchemy (Herbalism pouch, Blacksmithing, Autobrew)
            vm.SwitchMainTabCommand.Execute("Handwerk");
            await CaptureAsync("11_crafting_alchemy.png");

            // 12. German Localization & Platform Overview
            var deLang = vm.AvailableLanguages.FirstOrDefault(x => x.Code.Equals("German", StringComparison.OrdinalIgnoreCase) || x.Code.Equals("Deutsch", StringComparison.OrdinalIgnoreCase));
            if (deLang != null)
            {
                vm.SelectedLanguage = deLang;
            }
            vm.SwitchMainTabCommand.Execute("Inventar");
            vm.SelectedInventoryTarget = "Henry";
            await CaptureAsync("12_overview_german.png");

            // 13. English Localization Overview
            var engLang = vm.AvailableLanguages.FirstOrDefault(x => x.Code.Equals("English", StringComparison.OrdinalIgnoreCase));
            if (engLang != null)
            {
                vm.SelectedLanguage = engLang;
            }
            vm.SwitchMainTabCommand.Execute("Inventar");
            vm.SelectedInventoryTarget = "Henry";
            await CaptureAsync("13_overview_english.png");
        }
        catch (Exception ex)
        {
            try { File.WriteAllText("screenshot_error.log", ex.ToString()); } catch { }
        }
        finally
        {
            Application.Current.Shutdown(0);
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

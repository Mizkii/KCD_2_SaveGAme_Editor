using System;
using System.IO;
using System.Text.Json;

namespace KCD2SaveEditor.UI.Services
{
    public class AppSettings
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KCD2SaveEditor");

        private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");

        private static AppSettings? _current;
        public static AppSettings Current => _current ??= Load();

        public string GameExePath { get; set; } = string.Empty;
        public string GameDirectory { get; set; } = string.Empty;
        public string LastSaveDirectory { get; set; } = string.Empty;
        public string LastSavePlatform { get; set; } = string.Empty;
        public string SelectedLanguageCode { get; set; } = "English";
        public bool FirstStartCompleted { get; set; } = false;

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        // Validate paths
                        if (!string.IsNullOrEmpty(settings.GameExePath) && !File.Exists(settings.GameExePath))
                        {
                            settings.GameExePath = string.Empty;
                        }
                        if (!string.IsNullOrEmpty(settings.GameDirectory) && !Directory.Exists(settings.GameDirectory))
                        {
                            settings.GameDirectory = string.Empty;
                        }
                        return settings;
                    }
                }
            }
            catch { }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(SettingsDir))
                {
                    Directory.CreateDirectory(SettingsDir);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch { }
        }

        public static bool IsValidGameDirectory(string dir)
        {
            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return false;

            // Direct check: Localization folder or Data\Tables.pak or KingdomCome.exe
            if (Directory.Exists(Path.Combine(dir, "Localization")) ||
                File.Exists(Path.Combine(dir, "Data", "Tables.pak")) ||
                File.Exists(Path.Combine(dir, "KingdomCome.exe")))
            {
                return true;
            }

            // Check Content subfolder (e.g. Xbox installation)
            string contentSub = Path.Combine(dir, "Content");
            if (Directory.Exists(contentSub) &&
                (Directory.Exists(Path.Combine(contentSub, "Localization")) ||
                 File.Exists(Path.Combine(contentSub, "KingdomCome.exe"))))
            {
                return true;
            }

            return false;
        }

        public static string ResolveGameDirectory(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath)) return string.Empty;

            string? dir = Path.GetDirectoryName(exePath);
            if (string.IsNullOrEmpty(dir)) return string.Empty;

            // 1. If dir has Localization folder, it's the game directory
            if (Directory.Exists(Path.Combine(dir, "Localization")))
            {
                return dir;
            }

            // 2. If dir has Content\Localization (e.g., Xbox installation root)
            if (Directory.Exists(Path.Combine(dir, "Content", "Localization")))
            {
                return Path.Combine(dir, "Content");
            }

            // 3. If parent has Localization folder (e.g., exe in Bin\Win64MasterMasterPCOptimized)
            string? parent = Path.GetDirectoryName(dir);
            if (!string.IsNullOrEmpty(parent) && Directory.Exists(Path.Combine(parent, "Localization")))
            {
                return parent;
            }

            // 4. Fallback to exe folder
            return dir;
        }
    }
}

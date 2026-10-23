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

            // Direct check: Localization folder or Data\Tables.pak
            if (Directory.Exists(Path.Combine(dir, "Localization")) ||
                File.Exists(Path.Combine(dir, "Data", "Tables.pak")) ||
                Directory.Exists(Path.Combine(dir, "Data")))
            {
                return true;
            }

            // Check Content subfolder (e.g. Xbox installation)
            string contentSub = Path.Combine(dir, "Content");
            if (Directory.Exists(contentSub) &&
                (Directory.Exists(Path.Combine(contentSub, "Localization")) ||
                 File.Exists(Path.Combine(contentSub, "Data", "Tables.pak")) ||
                 Directory.Exists(Path.Combine(contentSub, "Data"))))
            {
                return true;
            }

            // Check if KingdomCome.exe is located here and parent has Localization
            string exe = FindGameExe(dir);
            if (!string.IsNullOrEmpty(exe))
            {
                string resolved = ResolveGameDirectory(exe);
                if (!string.IsNullOrEmpty(resolved) && Directory.Exists(Path.Combine(resolved, "Localization")))
                {
                    return true;
                }
            }

            return false;
        }

        public static string FindGameExe(string gameDir)
        {
            if (string.IsNullOrWhiteSpace(gameDir) || !Directory.Exists(gameDir)) return string.Empty;

            // 1. Steam specific structure: Bin\Win64MasterMasterSteamPGO\KingdomCome.exe
            string steamPgoExe = Path.Combine(gameDir, "Bin", "Win64MasterMasterSteamPGO", "KingdomCome.exe");
            if (File.Exists(steamPgoExe)) return steamPgoExe;

            // 2. Alternative Steam/GOG/Epic optimized builds
            string steamOptExe = Path.Combine(gameDir, "Bin", "Win64MasterMasterPCOptimized", "KingdomCome.exe");
            if (File.Exists(steamOptExe)) return steamOptExe;

            string win64Exe = Path.Combine(gameDir, "Bin", "Win64", "KingdomCome.exe");
            if (File.Exists(win64Exe)) return win64Exe;

            // 3. Direct root (Xbox / standard)
            string rootExe = Path.Combine(gameDir, "KingdomCome.exe");
            if (File.Exists(rootExe)) return rootExe;

            // 4. Content subfolder (Xbox root)
            string contentExe = Path.Combine(gameDir, "Content", "KingdomCome.exe");
            if (File.Exists(contentExe)) return contentExe;

            // 5. Search any KingdomCome.exe inside Bin/
            string binDir = Path.Combine(gameDir, "Bin");
            if (Directory.Exists(binDir))
            {
                try
                {
                    var files = Directory.GetFiles(binDir, "KingdomCome.exe", SearchOption.AllDirectories);
                    if (files.Length > 0) return files[0];
                }
                catch { }
            }

            return string.Empty;
        }

        public static string ResolveGameDirectory(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath)) return string.Empty;

            string? current = Path.GetDirectoryName(exePath);
            int depth = 0;
            while (!string.IsNullOrEmpty(current) && depth < 5)
            {
                // Check if current directory has Localization or Data
                if (Directory.Exists(Path.Combine(current, "Localization")) ||
                    File.Exists(Path.Combine(current, "Data", "Tables.pak")) ||
                    Directory.Exists(Path.Combine(current, "Data")))
                {
                    return current;
                }

                // Check if Content has Localization (Xbox installation)
                if (Directory.Exists(Path.Combine(current, "Content", "Localization")))
                {
                    return Path.Combine(current, "Content");
                }

                var parent = Directory.GetParent(current);
                if (parent == null || parent.FullName == current) break;
                current = parent.FullName;
                depth++;
            }

            return Path.GetDirectoryName(exePath) ?? string.Empty;
        }
    }
}

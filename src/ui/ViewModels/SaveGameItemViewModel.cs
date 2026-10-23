using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class SaveGameItemViewModel : ViewModelBase
    {
        private string _levelNameFallback = "—";
        private string _questNameFallback = "—";

        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string SaveType { get; set; } = "Unknown";
        public string SaveTypeBadgeColor { get; set; } = "#888888";
        public string Platform { get; set; } = "Steam";
        public string PlatformText => Platform.ToUpperInvariant();
        public string PlatformIcon => Platform == "Xbox" ? "🎮" : (Platform == "Steam" ? "💨" : "💾");
        public string PlatformBadgeColor => Platform == "Xbox" ? "#15803D" : (Platform == "Steam" ? "#0369A1" : "#475569");
        public int SaveId { get; set; }
        public string RawQuestKey { get; set; } = string.Empty;
        public string RawObjectiveKey { get; set; } = string.Empty;
        public string RawLevelKey { get; set; } = string.Empty;

        public string LevelName
        {
            get
            {
                if (!string.IsNullOrEmpty(RawLevelKey) && GameDataService.Instance.HasString(RawLevelKey))
                {
                    return GameDataService.Instance.GetString(RawLevelKey);
                }
                return _levelNameFallback;
            }
            set => _levelNameFallback = value;
        }

        public string QuestName
        {
            get
            {
                if (!string.IsNullOrEmpty(RawQuestKey))
                {
                    string key = RawQuestKey.TrimStart('@');
                    if (GameDataService.Instance.HasString(key))
                    {
                        return GameDataService.Instance.GetString(key);
                    }
                }
                return _questNameFallback;
            }
            set => _questNameFallback = value;
        }

        public long Timestamp { get; set; }
        public DateTime LastModified { get; set; } = DateTime.MinValue;
        public string FormattedDate { get; set; } = "—";
        public string PlaytimeFormatted { get; set; } = "—";
        public string FileSizeFormatted { get; set; } = "—";
        public string SaveSlotDisplay => $"{FileName} — {LevelName} ({QuestName} • {PlaytimeFormatted})";

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(LevelName));
            OnPropertyChanged(nameof(QuestName));
            OnPropertyChanged(nameof(SaveSlotDisplay));
        }

        public static SaveGameItemViewModel? FromFile(string filePath, string? friendlyName = null)
        {
            if (!File.Exists(filePath)) return null;

            var item = new SaveGameItemViewModel
            {
                FilePath = filePath,
                FileName = !string.IsNullOrEmpty(friendlyName) ? friendlyName : Path.GetFileName(filePath),
                Platform = (filePath.Contains("Packages", StringComparison.OrdinalIgnoreCase) ||
                            filePath.Contains("wgs", StringComparison.OrdinalIgnoreCase) ||
                            filePath.Contains("SystemAppData", StringComparison.OrdinalIgnoreCase)) ? "Xbox" : "Steam"
            };

            try
            {
                var fi = new FileInfo(filePath);
                item.FileSizeFormatted = $"{fi.Length / (1024.0 * 1024.0):F2} MB";
                item.LastModified = fi.LastWriteTime;

                // Fast header read: first 2048 bytes
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] header = new byte[Math.Min(2048, (int)fs.Length)];
                int read = fs.Read(header, 0, header.Length);

                if (read < 8) return item;

                uint magic = BitConverter.ToUInt32(header, 0);
                if (magic != 0xFFFFFFFF) return item;

                int xmlLen = BitConverter.ToInt32(header, 4);
                if (xmlLen <= 0) return item;

                int toReadXml = Math.Min(xmlLen, read - 8);
                string xml = Encoding.UTF8.GetString(header, 8, toReadXml);

                // If XML was longer than 2048 bytes, read full XML
                if (xmlLen > toReadXml && fs.Length >= 8 + xmlLen)
                {
                    byte[] fullXmlBytes = new byte[xmlLen];
                    fs.Seek(8, SeekOrigin.Begin);
                    fs.ReadExactly(fullXmlBytes, 0, xmlLen);
                    xml = Encoding.UTF8.GetString(fullXmlBytes);
                }

                string rawSaveType = ExtractAttr(xml, "SaveType");
                if (item.FileName.StartsWith("permanent", StringComparison.OrdinalIgnoreCase))
                {
                    item.SaveType = "Permanent";
                }
                else if (item.FileName.StartsWith("exit", StringComparison.OrdinalIgnoreCase) || rawSaveType.Equals("ExitSave", StringComparison.OrdinalIgnoreCase))
                {
                    item.SaveType = "Exit Save";
                }
                else if (rawSaveType.Equals("AutoSave", StringComparison.OrdinalIgnoreCase))
                {
                    item.SaveType = "Autosave";
                }
                else if (rawSaveType.Equals("QuickSave", StringComparison.OrdinalIgnoreCase))
                {
                    item.SaveType = "Quicksave";
                }
                else if (rawSaveType.Equals("ManualSave", StringComparison.OrdinalIgnoreCase))
                {
                    item.SaveType = "Manual";
                }
                else if (!string.IsNullOrEmpty(rawSaveType))
                {
                    item.SaveType = rawSaveType;
                }
                else
                {
                    item.SaveType = DetermineTypeFromFilename(item.FileName);
                }
                item.SaveTypeBadgeColor = GetColorForSaveType(item.SaveType, item.FileName);

                if (int.TryParse(ExtractAttr(xml, "SaveId"), out int id)) item.SaveId = id;
                item.LevelName = ExtractAttr(xml, "LevelName");

                // Parse UIDescription: e.g. "2|8|@qname_zachrana_FbKt||ui_countryside|1739117311|09/02/2025 16:08|1.572715|"
                string uiDesc = ExtractAttr(xml, "UIDescription");
                if (!string.IsNullOrEmpty(uiDesc))
                {
                    string[] parts = uiDesc.Split('|');
                    if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
                    {
                        item.RawQuestKey = parts[2];
                        item.QuestName = CleanQuestName(parts[2]);
                    }
                    if (parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]))
                    {
                        item.RawObjectiveKey = parts[3];
                    }
                    if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
                    {
                        item.RawLevelKey = parts[4];
                    }
                    if (parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]))
                    {
                        item.FormattedDate = parts[6];
                    }
                    if (parts.Length > 7 && float.TryParse(parts[7], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float hours))
                    {
                        int h = (int)hours;
                        int m = (int)((hours - h) * 60);
                        item.PlaytimeFormatted = $"{h}h {m}m";
                    }
                }

                string qOverride = ExtractAttr(xml, "QuestNameOverride");
                if (!string.IsNullOrEmpty(qOverride))
                {
                    string[] ovParts = qOverride.Split('|');
                    if (ovParts.Length > 0 && !string.IsNullOrWhiteSpace(ovParts[0]) && string.IsNullOrEmpty(item.RawQuestKey))
                    {
                        item.RawQuestKey = ovParts[0];
                    }
                    if (ovParts.Length > 1 && !string.IsNullOrWhiteSpace(ovParts[1]) && string.IsNullOrEmpty(item.RawObjectiveKey))
                    {
                        item.RawObjectiveKey = ovParts[1];
                    }
                }

                if (long.TryParse(ExtractAttr(xml, "SaveTime"), out long time))
                {
                    item.Timestamp = time;
                    if (item.FormattedDate == "—" && time > 0)
                    {
                        var dt = DateTimeOffset.FromUnixTimeSeconds(time).LocalDateTime;
                        item.FormattedDate = dt.ToString("yyyy-MM-dd HH:mm");
                    }
                }
            }
            catch
            {
                // Fallback to basic file info
                item.SaveType = DetermineTypeFromFilename(item.FileName);
                item.SaveTypeBadgeColor = GetColorForSaveType(item.SaveType, item.FileName);
            }

            return item;
        }

        private static string ExtractAttr(string xml, string attr)
        {
            var match = Regex.Match(xml, $"{attr}=\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string CleanQuestName(string raw)
        {
            if (raw.StartsWith("@qname_"))
            {
                string name = raw.Substring(7);
                int lastUnderscore = name.LastIndexOf('_');
                if (lastUnderscore > 0)
                {
                    name = name.Substring(0, lastUnderscore);
                }
                return name.Replace('_', ' ');
            }
            return raw;
        }

        private static string DetermineTypeFromFilename(string filename)
        {
            string lower = filename.ToLowerInvariant();
            if (lower.StartsWith("crucialdecision")) return "Crucial Decision";
            if (lower.StartsWith("permanent")) return "Permanent";
            if (lower.StartsWith("autosave")) return "Autosave";
            if (lower.StartsWith("quicksave")) return "Quicksave";
            if (lower.StartsWith("exit")) return "Exit Save";
            if (lower.StartsWith("save")) return "Manual";
            return "Savegame";
        }

        private static string GetColorForSaveType(string saveType, string filename)
        {
            string lower = (saveType + " " + filename).ToLowerInvariant();
            if (lower.Contains("crucial")) return "#FF4444";   // Vibrant Crimson for Crucial Decision
            if (lower.Contains("permanent")) return "#00E5FF"; // Bright Cyan for Permanent Milestone
            if (lower.Contains("auto")) return "#FF9100";      // Warm Orange for Autosave
            if (lower.Contains("quick")) return "#B388FF";     // Purple for Quicksave
            if (lower.Contains("exit")) return "#FFD600";      // Amber for Exit Save
            return "#D4AF37";                                  // Classic Medieval Gold for Manual Saves
        }
    }
}

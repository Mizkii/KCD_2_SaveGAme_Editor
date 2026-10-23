using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Win32;

namespace KCD2SaveEditor.UI.Services
{
    public class LanguageOption
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PakFileName { get; set; } = string.Empty;
        public string FlagEmoji { get; set; } = string.Empty;

        public override string ToString() => $"{FlagEmoji} {DisplayName}";
    }

    public class ItemMetadata
    {
        public string Guid { get; set; } = string.Empty;
        public string UiNameKey { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public float Weight { get; set; }
        public int Price { get; set; }
    }

    public class GameRecipeMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string RecipeType { get; set; } = "Alchemy"; // "Alchemy" or "Blacksmithing"
        public string Category { get; set; } = "All";
        public string UiNameKey { get; set; } = string.Empty;
        public string UiDescKey { get; set; } = string.Empty;
        public string FallbackNameDe { get; set; } = string.Empty;
        public string FallbackNameEn { get; set; } = string.Empty;
        public string BaseMaterial { get; set; } = string.Empty;
        public string IngredientsDe { get; set; } = string.Empty;
        public string IngredientsEn { get; set; } = string.Empty;
        public int MinSkillLevel { get; set; } = 1;
        public string PerkId { get; set; } = string.Empty;
        public string Icon { get; set; } = "🧪";
    }

    public class GamePerkMetadata
    {
        public string PerkId { get; set; } = string.Empty;
        public string SkillType { get; set; } = "Blacksmithing"; // "Blacksmithing" or "Alchemy"
        public string UiNameKey { get; set; } = string.Empty;
        public string UiDescKey { get; set; } = string.Empty;
        public string FallbackNameDe { get; set; } = string.Empty;
        public string FallbackNameEn { get; set; } = string.Empty;
        public string FallbackDescDe { get; set; } = string.Empty;
        public string FallbackDescEn { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public string Icon { get; set; } = "⚒️";
    }

    public class GameItemCodexDefinition
    {
        public string Guid { get; set; } = string.Empty;
        public string UiNameKey { get; set; } = string.Empty;
        public string InternalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string RawCategory { get; set; } = "Other";
        public float Weight { get; set; }
        public int Price { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Icon { get; set; } = "📦";

        public string ShortGuid => Guid.Length >= 8 ? Guid[..8] : Guid;
        public string FormattedPrice => $"{Price:N0} Gr.";
        public string FormattedWeight => $"{Weight:0.0} kg";
    }

    public class GameDataService
    {
        private static GameDataService? _instance;
        public static GameDataService Instance => _instance ??= new GameDataService();

        public event Action? LanguageChanged;
        public event Action? GameDirectoryChanged;

        private string? _gameContentPath;
        private string? _gameExePath;
        private string _activeLanguage = "English";
        private readonly Dictionary<string, ItemMetadata> _itemsByGuid = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _localizedStrings = new(StringComparer.OrdinalIgnoreCase);
        private List<GameItemCodexDefinition>? _cachedCodexItems;

        public List<LanguageOption> SupportedLanguages { get; } = new()
        {
            new LanguageOption { Code = "English", DisplayName = "English (English)", PakFileName = "English_xml.pak", FlagEmoji = "🇬🇧" },
            new LanguageOption { Code = "German", DisplayName = "Deutsch (German)", PakFileName = "German_xml.pak", FlagEmoji = "🇩🇪" },
            new LanguageOption { Code = "Czech", DisplayName = "Čeština (Czech)", PakFileName = "Czech_xml.pak", FlagEmoji = "🇨🇿" },
            new LanguageOption { Code = "French", DisplayName = "Français (French)", PakFileName = "French_xml.pak", FlagEmoji = "🇫🇷" },
            new LanguageOption { Code = "Spanish", DisplayName = "Español (Spanish)", PakFileName = "Spanish_xml.pak", FlagEmoji = "🇪🇸" },
            new LanguageOption { Code = "Italian", DisplayName = "Italiano (Italian)", PakFileName = "Italian_xml.pak", FlagEmoji = "🇮🇹" },
            new LanguageOption { Code = "Polish", DisplayName = "Polski (Polish)", PakFileName = "Polish_xml.pak", FlagEmoji = "🇵🇱" },
            new LanguageOption { Code = "Russian", DisplayName = "Русский (Russian)", PakFileName = "Russian_xml.pak", FlagEmoji = "🇷🇺" },
            new LanguageOption { Code = "Ukrainian", DisplayName = "Українська (Ukrainian)", PakFileName = "Ukrainian_xml.pak", FlagEmoji = "🇺🇦" },
            new LanguageOption { Code = "Turkish", DisplayName = "Türkçe (Turkish)", PakFileName = "Turkish_xml.pak", FlagEmoji = "🇹🇷" },
            new LanguageOption { Code = "Portuguese", DisplayName = "Português (Portuguese)", PakFileName = "Portuguese_xml.pak", FlagEmoji = "🇵🇹" },
            new LanguageOption { Code = "Japanese", DisplayName = "日本語 (Japanese)", PakFileName = "Japanese_xml.pak", FlagEmoji = "🇯🇵" },
            new LanguageOption { Code = "Korean", DisplayName = "한국어 (Korean)", PakFileName = "Korean_xml.pak", FlagEmoji = "🇰🇷" },
            new LanguageOption { Code = "Chineses", DisplayName = "简体中文 (Simplified Chinese)", PakFileName = "Chineses_xml.pak", FlagEmoji = "🇨🇳" },
            new LanguageOption { Code = "Chineset", DisplayName = "繁體中文 (Traditional Chinese)", PakFileName = "Chineset_xml.pak", FlagEmoji = "🇭🇰" },
            new LanguageOption { Code = "Vietnamese", DisplayName = "Tiếng Việt (Vietnamese)", PakFileName = "Vietnamese_xml.pak", FlagEmoji = "🇻🇳" }
        };

        public string ActiveLanguage => _activeLanguage;
        public bool IsGerman => _activeLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
        public string? GameContentPath => _gameContentPath;
        public string? GameExePath => _gameExePath;
        public bool IsGameDataLoaded => _itemsByGuid.Count > 0 && _localizedStrings.Count > 0;

        public List<GameRecipeMetadata> LoadedRecipes { get; } = new();
        public List<GamePerkMetadata> LoadedCraftingPerks { get; } = new();

        private GameDataService()
        {
            Initialize();
        }

        public void Initialize(string? customGamePath = null)
        {
            var settings = AppSettings.Current;

            // 1. Resolve Game Directory
            string? targetPath = customGamePath;
            if (string.IsNullOrEmpty(targetPath))
            {
                if (!string.IsNullOrEmpty(settings.GameDirectory) && Directory.Exists(settings.GameDirectory))
                {
                    targetPath = settings.GameDirectory;
                }
                else if (!string.IsNullOrEmpty(settings.GameExePath) && File.Exists(settings.GameExePath))
                {
                    targetPath = AppSettings.ResolveGameDirectory(settings.GameExePath);
                }
                else
                {
                    targetPath = DetectGamePath();
                }
            }

            if (string.IsNullOrEmpty(targetPath) || !Directory.Exists(targetPath))
            {
                return;
            }

            _gameContentPath = targetPath;
            string foundExe = AppSettings.FindGameExe(targetPath);
            if (!string.IsNullOrEmpty(foundExe))
            {
                _gameExePath = foundExe;
            }
            else if (!string.IsNullOrEmpty(settings.GameExePath) && File.Exists(settings.GameExePath))
            {
                _gameExePath = settings.GameExePath;
            }

            settings.GameDirectory = _gameContentPath;
            if (!string.IsNullOrEmpty(_gameExePath)) settings.GameExePath = _gameExePath;
            settings.Save();

            // 2. Resolve Active Language
            if (!string.IsNullOrEmpty(settings.SelectedLanguageCode))
            {
                _activeLanguage = settings.SelectedLanguageCode;
            }
            else
            {
                // Default to English on first start
                _activeLanguage = "English";
                settings.SelectedLanguageCode = _activeLanguage;
                settings.Save();
            }

            // 3. Load item definitions from Tables.pak
            LoadItemTables();

            // 4. Load localization strings from active pak
            LoadLocalizationPak(_activeLanguage);

            GameDirectoryChanged?.Invoke();
            LanguageChanged?.Invoke();
        }

        public bool PromptSelectGameExe(bool forcePrompt = false)
        {
            var settings = AppSettings.Current;

            if (!forcePrompt && settings.FirstStartCompleted &&
                !string.IsNullOrEmpty(_gameContentPath) && Directory.Exists(_gameContentPath))
            {
                return true;
            }

            // Determine initial directory & exe
            string initialDir = !string.IsNullOrEmpty(_gameContentPath) && Directory.Exists(_gameContentPath)
                ? _gameContentPath
                : DetectGamePath();

            string defaultExe = !string.IsNullOrEmpty(initialDir) ? AppSettings.FindGameExe(initialDir) : string.Empty;
            string dialogInitialDir = !string.IsNullOrEmpty(defaultExe) ? (Path.GetDirectoryName(defaultExe) ?? initialDir) : initialDir;

            if (string.IsNullOrEmpty(dialogInitialDir) || !Directory.Exists(dialogInitialDir))
            {
                dialogInitialDir = @"C:\Program Files (x86)\Steam\steamapps\common\KingdomComeDeliverance2";
                if (!Directory.Exists(dialogInitialDir))
                {
                    dialogInitialDir = @"D:\XboxGames\Kingdom Come- Deliverance II\Content";
                }
                if (!Directory.Exists(dialogInitialDir))
                {
                    dialogInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
            }

            var dialog = new OpenFileDialog
            {
                Title = "Kingdom Come: Deliverance II — Bitte KingdomCome.exe auswählen / Select KingdomCome.exe",
                Filter = "Kingdom Come: Deliverance II (KingdomCome.exe)|KingdomCome.exe|Alle Dateien (*.*)|*.*",
                FileName = !string.IsNullOrEmpty(defaultExe) ? defaultExe : "KingdomCome.exe",
                InitialDirectory = dialogInitialDir,
                CheckFileExists = true
            };

            var owner = System.Windows.Application.Current?.MainWindow;
            bool? dialogResult = (owner != null && owner.IsVisible) ? dialog.ShowDialog(owner) : dialog.ShowDialog();

            if (dialogResult == true)
            {
                string exePath = dialog.FileName;
                string gameDir = AppSettings.ResolveGameDirectory(exePath);

                settings.GameExePath = exePath;
                settings.GameDirectory = gameDir;
                settings.FirstStartCompleted = true;
                settings.Save();

                Initialize(gameDir);
                return true;
            }
            else
            {
                // If user cancels on first start but a valid path was detected or already present, mark completed
                if (!string.IsNullOrEmpty(_gameContentPath) && Directory.Exists(_gameContentPath))
                {
                    settings.GameDirectory = _gameContentPath;
                    string candidateExe = AppSettings.FindGameExe(_gameContentPath);
                    if (!string.IsNullOrEmpty(candidateExe)) settings.GameExePath = candidateExe;
                    settings.FirstStartCompleted = true;
                    settings.Save();
                    return true;
                }
                else if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                {
                    settings.GameDirectory = initialDir;
                    string candidateExe = AppSettings.FindGameExe(initialDir);
                    if (!string.IsNullOrEmpty(candidateExe)) settings.GameExePath = candidateExe;
                    settings.FirstStartCompleted = true;
                    settings.Save();
                    Initialize(initialDir);
                    return true;
                }
            }

            return false;
        }

        public string DetectGamePath()
        {
            var candidates = new List<string>();

            // 1. Steam Library Detection via Registry & libraryfolders.vdf
            try
            {
                var steamBases = new List<string>();
                string? regSteamPath = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")?.GetValue("SteamPath")?.ToString();
                if (!string.IsNullOrEmpty(regSteamPath)) steamBases.Add(regSteamPath.Replace('/', '\\'));

                string? regInstallPath = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam")?.GetValue("InstallPath")?.ToString();
                if (!string.IsNullOrEmpty(regInstallPath)) steamBases.Add(regInstallPath);

                steamBases.Add(@"C:\Program Files (x86)\Steam");
                steamBases.Add(@"D:\SteamLibrary");

                foreach (var sPath in steamBases.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!Directory.Exists(sPath)) continue;

                    candidates.Add(Path.Combine(sPath, "steamapps", "common", "KingdomComeDeliverance2"));
                    candidates.Add(Path.Combine(sPath, "steamapps", "common", "Kingdom Come Deliverance II"));

                    string vdf = Path.Combine(sPath, "steamapps", "libraryfolders.vdf");
                    if (File.Exists(vdf))
                    {
                        var lines = File.ReadAllLines(vdf);
                        foreach (var line in lines)
                        {
                            var match = Regex.Match(line, "\"path\"\\s+\"([^\"]+)\"");
                            if (match.Success)
                            {
                                string libPath = match.Groups[1].Value.Replace(@"\\", @"\");
                                if (Directory.Exists(libPath))
                                {
                                    candidates.Add(Path.Combine(libPath, "steamapps", "common", "KingdomComeDeliverance2"));
                                    candidates.Add(Path.Combine(libPath, "steamapps", "common", "Kingdom Come Deliverance II"));
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            // 2. Drive Scan for Steam, Xbox Game Pass PC & Epic
            string[] drives = { "C", "D", "E", "F", "G" };
            foreach (var d in drives)
            {
                candidates.Add($@"{d}:\Program Files (x86)\Steam\steamapps\common\KingdomComeDeliverance2");
                candidates.Add($@"{d}:\SteamLibrary\steamapps\common\KingdomComeDeliverance2");
                candidates.Add($@"{d}:\XboxGames\Kingdom Come- Deliverance II\Content");
                candidates.Add($@"{d}:\XboxGames\Kingdom Come- Deliverance II");
                candidates.Add($@"{d}:\Program Files\Epic Games\KingdomComeDeliverance2");
            }

            foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (Directory.Exists(path) && AppSettings.IsValidGameDirectory(path))
                {
                    if (Directory.Exists(Path.Combine(path, "Content", "Localization")))
                    {
                        return Path.Combine(path, "Content");
                    }
                    return path;
                }
            }

            return string.Empty;
        }

        private string DetectLanguageFromGameSettings(string gamePath)
        {
            try
            {
                string logBackupsDir = Path.Combine(gamePath, "%user%", "logbackups");
                if (Directory.Exists(logBackupsDir))
                {
                    var logFiles = Directory.GetFiles(logBackupsDir, "kcd*.log")
                        .OrderByDescending(f => File.GetLastWriteTime(f));

                    foreach (var log in logFiles)
                    {
                        try
                        {
                            using var fs = new FileStream(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            using var reader = new StreamReader(fs, Encoding.UTF8);
                            string? line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Contains("[Localization] - language from profile:", StringComparison.OrdinalIgnoreCase))
                                {
                                    var match = Regex.Match(line, @"language from profile:\s*'([^']+)'", RegexOptions.IgnoreCase);
                                    if (match.Success)
                                    {
                                        string langFound = match.Groups[1].Value.Trim();
                                        var matched = SupportedLanguages.FirstOrDefault(l => l.Code.Equals(langFound, StringComparison.OrdinalIgnoreCase));
                                        if (matched != null) return matched.Code;
                                    }
                                }
                                else if (line.Contains("System language:", StringComparison.OrdinalIgnoreCase))
                                {
                                    var match = Regex.Match(line, @"System language:\s*(\w+)", RegexOptions.IgnoreCase);
                                    if (match.Success)
                                    {
                                        string sysLang = match.Groups[1].Value.Trim();
                                        var matched = SupportedLanguages.FirstOrDefault(l => l.Code.Equals(sysLang, StringComparison.OrdinalIgnoreCase));
                                        if (matched != null) return matched.Code;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            return twoLetter switch
            {
                "de" => "German",
                "cs" => "Czech",
                "fr" => "French",
                "es" => "Spanish",
                "it" => "Italian",
                "pl" => "Polish",
                "ru" => "Russian",
                "uk" => "Ukrainian",
                "tr" => "Turkish",
                "pt" => "Portuguese",
                "ja" => "Japanese",
                "ko" => "Korean",
                "zh" => "Chineses",
                "vi" => "Vietnamese",
                _ => "English"
            };
        }

        private void LoadItemTables()
        {
            _itemsByGuid.Clear();
            if (string.IsNullOrEmpty(_gameContentPath)) return;

            string tablesPakPath = Path.Combine(_gameContentPath, "Data", "Tables.pak");
            if (!File.Exists(tablesPakPath)) return;

            try
            {
                using var zip = ZipFile.OpenRead(tablesPakPath);
                var itemEntries = zip.Entries.Where(e =>
                    e.FullName.StartsWith("Libs/Tables/item/item", StringComparison.OrdinalIgnoreCase) &&
                    e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                    !e.FullName.Contains("category", StringComparison.OrdinalIgnoreCase) &&
                    !e.FullName.Contains("sound", StringComparison.OrdinalIgnoreCase) &&
                    !e.FullName.Contains("manipulation", StringComparison.OrdinalIgnoreCase) &&
                    !e.FullName.Contains("tag", StringComparison.OrdinalIgnoreCase));

                foreach (var entry in itemEntries)
                {
                    try
                    {
                        using var stream = entry.Open();
                        var doc = XDocument.Load(stream);
                        var itemClasses = doc.Root?.Element("ItemClasses");
                        if (itemClasses == null) continue;

                        bool isHorseEntry = entry.FullName.IndexOf("horse", StringComparison.OrdinalIgnoreCase) >= 0;

                        foreach (var elem in itemClasses.Elements())
                        {
                            string? id = elem.Attribute("Id")?.Value;
                            if (string.IsNullOrEmpty(id)) continue;

                            string uiName = elem.Attribute("UIName")?.Value ?? string.Empty;
                            string internalName = elem.Attribute("Name")?.Value ?? string.Empty;
                            float.TryParse(elem.Attribute("Weight")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float weight);
                            int.TryParse(elem.Attribute("Price")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int price);

                            var meta = new ItemMetadata
                            {
                                Guid = id.ToLowerInvariant(),
                                UiNameKey = uiName,
                                Tag = isHorseEntry ? "horse" : elem.Name.LocalName,
                                InternalName = internalName,
                                Weight = weight,
                                Price = price
                            };

                            _itemsByGuid[meta.Guid] = meta;
                        }
                    }
                    catch { }
                }

                // Load Alchemy Recipes, Blacksmith Recipes, and Crafting Perks
                LoadCraftingTables(zip);
            }
            catch
            {
                LoadCraftingTables(null);
            }
        }

        private void LoadCraftingTables(ZipArchive? zip)
        {
            LoadedRecipes.Clear();
            LoadedCraftingPerks.Clear();

            if (zip != null)
            {
                // 1. Load Alchemy Recipes from Libs/Tables/minigame/AlchemyRecipe.xml
                try
                {
                    var alchEntry = zip.GetEntry("Libs/Tables/minigame/AlchemyRecipe.xml");
                    if (alchEntry != null)
                    {
                        using var stream = alchEntry.Open();
                        var doc = XDocument.Load(stream);
                        var recipes = doc.Root?.Element("AlchemyRecipes")?.Elements("AlchemyRecipe")
                                   ?? doc.Descendants("AlchemyRecipe");

                        foreach (var elem in recipes)
                        {
                            string id = elem.Attribute("RecipeId")?.Value ?? "";
                            if (string.IsNullOrEmpty(id)) continue;

                            string uiName = elem.Attribute("UIName")?.Value ?? "";
                            string uiUsage = elem.Attribute("UIUsage")?.Value ?? "";
                            string baseMat = elem.Attribute("BaseMaterial")?.Value ?? "Water";

                            var ingList = new List<string>();
                            foreach (var ing in elem.Descendants("AlchemyIngredient"))
                            {
                                string ingId = ing.Attribute("IngredientItemId")?.Value?.ToLowerInvariant() ?? "";
                                string qty = ing.Attribute("Quantity")?.Value ?? "1";
                                string name = ingId;
                                if (_itemsByGuid.TryGetValue(ingId, out var im))
                                {
                                    if (!string.IsNullOrEmpty(im.UiNameKey) && !im.UiNameKey.Equals("ui_nm_warning", StringComparison.OrdinalIgnoreCase))
                                    {
                                        name = im.UiNameKey;
                                    }
                                    else if (!string.IsNullOrEmpty(im.InternalName))
                                    {
                                        name = CleanInternalName(im.InternalName);
                                    }
                                }
                                ingList.Add($"{qty}x {name}");
                            }
                            string ingText = $"{baseMat}, {string.Join(", ", ingList)}";

                            string cat = "Healing";
                            string icon = "🧪";
                            if (uiName.Contains("bane", StringComparison.OrdinalIgnoreCase) ||
                                uiName.Contains("poison", StringComparison.OrdinalIgnoreCase) ||
                                uiName.Contains("paralysis", StringComparison.OrdinalIgnoreCase))
                            {
                                cat = "Poison";
                                icon = "☠️";
                            }
                            else if (uiName.Contains("perfume", StringComparison.OrdinalIgnoreCase) ||
                                     uiName.Contains("soap", StringComparison.OrdinalIgnoreCase))
                            {
                                cat = "Special";
                                icon = "🧴";
                            }
                            else if (uiName.Contains("ammo", StringComparison.OrdinalIgnoreCase))
                            {
                                cat = "Special";
                                icon = "💣";
                            }

                            LoadedRecipes.Add(new GameRecipeMetadata
                            {
                                Id = "alch_" + id,
                                RecipeType = "Alchemy",
                                Category = cat,
                                UiNameKey = uiName,
                                UiDescKey = uiUsage,
                                BaseMaterial = baseMat,
                                IngredientsDe = ingText,
                                IngredientsEn = ingText,
                                MinSkillLevel = 1,
                                Icon = icon
                            });
                        }
                    }
                }
                catch { }

                // 2. Load Blacksmith Recipes from Libs/Tables/minigame/BlacksmithRecipes.xml
                try
                {
                    var smithEntry = zip.GetEntry("Libs/Tables/minigame/BlacksmithRecipes.xml");
                    if (smithEntry != null)
                    {
                        using var stream = smithEntry.Open();
                        var doc = XDocument.Load(stream);
                        var recipes = doc.Root?.Element("BlacksmithRecipes")?.Elements("BlacksmithRecipe")
                                   ?? doc.Descendants("BlacksmithRecipe");

                        foreach (var elem in recipes)
                        {
                            string id = elem.Attribute("Id")?.Value ?? "";
                            if (string.IsNullOrEmpty(id)) continue;

                            string cat = elem.Attribute("Category")?.Value ?? "Other";
                            int.TryParse(elem.Attribute("MinSkillLevel")?.Value, out int minSkill);
                            string perkId = elem.Attribute("PerkId")?.Value ?? "";

                            var uiNameElem = elem.Element("UIName");
                            string uiNameKey = uiNameElem?.Attribute("StringName")?.Value ?? "";
                            string fallbackText = uiNameElem?.Attribute("Text")?.Value ?? id;

                            var uiDescElem = elem.Element("UIDesc");
                            string uiDescKey = uiDescElem?.Attribute("StringName")?.Value ?? "";

                            var ingList = new List<string>();
                            foreach (var ing in elem.Descendants("BlacksmithRecipeIngredient"))
                            {
                                string ingId = ing.Attribute("ItemClassId")?.Value?.ToLowerInvariant() ?? "";
                                string qty = ing.Attribute("Amount")?.Value ?? "1";
                                string name = ingId;
                                if (_itemsByGuid.TryGetValue(ingId, out var im))
                                {
                                    if (!string.IsNullOrEmpty(im.UiNameKey) && !im.UiNameKey.Equals("ui_nm_warning", StringComparison.OrdinalIgnoreCase))
                                    {
                                        name = im.UiNameKey;
                                    }
                                    else if (!string.IsNullOrEmpty(im.InternalName))
                                    {
                                        name = im.InternalName.Contains("iron", StringComparison.OrdinalIgnoreCase) ? "Eisen" :
                                               im.InternalName.Contains("hilt", StringComparison.OrdinalIgnoreCase) ? "Griffbeschlag" :
                                               CleanInternalName(im.InternalName);
                                    }
                                }
                                ingList.Add($"{qty}x {name}");
                            }
                            string ingText = string.Join(", ", ingList);

                            string icon = "⚔️";
                            if (cat.Equals("Axe", StringComparison.OrdinalIgnoreCase)) icon = "🪓";
                            else if (cat.Equals("Other", StringComparison.OrdinalIgnoreCase) || id.Contains("horseshoe", StringComparison.OrdinalIgnoreCase)) icon = "🐎";
                            else if (cat.Equals("ShortSword", StringComparison.OrdinalIgnoreCase)) icon = "🗡️";

                            LoadedRecipes.Add(new GameRecipeMetadata
                            {
                                Id = "bs_" + id,
                                RecipeType = "Blacksmithing",
                                Category = cat,
                                UiNameKey = uiNameKey,
                                UiDescKey = uiDescKey,
                                FallbackNameDe = fallbackText,
                                FallbackNameEn = fallbackText,
                                IngredientsDe = ingText,
                                IngredientsEn = ingText,
                                MinSkillLevel = minSkill > 0 ? minSkill : 1,
                                PerkId = perkId,
                                Icon = icon
                            });
                        }
                    }
                }
                catch { }

                // 3. Load Craftsmanship & Alchemy Perks from Libs/Tables/rpg/perk__kcd2.xml
                try
                {
                    var perkEntry = zip.GetEntry("Libs/Tables/rpg/perk__kcd2.xml");
                    if (perkEntry != null)
                    {
                        using var stream = perkEntry.Open();
                        var doc = XDocument.Load(stream);
                        var perks = doc.Root?.Element("perks")?.Elements("perk")
                                 ?? doc.Descendants("perk");

                        foreach (var elem in perks)
                        {
                            string skillSel = elem.Attribute("skill_selector")?.Value ?? "";
                            if (skillSel != "6" && skillSel != "8") continue;

                            string perkId = elem.Attribute("perk_id")?.Value ?? "";
                            string perkName = elem.Attribute("perk_name")?.Value ?? "";
                            string perkUiName = elem.Attribute("perk_ui_name")?.Value ?? "";
                            string perkUiDesc = elem.Attribute("perk_ui_desc")?.Value ?? "";
                            string visibility = elem.Attribute("visibility")?.Value ?? "2";
                            if (visibility == "0" && string.IsNullOrEmpty(perkUiName)) continue;
                            int.TryParse(elem.Attribute("level")?.Value, out int level);

                            string skillType = skillSel == "8" ? "Blacksmithing" : "Alchemy";
                            string icon = skillSel == "8" ? "⚒️" : "🧪";

                            LoadedCraftingPerks.Add(new GamePerkMetadata
                            {
                                PerkId = perkId,
                                SkillType = skillType,
                                UiNameKey = perkUiName,
                                UiDescKey = perkUiDesc,
                                FallbackNameDe = perkName,
                                FallbackNameEn = perkName,
                                Level = level > 0 ? level : 1,
                                Icon = icon
                            });
                        }
                    }
                }
                catch { }
            }

            // Fallback catalog if tables were empty or unreadable
            if (LoadedRecipes.Count == 0)
            {
                PopulateDefaultRecipesFallback();
            }

            if (LoadedCraftingPerks.Count == 0)
            {
                PopulateDefaultPerksFallback();
            }
        }

        private void PopulateDefaultRecipesFallback()
        {
            // Alchemy Fallbacks
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_1", RecipeType = "Alchemy", Category = "Healing", UiNameKey = "recipe_potion_fever_name", UiDescKey = "recipe_potion_saviourSchnapps_usage", FallbackNameDe = "Retterschnaps", FallbackNameEn = "Saviour Schnapps", BaseMaterial = "Wine", IngredientsDe = "Wein, 1x Nessel, 2x Tollkirsche", IngredientsEn = "Wine, 1x Nettle, 2x Belladonna", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_2", RecipeType = "Alchemy", Category = "Healing", UiNameKey = "recipe_potion_marigold_decoction_name", UiDescKey = "recipe_potion_marigold_decoction_usage", FallbackNameDe = "Ringelblumensud", FallbackNameEn = "Marigold Decoction", BaseMaterial = "Water", IngredientsDe = "Wasser, 1x Nessel, 2x Ringelblume", IngredientsEn = "Water, 1x Nettle, 2x Marigold", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_3", RecipeType = "Alchemy", Category = "Healing", UiNameKey = "recipe_potion_lazarus_name", UiDescKey = "recipe_potion_lazarus_usage", FallbackNameDe = "Lazarus-Elixier", FallbackNameEn = "Lazarus Potion", BaseMaterial = "Water", IngredientsDe = "Wasser, 1x Baldrian, 2x Beinwell", IngredientsEn = "Water, 1x Valerian, 2x Comfrey", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_4", RecipeType = "Alchemy", Category = "Special", UiNameKey = "recipe_potion_nighthawk_name", UiDescKey = "recipe_potion_nighthawk_usage", FallbackNameDe = "Nachtfalke", FallbackNameEn = "Nighthawk", BaseMaterial = "Water", IngredientsDe = "Wasser, 2x Augentrost, 1x Tollkirsche", IngredientsEn = "Water, 2x Eyebright, 1x Belladonna", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_5", RecipeType = "Alchemy", Category = "Poison", UiNameKey = "recipe_potion_bane_name", UiDescKey = "recipe_potion_bane_usage", FallbackNameDe = "Fluchtrank", FallbackNameEn = "Bane Potion", BaseMaterial = "Oil", IngredientsDe = "Öl, 1x Wermut, 2x Tollkirsche", IngredientsEn = "Oil, 1x Wormwood, 2x Belladonna", Icon = "☠️" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_6", RecipeType = "Alchemy", Category = "Special", UiNameKey = "recipe_potion_sneak_name", UiDescKey = "recipe_potion_sneak_usage", FallbackNameDe = "Leisetreter", FallbackNameEn = "Padfoot Potion", BaseMaterial = "Water", IngredientsDe = "Wasser, 1x Baldrian, 1x Kamille", IngredientsEn = "Water, 1x Valerian, 1x Chamomile", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_7", RecipeType = "Alchemy", Category = "Healing", UiNameKey = "recipe_potion_antidote_name", UiDescKey = "recipe_potion_antidote_usage", FallbackNameDe = "Gegengift", FallbackNameEn = "Antidote", BaseMaterial = "Water", IngredientsDe = "Wasser, 2x Distel, 1x Brennnessel", IngredientsEn = "Water, 2x Thistle, 1x Nettle", Icon = "🧪" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "alch_8", RecipeType = "Alchemy", Category = "Special", UiNameKey = "recipe_potion_archer_name", UiDescKey = "recipe_potion_archer_usage", FallbackNameDe = "Bogenschützen-Gelee", FallbackNameEn = "Bowman's Brew", BaseMaterial = "Spiritus", IngredientsDe = "Weingeist, 2x Augentrost, 1x Johanniskraut", IngredientsEn = "Spirits, 2x Eyebright, 1x St. John's Wort", Icon = "🧪" });

            // Blacksmithing Fallbacks
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_radzig", RecipeType = "Blacksmithing", Category = "LongSword", UiNameKey = "ui_nm_radzigSword", FallbackNameDe = "Radzigs Schwert (Neu geschmiedet)", FallbackNameEn = "Sir Radzig's Sword (Reforged)", IngredientsDe = "5x Hochwertiger Stahl, 2x Leder, 1x Goldfaden", IngredientsEn = "5x High Quality Steel, 2x Leather, 1x Gold Thread", MinSkillLevel = 15, Icon = "⚔️" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_henry", RecipeType = "Blacksmithing", Category = "LongSword", UiNameKey = "ui_nm_henrySword", FallbackNameDe = "Heinrichs Meisterschwert", FallbackNameEn = "Henry's Master Longsword", IngredientsDe = "4x Gehärteter Stahl, 2x Lederriemen", IngredientsEn = "4x Hardened Steel, 2x Leather Straps", MinSkillLevel = 10, Icon = "⚔️" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_stgeorge", RecipeType = "Blacksmithing", Category = "ShortSword", UiNameKey = "ui_nm_swordStGeorge", FallbackNameDe = "St. Georgs-Kurzschwert", FallbackNameEn = "St. George Shortsword", IngredientsDe = "3x Damaszener Stahl, 1x Silberbeschlag", IngredientsEn = "3x Damascus Steel, 1x Silver Fitting", MinSkillLevel = 8, Icon = "🗡️" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_lilium", RecipeType = "Blacksmithing", Category = "LongSword", UiNameKey = "ui_nm_longSwordLilium", FallbackNameDe = "Lilien-Langschwert", FallbackNameEn = "Lilium Longsword", IngredientsDe = "4x Gussstahl, 2x Feines Leder", IngredientsEn = "4x Cast Steel, 2x Fine Leather", MinSkillLevel = 7, Icon = "⚔️" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_battleaxe", RecipeType = "Blacksmithing", Category = "Axe", UiNameKey = "ui_nm_axeBattle01", FallbackNameDe = "Streitaxt des Ritters", FallbackNameEn = "Knight's Battle Axe", IngredientsDe = "4x Gehärtetes Eisen, 1x Eschenholz", IngredientsEn = "4x Hardened Iron, 1x Ashwood Handle", MinSkillLevel = 5, Icon = "🪓" });
            LoadedRecipes.Add(new GameRecipeMetadata { Id = "bs_horseshoe", RecipeType = "Blacksmithing", Category = "Other", UiNameKey = "ui_nm_horseshoeMilitary", FallbackNameDe = "Ritter-Hufeisen (+5 Tempo)", FallbackNameEn = "Knight's Horseshoes (+5 Speed)", IngredientsDe = "5x Eisen, 2x Hufnägel", IngredientsEn = "5x Iron, 2x Horseshoe Nails", MinSkillLevel = 3, Icon = "🐎" });
        }

        private void PopulateDefaultPerksFallback()
        {
            // Blacksmithing Perks
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_martin", SkillType = "Blacksmithing", UiNameKey = "perk_the_secret_of_blacksmith_martin_name", FallbackNameDe = "Das Geheimnis des Schmieds Martin", FallbackNameEn = "The Secret of Blacksmith Martin", FallbackDescDe = "Alle selbst geschmiedeten Waffen erhalten +15% Schärfe und Haltbarkeit.", FallbackDescEn = "All self-forged weapons gain +15% sharpness and durability.", Level = 15, Icon = "⚒️" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_steel", SkillType = "Blacksmithing", UiNameKey = "perk_hardened_steel_name", FallbackNameDe = "Gehärteter Stahl", FallbackNameEn = "Hardened Steel", FallbackDescDe = "Klingen bleiben nach dem Schleifen doppelt so lange scharf.", FallbackDescEn = "Blades stay sharp twice as long after grinding.", Level = 8, Icon = "⚒️" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_razor", SkillType = "Blacksmithing", UiNameKey = "perk_razor_sharp_name", FallbackNameDe = "Rasiermesserscharf", FallbackNameEn = "Razor-Sharp", FallbackDescDe = "Erhöht den Hiebschaden von geschärften Waffen um 12%.", FallbackDescEn = "Increases slash damage of sharpened weapons by 12%.", Level = 5, Icon = "⚒️" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_help", SkillType = "Blacksmithing", UiNameKey = "perk_help_in_the_workshop_name", FallbackNameDe = "Helfende Hand in der Schmiede", FallbackNameEn = "Helping Hand in the Workshop", FallbackDescDe = "Verringert den Materialverbrauch beim Schmieden um 25%.", FallbackDescEn = "Reduces material consumption during smithing by 25%.", Level = 3, Icon = "⚒️" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_maint", SkillType = "Blacksmithing", UiNameKey = "perk_thorough_maintenance_name", FallbackNameDe = "Gründliche Waffenpflege I & II", FallbackNameEn = "Thorough Maintenance I & II", FallbackDescDe = "Rüstungs- und Waffenreparaturen halten 50% länger.", FallbackDescEn = "Armor and weapon repairs last 50% longer.", Level = 6, Icon = "⚒️" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_bs_seven", SkillType = "Blacksmithing", UiNameKey = "perk_sevenleague_name", FallbackNameDe = "Siebenmeilenstiefel", FallbackNameEn = "Seven-League Boots", FallbackDescDe = "Erhöht die Reit- und Laufgeschwindigkeit mit selbstgepflegtem Schuhwerk.", FallbackDescEn = "Increases running and riding speed with maintained footwear.", Level = 10, Icon = "⚒️" });

            // Alchemy Perks
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_alch_water", SkillType = "Alchemy", UiNameKey = "perk_water_of_life_name", FallbackNameDe = "Wasser des Lebens", FallbackNameEn = "Water of Life", FallbackDescDe = "Heiltränke heilen 20% schneller und stellen zusätzliche Energie wieder her.", FallbackDescEn = "Healing potions heal 20% faster and restore additional energy.", Level = 10, Icon = "🧪" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_alch_poison", SkillType = "Alchemy", UiNameKey = "perk_poison_specialist_name", FallbackNameDe = "Giftspezialist I & II", FallbackNameEn = "Poison Specialist I & II", FallbackDescDe = "Vergiftete Klingen behalten ihre toxische Wirkung für dreimal mehr Schläge.", FallbackDescEn = "Poisoned blades retain their toxic effect for three times more strikes.", Level = 8, Icon = "🧪" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_alch_powder", SkillType = "Alchemy", UiNameKey = "perk_experimental_powder_name", FallbackNameDe = "Experimentelles Schießpulver", FallbackNameEn = "Experimental Powder", FallbackDescDe = "Erhöht die Durchschlagskraft von Musketen und Handbüchsen um 15%.", FallbackDescEn = "Increases armor penetration of firearms by 15%.", Level = 12, Icon = "💣" });
            LoadedCraftingPerks.Add(new GamePerkMetadata { PerkId = "prk_alch_purple", SkillType = "Alchemy", UiNameKey = "perk_purple_haze_name", FallbackNameDe = "Purpurner Dunst", FallbackNameEn = "Purple Haze", FallbackDescDe = "Erlaubt das automatische Brauen von bekannten Rezepten in doppelter Menge.", FallbackDescEn = "Allows auto-brewing of known recipes in double quantity.", Level = 14, Icon = "🧪" });
        }

        public void SetLanguage(string languageCode)
        {
            var opt = SupportedLanguages.FirstOrDefault(x => x.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
            if (opt == null) return;

            _activeLanguage = opt.Code;
            AppSettings.Current.SelectedLanguageCode = _activeLanguage;
            AppSettings.Current.Save();

            InvalidateCodexCache();
            LoadLocalizationPak(_activeLanguage);
            LanguageChanged?.Invoke();
        }

        private void LoadLocalizationPak(string languageCode)
        {
            _localizedStrings.Clear();
            if (string.IsNullOrEmpty(_gameContentPath)) return;

            var opt = SupportedLanguages.FirstOrDefault(x => x.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
            string pakName = opt?.PakFileName ?? $"{languageCode}_xml.pak";
            string pakPath = Path.Combine(_gameContentPath, "Localization", pakName);

            if (!File.Exists(pakPath))
            {
                pakPath = Path.Combine(_gameContentPath, "Localization", "English_xml.pak");
                if (!File.Exists(pakPath)) return;
            }

            try
            {
                using var zip = ZipFile.OpenRead(pakPath);
                // Load all UI, item, quest, soul, misc and rich presence XMLs (skip 31MB text_ui_dialog.xml for high speed)
                var textEntries = zip.Entries.Where(e =>
                    e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) &&
                    !e.FullName.Equals("text_ui_dialog.xml", StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var entry in textEntries)
                {
                    try
                    {
                        using var stream = entry.Open();
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        var doc = XDocument.Load(reader);

                        foreach (var row in doc.Descendants("Row"))
                        {
                            var cells = row.Elements("Cell").ToList();
                            if (cells.Count >= 2)
                            {
                                string key = cells[0].Value.Trim();
                                string val = cells.Last().Value.Trim();
                                if (string.IsNullOrEmpty(val) && cells.Count > 1)
                                {
                                    val = cells[1].Value.Trim();
                                }
                                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                                {
                                    _localizedStrings[key] = val;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public bool HasString(string key)
        {
            return !string.IsNullOrEmpty(key) && _localizedStrings.ContainsKey(key);
        }

        public string GetString(string key, string fallback = "")
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            if (_localizedStrings.TryGetValue(key, out string? val) && !string.IsNullOrWhiteSpace(val))
            {
                return val;
            }
            return fallback;
        }

        // =====================================================================
        // OFFICIAL GAME KEY MAPPINGS & MULTI-LANGUAGE EDITOR LABELS
        // =====================================================================
        private static readonly Dictionary<string, string> OfficialKeyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Nav_Questlog", "ui_tab_questlog_map" },
            { "Nav_Inventar", "ui_tab_inventory_player" },
            { "Nav_Spieler", "ui_helpoverlay_RPG_s0000" },
            { "Nav_Handwerk", "ui_tab_crafts" },
            { "Henry", "char_26_uiName" },
            { "Horse", "ui_tab_horse" },
            { "Dog", "char_667_uiName" },
            { "Pebbles", "char_KUN_SEDIVKA_uiName" },
            { "Strength", "ui_helpoverlay_RPG_s1300" },
            { "Agility", "ui_helpoverlay_RPG_s1400" },
            { "Vitality", "ui_helpoverlay_RPG_s1500" },
            { "Speech", "ui_helpoverlay_RPG_s1600" },
            { "Stamina", "ui_derivstat_maxstamina" },
            { "Health", "ui_helpoverlay_RPG_s1800" },
            { "Energy", "ui_helpoverlay_RPG_s1900" },
            { "Nourishment", "ui_helpoverlay_RPG_s2000" },
            { "Charisma", "ui_derivstat_charisma" },
            { "Courage", "ui_stat_courage" },
            { "Capacity", "ui_derivstat_inventorycapacity" },
            { "All", "ui_tab_all" },
            { "Main", "ui_tab_main" },
            { "Side", "ui_tab_side" },
            { "Armour", "ui_tab_armor" },
            { "Food", "ui_tab_food" },
            { "Books", "ui_tab_documents" },
            { "Herbs", "ui_tab_craft_herbs" },
            { "Potions", "ui_tab_craft_potions" },
            { "Completed", "ui_tab_completed" },
            { "Failed", "ui_tab_failed" },
            { "Dlcs", "ui_menu_dlcs_group" },
            { "Reputation", "ui_tab_reputation" },
            { "Perks", "ui_helpoverlay_RPG_s2200" }
        };

        public string GetUiText(string uiKey, string? fallbackDe = null, string? fallbackEn = null)
        {
            // 1. Check if it's an official game key or mapped official key
            if (OfficialKeyMap.TryGetValue(uiKey, out string? officialKey))
            {
                if (_localizedStrings.TryGetValue(officialKey, out string? gameStr) && !string.IsNullOrWhiteSpace(gameStr))
                {
                    return gameStr;
                }
            }

            // 2. Check if the uiKey itself is directly present in game localization
            if (_localizedStrings.TryGetValue(uiKey, out string? directStr) && !string.IsNullOrWhiteSpace(directStr))
            {
                return directStr;
            }

            // 3. Fallback to Multi-Language Editor Dictionary
            if (EditorLocDictionary.TryGetValue(uiKey, out var langDict))
            {
                if (langDict.TryGetValue(_activeLanguage, out string? localized))
                {
                    return localized;
                }
                if (langDict.TryGetValue("English", out string? enVal))
                {
                    return enVal;
                }
            }

            // 4. Fallback to provided De / En
            bool isGerman = _activeLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
            if (isGerman && !string.IsNullOrEmpty(fallbackDe)) return fallbackDe;
            if (!string.IsNullOrEmpty(fallbackEn)) return fallbackEn;
            return fallbackDe ?? uiKey;
        }

        private static readonly Dictionary<string, Dictionary<string, string>> EditorLocDictionary = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Loc_ActiveSave"] = new()
            {
                ["German"] = "AKTIVER SPIELSTAND (.WHS)",
                ["English"] = "ACTIVE SAVEGAME FILE (.WHS)",
                ["Czech"] = "AKTIVNÍ ULOŽENÁ POZICE (.WHS)",
                ["French"] = "SAUVEGARDE ACTIVE (.WHS)",
                ["Spanish"] = "PARTIDA GUARDADA ACTIVA (.WHS)",
                ["Italian"] = "SALVATAGGIO ATTIVO (.WHS)",
                ["Polish"] = "AKTYWNY ZAPIS GRY (.WHS)",
                ["Russian"] = "АКТИВНОЕ СОХРАНЕНИЕ (.WHS)",
                ["Ukrainian"] = "АКТИВНЕ ЗБЕРЕЖЕННЯ (.WHS)",
                ["Turkish"] = "AKTİF KAYIT DOSYASI (.WHS)",
                ["Portuguese"] = "SALVAMENTO ATIVO (.WHS)",
                ["Japanese"] = "アクティブなセーブファイル (.WHS)",
                ["Korean"] = "활성 저장 파일 (.WHS)",
                ["Chineses"] = "当前存档文件 (.WHS)",
                ["Chineset"] = "目前存檔檔案 (.WHS)",
                ["Vietnamese"] = "TẬP TIN LƯU HIỆN TẠI (.WHS)"
            },
            ["Loc_OpenSave"] = new()
            {
                ["German"] = "📁 Öffnen...",
                ["English"] = "📁 Open...",
                ["Czech"] = "📁 Otevřít...",
                ["French"] = "📁 Ouvrir...",
                ["Spanish"] = "📁 Abrir...",
                ["Italian"] = "📁 Apri...",
                ["Polish"] = "📁 Otwórz...",
                ["Russian"] = "📁 Открыть...",
                ["Ukrainian"] = "📁 Відкрити...",
                ["Turkish"] = "📁 Aç...",
                ["Portuguese"] = "📁 Abrir...",
                ["Japanese"] = "📁 開く...",
                ["Korean"] = "📁 열기...",
                ["Chineses"] = "📁 打开...",
                ["Chineset"] = "📁 開啟...",
                ["Vietnamese"] = "📁 Mở..."
            },
            ["Loc_OpenSaveTip"] = new()
            {
                ["German"] = "Einzelne .whs Save-Datei auswählen",
                ["English"] = "Select individual .whs save file",
                ["Czech"] = "Vybrat konkrétní soubor .whs",
                ["French"] = "Sélectionner un fichier de sauvegarde .whs",
                ["Spanish"] = "Seleccionar archivo de guardado .whs",
                ["Italian"] = "Seleziona file di salvataggio .whs",
                ["Polish"] = "Wybierz pojedynczy plik zapisu .whs",
                ["Russian"] = "Выбрать отдельный файл сохранения .whs",
                ["Ukrainian"] = "Вибрати окремий файл збереження .whs",
                ["Turkish"] = "Tek bir .whs kayıt dosyası seçin",
                ["Portuguese"] = "Selecionar arquivo de salvamento .whs",
                ["Japanese"] = "個別の .whs セーブファイルを選択",
                ["Korean"] = "개별 .whs 저장 파일 선택",
                ["Chineses"] = "选择单个 .whs 存档文件",
                ["Chineset"] = "選擇單個 .whs 存檔檔案",
                ["Vietnamese"] = "Chọn tệp lưu .whs riêng lẻ"
            },
            ["Loc_AutoDetect"] = new()
            {
                ["German"] = "🔍 Auto-Detect",
                ["English"] = "🔍 Auto-Detect",
                ["Czech"] = "🔍 Automaticky najít",
                ["French"] = "🔍 Détection auto",
                ["Spanish"] = "🔍 Autodetectar",
                ["Italian"] = "🔍 Rilevamento auto",
                ["Polish"] = "🔍 Autowykrywanie",
                ["Russian"] = "🔍 Автопоиск",
                ["Ukrainian"] = "🔍 Автопошук",
                ["Turkish"] = "🔍 Otomatik Bul",
                ["Portuguese"] = "🔍 Detecção auto",
                ["Japanese"] = "🔍 自動検出",
                ["Korean"] = "🔍 자동 감지",
                ["Chineses"] = "🔍 自动检测",
                ["Chineset"] = "🔍 自動偵測",
                ["Vietnamese"] = "🔍 Tự động phát hiện"
            },
            ["Loc_AutoDetectTip"] = new()
            {
                ["German"] = "Savegame-Verzeichnis automatisch scannen (.whs)",
                ["English"] = "Automatically scan savegame directory (.whs)",
                ["Czech"] = "Automaticky prohledat složku se savy (.whs)",
                ["French"] = "Scanner automatiquement les sauvegardes (.whs)",
                ["Spanish"] = "Escanear automáticamente partidas (.whs)",
                ["Italian"] = "Scansiona cartella di salvataggio (.whs)",
                ["Polish"] = "Automatycznie przeskanuj folder zapisów (.whs)",
                ["Russian"] = "Автоматически сканировать папку сохранений (.whs)",
                ["Ukrainian"] = "Автоматично сканувати каталог збережень (.whs)",
                ["Turkish"] = "Kayıt dizinini otomatik tara (.whs)",
                ["Portuguese"] = "Escanear pasta de salvamentos (.whs)",
                ["Japanese"] = "セーブゲームフォルダを自動スキャン (.whs)",
                ["Korean"] = "저장 폴더 자동 스캔 (.whs)",
                ["Chineses"] = "自动扫描存档目录 (.whs)",
                ["Chineset"] = "自動掃描存檔目錄 (.whs)",
                ["Vietnamese"] = "Tự động quét thư mục lưu (.whs)"
            },
            ["Loc_RepairAll"] = new()
            {
                ["German"] = "100% Reparieren",
                ["English"] = "Repair 100%",
                ["Czech"] = "Opravit na 100%",
                ["French"] = "Réparer à 100%",
                ["Spanish"] = "Reparar al 100%",
                ["Italian"] = "Ripara al 100%",
                ["Polish"] = "Napraw w 100%",
                ["Russian"] = "Починить на 100%",
                ["Ukrainian"] = "Полагодити на 100%",
                ["Turkish"] = "%100 Onar",
                ["Portuguese"] = "Reparar 100%",
                ["Japanese"] = "100% 修理",
                ["Korean"] = "100% 수리",
                ["Chineses"] = "100% 修复",
                ["Chineset"] = "100% 修復",
                ["Vietnamese"] = "Sửa chữa 100%"
            },
            ["Loc_Legalize"] = new()
            {
                ["German"] = "Legalisieren",
                ["English"] = "Legalize",
                ["Czech"] = "Legalizovat",
                ["French"] = "Légaliser",
                ["Spanish"] = "Legalizar",
                ["Italian"] = "Legalizza",
                ["Polish"] = "Zalegalizuj",
                ["Russian"] = "Очистить краденое",
                ["Ukrainian"] = "Очистити крадене",
                ["Turkish"] = "Yasallaştır",
                ["Portuguese"] = "Legalizar",
                ["Japanese"] = "盗品解除",
                ["Korean"] = "장물 해제",
                ["Chineses"] = "清除赃物标记",
                ["Chineset"] = "清除贓物標記",
                ["Vietnamese"] = "Hợp pháp hóa"
            },
            ["Loc_MaxStats"] = new()
            {
                ["German"] = "Max Werte (30)",
                ["English"] = "Max Stats (30)",
                ["Czech"] = "Max statistiky (30)",
                ["French"] = "Stats max (30)",
                ["Spanish"] = "Estadísticas máx (30)",
                ["Italian"] = "Statistiche max (30)",
                ["Polish"] = "Maks. statystyki (30)",
                ["Russian"] = "Макс. характеристики (30)",
                ["Ukrainian"] = "Макс. характеристики (30)",
                ["Turkish"] = "Maks Nitelikler (30)",
                ["Portuguese"] = "Atributos máx (30)",
                ["Japanese"] = "最大ステータス (30)",
                ["Korean"] = "최대 능력치 (30)",
                ["Chineses"] = "最大属性 (30)",
                ["Chineset"] = "最大屬性 (30)",
                ["Vietnamese"] = "Chỉ số tối đa (30)"
            },
            ["Loc_ExportSave"] = new()
            {
                ["German"] = "Savegame Exportieren (.WHS)",
                ["English"] = "Export Savegame (.WHS)",
                ["Czech"] = "Exportovat pozici (.WHS)",
                ["French"] = "Exporter la sauvegarde (.WHS)",
                ["Spanish"] = "Exportar partida (.WHS)",
                ["Italian"] = "Esporta salvataggio (.WHS)",
                ["Polish"] = "Eksportuj zapis (.WHS)",
                ["Russian"] = "Экспорт сохранения (.WHS)",
                ["Ukrainian"] = "Експорт збереження (.WHS)",
                ["Turkish"] = "Kaydı Dışa Aktar (.WHS)",
                ["Portuguese"] = "Exportar salvamento (.WHS)",
                ["Japanese"] = "セーブをエクスポート (.WHS)",
                ["Korean"] = "저장 파일 내보내기 (.WHS)",
                ["Chineses"] = "导出存档 (.WHS)",
                ["Chineset"] = "匯出存檔 (.WHS)",
                ["Vietnamese"] = "Xuất tệp lưu (.WHS)"
            },
            ["Loc_GameExePath"] = new()
            {
                ["German"] = "🎮 Spieldateien / Exe:",
                ["English"] = "🎮 Game Files / Exe:",
                ["Czech"] = "🎮 Herní složka / Exe:",
                ["French"] = "🎮 Fichiers du jeu / Exe:",
                ["Spanish"] = "🎮 Archivos del juego / Exe:",
                ["Italian"] = "🎮 File di gioco / Exe:",
                ["Polish"] = "🎮 Pliki gry / Exe:",
                ["Russian"] = "🎮 Файлы игры / Exe:",
                ["Ukrainian"] = "🎮 Файли гри / Exe:",
                ["Turkish"] = "🎮 Oyun Dosyaları / Exe:",
                ["Portuguese"] = "🎮 Arquivos do jogo / Exe:",
                ["Japanese"] = "🎮 ゲームファイル / Exe:",
                ["Korean"] = "🎮 게임 파일 / Exe:",
                ["Chineses"] = "🎮 游戏文件 / Exe:",
                ["Chineset"] = "🎮 遊戲檔案 / Exe:",
                ["Vietnamese"] = "🎮 Tệp trò chơi / Exe:"
            },
            ["Loc_SelectHorseLabel"] = new()
            {
                ["German"] = "REITTIER / PFERD WÄHLEN",
                ["English"] = "SELECT MOUNT / HORSE",
                ["Czech"] = "VYBRAT KONĚ / OSEDLAT",
                ["French"] = "CHOISIR LA MONTURE",
                ["Spanish"] = "ELEGIR MONTURA / CABALLO",
                ["Italian"] = "SCEGLI CAVALLO",
                ["Polish"] = "WYBIERZ WIERZCHOWCA",
                ["Russian"] = "ВЫБРАТЬ ЛОШАДЬ / СКАКУНА",
                ["Ukrainian"] = "ВИБРАТИ КОНЯ",
                ["Turkish"] = "BİNEK / AT SEÇ",
                ["Portuguese"] = "SELECIONAR MONTARIA",
                ["Japanese"] = "乗用馬を選択",
                ["Korean"] = "말 / 탑승마 선택",
                ["Chineses"] = "选择坐骑 / 马匹",
                ["Chineset"] = "選擇坐騎 / 馬匹",
                ["Vietnamese"] = "CHỌN NGỰA CƯỠI"
            },
            ["Loc_StorageHeader"] = new()
            {
                ["German"] = "LAGERORT / CONTAINER:",
                ["English"] = "STORAGE / CONTAINER:"
            },
            ["Loc_StolenTooltip"] = new()
            {
                ["German"] = "Diebesgut",
                ["English"] = "Stolen Item"
            },
            ["Loc_EquippedTooltip"] = new()
            {
                ["German"] = "Ausgerüstet",
                ["English"] = "Equipped"
            },
            ["Loc_BloodyTooltip"] = new()
            {
                ["German"] = "Blut am Gegenstand",
                ["English"] = "Bloody Item"
            },
            ["Loc_MoveToStash"] = new()
            {
                ["German"] = "📦 In Truhe verschieben",
                ["English"] = "📦 Move to Stash"
            },
            ["Loc_TakeToInventory"] = new()
            {
                ["German"] = "🎒 Ins Inventar nehmen",
                ["English"] = "🎒 Take to Inventory"
            },
            ["Loc_MaxHorseStatsTip"] = new()
            {
                ["German"] = "Alle Pferdewerte maximieren",
                ["English"] = "Maximize all horse stats"
            },
            ["Loc_MaxHorseStatsBtn"] = new()
            {
                ["German"] = "Max Pferd",
                ["English"] = "Max Horse"
            },
            ["Loc_CraftSummary"] = new()
            {
                ["German"] = "  ❖  92 Rezepte (31 Alchemie, 61 Schmieden) • 31 Handwerkstalente",
                ["English"] = "  ❖  92 Recipes (31 Alchemy, 61 Blacksmithing) • 31 Crafting Perks"
            },
            ["Loc_IngredientsHeader"] = new()
            {
                ["German"] = "🌾 Zutaten / Material: ",
                ["English"] = "🌾 Ingredients / Material: "
            },
            ["Loc_SpawnerCatAll"] = new()
            {
                ["German"] = "🌐 Alle (5.604)",
                ["English"] = "🌐 All (5,604)"
            },
            ["Loc_SpawnerCatWeapons"] = new()
            {
                ["German"] = "⚔️ Waffen (763)",
                ["English"] = "⚔️ Weapons (763)"
            },
            ["Loc_SpawnerCatArmour"] = new()
            {
                ["German"] = "🛡️ Rüstung (2.622)",
                ["English"] = "🛡️ Armour (2,622)"
            },
            ["Loc_SpawnerCatFood"] = new()
            {
                ["German"] = "🍗 Nahrung (322)",
                ["English"] = "🍗 Food (322)"
            },
            ["Loc_SpawnerCatBooks"] = new()
            {
                ["German"] = "📜 Bücher (392)",
                ["English"] = "📜 Books (392)"
            },
            ["Loc_SpawnerCatHerbs"] = new()
            {
                ["German"] = "🌿 Kräuter (116)",
                ["English"] = "🌿 Herbs (116)"
            },
            ["Loc_SpawnerCatHorse"] = new()
            {
                ["German"] = "🐎 Pferd (555)",
                ["English"] = "🐎 Horse (555)"
            },
            ["Loc_SpawnerCatOther"] = new()
            {
                ["German"] = "📦 Sonstiges (834)",
                ["English"] = "📦 Other (834)"
            },
            ["Loc_PriceAll"] = new()
            {
                ["German"] = "Alle Preisklassen",
                ["English"] = "All Price Ranges"
            },
            ["Loc_PriceCheap"] = new()
            {
                ["German"] = "Günstig (< 50 Gr.)",
                ["English"] = "Inexpensive (< 50 Gr.)"
            },
            ["Loc_PriceMedium"] = new()
            {
                ["German"] = "Mittel (50 - 500 Gr.)",
                ["English"] = "Medium (50 - 500 Gr.)"
            },
            ["Loc_PriceValuable"] = new()
            {
                ["German"] = "Wertvoll (500 - 2.500 Gr.)",
                ["English"] = "Valuable (500 - 2,500 Gr.)"
            },
            ["Loc_PriceLegendary"] = new()
            {
                ["German"] = "Legendär (> 2.500 Gr.)",
                ["English"] = "Legendary (> 2,500 Gr.)"
            },
            ["Loc_SortNameAsc"] = new()
            {
                ["German"] = "Name (A - Z)",
                ["English"] = "Name (A - Z)"
            },
            ["Loc_SortNameDesc"] = new()
            {
                ["German"] = "Name (Z - A)",
                ["English"] = "Name (Z - A)"
            },
            ["Loc_SortPriceDesc"] = new()
            {
                ["German"] = "Wert (Absteigend)",
                ["English"] = "Value (High to Low)"
            },
            ["Loc_SortPriceAsc"] = new()
            {
                ["German"] = "Wert (Aufsteigend)",
                ["English"] = "Value (Low to High)"
            },
            ["Loc_SortWeight"] = new()
            {
                ["German"] = "Gewicht",
                ["English"] = "Weight"
            },
            ["Loc_BasePrice"] = new()
            {
                ["German"] = "GRUNDWERT",
                ["English"] = "BASE VALUE"
            },
            ["Loc_Weight"] = new()
            {
                ["German"] = "GEWICHT",
                ["English"] = "WEIGHT"
            },
            ["Loc_TargetStorageInventory"] = new()
            {
                ["German"] = "🎒 Inventar",
                ["English"] = "🎒 Inventory"
            },
            ["Loc_TargetStorageStash"] = new()
            {
                ["German"] = "📦 Truhe",
                ["English"] = "📦 Stash"
            },
            ["Loc_TargetStorageHorse"] = new()
            {
                ["German"] = "🐎 Pferd",
                ["English"] = "🐎 Horse"
            },
            ["Loc_WindowMinTip"] = new()
            {
                ["German"] = "Minimieren",
                ["English"] = "Minimize"
            },
            ["Loc_WindowMaxTip"] = new()
            {
                ["German"] = "Maximieren",
                ["English"] = "Maximize"
            },
            ["Loc_WindowCloseTip"] = new()
            {
                ["German"] = "Schließen",
                ["English"] = "Close"
            },
            ["Loc_CaparisonDefault"] = new()
            {
                ["German"] = "Kuttenberger Gold",
                ["English"] = "Kuttenberg Gold"
            },
            ["Loc_SaddleDefault"] = new()
            {
                ["German"] = "Edler 4-Taschen",
                ["English"] = "Noble 4-Pouch"
            },
            ["Loc_BridleDefault"] = new()
            {
                ["German"] = "Ritterzaumzeug",
                ["English"] = "Knight's Bridle"
            },
            ["Loc_HorseshoesDefault"] = new()
            {
                ["German"] = "Militär-Hufeisen",
                ["English"] = "Military Horseshoes"
            },
            ["Loc_WindowRestoreTip"] = new()
            {
                ["German"] = "Wiederherstellen",
                ["English"] = "Restore",
                ["Czech"] = "Obnovit",
                ["French"] = "Restaurer",
                ["Spanish"] = "Restaurar",
                ["Italian"] = "Ripristina",
                ["Polish"] = "Przywróć",
                ["Russian"] = "Восстановить"
            },
            ["Badge_Stash"] = new()
            {
                ["German"] = "📦 Truhe",
                ["English"] = "📦 Stash",
                ["Czech"] = "📦 Truhla",
                ["French"] = "📦 Coffre",
                ["Spanish"] = "📦 Alijo",
                ["Italian"] = "📦 Baule",
                ["Polish"] = "📦 Skrzynia",
                ["Russian"] = "📦 Сундук"
            },
            ["Badge_Horse"] = new()
            {
                ["German"] = "🐎 Pferd",
                ["English"] = "🐎 Horse",
                ["Czech"] = "🐎 Kůň",
                ["French"] = "🐎 Cheval",
                ["Spanish"] = "🐎 Caballo",
                ["Italian"] = "🐎 Cavallo",
                ["Polish"] = "🐎 Koń",
                ["Russian"] = "🐎 Лошадь"
            },
            ["Badge_Henry"] = new()
            {
                ["German"] = "🎒 Heinrich",
                ["English"] = "🎒 Henry",
                ["Czech"] = "🎒 Jindra",
                ["French"] = "🎒 Henry",
                ["Spanish"] = "🎒 Henry",
                ["Italian"] = "🎒 Henry",
                ["Polish"] = "🎒 Henryk",
                ["Russian"] = "🎒 Индро"
            },
            ["Loc_InspectorOutfit1"] = new()
            {
                ["German"] = "Outfit I",
                ["English"] = "Outfit I",
                ["Czech"] = "Oblek I",
                ["French"] = "Tenue I",
                ["Spanish"] = "Atuendo I",
                ["Italian"] = "Tenuta I",
                ["Polish"] = "Strój I",
                ["Russian"] = "Наряд I"
            },
            ["Loc_InspectorOutfit2"] = new()
            {
                ["German"] = "Outfit II",
                ["English"] = "Outfit II",
                ["Czech"] = "Oblek II",
                ["French"] = "Tenue II",
                ["Spanish"] = "Atuendo II",
                ["Italian"] = "Tenuta II",
                ["Polish"] = "Strój II",
                ["Russian"] = "Наряд II"
            },
            ["Loc_InspectorOutfit3"] = new()
            {
                ["German"] = "Outfit III",
                ["English"] = "Outfit III",
                ["Czech"] = "Oblek III",
                ["French"] = "Tenue III",
                ["Spanish"] = "Atuendo III",
                ["Italian"] = "Tenuta III",
                ["Polish"] = "Strój III",
                ["Russian"] = "Наряд III"
            },
            ["Loc_OutfitLabel"] = new()
            {
                ["German"] = "OUTFIT",
                ["English"] = "OUTFIT",
                ["Czech"] = "OBLEK",
                ["French"] = "TENUE",
                ["Spanish"] = "ATUENDO",
                ["Italian"] = "TENUTA",
                ["Polish"] = "STRÓJ",
                ["Russian"] = "НАРЯД"
            }
        };

        public ItemMetadata? GetItemMetadata(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (_itemsByGuid.TryGetValue(guid, out var meta)) return meta;
            return null;
        }

        public string GetItemName(string guid, string? fallbackName = null)
        {
            if (string.IsNullOrEmpty(guid))
                return fallbackName ?? GetUiText("Unknown_Item", "Unbekannter Gegenstand", "Unknown Item");

            // 1. Direct table metadata lookup
            if (_itemsByGuid.TryGetValue(guid, out var meta))
            {
                if (!string.IsNullOrEmpty(meta.UiNameKey) && _localizedStrings.TryGetValue(meta.UiNameKey, out string? locText))
                {
                    return locText;
                }

                if (!string.IsNullOrEmpty(meta.InternalName))
                {
                    return CleanInternalName(meta.InternalName);
                }
            }

            // 2. Known items hardcoded table
            if (KnownItems.TryGetValue(guid, out var pair))
            {
                return _activeLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? pair.De : pair.En;
            }

            // 3. Fallback name cleanup
            if (!string.IsNullOrEmpty(fallbackName))
            {
                return CleanMojibake(fallbackName);
            }

            string shortGuid = guid.Substring(0, Math.Min(8, guid.Length));
            return $"{GetUiText("Item", "Gegenstand", "Item")} ({shortGuid})";
        }

        private static readonly Dictionary<string, (string De, string En)> KnownItems = new(StringComparer.OrdinalIgnoreCase)
        {
            { "5ef63059-322e-4e1b-abe8-926e100c770e", ("Groschen (Beutel)", "Groschen (Pouch)") },
            { "dfea5d01-b25c-414a-9ab4-6911a5f82118", ("Pfeile / Jagdpfeil", "Hunting Arrows") },
            { "b9de1d84-a0c1-4b81-9f60-8d7fbb3cb9d4", ("Kugeln / Munition", "Handgonne Bullets") },
            { "159047f2-5e2e-5258-8308-18fd5fe2aff4", ("Verband", "Bandage") },
            { "9fa3000e-3807-48a8-bed8-81427f0bda55", ("Verband (Klassisch)", "Bandage (Classic)") },
            { "f10ded12-a41c-40bf-a8ae-883d4e845059", ("Ringelblumen-Aufguss", "Marigold Decoction") },
            { "8d76f58e-a521-4205-a7e8-9ac077eee5f0", ("Retterschnaps", "Saviour Schnapps") },
            { "7259b9bc-dfae-487e-a8bb-c1f500894e0c", ("Schiesspulver", "Gunpowder") },
            { "5e9b4fa1-aafa-4352-b5d6-58df2c263caa", ("Bruncliks Schwert", "Brunclik's Sword") },
            { "4a6269c1-5c01-473d-ad69-e0a0c41643e7", ("Handkanone", "Hand Cannon") },
            { "e730436c-53f6-4041-bdd1-3f4826f15975", ("Luntenschloss-Gewehr", "Matchlock Rifle") },
            { "ac9b3dea-a1e6-49df-880b-7976e7d5abec", ("Kummer-Trank", "Painkiller Potion") },
            { "85310d06-2845-46ee-be8f-295503b35035", ("Kamillentee", "Chamomile Tea") },
            { "c707733a-c0a7-4f02-b684-9392b0b15b83", ("Lazarus-Trank", "Lazarus Potion") },
            { "167eb312-0e9d-4c2f-8ce3-56c32f5a84cb", ("Wundbalsam", "Wound Salve") },
            { "0cb47176-06c5-42a9-8d70-969e917eb999", ("Getrocknetes Fleisch", "Dried Meat") },
            { "3c056762-3e14-471a-8f0e-8d57919fb9c4", ("Frisches Brot", "Fresh Bread") },
            { "4eed0a2b-1233-40b4-88f5-7f67de916b58", ("Käse", "Cheese") },
            { "0cfe3456-8eee-4cf5-bbaf-b632e3879be7", ("Büchsenmacher-Set", "Armourer's Kit") },
            { "7da04bba-0564-42da-bcf1-9a2fc5faf025", ("Schmiedewerkzeug", "Blacksmith's Kit") },
            { "04dc56da-9f2e-4741-93a5-aa6ec3a7cece", ("Schneider-Set", "Tailor's Kit") },
            { "390c0dc8-23fd-42a0-91f2-a4d42f96a387", ("Dietrich", "Lockpick") },
            { "b2f8f5e3-8e5e-4600-a4bb-be17e2d4a058", ("Bruncliks Harnisch", "Brunclik's Cuirass") },
            { "38df365c-a4bb-462b-80cc-eb92f16930fa", ("Bruncliks Helm", "Brunclik's Helmet") },
            { "268622e0-27db-448d-93c1-f3f297184caf", ("Böhmisches Bier", "Bohemian Beer") },
            { "6a4858db-2c3e-442d-8bcd-4c79d855e43a", ("Edler Rotwein", "Fine Red Wine") },
            { "0e6f3e1b-961a-447e-ba58-17901f70896f", ("Gebratenes Schweinefleisch", "Roast Pork") },
            { "5dd0afa5-3c76-475c-9775-6ed5c69132fd", ("Brotlaib", "Loaf of Bread") },
            { "12c30ac1-f9fc-4b61-a337-b3eb98779ca6", ("Geräucherte Wurst", "Smoked Sausage") },
            { "301cc8ff-f3f5-4c39-b27b-129bb58792d0", ("Pferdefleisch", "Horse Meat") },
            { "0da553ab-9df7-4ed4-92b8-a9c9e42566a5", ("Apfel", "Apple") },
            { "cc2060b0-b588-4a54-9a73-293a8a4f2ff6", ("Bruncliks Armschienen", "Brunclik's Vambraces") },
            { "b1de7a91-1644-4ad6-b186-a5b40764be7f", ("Schwere Jagdpfeile", "Heavy Arrows") },
            { "0712d873-29bd-4fdd-8966-79aefb82c829", ("Bohrer-Pfeile", "Piercing Arrows") },
            { "c5e76f8d-a92b-4c18-b103-452f87e91a65", ("Hosen / Beinlinge", "Hose") },
            { "74bc6c64-73f9-4128-bbc1-3da5894cc28a", ("Bruncliks Beinlinge", "Brunclik's Chausses") },
            { "7c5126cd-b010-4484-8465-22a3d69fa0df", ("Edler Wappenrock", "Noble Jupon") },
            { "0faf833f-8e88-40a0-87b8-2669c0e64c03", ("Militär-Hufeisen", "Military Horseshoes") },
            { "5ded1ff4-9f81-4179-bb65-f786e6e80560", ("Edle Hufeisen", "Noble Horseshoes") },
            { "8007b1d2-6179-49bb-a9eb-132bfc1b0ab6", ("Einfacher Sattel", "Basic Saddle") },
            { "82242945-dcc8-4b6d-aec9-e12b80ff6f29", ("Edler Sattel", "Noble Saddle") },
            { "dd37d5f4-74e1-4ef5-a3a0-38682eef40ae", ("Östlicher Sattel", "Eastern Saddle") },
            { "596212ea-ce9b-4b8c-af09-278ae2d834bb", ("Einfaches Zaumzeug", "Basic Bridle") },
            { "68204d36-5d4d-412e-9b37-8bce360952f3", ("Edles Zaumzeug", "Noble Bridle") },
            { "cf252e68-842d-4e7d-a597-d07938c14439", ("Östliches Zaumzeug", "Eastern Bridle") },
            { "36694031-e85d-4547-8054-5e67b51aa8fe", ("Schabracke", "Caparison") },
            { "911df0af-e8c8-4ff6-b487-a13f29cefe38", ("Rossharnisch", "Horse Harness") },
            { "4cea28a0-0814-405a-bf24-4fd711f7eb63", ("Fackel", "Torch") }
        };

        private static string CleanMojibake(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("Ã¼", "ü")
                    .Replace("Ã¤", "ä")
                    .Replace("Ã¶", "ö")
                    .Replace("Ã–", "Ö")
                    .Replace("Ãœ", "Ü")
                    .Replace("Ã„", "Ä")
                    .Replace("ÃŸ", "ß");
        }

        public string GetItemCategory(string guid, string? fallbackCategory = null)
        {
            if (_itemsByGuid.TryGetValue(guid, out var meta))
            {
                string tag = meta.Tag.ToLowerInvariant();
                string internalName = meta.InternalName.ToLowerInvariant();

                if (tag.Contains("horse") || internalName.Contains("horse") ||
                    internalName.Contains("saddle") || internalName.Contains("bridle") ||
                    internalName.Contains("horseshoe") || internalName.Contains("caparison"))
                    return GetUiText("Horse", "Pferd", "Horse");

                if (tag.Contains("weapon") || tag.Contains("ammo") || tag.Contains("shield") || tag.Contains("arrow") || tag.Contains("bolt") || tag.Contains("crossbow"))
                    return GetUiText("Weapons", "Waffen", "Weapons");
                if (tag.Contains("armor") || tag.Contains("helmet") || tag.Contains("boots") || tag.Contains("gloves") || tag.Contains("hood") || tag.Contains("jacket") || tag.Contains("pants"))
                    return GetUiText("Armour", "Rüstung", "Armour");
                if (tag.Contains("food") || tag.Contains("drink") || tag.Contains("meat") || tag.Contains("bread") || tag.Contains("potion"))
                    return GetUiText("Food", "Nahrung", "Food");
                if (tag.Contains("book") || tag.Contains("document") || tag.Contains("recipe"))
                    return GetUiText("Books", "Bücher", "Books");
                if (tag.Contains("herb") || tag.Contains("ointment") || tag.Contains("material") || tag.Contains("ingredient"))
                    return GetUiText("Herbs", "Materialien", "Materials");
                if (tag.Contains("quest"))
                    return GetUiText("Quest", "Quest", "Quest");
                if (tag.Contains("money"))
                    return GetUiText("Other", "Sonstiges", "Other");
            }

            return fallbackCategory ?? GetUiText("Other", "Sonstiges", "Other");
        }

        private static string CleanInternalName(string internalName)
        {
            var parts = internalName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p => char.ToUpper(p[0]) + p[1..]));
        }
        public string GetItemRawCategory(string guid)
        {
            if (_itemsByGuid.TryGetValue(guid, out var meta))
            {
                string tag = meta.Tag.ToLowerInvariant();
                string internalName = meta.InternalName.ToLowerInvariant();

                if (tag.Contains("horse") || internalName.Contains("horse") ||
                    internalName.Contains("saddle") || internalName.Contains("bridle") ||
                    internalName.Contains("horseshoe") || internalName.Contains("caparison"))
                    return "Horse";

                if (tag.Contains("weapon") || tag.Contains("ammo") || tag.Contains("shield") || tag.Contains("arrow") || tag.Contains("bolt") || tag.Contains("crossbow"))
                    return "Weapons";
                if (tag.Contains("armor") || tag.Contains("helmet") || tag.Contains("boots") || tag.Contains("gloves") || tag.Contains("hood") || tag.Contains("jacket") || tag.Contains("pants"))
                    return "Armour";
                if (tag.Contains("food") || tag.Contains("drink") || tag.Contains("meat") || tag.Contains("bread") || tag.Contains("potion"))
                    return "Food";
                if (tag.Contains("book") || tag.Contains("document") || tag.Contains("recipe"))
                    return "Books";
                if (tag.Contains("herb") || tag.Contains("ointment") || tag.Contains("material") || tag.Contains("ingredient"))
                    return "Herbs";
                if (tag.Contains("quest"))
                    return "Quest";
                if (tag.Contains("money"))
                    return "Other";
            }
            return "Other";
        }

        public string GetItemIcon(string rawCategory, string name)
        {
            string lower = name.ToLowerInvariant();
            return rawCategory switch
            {
                "Weapons" => lower.Contains("arrow") || lower.Contains("pfeil") || lower.Contains("bolt") || lower.Contains("bolzen") ? "🏹" : (lower.Contains("kanone") || lower.Contains("gonne") || lower.Contains("gewehr") ? "💥" : "⚔️"),
                "Armour" => lower.Contains("helm") || lower.Contains("hood") ? "🪖" : (lower.Contains("stiefel") || lower.Contains("boot") || lower.Contains("schuh") ? "👢" : (lower.Contains("hose") || lower.Contains("beinling") ? "👖" : "🛡️")),
                "Food" => lower.Contains("trank") || lower.Contains("potion") || lower.Contains("schnapps") || lower.Contains("bier") || lower.Contains("wein") ? "🧪" : (lower.Contains("fleisch") || lower.Contains("wurst") ? "🍖" : (lower.Contains("brot") ? "🍞" : "🍗")),
                "Books" => "📜",
                "Herbs" => "🌿",
                "Horse" => "🐎",
                "Quest" => "👑",
                _ => "📦"
            };
        }

        public IReadOnlyList<GameItemCodexDefinition> GetAllCodexItems()
        {
            if (_cachedCodexItems != null) return _cachedCodexItems;

            var list = new List<GameItemCodexDefinition>(_itemsByGuid.Count);
            foreach (var kvp in _itemsByGuid)
            {
                var meta = kvp.Value;
                string rawCat = GetItemRawCategory(meta.Guid);
                string localizedCat = GetItemCategory(meta.Guid);
                string displayName = GetItemName(meta.Guid);
                string icon = GetItemIcon(rawCat, displayName);

                list.Add(new GameItemCodexDefinition
                {
                    Guid = meta.Guid,
                    UiNameKey = meta.UiNameKey,
                    InternalName = meta.InternalName,
                    DisplayName = displayName,
                    Category = localizedCat,
                    RawCategory = rawCat,
                    Weight = meta.Weight > 0 ? meta.Weight : 1.0f,
                    Price = meta.Price > 0 ? meta.Price : 10,
                    Tag = meta.Tag,
                    Icon = icon
                });
            }

            list.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCultureIgnoreCase));
            _cachedCodexItems = list;
            return _cachedCodexItems;
        }

        public void InvalidateCodexCache()
        {
            _cachedCodexItems = null;
        }
    }
}
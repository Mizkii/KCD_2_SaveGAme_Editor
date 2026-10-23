using System;



using System.Collections.Generic;



using System.Collections.ObjectModel;



using System.ComponentModel;



using System.Globalization;



using System.IO;



using System.Linq;



using System.Text;



using System.Text.RegularExpressions;



using System.Windows;



using System.Windows.Data;



using System.Windows.Input;



using System.Windows.Media;



using Microsoft.Win32;



using KCD2SaveEditor.UI.Interop;



using KCD2SaveEditor.UI.Services;







namespace KCD2SaveEditor.UI.ViewModels



{



    public class MainViewModel : ViewModelBase, IDisposable



    {



        private IntPtr _handle = IntPtr.Zero;



        private string _saveFilePath = string.Empty;



        private string _saveName = string.Empty;



        private string _levelName = string.Empty;



        private string _gameVersion = string.Empty;



        private string _playtimeString = string.Empty;







        // Vitals



        private float _health = 100.0f;



        private float _maxHealth = 100.0f;



        private float _stamina = 100.0f;



        private float _maxStamina = 100.0f;



        private float _energy = 100.0f;



        private float _nourishment = 93.0f;







        // Economy



        private long _groschen = 980419;







        // Base Attributes (Screenshot: Stärke 30, Agilität 30, Vitalität 30, Redekunst 30, Charisma 14, Geschwind. 30)



        private int _strength = 30;



        private int _agility = 30;



        private int _vitality = 30;



        private int _speech = 30;



        private int _charisma = 14;



        private int _speed = 30;







        // Character Level (Screenshot: Hauptstufe 29)



        private int _characterLevel = 29;







        // Stealth Ratings (Screenshot: Auffälligkeit 41, Geräusche 61, Sichtbarkeit 45)



        private int _conspicuousness = 41;



        private int _noise = 61;



        private int _visibility = 45;







        // Carrying Capacity (Screenshot: 210 / 390)



        private float _carryWeight = 210.0f;



        private float _maxCarryWeight = 390.0f;







        // Combat & Sub-Skills



        private int _warfare;



        private int _sword;



        private int _defense;



        private int _stealth;



        private int _locksPicked;



        private int _pickpocketing;



        private int _potionsConsumed;



        private int _marksmanship;



        private int _horsemanship;



        private int _houndmaster;



        private int _scholarship;



        private int _blacksmithing;







        // Horse Stats & Vitals



        private string _horseName = "Kein Pferd";



        private float _horseHealth = 100.0f;



        private float _horseMaxHealth = 100.0f;



        private float _horseStamina = 100.0f;



        private float _horseMaxStamina = 100.0f;



        private float _horseSpeed = 42.0f;



        private float _horseCapacity = 268.0f;



        private float _horseCourage = 15.0f;



        private bool _hasHorse = false;



        private InventoryItemViewModel? _selectedHorseItem;



        private HorseOptionViewModel? _selectedHorseOption;







        // Status & Session



        private string _statusMessage = "Ready. Please open a KCD2 save file (.whs) or auto-detect.";



        private bool _isSaveLoaded;



        private bool _createBackup = true;



        private string _currentDirectoryPath = string.Empty;



        private string _searchFilter = string.Empty;



        private string _selectedCategory = "All";



        private SaveGameItemViewModel? _selectedSaveItem;







        // Inventory Management



        private string _inventorySearchFilter = string.Empty;



        private string _selectedInventoryCategory = "Alle";



        private InventoryItemViewModel? _selectedInventoryItem;



        private LanguageOption? _selectedLanguage;



        private bool _filterEquippedOnly;



        private int _activeOutfit = 1;
        private int _characterDirtPercent = 35;

        // Defense & Attack Matrix Fields
        private int _armorHeadStab = 12;
        private int _armorHeadSlash = 12;
        private int _armorHeadBlunt = 14;
        private int _armorBodyStab = 140;
        private int _armorBodySlash = 138;
        private int _armorBodyBlunt = 75;
        private int _armorArmsStab = 13;
        private int _armorArmsSlash = 16;
        private int _armorArmsBlunt = 18;
        private int _armorLegsStab = 157;
        private int _armorLegsSlash = 137;
        private int _armorLegsBlunt = 73;

        private int _weaponMeleeStab = 138;
        private int _weaponMeleeSlash = 131;
        private int _weaponMeleeBlunt = 26;
        private int _weaponRangedAttack = 704;
        private int _weaponUnarmedAttack = 133;

        // Slot Picker Modal Fields
        private bool _isSlotPickerModalOpen;
        private PaperdollSlotViewModel? _selectedPaperdollSlot;
        private string _slotPickerSearchText = string.Empty;
        private string _slotPickerSelectedSource = "All";
        private string _slotPickerItemCountText = string.Empty;
        private SlotPickerItemViewModel? _selectedSlotPickerItem;



        private string _selectedRightView = "Split"; // "Split", "Perks", "Horse", "DLCs"







        // Navigation & V2 Architecture



        private string _selectedMainTab = "Inventar";



        private string _selectedInventoryTarget = "Henry";



        private string _selectedPlayerSubTab = "Henry";



        private bool _isDirty;







        // Dog / Companion Stats



        private int _dogLoyalty = 100;



        private int _dogObedience = 98;



        private int _dogHunger = 85;



        private int _dogAggression = 75;







        // Quests



        private string _selectedQuestFilter = "all";



        private string _questSearchFilter = string.Empty;



        private QuestItemViewModel? _selectedQuest;







        // Modal Codex Spawner



        private bool _isAddItemModalOpen;



        private string _newItemName = "Heinrichs Meister-Langschwert";



        private string _newItemCategory = "Waffen";



        private int _newItemQuantity = 1;



        private int _newItemPower = 165;



        private float _newItemWeight = 3.1f;



        private float _newItemPrice = 2450f;



        private string _selectedCodexPreset = "heinrich_sword";







        public ObservableCollection<SaveGameItemViewModel> AvailableSaves { get; } = new();



        public ICollectionView FilteredSaves { get; }







        public ObservableCollection<DlcItemViewModel> AvailableDlcs { get; } = new();







        public ObservableCollection<InventoryItemViewModel> InventoryItems { get; } = new();



        public ICollectionView FilteredInventory { get; }







        public ObservableCollection<InventoryItemViewModel> HorseItems { get; } = new();



        public ICollectionView FilteredHorseItems { get; }







        public ObservableCollection<HorseOptionViewModel> AvailableHorses { get; } = new();







        public ObservableCollection<CategoryTabViewModel> CategoryTabs { get; } = new();



        public ObservableCollection<PaperdollSlotViewModel> LeftPaperdollSlots { get; } = new();



        public ObservableCollection<PaperdollSlotViewModel> RightPaperdollSlots { get; } = new();

        public ObservableCollection<PaperdollSlotViewModel> HorseEquipmentSlots { get; } = new();

        public ObservableCollection<SlotPickerItemViewModel> SlotPickerItems { get; } = new();



        public ObservableCollection<FactionRepViewModel> Factions { get; } = new();



        public ObservableCollection<PerkItemViewModel> Perks { get; } = new();







        public ObservableCollection<DogPerkItemViewModel> DogPerks { get; } = new();



        public ObservableCollection<QuestItemViewModel> Quests { get; } = new();

        public ObservableCollection<GameCharacterFateViewModel> CharacterFates { get; } = new();

        public ICollectionView FilteredCharacterFates { get; }



        public ICollectionView FilteredQuests { get; }



        public ObservableCollection<HerbItemViewModel> Herbs { get; } = new();



        public ObservableCollection<AlchemyRecipeViewModel> AlchemyRecipes { get; } = new();



        public ObservableCollection<CraftingRecipeViewModel> CraftingRecipes { get; } = new();



        public ICollectionView FilteredCraftingRecipes { get; }



        public ObservableCollection<CraftingSkillPerkViewModel> CraftingPerks { get; } = new();



        public ICollectionView FilteredCraftingPerks { get; }







        public ObservableCollection<LanguageOption> AvailableLanguages { get; } = new();







        public ObservableCollection<string> Categories { get; } = new()



        {



            "All", "Manual", "Autosave", "Permanent", "Crucial Decision", "Exit Save"



        };







        public MainViewModel()



        {



            FilteredSaves = CollectionViewSource.GetDefaultView(AvailableSaves);



            FilteredSaves.Filter = FilterSaveItem;







            FilteredInventory = CollectionViewSource.GetDefaultView(InventoryItems);



            FilteredCodexItems = CollectionViewSource.GetDefaultView(AllCodexItems);



            FilteredCodexItems.Filter = FilterCodexItem;



            ApplyCodexSorting();



            FilteredInventory.Filter = FilterInventoryItem;



            FilteredInventory.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InventoryItemViewModel.SubCategory)));







            FilteredHorseItems = CollectionViewSource.GetDefaultView(HorseItems);







            FilteredQuests = CollectionViewSource.GetDefaultView(Quests);



            FilteredQuests.Filter = FilterQuestItem;







            FilteredCraftingRecipes = CollectionViewSource.GetDefaultView(CraftingRecipes);



            FilteredCraftingRecipes.Filter = FilterCraftingRecipe;
            FilteredCharacterFates = CollectionViewSource.GetDefaultView(CharacterFates);
            FilteredCharacterFates.Filter = FilterCharacterFate;







            FilteredCraftingPerks = CollectionViewSource.GetDefaultView(CraftingPerks);



            FilteredCraftingPerks.Filter = FilterCraftingPerk;







            InitializeHorses();



            InitializeCategoryTabs();



            InitializePaperdoll();
            InitializeHorseEquipment();



            InitializeFactionsAndPerks();



            InitializeQuests();

            InitializeCharacterFates();



            InitializeCompanionAndHerbs();







            foreach (var lang in GameDataService.Instance.SupportedLanguages)



            {



                AvailableLanguages.Add(lang);



            }



            _selectedLanguage = AvailableLanguages.FirstOrDefault(x => x.Code.Equals(GameDataService.Instance.ActiveLanguage, StringComparison.OrdinalIgnoreCase))



                ?? AvailableLanguages.FirstOrDefault();







            OpenSaveCommand = new RelayCommand(OpenSave);



            SaveCommand = new RelayCommand(SaveSave, () => IsSaveLoaded);



            AddGroschenCommand = new RelayCommand(param => AddGroschen(param), _ => IsSaveLoaded);



            SetGroschenCommand = new RelayCommand(param => SetGroschen(param), _ => IsSaveLoaded);



            MaxMoneyCommand = new RelayCommand(() => SetGroschen(999999), () => IsSaveLoaded);



            MaxAllStatsCommand = new RelayCommand(MaxAllStats, () => IsSaveLoaded);



            MaxHorseStatsCommand = new RelayCommand(MaxHorseStats, () => IsSaveLoaded && HasHorse);



            AutoDetectCommand = new RelayCommand(AutoDetectSaves);



            SwitchToXboxCommand = new RelayCommand(SwitchToXbox);



            SwitchToSteamCommand = new RelayCommand(SwitchToSteam);



            SwitchPlatformCommand = new RelayCommand(param => SwitchPlatform(param?.ToString()));



            BrowseFolderCommand = new RelayCommand(BrowseFolder);



            RefreshCommand = new RelayCommand(() => ScanDirectory(CurrentDirectoryPath));



            ImportWhsCommand = new RelayCommand(ImportWhs, () => IsSaveLoaded);



            ExportWhsCommand = new RelayCommand(ExportWhs, () => IsSaveLoaded);



            EnableAllDlcsCommand = new RelayCommand(EnableAllDlcs, () => IsSaveLoaded);



            DisableAllDlcsCommand = new RelayCommand(DisableAllDlcs, () => IsSaveLoaded);



            AddCustomDlcCommand = new RelayCommand(AddCustomDlc, () => IsSaveLoaded);







            // New Design Commands



            RepairAllCommand = new RelayCommand(RepairAllItems, () => IsSaveLoaded);



            ClearStolenCommand = new RelayCommand(ClearStolenFlags, () => IsSaveLoaded);



            CleanCharacterDirtCommand = new RelayCommand(CleanCharacterDirt);



            SwitchOutfitCommand = new RelayCommand(param => SwitchOutfit(param));
            SwitchHorseOutfitCommand = new RelayCommand(param => SwitchHorseOutfit(param));

            CloseSlotPickerModalCommand = new RelayCommand(() => IsSlotPickerModalOpen = false);
            EquipSelectedSlotItemCommand = new RelayCommand(EquipSelectedSlotItem, () => SelectedSlotPickerItem != null);
            UnequipSlotCommand = new RelayCommand(UnequipSlot, () => SelectedPaperdollSlot != null);
            SelectSlotPickerSourceCommand = new RelayCommand(param => SelectSlotPickerSource(param?.ToString()));



            SetRightViewCommand = new RelayCommand(param => SetRightView(param));



            SelectCategoryTabCommand = new RelayCommand(param => SelectCategoryTab(param?.ToString() ?? "Alle"));



            ToggleFilterEquippedCommand = new RelayCommand(() => FilterEquippedOnly = !FilterEquippedOnly);



            DuplicateSelectedItemCommand = new RelayCommand(DuplicateSelectedItem, () => SelectedInventoryItem != null);



            DeleteSelectedItemCommand = new RelayCommand(DeleteSelectedItem, () => SelectedInventoryItem != null);



            SelectStorageFilterCommand = new RelayCommand(param => SelectedStorageFilter = param?.ToString() ?? "All");



            SelectCodexCategoryCommand = new RelayCommand(param => CodexCategoryFilter = param?.ToString() ?? "All");



            SelectCodexWorthFilterCommand = new RelayCommand(param => CodexWorthFilter = param?.ToString() ?? "All");



            SelectSpawnTargetStorageCommand = new RelayCommand(param => SpawnTargetStorage = param?.ToString() ?? "Inventory");



            AddSpawnQuantityCommand = new RelayCommand(param =>



            {



                if (param != null && int.TryParse(param.ToString(), out int delta))



                {



                    SpawnQuantity = Math.Max(1, SpawnQuantity + delta);



                }



            });



            OpenAddItemModalCommand = new RelayCommand(OpenAddItemModal, () => IsSaveLoaded);



            CloseAddItemModalCommand = new RelayCommand(() => IsAddItemModalOpen = false);



            ConfirmAddItemCommand = new RelayCommand(ConfirmAddItem);



            SelectCodexPresetCommand = new RelayCommand(param => SelectCodexPreset(param?.ToString()));







            // V2 Architecture Commands



            SwitchMainTabCommand = new RelayCommand(param => SelectedMainTab = param?.ToString() ?? "Inventar");



            SwitchInventoryTargetCommand = new RelayCommand(param => SelectedInventoryTarget = param?.ToString() ?? "Henry");



            SwitchPlayerSubTabCommand = new RelayCommand(param => SelectedPlayerSubTab = param?.ToString() ?? "Henry");



            SwitchQuestSubTabCommand = new RelayCommand(param => SelectedQuestSubTab = param?.ToString() ?? "Missions");

            FilterQuestsCommand = new RelayCommand(param => SelectedQuestFilter = param?.ToString() ?? "all");

            FilterQuestRegionCommand = new RelayCommand(param => SelectedQuestRegionFilter = param?.ToString() ?? "all");

            FilterFateCategoryCommand = new RelayCommand(param => SelectedFateCategoryFilter = param?.ToString() ?? "all");

            SelectFateCharacterCommand = new RelayCommand(param => { if (param is GameCharacterFateViewModel f) SelectedCharacterFate = f; });

            ReviveCharacterCommand = new RelayCommand(param =>

            {

                var target = param as GameCharacterFateViewModel ?? SelectedCharacterFate;

                if (target != null)

                {

                    target.Status = "Alive";

                    IsDirty = true;

                    StatusMessage = IsGerman

                        ? $"✨ {target.DisplayName} wurde erfolgreich wiederbelebt!"

                        : $"✨ {target.DisplayName} was successfully revived!";

                }

            });

            SetCharacterStatusCommand = new RelayCommand(param =>

            {

                if (SelectedCharacterFate != null && param is string status)

                {

                    SelectedCharacterFate.Status = status;

                    IsDirty = true;

                    StatusMessage = IsGerman

                        ? $"{SelectedCharacterFate.DisplayName}: Status auf '{SelectedCharacterFate.StatusBadgeText}' gesetzt"

                        : $"{SelectedCharacterFate.DisplayName}: Status set to '{SelectedCharacterFate.StatusBadgeText}'";

                }

            });



            SelectQuestCommand = new RelayCommand(param => { if (param is QuestItemViewModel q) SelectedQuest = q; });



            UnlockAllQuestsCommand = new RelayCommand(() =>



            {



                foreach (var q in Quests)



                {



                    q.Status = "aktiv";



                    if (_handle != IntPtr.Zero)



                    {



                        NativeMethods.kcd2_save_set_quest_status(_handle, q.Id, "Active");



                    }



                }



                IsDirty = true;



                StatusMessage = IsGerman ? "Alle Quests im Logbuch aktiviert!" : "All quests in quest log activated!";



            });



            SetQuestStatusCommand = new RelayCommand(param =>



            {



                if (SelectedQuest != null && param is string status)



                {



                    SelectedQuest.Status = status;



                    if (status == "abgeschlossen")



                    {



                        foreach (var o in SelectedQuest.Objectives) o.IsDone = true;



                        SelectedQuest.Stage = SelectedQuest.MaxStage;



                    }



                    else if (status == "aktiv")



                    {



                        SelectedQuest.UpdateStageFromObjectives();



                    }



                    if (_handle != IntPtr.Zero)



                    {



                        string nativeStatus = status switch



                        {



                            "abgeschlossen" => "Done",



                            "fehlgeschlagen" => "Failed",



                            _ => "Active"



                        };



                        NativeMethods.kcd2_save_set_quest_status(_handle, SelectedQuest.Id, nativeStatus);



                    }



                    IsDirty = true;



                    StatusMessage = IsGerman ? $"Quest-Status auf '{status}' gesetzt" : $"Quest status set to '{status}'";



                }



            });



            AutocompleteObjectivesCommand = new RelayCommand(() =>



            {



                if (SelectedQuest != null)



                {



                    foreach (var obj in SelectedQuest.Objectives) obj.IsDone = true;



                    SelectedQuest.Status = "abgeschlossen";



                    SelectedQuest.Stage = SelectedQuest.MaxStage;



                    if (_handle != IntPtr.Zero)



                    {



                        NativeMethods.kcd2_save_set_quest_status(_handle, SelectedQuest.Id, "Done");



                    }



                    IsDirty = true;



                    StatusMessage = IsGerman ? $"Quest '{SelectedQuest.DisplayTitle}' abgeschlossen!" : $"Quest '{SelectedQuest.DisplayTitle}' completed!";



                }



            });



            MaxDogStatsCommand = new RelayCommand(() =>



            {



                DogLoyalty = 100;



                DogObedience = 100;



                DogHunger = 100;



                DogAggression = 100;



                foreach (var p in DogPerks) p.IsEnabled = true;



                IsDirty = true;



                StatusMessage = IsGerman ? "Köter: Treue 100% & alle Gefährten-Perks freigeschaltet!" : "Mutt: 100% Loyalty & all companion perks unlocked!";



            });



            UnlockAllPerksCommand = new RelayCommand(() =>



            {



                foreach (var p in Perks) p.IsEnabled = true;



                IsDirty = true;



                StatusMessage = IsGerman ? "Alle Spezial-Perks freigeschaltet!" : "All special perks unlocked!";



            });



            AddHerbsBatchCommand = new RelayCommand(param =>



            {



                int amount = 50;



                if (param != null && int.TryParse(param.ToString(), out int parsed)) amount = parsed;



                foreach (var h in Herbs) h.Quantity += amount;



                IsDirty = true;



                StatusMessage = IsGerman ? $"+{amount} zu allen Kräutern hinzugefügt!" : $"+{amount} added to all herbs!";



            });



            AddHerbCommand = new RelayCommand(param =>



            {



                if (param is HerbItemViewModel herb)



                {



                    herb.Quantity += 10;



                    IsDirty = true;



                }



            });



            MaxAllHerbsCommand = new RelayCommand(() =>



            {



                foreach (var h in Herbs) h.Quantity = 999;



                IsDirty = true;



                StatusMessage = IsGerman ? "Kräuterbeutel: Alle Kräuter auf 999 maximiert!" : "Herb Pouch: All herbs maximized to 999!";



            });



            UnlockAllRecipesCommand = new RelayCommand(UnlockAllCrafting);



            UnlockAllCraftingCommand = new RelayCommand(UnlockAllCrafting);



            LockAllCraftingCommand = new RelayCommand(LockAllCrafting);



            UnlockAllCraftingPerksCommand = new RelayCommand(UnlockAllCraftingPerks);



            LockAllCraftingPerksCommand = new RelayCommand(LockAllCraftingPerks);



            SelectCraftFilterCommand = new RelayCommand(param => SelectedCraftFilter = param?.ToString() ?? "All");



            SelectCraftStatusFilterCommand = new RelayCommand(param => SelectedCraftStatusFilter = param?.ToString() ?? "All");



            ToggleRecipeCommand = new RelayCommand(param =>



            {



                if (param is CraftingRecipeViewModel recipe)



                {



                    recipe.IsUnlocked = !recipe.IsUnlocked;



                    IsDirty = true;



                }



            });



            MaxSmithingStatsCommand = new RelayCommand(() =>



            {



                foreach (var item in InventoryItems)



                {



                    if (item.Category.Equals("Waffen", StringComparison.OrdinalIgnoreCase) || item.Category.Equals("Weapons", StringComparison.OrdinalIgnoreCase))



                    {



                        item.Condition = 1.0f;



                        item.Blood = 0;



                    }



                }



                IsDirty = true;



                StatusMessage = IsGerman ? "Schmiedekunst: Alle Klingen auf 100% geschärft!" : "Blacksmithing: All blades sharpened to 100%!";



            });







            // Game Exe Selection & Multi-Language Game Data Service



            SelectGameExeCommand = new RelayCommand(SelectGameExe);



            GameDataService.Instance.GameDirectoryChanged += () =>



            {



                OnPropertyChanged(nameof(ActiveGameExeButtonLabel));



                OnPropertyChanged(nameof(GameContentPathDisplay));



                NotifyAllLocalizationsChanged();



            };







            // Run initial auto-detection



            AutoDetectSaves();



        }







        public void CheckFirstStartGameExePrompt()



        {



            var settings = AppSettings.Current;



            if (!settings.FirstStartCompleted)



            {



                bool selected = GameDataService.Instance.PromptSelectGameExe(forcePrompt: true);



                if (selected)



                {



                    OnPropertyChanged(nameof(ActiveGameExeButtonLabel));



                    OnPropertyChanged(nameof(GameContentPathDisplay));



                    NotifyAllLocalizationsChanged();



                    StatusMessage = IsGerman



                        ? $"Spieldateien geladen aus: {GameDataService.Instance.GameContentPath}"



                        : $"Game files loaded from: {GameDataService.Instance.GameContentPath}";



                }



            }



        }







        public ICommand SelectGameExeCommand { get; }







        public string ActiveGameExeButtonLabel



        {



            get



            {



                if (!string.IsNullOrEmpty(GameDataService.Instance.GameContentPath))



                {



                    string dirName = Path.GetFileName(GameDataService.Instance.GameContentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));



                    return $"Game: {dirName}";



                }



                return IsGerman ? "Exe wählen..." : "Select Exe...";



            }



        }







        public string GameContentPathDisplay => GameDataService.Instance.GameContentPath ?? (IsGerman ? "Kein Spielverzeichnis festgelegt" : "No game directory set");







        private void SelectGameExe()



        {



            bool selected = GameDataService.Instance.PromptSelectGameExe(forcePrompt: true);



            if (selected)



            {



                OnPropertyChanged(nameof(ActiveGameExeButtonLabel));



                OnPropertyChanged(nameof(GameContentPathDisplay));



                InitializeCompanionAndHerbs();



                NotifyAllLocalizationsChanged();



                StatusMessage = IsGerman



                    ? $"Spieldateien geladen aus: {GameDataService.Instance.GameContentPath}"



                    : $"Game files loaded from: {GameDataService.Instance.GameContentPath}";



            }



        }







        #region Properties



        // Storage Filter



        private string _selectedStorageFilter = "All"; // "All", "Inventory", "Stash", "Horse"



        public string SelectedStorageFilter



        {



            get => _selectedStorageFilter;



            set



            {



                if (SetProperty(ref _selectedStorageFilter, value))



                {



                    FilteredInventory.Refresh();



                    UpdateCategoryTabCounts();



                    OnPropertyChanged(nameof(IsStorageFilterAll));



                    OnPropertyChanged(nameof(IsStorageFilterInventory));



                    OnPropertyChanged(nameof(IsStorageFilterStash));



                    OnPropertyChanged(nameof(IsStorageFilterHorse));



                }



            }



        }







        public bool IsStorageFilterAll



        {



            get => SelectedStorageFilter == "All";



            set { if (value) SelectedStorageFilter = "All"; }



        }



        public bool IsStorageFilterInventory



        {



            get => SelectedStorageFilter == "Inventory";



            set { if (value) SelectedStorageFilter = "Inventory"; }



        }



        public bool IsStorageFilterStash



        {



            get => SelectedStorageFilter == "Stash";



            set { if (value) SelectedStorageFilter = "Stash"; }



        }



        public bool IsStorageFilterHorse



        {



            get => SelectedStorageFilter == "Horse";



            set { if (value) SelectedStorageFilter = "Horse"; }



        }







        public int CountInventoryItems => InventoryItems.Count(i => i.StorageLocation == ItemStorageLocation.Inventory);



        public int CountStashItems => InventoryItems.Count(i => i.StorageLocation == ItemStorageLocation.Stash);



        public int CountHorseItems => InventoryItems.Count(i => i.StorageLocation == ItemStorageLocation.Horse);



        public int CountAllItems => InventoryItems.Count;







        // Codex Spawner Database



        public ObservableCollection<GameItemCodexDefinition> AllCodexItems { get; } = new();



        public ICollectionView FilteredCodexItems { get; private set; }







        private string _codexSearchFilter = string.Empty;



        public string CodexSearchFilter



        {



            get => _codexSearchFilter;



            set



            {



                if (SetProperty(ref _codexSearchFilter, value))



                {



                    FilteredCodexItems?.Refresh();



                }



            }



        }







        private string _codexCategoryFilter = "All"; // "All", "Weapons", "Armour", "Food", "Books", "Herbs", "Horse", "Other"



        public string CodexCategoryFilter



        {



            get => _codexCategoryFilter;



            set



            {



                if (SetProperty(ref _codexCategoryFilter, value))



                {



                    FilteredCodexItems?.Refresh();



                    OnPropertyChanged(nameof(IsCodexCatAll));



                    OnPropertyChanged(nameof(IsCodexCatWeapons));



                    OnPropertyChanged(nameof(IsCodexCatArmour));



                    OnPropertyChanged(nameof(IsCodexCatFood));



                    OnPropertyChanged(nameof(IsCodexCatBooks));



                    OnPropertyChanged(nameof(IsCodexCatHerbs));



                    OnPropertyChanged(nameof(IsCodexCatHorse));



                    OnPropertyChanged(nameof(IsCodexCatOther));



                }



            }



        }







        public bool IsCodexCatAll



        {



            get => CodexCategoryFilter == "All";



            set { if (value) CodexCategoryFilter = "All"; }



        }



        public bool IsCodexCatWeapons



        {



            get => CodexCategoryFilter == "Weapons";



            set { if (value) CodexCategoryFilter = "Weapons"; }



        }



        public bool IsCodexCatArmour



        {



            get => CodexCategoryFilter == "Armour";



            set { if (value) CodexCategoryFilter = "Armour"; }



        }



        public bool IsCodexCatFood



        {



            get => CodexCategoryFilter == "Food";



            set { if (value) CodexCategoryFilter = "Food"; }



        }



        public bool IsCodexCatBooks



        {



            get => CodexCategoryFilter == "Books";



            set { if (value) CodexCategoryFilter = "Books"; }



        }



        public bool IsCodexCatHerbs



        {



            get => CodexCategoryFilter == "Herbs";



            set { if (value) CodexCategoryFilter = "Herbs"; }



        }



        public bool IsCodexCatHorse



        {



            get => CodexCategoryFilter == "Horse";



            set { if (value) CodexCategoryFilter = "Horse"; }



        }



        public bool IsCodexCatOther



        {



            get => CodexCategoryFilter == "Other";



            set { if (value) CodexCategoryFilter = "Other"; }



        }







        private string _codexWorthFilter = "All"; // "All", "Under50", "50to500", "500to2500", "Above2500"



        public string CodexWorthFilter



        {



            get => _codexWorthFilter;



            set



            {



                if (SetProperty(ref _codexWorthFilter, value))



                {



                    FilteredCodexItems?.Refresh();



                }



            }



        }







        private string _codexSortOption = "NameAsc"; // "NameAsc", "NameDesc", "PriceDesc", "PriceAsc", "WeightDesc"



        public string CodexSortOption



        {



            get => _codexSortOption;



            set



            {



                if (SetProperty(ref _codexSortOption, value))



                {



                    ApplyCodexSorting();



                }



            }



        }







        private GameItemCodexDefinition? _selectedCodexItem;



        public GameItemCodexDefinition? SelectedCodexItem



        {



            get => _selectedCodexItem;



            set => SetProperty(ref _selectedCodexItem, value);



        }







        private string _spawnTargetStorage = "Inventory"; // "Inventory", "Stash", "Horse"



        public string SpawnTargetStorage



        {



            get => _spawnTargetStorage;



            set



            {



                if (SetProperty(ref _spawnTargetStorage, value))



                {



                    OnPropertyChanged(nameof(IsSpawnTargetInventory));



                    OnPropertyChanged(nameof(IsSpawnTargetStash));



                    OnPropertyChanged(nameof(IsSpawnTargetHorse));



                }



            }



        }







        public bool IsSpawnTargetInventory



        {



            get => SpawnTargetStorage == "Inventory";



            set { if (value) SpawnTargetStorage = "Inventory"; }



        }



        public bool IsSpawnTargetStash



        {



            get => SpawnTargetStorage == "Stash";



            set { if (value) SpawnTargetStorage = "Stash"; }



        }



        public bool IsSpawnTargetHorse



        {



            get => SpawnTargetStorage == "Horse";



            set { if (value) SpawnTargetStorage = "Horse"; }



        }







        private int _spawnQuantity = 1;



        public int SpawnQuantity



        {



            get => _spawnQuantity;



            set => SetProperty(ref _spawnQuantity, Math.Max(1, value));



        }







        private int _spawnCondition = 100;



        public int SpawnCondition



        {



            get => _spawnCondition;



            set => SetProperty(ref _spawnCondition, Math.Clamp(value, 0, 100));



        }







        private bool _spawnIsStolen = false;



        public bool SpawnIsStolen



        {



            get => _spawnIsStolen;



            set => SetProperty(ref _spawnIsStolen, value);



        }







        public ICommand SelectStorageFilterCommand { get; }



        public ICommand SelectCodexCategoryCommand { get; }



        public ICommand SelectCodexWorthFilterCommand { get; }



        public ICommand SelectSpawnTargetStorageCommand { get; }



        public ICommand AddSpawnQuantityCommand { get; }







        public string CurrentDirectoryPath



        {



            get => _currentDirectoryPath;



            set => SetProperty(ref _currentDirectoryPath, value);



        }







        public string SearchFilter



        {



            get => _searchFilter;



            set



            {



                if (SetProperty(ref _searchFilter, value))



                {



                    FilteredSaves.Refresh();



                }



            }



        }







        public string SelectedCategory



        {



            get => _selectedCategory;



            set



            {



                if (SetProperty(ref _selectedCategory, value))



                {



                    FilteredSaves.Refresh();



                }



            }



        }







        public SaveGameItemViewModel? SelectedSaveItem



        {



            get => _selectedSaveItem;



            set



            {



                if (SetProperty(ref _selectedSaveItem, value) && value != null)



                {



                    LoadSaveFile(value.FilePath);



                }



            }



        }







        public string SaveFilePath



        {



            get => _saveFilePath;



            set => SetProperty(ref _saveFilePath, value);



        }







        public string SaveName



        {



            get => _saveName;



            set => SetProperty(ref _saveName, value);



        }







        public string LevelName



        {



            get => _levelName;



            set => SetProperty(ref _levelName, value);



        }







        public string GameVersion



        {



            get => _gameVersion;



            set => SetProperty(ref _gameVersion, value);



        }







        public string PlaytimeString



        {



            get => _playtimeString;



            set => SetProperty(ref _playtimeString, value);



        }







        public LanguageOption? SelectedLanguage



        {



            get => _selectedLanguage;



            set



            {



                if (SetProperty(ref _selectedLanguage, value) && value != null)



                {



                    GameDataService.Instance.SetLanguage(value.Code);



                    foreach (var item in InventoryItems)



                    {



                        item.RefreshLocalization();



                    }



                    foreach (var item in HorseItems)



                    {



                        item.RefreshLocalization();



                    }



                    FilteredInventory.Refresh();



                    FilteredHorseItems.Refresh();



                    UpdateCategoryTabCounts();



                    NotifyAllLocalizationsChanged();



                    StatusMessage = IsGerman ? $"Spielsprache auf {value.DisplayName} gesetzt" : $"Game language set to {value.DisplayName}";



                }



            }



        }







        // =====================================================================



        // DYNAMIC LOCALIZATION PROPERTIES (Supports all 16 official languages)



        // =====================================================================



        public bool IsGerman => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);







        public string Loc_ActiveSave => GameDataService.Instance.GetUiText("Loc_ActiveSave", "AKTIVER SPIELSTAND (.WHS)", "ACTIVE SAVEGAME FILE (.WHS)");



        public string Loc_OpenSave => GameDataService.Instance.GetUiText("Loc_OpenSave", "📁 Öffnen...", "📁 Open...");



        public string Loc_OpenSaveTip => GameDataService.Instance.GetUiText("Loc_OpenSaveTip", "Einzelne .whs Save-Datei auswählen", "Select individual .whs save file");



        public string Loc_AutoDetect => GameDataService.Instance.GetUiText("Loc_AutoDetect", "🔍 Auto-Detect", "🔍 Auto-Detect");



        public string Loc_AutoDetectTip => GameDataService.Instance.GetUiText("Loc_AutoDetectTip", "Savegame-Verzeichnis automatisch scannen (.whs)", "Automatically scan savegame directory (.whs)");



        public string Loc_LanguageTip => GameDataService.Instance.GetUiText("Loc_LanguageTip", "Spielsprache für Texte und Item-Namen (aus Spieldateien geladen)", "Game language for texts and item names (loaded from game files)");



        public string Loc_RepairAll => GameDataService.Instance.GetUiText("Loc_RepairAll", "100% Reparieren", "Repair 100%");



        public string Loc_RepairAllTip => GameDataService.Instance.GetUiText("Loc_RepairAllTip", "Alle Waffen und Rüstungen auf 100% Zustand setzen und reinigen", "Restore all weapons and armor to 100% condition and clean");



        public string Loc_Legalize => GameDataService.Instance.GetUiText("Loc_Legalize", "Legalisieren", "Legalize");



        public string Loc_LegalizeTip => GameDataService.Instance.GetUiText("Loc_LegalizeTip", "Entfernt alle Diebesgut-Markierungen aus dem Inventar", "Remove all stolen flags from inventory");



        public string Loc_MaxStats => GameDataService.Instance.GetUiText("Loc_MaxStats", "Max Werte (30)", "Max Stats (30)");



        public string Loc_MaxStatsTip => GameDataService.Instance.GetUiText("Loc_MaxStatsTip", "Setzt alle Attribute von Heinrich auf das Maximum", "Set all of Henry's attributes to maximum (30)");



        public string Loc_ExportSave => GameDataService.Instance.GetUiText("Loc_ExportSave", "Savegame Exportieren (.WHS)", "Export Savegame (.WHS)");



        public string Loc_ExportSaveTip => GameDataService.Instance.GetUiText("Loc_ExportSaveTip", "Speichert und signiert die Änderungen mit CryEngine MD5 Trailer", "Saves and signs changes with CryEngine MD5 trailer");







        public string Loc_StorageHeader => GameDataService.Instance.GetUiText("Loc_StorageHeader", "LAGERORT / CONTAINER:", "STORAGE / CONTAINER:");



        public string Loc_StolenTooltip => GameDataService.Instance.GetUiText("Loc_StolenTooltip", "Diebesgut", "Stolen Item");



        public string Loc_EquippedTooltip => GameDataService.Instance.GetUiText("Loc_EquippedTooltip", "Ausgerüstet", "Equipped");



        public string Loc_BloodyTooltip => GameDataService.Instance.GetUiText("Loc_BloodyTooltip", "Blut am Gegenstand", "Bloody Item");



        public string Loc_MoveToStash => GameDataService.Instance.GetUiText("Loc_MoveToStash", "📦 In Truhe verschieben", "📦 Move to Stash");



        public string Loc_TakeToInventory => GameDataService.Instance.GetUiText("Loc_TakeToInventory", "🎒 Ins Inventar nehmen", "🎒 Take to Inventory");



        public string Loc_MaxHorseStatsTip => GameDataService.Instance.GetUiText("Loc_MaxHorseStatsTip", "Alle Pferdewerte maximieren", "Maximize all horse stats");



        public string Loc_MaxHorseStatsBtn => GameDataService.Instance.GetUiText("Loc_MaxHorseStatsBtn", "Max Pferd", "Max Horse");



        public string Loc_CraftSummary => GameDataService.Instance.GetUiText("Loc_CraftSummary", "  ❖  92 Rezepte (31 Alchemie, 61 Schmieden) • 31 Handwerkstalente", "  ❖  92 Recipes (31 Alchemy, 61 Blacksmithing) • 31 Crafting Perks");



        public string Loc_IngredientsHeader => GameDataService.Instance.GetUiText("Loc_IngredientsHeader", "🌾 Zutaten / Material: ", "🌾 Ingredients / Material: ");







        public string Loc_SpawnerCatAll => GameDataService.Instance.GetUiText("Loc_SpawnerCatAll", "🌐 Alle (5.604)", "🌐 All (5,604)");



        public string Loc_SpawnerCatWeapons => GameDataService.Instance.GetUiText("Loc_SpawnerCatWeapons", "⚔️ Waffen (763)", "⚔️ Weapons (763)");



        public string Loc_SpawnerCatArmour => GameDataService.Instance.GetUiText("Loc_SpawnerCatArmour", "🛡️ Rüstung (2.622)", "🛡️ Armour (2,622)");



        public string Loc_SpawnerCatFood => GameDataService.Instance.GetUiText("Loc_SpawnerCatFood", "🍗 Nahrung (322)", "🍗 Food (322)");



        public string Loc_SpawnerCatBooks => GameDataService.Instance.GetUiText("Loc_SpawnerCatBooks", "📜 Bücher (392)", "📜 Books (392)");



        public string Loc_SpawnerCatHerbs => GameDataService.Instance.GetUiText("Loc_SpawnerCatHerbs", "🌿 Kräuter (116)", "🌿 Herbs (116)");



        public string Loc_SpawnerCatHorse => GameDataService.Instance.GetUiText("Loc_SpawnerCatHorse", "🐎 Pferd (555)", "🐎 Horse (555)");



        public string Loc_SpawnerCatOther => GameDataService.Instance.GetUiText("Loc_SpawnerCatOther", "📦 Sonstiges (834)", "📦 Other (834)");







        public string Loc_PriceAll => GameDataService.Instance.GetUiText("Loc_PriceAll", "Alle Preisklassen", "All Price Ranges");



        public string Loc_PriceCheap => GameDataService.Instance.GetUiText("Loc_PriceCheap", "Günstig (< 50 Gr.)", "Inexpensive (< 50 Gr.)");



        public string Loc_PriceMedium => GameDataService.Instance.GetUiText("Loc_PriceMedium", "Mittel (50 - 500 Gr.)", "Medium (50 - 500 Gr.)");



        public string Loc_PriceValuable => GameDataService.Instance.GetUiText("Loc_PriceValuable", "Wertvoll (500 - 2.500)", "Valuable (500 - 2,500 Gr.)");



        public string Loc_PriceLegendary => GameDataService.Instance.GetUiText("Loc_PriceLegendary", "Legendär (> 2.500 Gr.)", "Legendary (> 2,500 Gr.)");







        public string Loc_SortNameAsc => GameDataService.Instance.GetUiText("Loc_SortNameAsc", "Name (A - Z)", "Name (A - Z)");



        public string Loc_SortNameDesc => GameDataService.Instance.GetUiText("Loc_SortNameDesc", "Name (Z - A)", "Name (Z - A)");



        public string Loc_SortPriceDesc => GameDataService.Instance.GetUiText("Loc_SortPriceDesc", "Wert (Abst.)", "Value (High to Low)");



        public string Loc_SortPriceAsc => GameDataService.Instance.GetUiText("Loc_SortPriceAsc", "Wert (Aufst.)", "Value (Low to High)");



        public string Loc_SortWeight => GameDataService.Instance.GetUiText("Loc_SortWeight", "Gewicht", "Weight");







        public string Loc_BasePrice => GameDataService.Instance.GetUiText("Loc_BasePrice", "GRUNDWERT", "BASE VALUE");



        public string Loc_Weight => GameDataService.Instance.GetUiText("Loc_Weight", "GEWICHT", "WEIGHT");







        public string Loc_TargetStorageInventory => GameDataService.Instance.GetUiText("Loc_TargetStorageInventory", "🎒 Inventar", "🎒 Inventory");



        public string Loc_TargetStorageStash => GameDataService.Instance.GetUiText("Loc_TargetStorageStash", "📦 Truhe", "📦 Stash");



        public string Loc_TargetStorageHorse => GameDataService.Instance.GetUiText("Loc_TargetStorageHorse", "🐎 Pferd", "🐎 Horse");







        public string Loc_WindowMinTip => GameDataService.Instance.GetUiText("Loc_WindowMinTip", "Minimieren", "Minimize");



        public string Loc_WindowMaxTip => GameDataService.Instance.GetUiText("Loc_WindowMaxTip", "Maximieren", "Maximize");



        public string Loc_WindowCloseTip => GameDataService.Instance.GetUiText("Loc_WindowCloseTip", "Schließen", "Close");



        public string Loc_WindowRestoreTip => GameDataService.Instance.GetUiText("Loc_WindowRestoreTip", "Wiederherstellen", "Restore");



        public string Loc_InspectorOutfit1 => GameDataService.Instance.GetUiText("Loc_InspectorOutfit1", "Outfit I", "Outfit I");



        public string Loc_InspectorOutfit2 => GameDataService.Instance.GetUiText("Loc_InspectorOutfit2", "Outfit II", "Outfit II");



        public string Loc_InspectorOutfit3 => GameDataService.Instance.GetUiText("Loc_InspectorOutfit3", "Outfit III", "Outfit III");



        public string Loc_OutfitLabel => GameDataService.Instance.GetUiText("Loc_OutfitLabel", "OUTFIT", "OUTFIT");
        public string Loc_HorseTackLabel => GameDataService.Instance.GetUiText("Loc_HorseTackLabel", "SATTELZEUG", "HORSE TACK");
        public string Loc_HorseTackWarhorseTip => GameDataService.Instance.GetUiText("Loc_HorseTackWarhorseTip", "Sattelzeug I: Streitross (Panzerung & Mut)", "Tack I: Warhorse (Armor & Courage)");
        public string Loc_HorseTackTravelTip => GameDataService.Instance.GetUiText("Loc_HorseTackTravelTip", "Sattelzeug II: Reise & Tempo (Max. Geschwindigkeit)", "Tack II: Travel & Speed (Max Speed)");
        public string Loc_HorseTackNobleTip => GameDataService.Instance.GetUiText("Loc_HorseTackNobleTip", "Sattelzeug III: Prunk & Adel (Max. Tragkraft & 4 Taschen)", "Tack III: Noble Courser (Max Capacity & 4 Bags)");

        public string Loc_SlotPickerTitle => GameDataService.Instance.GetUiText("Loc_SlotPickerTitle", "Ausrüstung auswählen", "Select Equipment");
        public string Loc_SlotPickerSubtitle => GameDataService.Instance.GetUiText("Loc_SlotPickerSubtitle", "Wähle einen Gegenstand zum Ausrüsten oder leere den Slot.", "Select an item to equip or unequip this slot.");
        public string Loc_SlotPickerCurrent => GameDataService.Instance.GetUiText("Loc_SlotPickerCurrent", "Aktuell in Slot:", "Currently in slot:");
        public string Loc_SlotPickerUnequip => GameDataService.Instance.GetUiText("Loc_SlotPickerUnequip", "✕ Slot leeren", "✕ Unequip Slot");
        public string Loc_SlotPickerEquip => GameDataService.Instance.GetUiText("Loc_SlotPickerEquip", "✓ Ausrüsten", "✓ Equip");
        public string Loc_SlotPickerCancel => GameDataService.Instance.GetUiText("Loc_SlotPickerCancel", "Abbrechen", "Cancel");
        public string Loc_SlotPickerSourceInv => GameDataService.Instance.GetUiText("Loc_SlotPickerSourceInv", "🎒 Spieler- & Reittier-Inventar", "🎒 Player & Mount Inventory");
        public string Loc_SlotPickerSourceAll => GameDataService.Instance.GetUiText("Loc_SlotPickerSourceAll", "📖 Alle Gegenstände", "📖 All Game Items");
        public string Loc_SlotPickerSearchPlaceholder => GameDataService.Instance.GetUiText("Loc_SlotPickerSearchPlaceholder", "Gegenstand suchen...", "Search item...");
        public string Loc_SlotPickerPrice => GameDataService.Instance.GetUiText("Loc_SlotPickerPrice", "Wert / Preis:", "Value / Price:");
        public string Loc_SlotPickerWeight => GameDataService.Instance.GetUiText("Loc_SlotPickerWeight", "Gewicht:", "Weight:");
        public string Loc_SlotPickerTier => GameDataService.Instance.GetUiText("Loc_SlotPickerTier", "Stufe / Rang:", "Tier / Rank:");
        public string Loc_SlotPickerCategory => GameDataService.Instance.GetUiText("Loc_SlotPickerCategory", "Kategorie", "Category");
        public string Loc_SlotPickerType => GameDataService.Instance.GetUiText("Loc_SlotPickerType", "Typ", "Type");
        public string Loc_SlotPickerCondition => GameDataService.Instance.GetUiText("Loc_SlotPickerCondition", "Zustand", "Condition");
        public string Loc_SlotPickerOutfitNotice => SelectedPaperdollSlot?.Id.StartsWith("horse_") == true
            ? GameDataService.Instance.GetUiText("Loc_SlotPickerHorseNotice", "Dieses Item wird für das aktuell aktive Pferde-Sattelzeug ausgerüstet.", "This item will be equipped for the currently active horse tack.")
            : GameDataService.Instance.GetUiText("Loc_SlotPickerOutfitNotice", "Dieses Item wird für das aktuell aktive Outfit ausgerüstet.", "This item will be equipped for the currently active outfit.");



        // Platform Detection (Xbox vs Steam)



        private string _currentPlatform = "Xbox";



        private string? _detectedXboxDir;



        private string? _detectedSteamDir;



        private int _xboxSavesCount;



        private int _steamSavesCount;







        public string CurrentPlatform



        {



            get => _currentPlatform;



            set



            {



                if (SetProperty(ref _currentPlatform, value))



                {



                    OnPropertyChanged(nameof(IsXboxActive));



                    OnPropertyChanged(nameof(IsSteamActive));



                    OnPropertyChanged(nameof(CurrentPlatformIcon));



                    OnPropertyChanged(nameof(CurrentPlatformName));



                    OnPropertyChanged(nameof(CurrentPlatformBadgeBackground));



                    OnPropertyChanged(nameof(CurrentPlatformBadgeBorder));



                    OnPropertyChanged(nameof(CurrentPlatformBadgeForeground));



                    OnPropertyChanged(nameof(CurrentPlatformTooltip));



                    OnPropertyChanged(nameof(XboxButtonBackground));



                    OnPropertyChanged(nameof(XboxButtonBorder));



                    OnPropertyChanged(nameof(XboxButtonForeground));



                    OnPropertyChanged(nameof(SteamButtonBackground));



                    OnPropertyChanged(nameof(SteamButtonBorder));



                    OnPropertyChanged(nameof(SteamButtonForeground));



                }



            }



        }







        public bool IsXboxActive => CurrentPlatform.Equals("Xbox", StringComparison.OrdinalIgnoreCase);



        public bool IsSteamActive => CurrentPlatform.Equals("Steam", StringComparison.OrdinalIgnoreCase);







        public string CurrentPlatformIcon => IsXboxActive ? "🎮" : (IsSteamActive ? "💨" : "💾");



        public string CurrentPlatformName => IsXboxActive ? "XBOX" : (IsSteamActive ? "STEAM" : "CUSTOM");



        public string CurrentPlatformBadgeBackground => IsXboxActive ? "#14532D" : (IsSteamActive ? "#0C4A6E" : "#1E293B");



        public string CurrentPlatformBadgeBorder => IsXboxActive ? "#22C55E" : (IsSteamActive ? "#38BDF8" : "#475569");



        public string CurrentPlatformBadgeForeground => IsXboxActive ? "#4ADE80" : (IsSteamActive ? "#7DD3FC" : "#94A3B8");



        public string CurrentPlatformTooltip => IsXboxActive



            ? (IsGerman ? "Xbox Game Pass Spielstände geladen" : "Xbox Game Pass saves loaded")



            : (IsSteamActive ? (IsGerman ? "Steam Spielstände geladen" : "Steam saves loaded") : (IsGerman ? "Benutzerdefinierte Spielstände" : "Custom saves"));







        public int XboxSavesCount



        {



            get => _xboxSavesCount;



            set



            {



                if (SetProperty(ref _xboxSavesCount, value))



                {



                    OnPropertyChanged(nameof(HasXboxSaves));



                    OnPropertyChanged(nameof(HasBothPlatforms));



                    OnPropertyChanged(nameof(HasXboxSavesVisibility));



                    OnPropertyChanged(nameof(HasBothPlatformsVisibility));



                }



            }



        }







        public int SteamSavesCount



        {



            get => _steamSavesCount;



            set



            {



                if (SetProperty(ref _steamSavesCount, value))



                {



                    OnPropertyChanged(nameof(HasSteamSaves));



                    OnPropertyChanged(nameof(HasBothPlatforms));



                    OnPropertyChanged(nameof(HasSteamSavesVisibility));



                    OnPropertyChanged(nameof(HasBothPlatformsVisibility));



                }



            }



        }







        public bool HasXboxSaves => XboxSavesCount > 0;



        public bool HasSteamSaves => SteamSavesCount > 0;



        public bool HasBothPlatforms => HasXboxSaves && HasSteamSaves;







        public System.Windows.Visibility HasXboxSavesVisibility => HasXboxSaves ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;



        public System.Windows.Visibility HasSteamSavesVisibility => HasSteamSaves ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;



        public System.Windows.Visibility HasBothPlatformsVisibility => HasBothPlatforms ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;







        public string XboxButtonBackground => IsXboxActive ? "#14532D" : "#181F2C";



        public string XboxButtonBorder => IsXboxActive ? "#22C55E" : "#334155";



        public string XboxButtonForeground => IsXboxActive ? "#4ADE80" : "#94A3B8";







        public string SteamButtonBackground => IsSteamActive ? "#0C4A6E" : "#181F2C";



        public string SteamButtonBorder => IsSteamActive ? "#38BDF8" : "#334155";



        public string SteamButtonForeground => IsSteamActive ? "#7DD3FC" : "#94A3B8";







        public string Loc_SwitchToXboxTip => GameDataService.Instance.GetUiText("SwitchToXboxTip", "Zu Xbox Game Pass Spielständen wechseln", "Switch to Xbox Game Pass saves");



        public string Loc_SwitchToSteamTip => GameDataService.Instance.GetUiText("SwitchToSteamTip", "Zu Steam Spielständen wechseln", "Switch to Steam saves");







        public string Loc_CaparisonDefault => GameDataService.Instance.GetUiText("Loc_CaparisonDefault", "Kuttenberger Gold", "Kuttenberg Gold");



        public string Loc_SaddleDefault => GameDataService.Instance.GetUiText("Loc_SaddleDefault", "Edler 4-Taschen", "Noble 4-Pouch");



        public string Loc_BridleDefault => GameDataService.Instance.GetUiText("Loc_BridleDefault", "Ritterzaumzeug", "Knight's Bridle");



        public string Loc_HorseshoesDefault => GameDataService.Instance.GetUiText("Loc_HorseshoesDefault", "Militär-Hufeisen", "Military Horseshoes");







        public string Loc_SearchPlaceholder => GameDataService.Instance.GetUiText("Loc_SearchPlaceholder", "Gegenstände oder ID durchsuchen...", "Search items or ID...");



        public string Loc_FilterEquipped => GameDataService.Instance.GetUiText("Loc_FilterEquipped", "Nur Ausgerüstet", "Equipped Only");



        public string Loc_AddItem => GameDataService.Instance.GetUiText("Loc_AddItem", "Item hinzufügen", "Add Item");







        public string Loc_ColItemCategory => GameDataService.Instance.GetUiText("Loc_ColItemCategory", "A-Z Gegenstand / Kategorie", "A-Z Item / Category");



        public string Loc_ColEquippedTip => GameDataService.Instance.GetUiText("Loc_ColEquippedTip", "Ausgerüstet in Outfit-Slot", "Equipped in outfit slot");



        public string Loc_ColQuantityTip => GameDataService.Instance.GetUiText("Loc_ColQuantityTip", "Menge / Stack", "Quantity / Stack");



        public string Loc_ColPowerTip => GameDataService.Instance.GetUiText("Loc_ColPowerTip", "Angriff / Schutzwert", "Attack / Defense Rating");



        public string Loc_ColConditionTip => GameDataService.Instance.GetUiText("Loc_ColConditionTip", "Zustand (% Haltbarkeit)", "Condition (% Durability)");



        public string Loc_ColWeightTip => GameDataService.Instance.GetUiText("Loc_ColWeightTip", "Gewicht (kg)", "Weight (kg)");



        public string Loc_ColPrice => GameDataService.Instance.GetUiText("Loc_ColPrice", "Preis", "Price");







        public string Loc_InspectorCondition => GameDataService.Instance.GetUiText("Loc_InspectorCondition", "Zustand:", "Condition:");



        public string Loc_InspectorBlood => GameDataService.Instance.GetUiText("Loc_InspectorBlood", "Blut/Schmutz:", "Blood/Dirt:");



        public string Loc_InspectorAmount => GameDataService.Instance.GetUiText("Loc_InspectorAmount", "Menge (#)", "Amount (#)");



        public string Loc_InspectorPower => GameDataService.Instance.GetUiText("Loc_InspectorPower", "Stärke/Schutz", "Power/Defense");



        public string Loc_InspectorWeight => GameDataService.Instance.GetUiText("Loc_InspectorWeight", "Gewicht", "Weight");



        public string Loc_InspectorPrice => GameDataService.Instance.GetUiText("Loc_InspectorPrice", "Preis (Gr.)", "Price (Gr.)");



        public string Loc_InspectorEquipment => GameDataService.Instance.GetUiText("Loc_InspectorEquipment", "Ausrüstung: ", "Equipment: ");



        public string Loc_InspectorStolen => GameDataService.Instance.GetUiText("Loc_InspectorStolen", "✋ Gestohlen", "✋ Stolen");



        public string Loc_InspectorDuplicate => GameDataService.Instance.GetUiText("Loc_InspectorDuplicate", "Duplizieren", "Duplicate");



        public string Loc_InspectorDelete => GameDataService.Instance.GetUiText("Loc_InspectorDelete", "Löschen", "Delete");







        public string Loc_FooterGroschen => "💰 GROSCHEN:";



        public string Loc_FooterCarryWeight => GameDataService.Instance.GetUiText("Loc_FooterCarryWeight", "TRAGLAST:", "CARRY WEIGHT:");



        public string Loc_FooterLevel => GameDataService.Instance.GetUiText("Loc_FooterLevel", "HAUPTSTUFE: ", "MAIN LEVEL: ");







        public string Loc_RightHenryTitle => IsGerman ? $"{GameDataService.Instance.GetUiText("Henry", "HEINRICH", "HENRY").ToUpperInvariant()} VON SKALITZ" : $"{GameDataService.Instance.GetUiText("Henry", "HEINRICH", "HENRY").ToUpperInvariant()} OF SKALITZ";



        public string Loc_RightTabSplit => GameDataService.Instance.GetUiText("All", "Gesamtansicht", "Overview");



        public string Loc_RightTabPerks => GameDataService.Instance.GetUiText("Reputation", "Ruf & Perks", "Rep & Perks");



        public string Loc_RightTabHorse => $"🐎 {GameDataService.Instance.GetUiText("Horse", "Pferd", "Horse")}";



        public string Loc_RightTabDlcs => $"🎁 {GameDataService.Instance.GetUiText("Dlcs", "DLCs", "DLCs")}";







        public string Loc_ArmorDefense => GameDataService.Instance.GetUiText("Armour", "RÜSTUNG (SCHUTZ)", "ARMOUR (DEFENSE)");



        public string Loc_Stab => "🗡️ " + GameDataService.Instance.GetUiText("ui_combat_stab", "Stich", "Stab");



        public string Loc_Slash => "⚔️ " + GameDataService.Instance.GetUiText("ui_combat_slash", "Hieb", "Slash");



        public string Loc_Smash => "🔨 " + GameDataService.Instance.GetUiText("ui_combat_blunt", "Schlag", "Blunt");



        public string Loc_Head => "🪖 " + GameDataService.Instance.GetUiText("ui_bodypart_head", "Kopf", "Head");



        public string Loc_Body => "🦺 " + GameDataService.Instance.GetUiText("ui_bodypart_torso", "Körper", "Body");



        public string Loc_Arms => "🧤 " + GameDataService.Instance.GetUiText("ui_bodypart_arms", "Arme", "Arms");



        public string Loc_Legs => "🦵 " + GameDataService.Instance.GetUiText("ui_bodypart_legs", "Beine", "Legs");







        public string Loc_WeaponAttack => GameDataService.Instance.GetUiText("Weapons", "WAFFEN (ANGRIFF)", "WEAPONS (ATTACK)");



        public string Loc_Melee => GameDataService.Instance.GetUiText("Melee", "Nahkampf", "Melee");



        public string Loc_Ranged => GameDataService.Instance.GetUiText("Ranged", "Fernkampf", "Ranged");



        public string Loc_Unarmed => GameDataService.Instance.GetUiText("Unarmed", "Waffenlos", "Unarmed");







        public string Loc_Strength => GameDataService.Instance.GetUiText("Strength", "Stärke", "Strength");



        public string Loc_Agility => GameDataService.Instance.GetUiText("Agility", "Agilität", "Agility");



        public string Loc_Vitality => GameDataService.Instance.GetUiText("Vitality", "Vitalität", "Vitality");



        public string Loc_Speech => GameDataService.Instance.GetUiText("Speech", "Rede", "Speech");



        public string Loc_Charisma => GameDataService.Instance.GetUiText("Charisma", "Charisma", "Charisma");



        public string Loc_Speed => GameDataService.Instance.GetUiText("Speed", "Geschw.", "Speed");







        public string Loc_Conspicuousness => GameDataService.Instance.GetUiText("Conspicuousness", "Auffällig", "Conspic.");



        public string Loc_Noise => GameDataService.Instance.GetUiText("Noise", "Geräusch", "Noise");



        public string Loc_Visibility => GameDataService.Instance.GetUiText("Visibility", "Sichtbar", "Visible");



        public string Loc_Health => GameDataService.Instance.GetUiText("Health", "Leben", "Health");



        public string Loc_Stamina => GameDataService.Instance.GetUiText("Stamina", "Ausdauer", "Stamina");



        public string Loc_Energy => GameDataService.Instance.GetUiText("Energy", "Energie", "Energy");







        public string Loc_FactionRepTitle => GameDataService.Instance.GetUiText("Reputation", "FRAKTIONS-RUF (BÖHMEN / KUTTENBERG & TROSKY)", "FACTION REPUTATION (BOHEMIA / KUTTENBERG & TROSKY)");



        public string Loc_SpecialPerksTitle => GameDataService.Instance.GetUiText("Perks", "SPEZIAL-PERKS (SAVEGAME FLAGS)", "SPECIAL PERKS (SAVEGAME FLAGS)");







        public string Loc_MaxHorseStats => GameDataService.Instance.GetUiText("Loc_MaxHorseStats", "Max Pferd Stats", "Max Horse Stats");



        public string Loc_HorseHealth => GameDataService.Instance.GetUiText("Health", "Gesundheit:", "Health:");



        public string Loc_HorseStamina => GameDataService.Instance.GetUiText("Stamina", "Ausdauer:", "Stamina:");



        public string Loc_HorseSpeed => GameDataService.Instance.GetUiText("Speed", "Geschwind.:", "Speed:");



        public string Loc_HorseCapacity => GameDataService.Instance.GetUiText("Capacity", "Tragkraft:", "Capacity:");



        public string Loc_HorseCourage => GameDataService.Instance.GetUiText("Courage", "Mut:", "Courage:");



        public string Loc_HorseInventory => GameDataService.Instance.GetUiText("HorseInventory", "PFERDE-INVENTAR (SATTELTASCHEN)", "HORSE INVENTORY (SADDLEBAGS)");







        public string Loc_DlcTitle => GameDataService.Instance.GetUiText("Dlcs", "DLC-MANAGER (MANIFEST STRIPPING)", "DLC MANAGER (MANIFEST STRIPPING)");

        public string Loc_DlcSubtitle => GameDataService.Instance.GetUiText("DlcSubtitle",
            "Verwalte DLC-Abhängigkeiten im Spielstand-Manifest. Deaktiviere DLCs ('Manifest-Stripping'), damit Spieler ohne diese DLCs deinen Spielstand problemlos laden können.",
            "Manage DLC dependencies in the savegame manifest. Disable DLCs ('Manifest Stripping') so players without those DLCs can seamlessly load your savegame.");

        public string Loc_EnableAllDlcs => GameDataService.Instance.GetUiText("EnableAllDlcs", "Alle DLCs aktivieren", "Enable All DLCs");

        public string Loc_DisableAllDlcs => GameDataService.Instance.GetUiText("DisableAllDlcs", "Alle DLCs deaktivieren (Manifest strippen)", "Disable All DLCs (Strip Manifest)");

        public string Loc_DlcHelpTitle => GameDataService.Instance.GetUiText("DlcHelpTitle", "Wie funktioniert Manifest-Stripping?", "How does Manifest Stripping work?");

        public string Loc_DlcHelpDesc => GameDataService.Instance.GetUiText("DlcHelpDesc",
            "In Kingdom Come: Deliverance II speichert die CryEngine installierte Erweiterungen im internen Manifest des Spielstands. Fehlt einem Spieler eine dieser Erweiterungen, verweigert das Spiel das Laden mit einem Fehler.\n\nDurch das Abwählen einzelner DLCs oder Klick auf 'Alle DLCs deaktivieren' entfernt der Editor die DLC-Verweise sauber aus dem Manifest. Der Spielstand wird damit universal kompatibel für alle Spieler, auch wenn sie nur das Basisspiel besitzen.",
            "In Kingdom Come: Deliverance II, CryEngine embeds all installed expansions in the savegame manifest XML. If a player lacks any of these expansions, the game rejects loading with a missing DLC error.\n\nBy unchecking individual DLCs or clicking 'Disable All DLCs', the editor cleanly strips the DLC references from the manifest. The savegame becomes universally compatible for all players, even those who only own the base game.");

        public string Loc_DlcHowToTitle => GameDataService.Instance.GetUiText("DlcHowToTitle", "Schritt-für-Schritt Anleitung", "Step-by-Step Instructions");

        public string Loc_DlcStep1 => GameDataService.Instance.GetUiText("DlcStep1", "1. Wähle die gewünschten DLC-Häkchen ab oder klicke auf 'Alle DLCs deaktivieren'.", "1. Uncheck unwanted DLCs or click 'Disable All DLCs'.");

        public string Loc_DlcStep2 => GameDataService.Instance.GetUiText("DlcStep2", "2. Klicke oben rechts auf 'Speichern & Signieren' (oder Schnellspeichern).", "2. Click 'Save & Sign' in the top right (or Quick Save).");

        public string Loc_DlcStep3 => GameDataService.Instance.GetUiText("DlcStep3", "3. Gib die .whs-Datei an Freunde weiter – sie lädt ohne DLC-Fehler auf jeder Installation!", "3. Share the .whs file with friends – it loads cleanly on any base installation!");

        public string Loc_DlcCustomAddTitle => GameDataService.Instance.GetUiText("DlcCustomAddTitle", "Eigenes DLC / Mod-Erweiterungs-ID hinzufügen", "Add Custom DLC / Mod Expansion ID");

        public string Loc_DlcCustomAddBtn => GameDataService.Instance.GetUiText("DlcCustomAddBtn", "+ DLC hinzufügen", "+ Add DLC");

        public string Loc_DlcCustomPlaceholder => GameDataService.Instance.GetUiText("DlcCustomPlaceholder", "DLC-ID eingeben (z. B. DLC_Exp1)...", "Enter DLC ID (e.g. DLC_Exp1)...");

        public string Loc_DlcActiveCountText => string.Format(GameDataService.Instance.GetUiText("DlcActiveCount", "{0} von {1} DLCs aktiv", "{0} of {1} DLCs active"), AvailableDlcs.Count(d => d.IsEnabled), AvailableDlcs.Count);







        public string Loc_BackupCheck => GameDataService.Instance.GetUiText("BackupCheck", "Backup erstellen (.bak)", "Create backup (.bak)");



        public string Loc_NoSaveLoaded => GameDataService.Instance.GetUiText("NoSaveLoaded", "Kein Spielstand geladen", "No savegame loaded");







        public string Loc_OutfitCombatTip => GameDataService.Instance.GetUiText("OutfitCombatTip", "Outfit 1: Kampfrüstung", "Outfit 1: Combat Armour");



        public string Loc_OutfitStealthTip => GameDataService.Instance.GetUiText("OutfitStealthTip", "Outfit 2: Jagd & Tarnung", "Outfit 2: Hunting & Stealth");



        public string Loc_OutfitNobleTip => GameDataService.Instance.GetUiText("OutfitNobleTip", "Outfit 3: Kuttenberger Adelstracht", "Outfit 3: Kuttenberg Noble Attire");



        public string Loc_CleanDirtTip => GameDataService.Instance.GetUiText("CleanDirtTip", "Blut & Schmutz vom Charakter waschen", "Wash blood & dirt from character");



        public string Loc_ArchPedestal => GameDataService.Instance.GetUiText("ArchPedestal", "RÜSTUNG: VOLLPLATTE • PWR 620", "ARMOUR: FULL PLATE • PWR 620");







        public string Loc_StorageAll => GameDataService.Instance.GetUiText("Storage_All", "🌐 Alle Gegenstände", "🌐 All Items");



        public string Loc_StorageInventory => GameDataService.Instance.GetUiText("Storage_Inventory", "🎒 Spieler-Inventar", "🎒 Player Inventory");



        public string Loc_StorageStash => GameDataService.Instance.GetUiText("Storage_Stash", "📦 Spieler-Truhe (Stash)", "📦 Player Stash");



        public string Loc_StorageHorse => GameDataService.Instance.GetUiText("Storage_Horse", "🐎 Reittier (Pferd)", "🐎 Horse");







        public string Loc_SpawnerTitle => GameDataService.Instance.GetUiText("SpawnerTitle", "✨ GEGENSTAND AUS SPIELDATEN HINZUFÜGEN", "✨ ADD ITEM FROM GAME DATABASE");



        public string Loc_SpawnerSubtitle => GameDataService.Instance.GetUiText("SpawnerSubtitle", "Durchsuche alle 5.604 offiziellen Gegenstände aus Kingdom Come: Deliverance II", "Browse all 5,604 official items from Kingdom Come: Deliverance II");



        public string Loc_SpawnerPresetLabel => GameDataService.Instance.GetUiText("SpawnerPresetLabel", "Vorlage aus Spieldatenbank wählen:", "Select preset from game database:");



        public string Loc_SpawnerPresetCustom => GameDataService.Instance.GetUiText("SpawnerPresetCustom", "-- Eigenes Item erstellen --", "-- Create custom item --");



        public string Loc_SpawnerName => GameDataService.Instance.GetUiText("SpawnerName", "Name (Anzeigename)", "Name (Display Name)");



        public string Loc_SpawnerCategory => GameDataService.Instance.GetUiText("SpawnerCategory", "Kategorie", "Category");



        public string Loc_SpawnerAmount => GameDataService.Instance.GetUiText("SpawnerAmount", "Anzahl (#)", "Quantity (#)");



        public string Loc_SpawnerPower => GameDataService.Instance.GetUiText("SpawnerPower", "Stärke/Schutz", "Power/Defense");



        public string Loc_SpawnerWeight => GameDataService.Instance.GetUiText("SpawnerWeight", "Gewicht", "Weight");



        public string Loc_SpawnerPrice => GameDataService.Instance.GetUiText("SpawnerPrice", "Preis (Gr.)", "Price (Gr.)");



        public string Loc_SpawnerCancel => GameDataService.Instance.GetUiText("Cancel", "Abbrechen", "Cancel");



        public string Loc_SpawnerAdd => GameDataService.Instance.GetUiText("Add", "✨ Gegenstand hinzufügen", "✨ Add Item");



        public string Loc_SpawnerTargetStorage => GameDataService.Instance.GetUiText("SpawnerTargetStorage", "Ziel-Lagerort:", "Target Storage Container:");



        public string Loc_SpawnerCondition => GameDataService.Instance.GetUiText("SpawnerCondition", "Zustand (%):", "Condition (%):");



        public string Loc_SpawnerQuantity => GameDataService.Instance.GetUiText("SpawnerQuantity", "Menge:", "Quantity:");



        public string Loc_SpawnerStolen => GameDataService.Instance.GetUiText("SpawnerStolen", "✋ Als Diebesgut markieren", "✋ Mark as Stolen");



        public string Loc_SpawnerPriceFilter => GameDataService.Instance.GetUiText("SpawnerPriceFilter", "Preisklasse:", "Price Range:");



        public string Loc_SpawnerSort => GameDataService.Instance.GetUiText("SpawnerSort", "Sortierung:", "Sort By:");







        // V2 Navigation Ribbon



        public string Loc_NavQuestlog => GameDataService.Instance.GetUiText("Nav_Questlog", "QUESTLOG", "QUEST LOG");



        public string Loc_NavInventar => GameDataService.Instance.GetUiText("Nav_Inventar", "INVENTAR", "INVENTORY");



        public string Loc_NavSpieler => GameDataService.Instance.GetUiText("Nav_Spieler", "SPIELER", "PLAYER");



        public string Loc_NavHandwerk => GameDataService.Instance.GetUiText("Nav_Handwerk", "HANDWERKSKUNST", "CRAFTSMANSHIP");

        public string Loc_NavDlcs => GameDataService.Instance.GetUiText("Nav_Dlcs", "DLCs", "DLCs");







        // Target Switcher in Inventory Header



        public string Loc_TargetHenry => GameDataService.Instance.GetUiText("Henry", "Heinrich", "Henry");



        public string Loc_TargetHorse => GameDataService.Instance.GetUiText("Pebbles", "Pferd (Schimmel)", "Horse (Pebbles)");



        public string Loc_RightHorseTitle => $"{GameDataService.Instance.GetUiText("Pebbles", "Schimmel (Pebbles)", "Pebbles")} // {GameDataService.Instance.GetUiText("Horse", "Reittier & Satteltaschen", "Mount & Saddlebags")}";







        // Questlog



        public string Loc_QuestlogTitle => GameDataService.Instance.GetUiText("Nav_Questlog", "Böhmisches Questlog", "Bohemian Quest Log");



        public string Loc_QuestUnlockAll => GameDataService.Instance.GetUiText("UnlockAll", "Alle Freischalten", "Unlock All");



        public string Loc_QuestTabAll => GameDataService.Instance.GetUiText("All", "Alle", "All");



        public string Loc_QuestTabMain => GameDataService.Instance.GetUiText("Main", "Haupt", "Main");



        public string Loc_QuestTabSide => GameDataService.Instance.GetUiText("Side", "Neben", "Side");



        public string Loc_QuestTabActivity => GameDataService.Instance.GetUiText("Activity", "Aktivität", "Activity");



        public string Loc_QuestObjectivesHeader => GameDataService.Instance.GetUiText("QuestObjectives", "Quest-Aufgaben & Phasen-Fortschritt", "Quest Objectives & Stage Progress");



        public string Loc_QuestCompleteAll => GameDataService.Instance.GetUiText("CompleteAll", "✓ Alle Ziele Abhaken", "✓ Complete All Objectives");



        public string Loc_QuestStatusActive => GameDataService.Instance.GetUiText("Active", "Aktiv", "Active");



        public string Loc_QuestStatusCompleted => GameDataService.Instance.GetUiText("Completed", "Abgeschlossen", "Completed");



        public string Loc_QuestStatusFailed => GameDataService.Instance.GetUiText("Failed", "Fehlgeschlagen", "Failed");



        public string Loc_SelectHorseLabel => GameDataService.Instance.GetUiText("Loc_SelectHorseLabel", "REITTIER / PFERD WÄHLEN", "SELECT MOUNT / HORSE");



        public string Loc_QuestSearchPlaceholder => GameDataService.Instance.GetUiText("QuestSearchPlaceholder", "Quest suchen...", "Search quest...");







        // Spieler



        public string Loc_PlayerSubHenry => IsGerman ? $"{GameDataService.Instance.GetUiText("Henry", "Heinrich", "Henry")} von Skalitz" : $"{GameDataService.Instance.GetUiText("Henry", "Heinrich", "Henry")} of Skalitz";



        public string Loc_PlayerSubDog => IsGerman ? $"{GameDataService.Instance.GetUiText("Dog", "Köter", "Mutt")} (Gefährte / Hund)" : $"{GameDataService.Instance.GetUiText("Dog", "Köter", "Mutt")} (Companion / Dog)";



        public string Loc_PlayerSubHorse => IsGerman ? $"{GameDataService.Instance.GetUiText("Pebbles", "Schimmel", "Pebbles")} (Reittier / Streitross)" : $"{GameDataService.Instance.GetUiText("Pebbles", "Schimmel", "Pebbles")} (Mount / Warhorse)";



        public string Loc_CompanionAiStatus => GameDataService.Instance.GetUiText("CompanionAiStatus", "Gefährten-KI Status: Verbündet & Aktiv", "Companion AI Status: Allied & Active");



        public string Loc_DogTitle => $"{GameDataService.Instance.GetUiText("Dog", "Köter", "Mutt")}";



        public string Loc_DogSubtitle => GameDataService.Instance.GetUiText("DogSubtitle", "Treuer Gefährte von Skalitz", "Faithful Companion of Skalitz");



        public string Loc_DogCondition => GameDataService.Instance.GetUiText("Condition", "Zustand:", "Condition:");



        public string Loc_DogConditionVal => GameDataService.Instance.GetUiText("DogConditionVal", "Wohlgenährt & Wachsam", "Well-fed & Alert");



        public string Loc_DogRank => GameDataService.Instance.GetUiText("Rank", "Rang:", "Rank:");



        public string Loc_DogRankVal => GameDataService.Instance.GetUiText("DogRankVal", "Meisterhund (Rang 20)", "Master Hound (Rank 20)");



        public string Loc_DogCommand => GameDataService.Instance.GetUiText("Command", "Befehl:", "Command:");



        public string Loc_DogCommandVal => GameDataService.Instance.GetUiText("DogCommandVal", "Bei Fuß / Schutzsuche", "Heel / Seek Protection");



        public string Loc_DogMax => GameDataService.Instance.GetUiText("DogMax", "Köter Maximieren (Treue 100% & Alle Skills)", "Maximize Mutt (100% Loyalty & All Skills)");



        public string Loc_DogAttributesHeader => GameDataService.Instance.GetUiText("DogAttributes", "Gefährten-Attribute & Psychologie", "Companion Attributes & Psychology");



        public string Loc_DogPerksHeader => GameDataService.Instance.GetUiText("DogPerks", "Hundeflüsterer-Perks (Savegame-Flags)", "Houndmaster Perks (Savegame Flags)");



        public string Loc_DogLoyalty => GameDataService.Instance.GetUiText("Loyalty", "Treue", "Loyalty");



        public string Loc_DogObedience => GameDataService.Instance.GetUiText("Obedience", "Gehorsam", "Obedience");



        public string Loc_DogNourishment => GameDataService.Instance.GetUiText("Nourishment", "Sättigung", "Nourishment");



        public string Loc_DogAggression => GameDataService.Instance.GetUiText("Aggression", "Aggression (Gegen Feinde)", "Aggression (Against Enemies)");







        public string Loc_FactionsTitle => GameDataService.Instance.GetUiText("Reputation", "Fraktions-Ruf (Böhmisches Reich & Kuttenberg)", "Faction Reputation (Bohemian Realm & Kuttenberg)");



        public string Loc_PerksTitle => GameDataService.Instance.GetUiText("Perks", "Spezial-Perks & Meister-Fähigkeiten", "Special Perks & Master Abilities");



        public string Loc_PerksUnlockAll => GameDataService.Instance.GetUiText("UnlockAll", "Alle Freischalten", "Unlock All");







        public string Loc_HorseFullMountTitle => $"{GameDataService.Instance.GetUiText("Pebbles", "Schimmel (Pebbles)", "Pebbles")} — {GameDataService.Instance.GetUiText("HorseEquipment", "Reittier-Ausrüstung", "Mount Equipment")}";



        public string Loc_HorseFullMountDesc => GameDataService.Instance.GetUiText("HorseDesc", "Verwaltung des Streitross-Inventars & Sattelmodifikationen.", "Warhorse inventory management & saddle modifications.");



        public string Loc_HorseCaparison => GameDataService.Instance.GetUiText("Caparison", "Schabracke", "Caparison");



        public string Loc_HorseSaddle => GameDataService.Instance.GetUiText("Saddle", "Sattel", "Saddle");



        public string Loc_HorseBridle => GameDataService.Instance.GetUiText("Bridle", "Zaumzeug", "Bridle");



        public string Loc_HorseShoes => GameDataService.Instance.GetUiText("Horseshoes", "Hufeisen", "Horseshoes");



        public string Loc_HorseRidingSkills => GameDataService.Instance.GetUiText("HorseSkills", "Reitkunst & Pferdewerte", "Horsemanship & Mount Stats");







        // Handwerk



        public string Loc_HerbsTitle => GameDataService.Instance.GetUiText("Herbs", "Kräuterbeutel & Alchemistische Zutaten", "Herbs Pouch & Alchemical Ingredients");



        public string Loc_HerbsBatch => GameDataService.Instance.GetUiText("HerbsBatch", "+50 auf alle Kräuter", "+50 to all Herbs");



        public string Loc_HerbsMaxAll => GameDataService.Instance.GetUiText("Loc_HerbsMaxAll", "Max (999)", "Max (999)");



        public string Loc_SmithingTitle => GameDataService.Instance.GetUiText("SmithingTitle", "Schmiedekunst & Schleifstein", "Blacksmithing & Grindstone");



        public string Loc_SmithingMax => GameDataService.Instance.GetUiText("SmithingMax", "Meister-Schärfe (100%)", "Master Sharpness (100%)");



        public string Loc_SmithingLevel => GameDataService.Instance.GetUiText("SmithingLevel", "Schmiedestufe", "Blacksmithing Level");



        public string Loc_SmithingLevelVal => GameDataService.Instance.GetUiText("SmithingMasterLevel", "30 / 30 (Meister)", "30 / 30 (Master)");



        public string Loc_SmithingHardness => GameDataService.Instance.GetUiText("SmithingHardness", "Klingenhärte & Rauch", "Blade Tempering & Finish");



        public string Loc_SmithingHardnessVal => GameDataService.Instance.GetUiText("SmithingHardnessVal", "Perfekt gehärtet", "Perfect Tempering");



        public string Loc_RecipesTitle => GameDataService.Instance.GetUiText("Potions", "Alchemie-Rezeptbuch (Erlernt)", "Alchemy Recipe Codex (Learned)");



        public string Loc_RecipesUnlockAll => GameDataService.Instance.GetUiText("UnlockAll", "Alle Rezepte Freischalten", "Unlock All Recipes");



        public string Loc_RecipeLearned => GameDataService.Instance.GetUiText("RecipeLearned", "Gelernt ✓", "Learned ✓");



        public string Loc_RecipeLock => GameDataService.Instance.GetUiText("RecipeLock", "Sperren", "Lock");







        public string Loc_CraftTitle => GameDataService.Instance.GetUiText("Loc_CraftTitle", "SCHMIEDE- & ALCHEMIE-KODEX", "CRAFTING & ALCHEMY CODEX");



        public string Loc_CraftSearchPlaceholder => GameDataService.Instance.GetUiText("Loc_CraftSearchPlaceholder", "🔍 Rezept, Zutat oder Talent suchen...", "🔍 Search recipe, ingredient or perk...");



        public string Loc_CraftFilterAll => GameDataService.Instance.GetUiText("Loc_CraftFilterAll", "Alle Rezepte", "All Recipes");



        public string Loc_CraftFilterAlchemy => GameDataService.Instance.GetUiText("Loc_CraftFilterAlchemy", "🧪 Alchemie", "🧪 Alchemy");



        public string Loc_CraftFilterBlacksmith => GameDataService.Instance.GetUiText("Loc_CraftFilterBlacksmith", "⚔️ Schmiedekunst", "⚔️ Blacksmithing");



        public string Loc_CraftFilterPerks => GameDataService.Instance.GetUiText("Loc_CraftFilterPerks", "⭐ Handwerkstalente", "⭐ Crafting Perks");



        public string Loc_CraftStatusAll => GameDataService.Instance.GetUiText("Loc_CraftStatusAll", "Alle Status", "All Status");



        public string Loc_CraftStatusLearned => GameDataService.Instance.GetUiText("Loc_CraftStatusLearned", "✔ Nur Gelernt", "✔ Learned Only");



        public string Loc_CraftStatusUnlearned => GameDataService.Instance.GetUiText("Loc_CraftStatusUnlearned", "❌ Nur Nicht gelernt", "❌ Unlearned Only");



        public string Loc_CraftLearnAll => GameDataService.Instance.GetUiText("Loc_CraftLearnAll", "✨ Alle lernen", "✨ Learn All");



        public string Loc_CraftUnlearnAll => GameDataService.Instance.GetUiText("Loc_CraftUnlearnAll", "🔒 Alle verlernen", "🔒 Unlearn All");







        private string _selectedCraftFilter = "All";



        private string _selectedCraftStatusFilter = "All";



        private string _craftSearchFilter = string.Empty;







        public string SelectedCraftFilter



        {



            get => _selectedCraftFilter;



            set



            {



                if (SetProperty(ref _selectedCraftFilter, value))



                {



                    FilteredCraftingRecipes?.Refresh();



                    OnPropertyChanged(nameof(IsCraftTabPerksSelected));



                }



            }



        }







        public bool IsCraftTabPerksSelected => SelectedCraftFilter == "Perks";







        public string SelectedCraftStatusFilter



        {



            get => _selectedCraftStatusFilter;



            set



            {



                if (SetProperty(ref _selectedCraftStatusFilter, value))



                {



                    FilteredCraftingRecipes?.Refresh();



                    FilteredCraftingPerks?.Refresh();



                }



            }



        }







        public string CraftSearchFilter



        {



            get => _craftSearchFilter;



            set



            {



                if (SetProperty(ref _craftSearchFilter, value))



                {



                    FilteredCraftingRecipes?.Refresh();



                    FilteredCraftingPerks?.Refresh();



                }



            }



        }







        public string DisplayHorseName



        {



            get



            {



                if (SelectedHorseOption != null) return SelectedHorseOption.DisplayName;



                if (string.IsNullOrEmpty(_horseName) || _horseName.Equals("Kein Pferd", StringComparison.OrdinalIgnoreCase))



                    return GameDataService.Instance.GetUiText("NoHorse", "Kein Pferd", "No Horse");



                return _horseName;



            }



        }







        public void NotifyAllLocalizationsChanged()



        {



            foreach (var tab in CategoryTabs) tab.NotifyLanguageChanged();



            foreach (var f in Factions) f.NotifyLanguageChanged();



            foreach (var p in Perks) p.NotifyLanguageChanged();



            foreach (var s in LeftPaperdollSlots) s.NotifyLanguageChanged();



            foreach (var s in RightPaperdollSlots) s.NotifyLanguageChanged();
            foreach (var s in HorseEquipmentSlots) s.NotifyLanguageChanged();



            foreach (var q in Quests) q.NotifyLanguageChanged();



            foreach (var h in Herbs) h.NotifyLanguageChanged();



            foreach (var r in AlchemyRecipes) r.NotifyLanguageChanged();



            foreach (var dp in DogPerks) dp.NotifyLanguageChanged();
            foreach (var f in CharacterFates) f.NotifyLanguageChanged();
            foreach (var d in AvailableDlcs) d.NotifyLanguageChanged();
            foreach (var c in CraftingRecipes) c.NotifyLanguageChanged();
            foreach (var c in CraftingPerks) c.NotifyLanguageChanged();
            OnPropertyChanged(nameof(Loc_MissionsTab));
            OnPropertyChanged(nameof(Loc_CharactersAndFatesTab));
            OnPropertyChanged(nameof(Loc_QuestTabAllRegions));
            OnPropertyChanged(nameof(Loc_QuestTabTrosecko));
            OnPropertyChanged(nameof(Loc_QuestTabKutnohorsko));
            OnPropertyChanged(nameof(Loc_FateFilterAll));
            OnPropertyChanged(nameof(Loc_FateFilterVip));
            OnPropertyChanged(nameof(Loc_FateFilterDecision));
            OnPropertyChanged(nameof(Loc_FateFilterMerchant));
            OnPropertyChanged(nameof(Loc_WorldStateHeader));
            OnPropertyChanged(nameof(Loc_CurrentStoryRegion));
            OnPropertyChanged(nameof(Loc_RaczekSwordReforgedLabel));
            OnPropertyChanged(nameof(Loc_ChoiceSpared));
            OnPropertyChanged(nameof(Loc_ChoiceExecuted));
            OnPropertyChanged(nameof(Loc_ReviveButton));
            OnPropertyChanged(nameof(Loc_PointOfNoReturnBadge));
            OnPropertyChanged(nameof(CurrentStoryRegionDisplay));
            OnPropertyChanged(nameof(Loc_FateSearchPlaceholder));
            OnPropertyChanged(nameof(Loc_NoCharacterSelected));
            OnPropertyChanged(nameof(Loc_FateLoreHeader));
            OnPropertyChanged(nameof(Loc_FateActionsHeader));
            OnPropertyChanged(nameof(Loc_FateVipProtectionTitle));
            OnPropertyChanged(nameof(Loc_FateVipProtectionDesc));
            OnPropertyChanged(nameof(Loc_FateDecisionToggleHeader));
            OnPropertyChanged(nameof(Loc_FateMortalCraftsmanTitle));
            OnPropertyChanged(nameof(Loc_FateMortalCraftsmanDesc));
            OnPropertyChanged(nameof(Loc_RaczekSwordReforgedDesc));
            OnPropertyChanged(nameof(Loc_SmithingLevelVal));







            OnPropertyChanged(nameof(IsGerman));



            OnPropertyChanged(nameof(HorseName));



            OnPropertyChanged(nameof(DisplayHorseName));



            OnPropertyChanged(nameof(StatusSyncText));







            OnPropertyChanged(nameof(Loc_NavQuestlog));



            OnPropertyChanged(nameof(Loc_NavInventar));



            OnPropertyChanged(nameof(Loc_NavSpieler));



            OnPropertyChanged(nameof(Loc_NavHandwerk));

            OnPropertyChanged(nameof(Loc_NavDlcs));
            OnPropertyChanged(nameof(Loc_DlcSubtitle));
            OnPropertyChanged(nameof(Loc_DlcHelpTitle));
            OnPropertyChanged(nameof(Loc_DlcHelpDesc));
            OnPropertyChanged(nameof(Loc_DlcHowToTitle));
            OnPropertyChanged(nameof(Loc_DlcStep1));
            OnPropertyChanged(nameof(Loc_DlcStep2));
            OnPropertyChanged(nameof(Loc_DlcStep3));
            OnPropertyChanged(nameof(Loc_DlcCustomAddTitle));
            OnPropertyChanged(nameof(Loc_DlcCustomAddBtn));
            OnPropertyChanged(nameof(Loc_DlcCustomPlaceholder));
            OnPropertyChanged(nameof(Loc_DlcActiveCountText));







            OnPropertyChanged(nameof(Loc_TargetHenry));



            OnPropertyChanged(nameof(Loc_TargetHorse));



            OnPropertyChanged(nameof(Loc_RightHorseTitle));







            OnPropertyChanged(nameof(Loc_QuestlogTitle));



            OnPropertyChanged(nameof(Loc_QuestUnlockAll));



            OnPropertyChanged(nameof(Loc_QuestTabAll));



            OnPropertyChanged(nameof(Loc_QuestTabMain));



            OnPropertyChanged(nameof(Loc_QuestTabSide));



            OnPropertyChanged(nameof(Loc_QuestTabActivity));



            OnPropertyChanged(nameof(Loc_QuestObjectivesHeader));



            OnPropertyChanged(nameof(Loc_QuestCompleteAll));



            OnPropertyChanged(nameof(Loc_QuestStatusActive));



            OnPropertyChanged(nameof(Loc_QuestStatusCompleted));



            OnPropertyChanged(nameof(Loc_QuestStatusFailed));



            OnPropertyChanged(nameof(Loc_SelectHorseLabel));



            OnPropertyChanged(nameof(Loc_QuestSearchPlaceholder));







            foreach (var h in AvailableHorses) h.NotifyLanguageChanged();



            foreach (var s in AvailableSaves) s.NotifyLanguageChanged();



            foreach (var q in Quests) q.NotifyLanguageChanged();







            OnPropertyChanged(nameof(Loc_PlayerSubHenry));



            OnPropertyChanged(nameof(Loc_PlayerSubDog));



            OnPropertyChanged(nameof(Loc_PlayerSubHorse));



            OnPropertyChanged(nameof(Loc_CompanionAiStatus));



            OnPropertyChanged(nameof(Loc_DogTitle));



            OnPropertyChanged(nameof(Loc_DogSubtitle));



            OnPropertyChanged(nameof(Loc_DogCondition));



            OnPropertyChanged(nameof(Loc_DogConditionVal));



            OnPropertyChanged(nameof(Loc_DogRank));



            OnPropertyChanged(nameof(Loc_DogRankVal));



            OnPropertyChanged(nameof(Loc_DogCommand));



            OnPropertyChanged(nameof(Loc_DogCommandVal));



            OnPropertyChanged(nameof(Loc_DogMax));



            OnPropertyChanged(nameof(Loc_DogAttributesHeader));



            OnPropertyChanged(nameof(Loc_DogPerksHeader));



            OnPropertyChanged(nameof(Loc_DogLoyalty));



            OnPropertyChanged(nameof(Loc_DogObedience));



            OnPropertyChanged(nameof(Loc_DogNourishment));



            OnPropertyChanged(nameof(Loc_DogAggression));







            OnPropertyChanged(nameof(Loc_FactionsTitle));



            OnPropertyChanged(nameof(Loc_PerksTitle));



            OnPropertyChanged(nameof(Loc_PerksUnlockAll));







            OnPropertyChanged(nameof(Loc_HorseFullMountTitle));



            OnPropertyChanged(nameof(Loc_HorseFullMountDesc));



            OnPropertyChanged(nameof(Loc_HorseCaparison));



            OnPropertyChanged(nameof(Loc_HorseSaddle));



            OnPropertyChanged(nameof(Loc_HorseBridle));



            OnPropertyChanged(nameof(Loc_HorseShoes));



            OnPropertyChanged(nameof(Loc_HorseRidingSkills));







            OnPropertyChanged(nameof(Loc_HerbsTitle));



            OnPropertyChanged(nameof(Loc_HerbsBatch));



            OnPropertyChanged(nameof(Loc_SmithingTitle));



            OnPropertyChanged(nameof(Loc_SmithingMax));



            OnPropertyChanged(nameof(Loc_SmithingLevel));



            OnPropertyChanged(nameof(Loc_SmithingLevelVal));



            OnPropertyChanged(nameof(Loc_SmithingHardness));



            OnPropertyChanged(nameof(Loc_SmithingHardnessVal));



            OnPropertyChanged(nameof(Loc_RecipesTitle));



            OnPropertyChanged(nameof(Loc_RecipesUnlockAll));



            OnPropertyChanged(nameof(Loc_RecipeLearned));



            OnPropertyChanged(nameof(Loc_RecipeLock));







            foreach (var r in CraftingRecipes) r.NotifyLanguageChanged();



            foreach (var p in CraftingPerks) p.NotifyLanguageChanged();



            FilteredCraftingRecipes?.Refresh();



            FilteredCraftingPerks?.Refresh();







            OnPropertyChanged(nameof(Loc_CraftTitle));



            OnPropertyChanged(nameof(Loc_CraftSearchPlaceholder));



            OnPropertyChanged(nameof(Loc_CraftFilterAll));



            OnPropertyChanged(nameof(Loc_CraftFilterAlchemy));



            OnPropertyChanged(nameof(Loc_CraftFilterBlacksmith));



            OnPropertyChanged(nameof(Loc_CraftFilterPerks));



            OnPropertyChanged(nameof(Loc_CraftStatusAll));



            OnPropertyChanged(nameof(Loc_CraftStatusLearned));



            OnPropertyChanged(nameof(Loc_CraftStatusUnlearned));



            OnPropertyChanged(nameof(Loc_CraftLearnAll));



            OnPropertyChanged(nameof(Loc_CraftUnlearnAll));



            OnPropertyChanged(nameof(Loc_HerbsMaxAll));







            OnPropertyChanged(nameof(IsGerman));



            OnPropertyChanged(nameof(HorseName));



            OnPropertyChanged(nameof(DisplayHorseName));







            OnPropertyChanged(nameof(Loc_ActiveSave));



            OnPropertyChanged(nameof(Loc_OpenSave));



            OnPropertyChanged(nameof(Loc_OpenSaveTip));



            OnPropertyChanged(nameof(Loc_AutoDetect));



            OnPropertyChanged(nameof(Loc_AutoDetectTip));



            OnPropertyChanged(nameof(Loc_LanguageTip));



            OnPropertyChanged(nameof(Loc_RepairAll));



            OnPropertyChanged(nameof(Loc_RepairAllTip));



            OnPropertyChanged(nameof(Loc_Legalize));



            OnPropertyChanged(nameof(Loc_LegalizeTip));



            OnPropertyChanged(nameof(Loc_MaxStats));



            OnPropertyChanged(nameof(Loc_MaxStatsTip));



            OnPropertyChanged(nameof(Loc_ExportSave));



            OnPropertyChanged(nameof(Loc_ExportSaveTip));







            OnPropertyChanged(nameof(Loc_StorageHeader));



            OnPropertyChanged(nameof(Loc_StolenTooltip));



            OnPropertyChanged(nameof(Loc_EquippedTooltip));



            OnPropertyChanged(nameof(Loc_BloodyTooltip));



            OnPropertyChanged(nameof(Loc_MoveToStash));



            OnPropertyChanged(nameof(Loc_TakeToInventory));



            OnPropertyChanged(nameof(Loc_MaxHorseStatsTip));



            OnPropertyChanged(nameof(Loc_MaxHorseStatsBtn));



            OnPropertyChanged(nameof(Loc_CraftSummary));



            OnPropertyChanged(nameof(Loc_IngredientsHeader));







            OnPropertyChanged(nameof(Loc_SpawnerCatAll));



            OnPropertyChanged(nameof(Loc_SpawnerCatWeapons));



            OnPropertyChanged(nameof(Loc_SpawnerCatArmour));



            OnPropertyChanged(nameof(Loc_SpawnerCatFood));



            OnPropertyChanged(nameof(Loc_SpawnerCatBooks));



            OnPropertyChanged(nameof(Loc_SpawnerCatHerbs));



            OnPropertyChanged(nameof(Loc_SpawnerCatHorse));



            OnPropertyChanged(nameof(Loc_SpawnerCatOther));







            OnPropertyChanged(nameof(Loc_PriceAll));



            OnPropertyChanged(nameof(Loc_PriceCheap));



            OnPropertyChanged(nameof(Loc_PriceMedium));



            OnPropertyChanged(nameof(Loc_PriceValuable));



            OnPropertyChanged(nameof(Loc_PriceLegendary));







            OnPropertyChanged(nameof(Loc_SortNameAsc));



            OnPropertyChanged(nameof(Loc_SortNameDesc));



            OnPropertyChanged(nameof(Loc_SortPriceDesc));



            OnPropertyChanged(nameof(Loc_SortPriceAsc));



            OnPropertyChanged(nameof(Loc_SortWeight));







            OnPropertyChanged(nameof(Loc_BasePrice));



            OnPropertyChanged(nameof(Loc_Weight));







            OnPropertyChanged(nameof(Loc_TargetStorageInventory));



            OnPropertyChanged(nameof(Loc_TargetStorageStash));



            OnPropertyChanged(nameof(Loc_TargetStorageHorse));







            OnPropertyChanged(nameof(Loc_WindowMinTip));



            OnPropertyChanged(nameof(Loc_WindowMaxTip));



            OnPropertyChanged(nameof(Loc_WindowCloseTip));



            OnPropertyChanged(nameof(Loc_WindowRestoreTip));



            OnPropertyChanged(nameof(Loc_InspectorOutfit1));



            OnPropertyChanged(nameof(Loc_InspectorOutfit2));



            OnPropertyChanged(nameof(Loc_InspectorOutfit3));



            OnPropertyChanged(nameof(Loc_OutfitLabel));
            OnPropertyChanged(nameof(CurrentOutfitTitle));
            OnPropertyChanged(nameof(Loc_SlotPickerTitle));
            OnPropertyChanged(nameof(Loc_SlotPickerSubtitle));
            OnPropertyChanged(nameof(Loc_SlotPickerCurrent));
            OnPropertyChanged(nameof(Loc_SlotPickerUnequip));
            OnPropertyChanged(nameof(Loc_SlotPickerEquip));
            OnPropertyChanged(nameof(Loc_SlotPickerCancel));
            OnPropertyChanged(nameof(Loc_SlotPickerSourceInv));
            OnPropertyChanged(nameof(Loc_SlotPickerSourceAll));
            OnPropertyChanged(nameof(Loc_SlotPickerSearchPlaceholder));
            OnPropertyChanged(nameof(Loc_SlotPickerPrice));
            OnPropertyChanged(nameof(Loc_SlotPickerWeight));
            OnPropertyChanged(nameof(Loc_SlotPickerTier));
            OnPropertyChanged(nameof(Loc_SlotPickerCategory));
            OnPropertyChanged(nameof(Loc_SlotPickerType));
            OnPropertyChanged(nameof(Loc_SlotPickerCondition));
            OnPropertyChanged(nameof(Loc_SlotPickerOutfitNotice));
            OnPropertyChanged(nameof(Loc_HorseTackLabel));
            OnPropertyChanged(nameof(Loc_HorseTackWarhorseTip));
            OnPropertyChanged(nameof(Loc_HorseTackTravelTip));
            OnPropertyChanged(nameof(Loc_HorseTackNobleTip));
            OnPropertyChanged(nameof(CurrentHorseOutfitTitle));
            OnPropertyChanged(nameof(CurrentHorseOutfitBadge));



            OnPropertyChanged(nameof(CurrentPlatformTooltip));



            OnPropertyChanged(nameof(Loc_SwitchToXboxTip));



            OnPropertyChanged(nameof(Loc_SwitchToSteamTip));



            foreach (var dlc in AvailableDlcs) dlc.NotifyLanguageChanged();



            if (AllCodexItems.Count > 0)



            {



                AllCodexItems.Clear();



                foreach (var it in GameDataService.Instance.GetAllCodexItems())



                {



                    AllCodexItems.Add(it);



                }



                FilteredCodexItems?.Refresh();



            }







            OnPropertyChanged(nameof(Loc_CaparisonDefault));



            OnPropertyChanged(nameof(Loc_SaddleDefault));



            OnPropertyChanged(nameof(Loc_BridleDefault));



            OnPropertyChanged(nameof(Loc_HorseshoesDefault));







            OnPropertyChanged(nameof(Loc_SearchPlaceholder));



            OnPropertyChanged(nameof(Loc_FilterEquipped));



            OnPropertyChanged(nameof(Loc_AddItem));







            OnPropertyChanged(nameof(Loc_ColItemCategory));



            OnPropertyChanged(nameof(Loc_ColEquippedTip));



            OnPropertyChanged(nameof(Loc_ColQuantityTip));



            OnPropertyChanged(nameof(Loc_ColPowerTip));



            OnPropertyChanged(nameof(Loc_ColConditionTip));



            OnPropertyChanged(nameof(Loc_ColWeightTip));



            OnPropertyChanged(nameof(Loc_ColPrice));







            OnPropertyChanged(nameof(Loc_InspectorCondition));



            OnPropertyChanged(nameof(Loc_InspectorBlood));



            OnPropertyChanged(nameof(Loc_InspectorAmount));



            OnPropertyChanged(nameof(Loc_InspectorPower));



            OnPropertyChanged(nameof(Loc_InspectorWeight));



            OnPropertyChanged(nameof(Loc_InspectorPrice));



            OnPropertyChanged(nameof(Loc_InspectorEquipment));



            OnPropertyChanged(nameof(Loc_InspectorStolen));



            OnPropertyChanged(nameof(Loc_InspectorDuplicate));



            OnPropertyChanged(nameof(Loc_InspectorDelete));







            OnPropertyChanged(nameof(Loc_FooterGroschen));



            OnPropertyChanged(nameof(Loc_FooterCarryWeight));



            OnPropertyChanged(nameof(Loc_FooterLevel));







            OnPropertyChanged(nameof(Loc_RightHenryTitle));



            OnPropertyChanged(nameof(Loc_RightTabSplit));



            OnPropertyChanged(nameof(Loc_RightTabPerks));



            OnPropertyChanged(nameof(Loc_RightTabHorse));



            OnPropertyChanged(nameof(Loc_RightTabDlcs));







            OnPropertyChanged(nameof(Loc_ArmorDefense));



            OnPropertyChanged(nameof(Loc_Stab));



            OnPropertyChanged(nameof(Loc_Slash));



            OnPropertyChanged(nameof(Loc_Smash));



            OnPropertyChanged(nameof(Loc_Head));



            OnPropertyChanged(nameof(Loc_Body));



            OnPropertyChanged(nameof(Loc_Arms));



            OnPropertyChanged(nameof(Loc_Legs));







            OnPropertyChanged(nameof(Loc_WeaponAttack));



            OnPropertyChanged(nameof(Loc_Melee));



            OnPropertyChanged(nameof(Loc_Ranged));



            OnPropertyChanged(nameof(Loc_Unarmed));







            OnPropertyChanged(nameof(Loc_Strength));



            OnPropertyChanged(nameof(Loc_Agility));



            OnPropertyChanged(nameof(Loc_Vitality));



            OnPropertyChanged(nameof(Loc_Speech));



            OnPropertyChanged(nameof(Loc_Charisma));



            OnPropertyChanged(nameof(Loc_Speed));







            OnPropertyChanged(nameof(Loc_Conspicuousness));



            OnPropertyChanged(nameof(Loc_Noise));



            OnPropertyChanged(nameof(Loc_Visibility));



            OnPropertyChanged(nameof(Loc_Health));



            OnPropertyChanged(nameof(Loc_Stamina));



            OnPropertyChanged(nameof(Loc_Energy));







            OnPropertyChanged(nameof(Loc_FactionRepTitle));



            OnPropertyChanged(nameof(Loc_SpecialPerksTitle));







            OnPropertyChanged(nameof(Loc_MaxHorseStats));



            OnPropertyChanged(nameof(Loc_HorseHealth));



            OnPropertyChanged(nameof(Loc_HorseStamina));



            OnPropertyChanged(nameof(Loc_HorseSpeed));



            OnPropertyChanged(nameof(Loc_HorseCapacity));



            OnPropertyChanged(nameof(Loc_HorseCourage));



            OnPropertyChanged(nameof(Loc_HorseInventory));







            OnPropertyChanged(nameof(Loc_DlcTitle));



            OnPropertyChanged(nameof(Loc_EnableAllDlcs));



            OnPropertyChanged(nameof(Loc_DisableAllDlcs));







            OnPropertyChanged(nameof(Loc_BackupCheck));



            OnPropertyChanged(nameof(Loc_NoSaveLoaded));







            OnPropertyChanged(nameof(Loc_OutfitCombatTip));



            OnPropertyChanged(nameof(Loc_OutfitStealthTip));



            OnPropertyChanged(nameof(Loc_OutfitNobleTip));



            OnPropertyChanged(nameof(Loc_CleanDirtTip));



            OnPropertyChanged(nameof(Loc_ArchPedestal));







            OnPropertyChanged(nameof(Loc_SpawnerTitle));



            OnPropertyChanged(nameof(Loc_SpawnerPresetLabel));



            OnPropertyChanged(nameof(Loc_SpawnerPresetCustom));



            OnPropertyChanged(nameof(Loc_SpawnerName));



            OnPropertyChanged(nameof(Loc_SpawnerCategory));



            OnPropertyChanged(nameof(Loc_SpawnerAmount));



            OnPropertyChanged(nameof(Loc_SpawnerPower));



            OnPropertyChanged(nameof(Loc_SpawnerWeight));



            OnPropertyChanged(nameof(Loc_SpawnerPrice));



            OnPropertyChanged(nameof(Loc_SpawnerCancel));



            OnPropertyChanged(nameof(Loc_SpawnerAdd));



        }







        public long Groschen



        {



            get => _groschen;



            set



            {



                long clamped = Math.Max(0, value);



                if (SetProperty(ref _groschen, clamped))



                {



                    var groschenItem = InventoryItems.FirstOrDefault(x => x.ItemId.Equals("5ef63059-322e-4e1b-abe8-926e100c770e", StringComparison.OrdinalIgnoreCase));



                    if (groschenItem != null && groschenItem.Amount != (int)Math.Clamp(clamped, 0, int.MaxValue))



                    {



                        groschenItem.Amount = (int)Math.Clamp(clamped, 0, int.MaxValue);



                    }



                }



            }



        }







        public float Health



        {



            get => _health;



            set => SetProperty(ref _health, value);



        }







        public float MaxHealth



        {



            get => _maxHealth;



            set => SetProperty(ref _maxHealth, value);



        }







        public float Stamina



        {



            get => _stamina;



            set => SetProperty(ref _stamina, value);



        }







        public float MaxStamina



        {



            get => _maxStamina;



            set => SetProperty(ref _maxStamina, value);



        }







        public int LocksPicked



        {



            get => _locksPicked;



            set => SetProperty(ref _locksPicked, value);



        }







        public int PotionsConsumed



        {



            get => _potionsConsumed;



            set => SetProperty(ref _potionsConsumed, value);



        }







        public string StatusMessage



        {



            get => _statusMessage;



            set => SetProperty(ref _statusMessage, value);



        }







        public bool IsSaveLoaded



        {



            get => _isSaveLoaded;



            set => SetProperty(ref _isSaveLoaded, value);



        }







        public bool CreateBackup



        {



            get => _createBackup;



            set => SetProperty(ref _createBackup, value);



        }







        public float Energy



        {



            get => _energy;



            set => SetProperty(ref _energy, value);



        }







        public float Nourishment



        {



            get => _nourishment;



            set => SetProperty(ref _nourishment, value);



        }







        public int Strength



        {



            get => _strength;



            set => SetProperty(ref _strength, value);



        }







        public int Agility



        {



            get => _agility;



            set => SetProperty(ref _agility, value);



        }







        public int Vitality



        {



            get => _vitality;



            set => SetProperty(ref _vitality, value);



        }







        public int Speech



        {



            get => _speech;



            set => SetProperty(ref _speech, value);



        }







        public int Charisma



        {



            get => _charisma;



            set => SetProperty(ref _charisma, value);



        }







        public int Speed



        {



            get => _speed;



            set => SetProperty(ref _speed, value);



        }







        public int CharacterLevel



        {



            get => _characterLevel;



            set => SetProperty(ref _characterLevel, value);



        }







        public int Conspicuousness



        {



            get => _conspicuousness;



            set => SetProperty(ref _conspicuousness, value);



        }







        public int Noise



        {



            get => _noise;



            set => SetProperty(ref _noise, value);



        }







        public int Visibility



        {



            get => _visibility;



            set => SetProperty(ref _visibility, value);



        }







        public float CarryWeight



        {



            get => _carryWeight;



            set => SetProperty(ref _carryWeight, value);



        }







        public float MaxCarryWeight



        {



            get => _maxCarryWeight;



            set => SetProperty(ref _maxCarryWeight, value);



        }







        public int Warfare



        {



            get => _warfare;



            set => SetProperty(ref _warfare, value);



        }







        public int Sword



        {



            get => _sword;



            set => SetProperty(ref _sword, value);



        }







        public int Defense



        {



            get => _defense;



            set => SetProperty(ref _defense, value);



        }







        public int Stealth



        {



            get => _stealth;



            set => SetProperty(ref _stealth, value);



        }







        public int Pickpocketing



        {



            get => _pickpocketing;



            set => SetProperty(ref _pickpocketing, value);



        }







        public int Marksmanship



        {



            get => _marksmanship;



            set => SetProperty(ref _marksmanship, value);



        }







        public int Horsemanship



        {



            get => _horsemanship;



            set => SetProperty(ref _horsemanship, value);



        }







        public int Houndmaster



        {



            get => _houndmaster;



            set => SetProperty(ref _houndmaster, value);



        }







        public int Scholarship



        {



            get => _scholarship;



            set => SetProperty(ref _scholarship, value);



        }







        public int Blacksmithing



        {



            get => _blacksmithing;



            set => SetProperty(ref _blacksmithing, value);



        }







        public string InventorySearchFilter



        {



            get => _inventorySearchFilter;



            set



            {



                if (SetProperty(ref _inventorySearchFilter, value))



                {



                    FilteredInventory.Refresh();



                }



            }



        }







        public string SelectedInventoryCategory



        {



            get => _selectedInventoryCategory;



            set



            {



                if (SetProperty(ref _selectedInventoryCategory, value))



                {



                    FilteredInventory.Refresh();



                }



            }



        }







        public InventoryItemViewModel? SelectedInventoryItem



        {



            get => _selectedInventoryItem;



            set



            {



                if (SetProperty(ref _selectedInventoryItem, value))



                {



                    UpdatePaperdollSelection(value);



                }



            }



        }







        public InventoryItemViewModel? SelectedHorseItem



        {



            get => _selectedHorseItem;



            set => SetProperty(ref _selectedHorseItem, value);



        }







        public bool FilterEquippedOnly



        {



            get => _filterEquippedOnly;



            set



            {



                if (SetProperty(ref _filterEquippedOnly, value))



                {



                    FilteredInventory.Refresh();



                }



            }



        }







        public int ActiveOutfit
        {
            get => _activeOutfit;
            set
            {
                if (SetProperty(ref _activeOutfit, value))
                {
                    OnPropertyChanged(nameof(IsOutfit1Active));
                    OnPropertyChanged(nameof(IsOutfit2Active));
                    OnPropertyChanged(nameof(IsOutfit3Active));
                    OnPropertyChanged(nameof(CurrentOutfitTitle));
                    OnPropertyChanged(nameof(CurrentOutfitPedestal));
                }
            }
        }

        public bool IsOutfit1Active => ActiveOutfit == 1;
        public bool IsOutfit2Active => ActiveOutfit == 2;
        public bool IsOutfit3Active => ActiveOutfit == 3;

        public string CurrentOutfitTitle => ActiveOutfit switch
        {
            1 => IsGerman ? "OUTFIT I • KAMPF" : "OUTFIT I • COMBAT",
            2 => IsGerman ? "OUTFIT II • HEIMLICHKEIT" : "OUTFIT II • STEALTH",
            3 => IsGerman ? "OUTFIT III • ADELIG" : "OUTFIT III • NOBLE",
            _ => $"OUTFIT {ActiveOutfit}"
        };

        public string CurrentOutfitPedestal => ActiveOutfit switch
        {
            1 => "ARMOUR: FULL PLATE • PWR 620",
            2 => "ARMOUR: STEALTH LEATHER • PWR 340",
            3 => "ARMOUR: NOBLE ATTIRE • PWR 410",
            _ => $"ARMOUR: PWR {ActiveOutfit * 200}"
        };

        private int _activeHorseOutfit = 1;
        public int ActiveHorseOutfit
        {
            get => _activeHorseOutfit;
            set
            {
                if (SetProperty(ref _activeHorseOutfit, value))
                {
                    OnPropertyChanged(nameof(IsHorseOutfit1Active));
                    OnPropertyChanged(nameof(IsHorseOutfit2Active));
                    OnPropertyChanged(nameof(IsHorseOutfit3Active));
                    OnPropertyChanged(nameof(CurrentHorseOutfitTitle));
                    OnPropertyChanged(nameof(CurrentHorseOutfitBadge));
                }
            }
        }

        public bool IsHorseOutfit1Active => ActiveHorseOutfit == 1;
        public bool IsHorseOutfit2Active => ActiveHorseOutfit == 2;
        public bool IsHorseOutfit3Active => ActiveHorseOutfit == 3;

        public string CurrentHorseOutfitTitle => ActiveHorseOutfit switch
        {
            1 => IsGerman ? "SATTELZEUG I • STREITROSS" : "TACK I • WARHORSE",
            2 => IsGerman ? "SATTELZEUG II • REISE & TEMPO" : "TACK II • TRAVEL & SPEED",
            3 => IsGerman ? "SATTELZEUG III • PRUNK & ADEL" : "TACK III • NOBLE COURSER",
            _ => $"TACK {ActiveHorseOutfit}"
        };

        public string CurrentHorseOutfitBadge => ActiveHorseOutfit switch
        {
            1 => IsGerman ? "PANZERUNG: HOCH • KAMPF" : "ARMOUR: HIGH • COMBAT",
            2 => IsGerman ? "LEICHT • MAX. TEMPO" : "LIGHT • MAX SPEED",
            3 => IsGerman ? "TRACHT • MAX. TRAGKRAFT" : "DRESS • MAX CAPACITY",
            _ => ""
        };

        public int CharacterDirtPercent
        {
            get => _characterDirtPercent;
            set
            {
                if (SetProperty(ref _characterDirtPercent, Math.Clamp(value, 0, 100)))
                {
                    OnPropertyChanged(nameof(CharacterDirtDisplay));
                    OnPropertyChanged(nameof(CharacterDirtForeground));
                }
            }
        }

        public string CharacterDirtDisplay => _characterDirtPercent > 0 ? $"🩸 {_characterDirtPercent}%" : "✨ 0%";
        public string CharacterDirtForeground => _characterDirtPercent > 0 ? "#FB7185" : "#4ADE80";

        // Defense & Attack Matrix Properties
        public int ArmorHeadStab { get => _armorHeadStab; set => SetProperty(ref _armorHeadStab, value); }
        public int ArmorHeadSlash { get => _armorHeadSlash; set => SetProperty(ref _armorHeadSlash, value); }
        public int ArmorHeadBlunt { get => _armorHeadBlunt; set => SetProperty(ref _armorHeadBlunt, value); }

        public int ArmorBodyStab { get => _armorBodyStab; set => SetProperty(ref _armorBodyStab, value); }
        public int ArmorBodySlash { get => _armorBodySlash; set => SetProperty(ref _armorBodySlash, value); }
        public int ArmorBodyBlunt { get => _armorBodyBlunt; set => SetProperty(ref _armorBodyBlunt, value); }

        public int ArmorArmsStab { get => _armorArmsStab; set => SetProperty(ref _armorArmsStab, value); }
        public int ArmorArmsSlash { get => _armorArmsSlash; set => SetProperty(ref _armorArmsSlash, value); }
        public int ArmorArmsBlunt { get => _armorArmsBlunt; set => SetProperty(ref _armorArmsBlunt, value); }

        public int ArmorLegsStab { get => _armorLegsStab; set => SetProperty(ref _armorLegsStab, value); }
        public int ArmorLegsSlash { get => _armorLegsSlash; set => SetProperty(ref _armorLegsSlash, value); }
        public int ArmorLegsBlunt { get => _armorLegsBlunt; set => SetProperty(ref _armorLegsBlunt, value); }

        public int WeaponMeleeStab { get => _weaponMeleeStab; set => SetProperty(ref _weaponMeleeStab, value); }
        public int WeaponMeleeSlash { get => _weaponMeleeSlash; set => SetProperty(ref _weaponMeleeSlash, value); }
        public int WeaponMeleeBlunt { get => _weaponMeleeBlunt; set => SetProperty(ref _weaponMeleeBlunt, value); }

        public int WeaponRangedAttack { get => _weaponRangedAttack; set => SetProperty(ref _weaponRangedAttack, value); }
        public int WeaponUnarmedAttack { get => _weaponUnarmedAttack; set => SetProperty(ref _weaponUnarmedAttack, value); }

        // Slot Picker Modal Properties
        public bool IsSlotPickerModalOpen
        {
            get => _isSlotPickerModalOpen;
            set
            {
                if (SetProperty(ref _isSlotPickerModalOpen, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public PaperdollSlotViewModel? SelectedPaperdollSlot
        {
            get => _selectedPaperdollSlot;
            set
            {
                if (SetProperty(ref _selectedPaperdollSlot, value))
                {
                    OnPropertyChanged(nameof(SlotPickerTitle));
                    OnPropertyChanged(nameof(SlotPickerSubtitle));
                    OnPropertyChanged(nameof(SlotPickerCurrentItemName));
                }
            }
        }

        public string SlotPickerTitle
        {
            get
            {
                if (SelectedPaperdollSlot == null) return Loc_SlotPickerTitle;
                string slotName = SelectedPaperdollSlot.DisplayLabel;
                bool isHorse = SelectedPaperdollSlot.Id.StartsWith("horse_");
                string outfit = isHorse ? $"TACK {ActiveHorseOutfit}" : $"OUTFIT {ActiveOutfit}";
                return IsGerman
                    ? $"{slotName.ToUpperInvariant()} AUSWÄHLEN ({outfit})"
                    : $"SELECT {slotName.ToUpperInvariant()} ({outfit})";
            }
        }

        public string SlotPickerSubtitle => IsGerman
            ? "Wähle einen Gegenstand aus dem Inventar oder dem Spielkatalog für diesen Ausrüstungsslot."
            : "Choose an item from inventory or game catalog to equip into this slot.";

        public string SlotPickerCurrentItemName => SelectedPaperdollSlot?.DisplayLabel ?? "— Leer / Empty —";

        public string SlotPickerSearchText
        {
            get => _slotPickerSearchText;
            set
            {
                if (SetProperty(ref _slotPickerSearchText, value))
                {
                    PopulateSlotPickerItems();
                }
            }
        }

        public string SlotPickerSelectedSource
        {
            get => _slotPickerSelectedSource;
            set
            {
                if (SetProperty(ref _slotPickerSelectedSource, value))
                {
                    OnPropertyChanged(nameof(IsSlotPickerSourceAll));
                    OnPropertyChanged(nameof(IsSlotPickerSourceInventory));
                    PopulateSlotPickerItems();
                }
            }
        }

        public bool IsSlotPickerSourceAll
        {
            get => SlotPickerSelectedSource.Equals("All", StringComparison.OrdinalIgnoreCase);
            set { if (value) SlotPickerSelectedSource = "All"; }
        }

        public bool IsSlotPickerSourceInventory
        {
            get => SlotPickerSelectedSource.Equals("Inventory", StringComparison.OrdinalIgnoreCase);
            set { if (value) SlotPickerSelectedSource = "Inventory"; }
        }

        public string SlotPickerItemCountText
        {
            get => _slotPickerItemCountText;
            set => SetProperty(ref _slotPickerItemCountText, value);
        }

        public SlotPickerItemViewModel? SelectedSlotPickerItem
        {
            get => _selectedSlotPickerItem;
            set
            {
                if (SetProperty(ref _selectedSlotPickerItem, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }







        public string SelectedRightView



        {



            get => _selectedRightView;



            set => SetProperty(ref _selectedRightView, value);



        }







        #region Horse Properties







        public HorseOptionViewModel? SelectedHorseOption



        {



            get => _selectedHorseOption;



            set



            {



                if (SetProperty(ref _selectedHorseOption, value) && value != null)



                {



                    ApplySelectedHorse(value);



                }



            }



        }







        public string HorseName



        {



            get => _horseName;



            set => SetProperty(ref _horseName, value);



        }







        public bool HasHorse



        {



            get => _hasHorse;



            set => SetProperty(ref _hasHorse, value);



        }







        public float HorseHealth



        {



            get => _horseHealth;



            set => SetProperty(ref _horseHealth, value);



        }







        public float HorseMaxHealth



        {



            get => _horseMaxHealth;



            set => SetProperty(ref _horseMaxHealth, value);



        }







        public float HorseStamina



        {



            get => _horseStamina;



            set => SetProperty(ref _horseStamina, value);



        }







        public float HorseMaxStamina



        {



            get => _horseMaxStamina;



            set => SetProperty(ref _horseMaxStamina, value);



        }







        public float HorseSpeed



        {



            get => _horseSpeed;



            set => SetProperty(ref _horseSpeed, value);



        }







        public float HorseCapacity



        {



            get => _horseCapacity;



            set => SetProperty(ref _horseCapacity, value);



        }







        public float HorseCourage



        {



            get => _horseCourage;



            set => SetProperty(ref _horseCourage, value);



        }







        private void ApplySelectedHorse(HorseOptionViewModel horse)



        {



            _horseName = horse.DisplayName;



            OnPropertyChanged(nameof(HorseName));



            OnPropertyChanged(nameof(DisplayHorseName));



            HasHorse = true;







            HorseSpeed = horse.DefaultSpeed;



            HorseCapacity = horse.DefaultCapacity;



            HorseCourage = horse.DefaultCourage;



            HorseMaxStamina = horse.DefaultStamina;



            HorseStamina = horse.DefaultStamina;



            HorseMaxHealth = horse.DefaultMaxHealth;



            HorseHealth = horse.DefaultMaxHealth;







            if (_handle != IntPtr.Zero)



            {



                NativeMethods.kcd2_save_set_horse(_handle, horse.Id);



                var hStats = new Kcd2HorseStats



                {



                    HorseName = horse.DisplayName,



                    Health = HorseHealth,



                    MaxHealth = HorseMaxHealth,



                    Stamina = HorseStamina,



                    MaxStamina = HorseMaxStamina,



                    Speed = HorseSpeed,



                    Capacity = HorseCapacity,



                    Courage = HorseCourage



                };



                NativeMethods.kcd2_save_set_horse_stats(_handle, in hStats);



            }







            IsDirty = true;



            StatusMessage = IsGerman



                ? $"Reittier gewechselt: {horse.DisplayName} ({horse.DisplayBreed}, Tier {horse.Tier})"



                : $"Switched mount: {horse.DisplayName} ({horse.DisplayBreed}, Tier {horse.Tier})";



        }







        public void SyncHorseSelection(string? rawName)



        {



            if (string.IsNullOrEmpty(rawName) || rawName.Contains("Kein Pferd", StringComparison.OrdinalIgnoreCase) || rawName.Contains("No Horse", StringComparison.OrdinalIgnoreCase))



            {



                _selectedHorseOption = null;



                OnPropertyChanged(nameof(SelectedHorseOption));



                return;



            }







            var match = AvailableHorses.FirstOrDefault(h => h.Id.Equals(rawName, StringComparison.OrdinalIgnoreCase))



                        ?? AvailableHorses.FirstOrDefault(h => rawName.Contains(h.Id, StringComparison.OrdinalIgnoreCase))



                        ?? (rawName.Contains("sedivka", StringComparison.OrdinalIgnoreCase) || rawName.Contains("pebbles", StringComparison.OrdinalIgnoreCase) || rawName.Contains("schimmel", StringComparison.OrdinalIgnoreCase)



                            ? AvailableHorses.FirstOrDefault(h => h.Id == "tsem_sedivka")



                            : null)



                        ?? AvailableHorses.FirstOrDefault(h => h.NameDe.Equals(rawName, StringComparison.OrdinalIgnoreCase) || h.NameEn.Equals(rawName, StringComparison.OrdinalIgnoreCase))



                        ?? AvailableHorses.FirstOrDefault();







            _selectedHorseOption = match;



            if (match != null)



            {



                _horseName = match.DisplayName;



                OnPropertyChanged(nameof(HorseName));



                OnPropertyChanged(nameof(DisplayHorseName));



            }



            OnPropertyChanged(nameof(SelectedHorseOption));



        }







        #endregion







        #region Modal Codex Spawner Properties







        public bool IsAddItemModalOpen



        {



            get => _isAddItemModalOpen;



            set => SetProperty(ref _isAddItemModalOpen, value);



        }







        public string NewItemName



        {



            get => _newItemName;



            set => SetProperty(ref _newItemName, value);



        }







        public string NewItemCategory



        {



            get => _newItemCategory;



            set => SetProperty(ref _newItemCategory, value);



        }







        public int NewItemQuantity



        {



            get => _newItemQuantity;



            set => SetProperty(ref _newItemQuantity, value);



        }







        public int NewItemPower



        {



            get => _newItemPower;



            set => SetProperty(ref _newItemPower, value);



        }







        public float NewItemWeight



        {



            get => _newItemWeight;



            set => SetProperty(ref _newItemWeight, value);



        }







        public float NewItemPrice



        {



            get => _newItemPrice;



            set => SetProperty(ref _newItemPrice, value);



        }







        public string SelectedCodexPreset



        {



            get => _selectedCodexPreset;



            set



            {



                if (SetProperty(ref _selectedCodexPreset, value))



                {



                    SelectCodexPreset(value);



                }



            }



        }







        #endregion







        #region V2 Architecture Properties







        public string SelectedMainTab



        {



            get => _selectedMainTab;



            set => SetProperty(ref _selectedMainTab, value);



        }







        public string SelectedInventoryTarget



        {



            get => _selectedInventoryTarget;



            set => SetProperty(ref _selectedInventoryTarget, value);



        }







        public string SelectedPlayerSubTab



        {



            get => _selectedPlayerSubTab;



            set => SetProperty(ref _selectedPlayerSubTab, value);



        }







        public bool IsDirty



        {



            get => _isDirty;



            set



            {



                if (SetProperty(ref _isDirty, value))



                {



                    OnPropertyChanged(nameof(StatusSyncText));



                    OnPropertyChanged(nameof(StatusSyncColor));



                }



            }



        }







        public string StatusSyncText => IsDirty 



            ? (IsGerman ? "● Modifiziert (Ungespeichert)" : "● Modified (Unsaved)") 



            : (IsGerman ? "● Synchronisiert" : "● Synchronized");







        public Brush StatusSyncColor => IsDirty 



            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")) 



            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));







        public int DogLoyalty



        {



            get => _dogLoyalty;



            set { SetProperty(ref _dogLoyalty, Math.Clamp(value, 0, 100)); IsDirty = true; }



        }







        public int DogObedience



        {



            get => _dogObedience;



            set { SetProperty(ref _dogObedience, Math.Clamp(value, 0, 100)); IsDirty = true; }



        }







        public int DogHunger



        {



            get => _dogHunger;



            set { SetProperty(ref _dogHunger, Math.Clamp(value, 0, 100)); IsDirty = true; }



        }







        public int DogAggression



        {



            get => _dogAggression;



            set { SetProperty(ref _dogAggression, Math.Clamp(value, 0, 100)); IsDirty = true; }



        }







                // Quest Sub-Tab: "Missions" vs "Fates"

        private string _selectedQuestSubTab = "Missions";

        public string SelectedQuestSubTab

        {

            get => _selectedQuestSubTab;

            set

            {

                if (SetProperty(ref _selectedQuestSubTab, value))

                {

                    OnPropertyChanged(nameof(IsQuestSubTabMissions));

                    OnPropertyChanged(nameof(IsQuestSubTabFates));

                }

            }

        }

        public bool IsQuestSubTabMissions => SelectedQuestSubTab == "Missions";

        public bool IsQuestSubTabFates => SelectedQuestSubTab == "Fates";



        // Quest Region Filter

        private string _selectedQuestRegionFilter = "all"; // "all", "trosecko", "kutnohorsko"

        public string SelectedQuestRegionFilter

        {

            get => _selectedQuestRegionFilter;

            set

            {

                if (SetProperty(ref _selectedQuestRegionFilter, value))

                {

                    FilteredQuests.Refresh();

                    OnPropertyChanged(nameof(IsQuestRegionAll));

                    OnPropertyChanged(nameof(IsQuestRegionTrosecko));

                    OnPropertyChanged(nameof(IsQuestRegionKutnohorsko));

                }

            }

        }

        public bool IsQuestRegionAll => SelectedQuestRegionFilter == "all";

        public bool IsQuestRegionTrosecko => SelectedQuestRegionFilter == "trosecko";

        public bool IsQuestRegionKutnohorsko => SelectedQuestRegionFilter == "kutnohorsko";



        // Character Fates & World State

        private GameCharacterFateViewModel? _selectedCharacterFate;

        public GameCharacterFateViewModel? SelectedCharacterFate

        {

            get => _selectedCharacterFate;

            set => SetProperty(ref _selectedCharacterFate, value);

        }



        private string _selectedFateCategoryFilter = "all"; // "all", "vip", "decision", "merchant"

        public string SelectedFateCategoryFilter

        {

            get => _selectedFateCategoryFilter;

            set

            {

                if (SetProperty(ref _selectedFateCategoryFilter, value))

                {

                    FilteredCharacterFates.Refresh();

                    OnPropertyChanged(nameof(IsFateFilterAll));

                    OnPropertyChanged(nameof(IsFateFilterVip));

                    OnPropertyChanged(nameof(IsFateFilterDecision));

                    OnPropertyChanged(nameof(IsFateFilterMerchant));

                }

            }

        }

        public bool IsFateFilterAll => SelectedFateCategoryFilter == "all";

        public bool IsFateFilterVip => SelectedFateCategoryFilter == "vip";

        public bool IsFateFilterDecision => SelectedFateCategoryFilter == "decision";

        public bool IsFateFilterMerchant => SelectedFateCategoryFilter == "merchant";



        private string _fateSearchFilter = string.Empty;

        public string FateSearchFilter

        {

            get => _fateSearchFilter;

            set

            {

                if (SetProperty(ref _fateSearchFilter, value))

                {

                    FilteredCharacterFates.Refresh();

                }

            }

        }



        private bool _isRaczekSwordReforged;

        public bool IsRaczekSwordReforged

        {

            get => _isRaczekSwordReforged;

            set

            {

                if (SetProperty(ref _isRaczekSwordReforged, value))

                {

                    if (_handle != IntPtr.Zero)

                    {

                        NativeMethods.kcd2_save_set_uint32_prop(_handle, "RaczekSwordReforged", value ? 1u : 0u);

                    }

                    IsDirty = true;

                    StatusMessage = IsGerman

                        ? (value ? "Radzigs Schwert: Als umgeschmiedet markiert" : "Radzigs Schwert: Als nicht umgeschmiedet markiert")

                        : (value ? "Radzig's Sword: Marked as reforged" : "Radzig's Sword: Marked as not reforged");

                }

            }

        }



        public string CurrentStoryRegionDisplay

        {

            get

            {

                if (SelectedSaveItem != null && !string.IsNullOrEmpty(SelectedSaveItem.RawLevelKey))

                {

                    if (SelectedSaveItem.RawLevelKey.Contains("kutno", StringComparison.OrdinalIgnoreCase))

                        return IsGerman ? "Kutnohorsko (Kuttenberg & Suchdol)" : "Kutnohorsko (Kuttenberg & Suchdol)";

                    if (SelectedSaveItem.RawLevelKey.Contains("trose", StringComparison.OrdinalIgnoreCase))

                        return IsGerman ? "Trosecko (Burg Trosky & Semín)" : "Trosecko (Trosky Castle & Semín)";

                }

                return IsGerman ? "Trosecko (Böhmisches Paradies)" : "Trosecko (Bohemian Paradise)";

            }

        }



        // Localized UI Labels for Quests & Fates

        public string Loc_QuestTabAllRegions => GameDataService.Instance.GetUiText("AllRegions", "Alle Regionen", "All Regions");

        public string Loc_QuestTabTrosecko => "Trosecko";

        public string Loc_QuestTabKutnohorsko => "Kutnohorsko";

        public string Loc_MissionsTab => GameDataService.Instance.GetUiText("MissionsTab", "📜 Missions-Logbuch", "📜 Quest Log");

        public string Loc_CharactersAndFatesTab => GameDataService.Instance.GetUiText("FatesTab", "👥 Charaktere & Weltschicksale", "👥 Characters & Fates");

        public string Loc_PointOfNoReturnBadge => GameDataService.Instance.GetUiText("PointOfNoReturnBadge", "⚠️ WENDEPUNKT", "⚠️ POINT OF NO RETURN");

        public string Loc_ReviveButton => GameDataService.Instance.GetUiText("ReviveNpc", "✨ NPC Wiederbeleben", "✨ Revive NPC");

        public string Loc_FateFilterAll => GameDataService.Instance.GetUiText("AllCharacters", "Alle", "All");

        public string Loc_FateFilterVip => GameDataService.Instance.GetUiText("StoryVips", "Story-VIPs (Unsterblich)", "Story VIPs (Immortal)");

        public string Loc_FateFilterDecision => GameDataService.Instance.GetUiText("StoryDecisions", "Schicksals-Wahl", "Story Decisions");

        public string Loc_FateFilterMerchant => GameDataService.Instance.GetUiText("Craftsmen", "Meister-Handwerker", "Master Craftsmen");

        public string Loc_WorldStateHeader => GameDataService.Instance.GetUiText("WorldStateHeader", "Weltzustand & Story-Meilensteine", "World State & Story Milestones");

        public string Loc_CurrentStoryRegion => GameDataService.Instance.GetUiText("CurrentRegion", "Aktive Haupt-Region:", "Active Main Region:");

        public string Loc_RaczekSwordReforgedLabel => GameDataService.Instance.GetUiText("RaczekSwordReforged", "Radzigs Schwert umgeschmiedet (RaczekSwordReforged)", "Radzig's Sword Reforged (RaczekSwordReforged)");

        public string Loc_FateSearchPlaceholder => GameDataService.Instance.GetUiText("FateSearchPlaceholder", "🔍 Charakter suchen...", "🔍 Search character...");
        public string Loc_NoCharacterSelected => GameDataService.Instance.GetUiText("NoCharacterSelected", "Kein Charakter ausgewählt", "No character selected");
        public string Loc_FateLoreHeader => GameDataService.Instance.GetUiText("FateLoreHeader", "Hintergrund & Bedeutung:", "Background & Significance:");
        public string Loc_FateActionsHeader => GameDataService.Instance.GetUiText("FateActionsHeader", "Status & Handlungsmöglichkeiten:", "Status & Available Actions:");
        public string Loc_FateVipProtectionTitle => GameDataService.Instance.GetUiText("FateVipProtectionTitle", "Story-VIP (Unsterblichkeits-Schutz aktiv)", "Story VIP (Immunity Protection Active)");
        public string Loc_FateVipProtectionDesc => GameDataService.Instance.GetUiText("FateVipProtectionDesc", "Dieser Charakter ist essenziell für die Hauptstory und kann durch Spielregeln nicht getötet werden.", "This character is essential to the main story and cannot be killed by game rules.");
        public string Loc_FateDecisionToggleHeader => GameDataService.Instance.GetUiText("FateDecisionToggleHeader", "Entscheidungs-Status umschalten:", "Toggle Decision Status:");
        public string Loc_FateMortalCraftsmanTitle => GameDataService.Instance.GetUiText("FateMortalCraftsmanTitle", "Sterblicher Meister-NPC (Handwerker / Händler)", "Mortal Master NPC (Craftsman / Merchant)");
        public string Loc_FateMortalCraftsmanDesc => GameDataService.Instance.GetUiText("FateMortalCraftsmanDesc", "Kann durch Überfälle oder Banditen getötet werden.", "Can be killed by ambushes or bandits.");
        public string Loc_RaczekSwordReforgedDesc => GameDataService.Instance.GetUiText("RaczekSwordReforgedDesc", "Prop: RaczekSwordReforged (Statistiken / Story-Flag für Radzigs Meisterschwert)", "Prop: RaczekSwordReforged (Statistics / Story Flag for Radzig's Master Sword)");

        public string Loc_ChoiceSpared => GameDataService.Instance.GetUiText("ChoiceSpared", "🟢 Verschonen / Leben", "🟢 Spare / Live");

        public string Loc_ChoiceExecuted => GameDataService.Instance.GetUiText("ChoiceExecuted", "💀 Hinrichten / Töten", "💀 Execute / Kill");



        public string SelectedQuestFilter



        {



            get => _selectedQuestFilter;



            set



            {



                if (SetProperty(ref _selectedQuestFilter, value))



                {



                    FilteredQuests.Refresh();



                }



            }



        }







        public string QuestSearchFilter



        {



            get => _questSearchFilter;



            set



            {



                if (SetProperty(ref _questSearchFilter, value))



                {



                    FilteredQuests.Refresh();



                }



            }



        }







        public QuestItemViewModel? SelectedQuest



        {



            get => _selectedQuest;



            set => SetProperty(ref _selectedQuest, value);



        }







        #endregion







        #endregion







        #region Commands







        public ICommand OpenSaveCommand { get; }



        public ICommand SaveCommand { get; }



        public ICommand AddGroschenCommand { get; }



        public ICommand SetGroschenCommand { get; }



        public ICommand MaxMoneyCommand { get; }



        public ICommand MaxAllStatsCommand { get; }



        public ICommand MaxHorseStatsCommand { get; }



        public ICommand AutoDetectCommand { get; }



        public ICommand SwitchToXboxCommand { get; }



        public ICommand SwitchToSteamCommand { get; }



        public ICommand SwitchPlatformCommand { get; }



        public ICommand BrowseFolderCommand { get; }



        public ICommand RefreshCommand { get; }



        public ICommand ImportWhsCommand { get; }



        public ICommand ExportWhsCommand { get; }



        public ICommand EnableAllDlcsCommand { get; }



        public ICommand DisableAllDlcsCommand { get; }

        public ICommand AddCustomDlcCommand { get; }

        private string _customDlcId = string.Empty;
        public string CustomDlcId
        {
            get => _customDlcId;
            set => SetProperty(ref _customDlcId, value);
        }







        public ICommand RepairAllCommand { get; }



        public ICommand ClearStolenCommand { get; }



        public ICommand CleanCharacterDirtCommand { get; }



        public ICommand SwitchOutfitCommand { get; }
        public ICommand SwitchHorseOutfitCommand { get; }

        public ICommand CloseSlotPickerModalCommand { get; }
        public ICommand EquipSelectedSlotItemCommand { get; }
        public ICommand UnequipSlotCommand { get; }
        public ICommand SelectSlotPickerSourceCommand { get; }



        public ICommand SetRightViewCommand { get; }



        public ICommand SelectCategoryTabCommand { get; }



        public ICommand ToggleFilterEquippedCommand { get; }



        public ICommand DuplicateSelectedItemCommand { get; }



        public ICommand DeleteSelectedItemCommand { get; }



        public ICommand OpenAddItemModalCommand { get; }



        public ICommand CloseAddItemModalCommand { get; }



        public ICommand ConfirmAddItemCommand { get; }



        public ICommand SelectCodexPresetCommand { get; }







        public ICommand SwitchMainTabCommand { get; }



        public ICommand SwitchInventoryTargetCommand { get; }



        public ICommand SwitchPlayerSubTabCommand { get; }



        public ICommand FilterQuestsCommand { get; }



        public ICommand SelectQuestCommand { get; }



        public ICommand SwitchQuestSubTabCommand { get; }

        public ICommand FilterQuestRegionCommand { get; }

        public ICommand FilterFateCategoryCommand { get; }

        public ICommand SelectFateCharacterCommand { get; }

        public ICommand ReviveCharacterCommand { get; }

        public ICommand SetCharacterStatusCommand { get; }

        public ICommand UnlockAllQuestsCommand { get; }



        public ICommand SetQuestStatusCommand { get; }



        public ICommand AutocompleteObjectivesCommand { get; }



        public ICommand MaxDogStatsCommand { get; }



        public ICommand UnlockAllPerksCommand { get; }



        public ICommand AddHerbsBatchCommand { get; }



        public ICommand AddHerbCommand { get; }



        public ICommand MaxAllHerbsCommand { get; }



        public ICommand UnlockAllRecipesCommand { get; }



        public ICommand UnlockAllCraftingCommand { get; }



        public ICommand LockAllCraftingCommand { get; }



        public ICommand UnlockAllCraftingPerksCommand { get; }



        public ICommand LockAllCraftingPerksCommand { get; }



        public ICommand SelectCraftFilterCommand { get; }



        public ICommand SelectCraftStatusFilterCommand { get; }



        public ICommand ToggleRecipeCommand { get; }



        public ICommand MaxSmithingStatsCommand { get; }







        #endregion







        #region Filtering & Scanning







        private bool FilterCraftingRecipe(object obj)



        {



            if (obj is not CraftingRecipeViewModel r) return false;







            // Type Filter



            if (SelectedCraftFilter == "Alchemy" && r.RecipeType != "Alchemy") return false;



            if (SelectedCraftFilter == "Blacksmithing" && r.RecipeType != "Blacksmithing") return false;







            // Status Filter



            if (SelectedCraftStatusFilter == "Learned" && !r.IsUnlocked) return false;



            if (SelectedCraftStatusFilter == "Unlearned" && r.IsUnlocked) return false;







            // Search Filter



            if (!string.IsNullOrWhiteSpace(CraftSearchFilter))



            {



                string query = CraftSearchFilter.Trim();



                bool match = r.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||



                             r.CategoryDisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||



                             r.DisplayIngredients.Contains(query, StringComparison.OrdinalIgnoreCase) ||



                             r.DisplayDescription.Contains(query, StringComparison.OrdinalIgnoreCase);



                if (!match) return false;



            }







            return true;



        }







        private bool FilterCraftingPerk(object obj)



        {



            if (obj is not CraftingSkillPerkViewModel p) return false;







            // Status Filter



            if (SelectedCraftStatusFilter == "Learned" && !p.IsUnlocked) return false;



            if (SelectedCraftStatusFilter == "Unlearned" && p.IsUnlocked) return false;







            // Search Filter



            if (!string.IsNullOrWhiteSpace(CraftSearchFilter))



            {



                string query = CraftSearchFilter.Trim();



                bool match = p.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||



                             p.DisplayDescription.Contains(query, StringComparison.OrdinalIgnoreCase);



                if (!match) return false;



            }







            return true;



        }







        private bool FilterSaveItem(object obj)



        {



            if (obj is not SaveGameItemViewModel item) return false;







            // STRICT REQUIREMENT: Only .whs files are selectable



            if (!item.FileName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase)) return false;







            if (SelectedCategory != "All")



            {



                if (!item.SaveType.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase))



                {



                    return false;



                }



            }







            if (!string.IsNullOrWhiteSpace(SearchFilter))



            {



                string search = SearchFilter.Trim().ToLowerInvariant();



                bool matchesName = item.FileName.ToLowerInvariant().Contains(search);



                bool matchesQuest = item.QuestName.ToLowerInvariant().Contains(search);



                bool matchesLevel = item.LevelName.ToLowerInvariant().Contains(search);



                bool matchesType = item.SaveType.ToLowerInvariant().Contains(search);



                return matchesName || matchesQuest || matchesLevel || matchesType;



            }







            return true;



        }







        private bool MatchesCategoryFilter(string itemCategory, string filterCategory)



        {



            if (string.IsNullOrEmpty(filterCategory) ||



                filterCategory.Equals("Alle", StringComparison.OrdinalIgnoreCase) ||



                filterCategory.Equals("All", StringComparison.OrdinalIgnoreCase))



                return true;







            if (itemCategory.Equals(filterCategory, StringComparison.OrdinalIgnoreCase)) return true;







            // Handle cross-language aliases



            if ((filterCategory.Equals("Waffen", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Weapons", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Waffen", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Weapons", StringComparison.OrdinalIgnoreCase)))



                return true;







            if ((filterCategory.Equals("Rüstung", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Armour", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Armor", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Rüstung", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Armour", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Armor", StringComparison.OrdinalIgnoreCase)))



                return true;







            if ((filterCategory.Equals("Nahrung", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Food", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Nahrung", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Food", StringComparison.OrdinalIgnoreCase)))



                return true;







            if ((filterCategory.Equals("Bücher", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Books", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Bücher", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Books", StringComparison.OrdinalIgnoreCase)))



                return true;







            if ((filterCategory.Equals("Materialien", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Materials", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Herbs", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Materialien", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Materials", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Herbs", StringComparison.OrdinalIgnoreCase)))



                return true;







            if (filterCategory.Equals("Quest", StringComparison.OrdinalIgnoreCase) && itemCategory.Equals("Quest", StringComparison.OrdinalIgnoreCase))



                return true;







            if ((filterCategory.Equals("Sonstiges", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Other", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Misc", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Sonstiges", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Other", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Misc", StringComparison.OrdinalIgnoreCase)))



                return true;







            if ((filterCategory.Equals("Pferd", StringComparison.OrdinalIgnoreCase) || filterCategory.Equals("Horse", StringComparison.OrdinalIgnoreCase)) &&



                (itemCategory.Equals("Pferd", StringComparison.OrdinalIgnoreCase) || itemCategory.Equals("Horse", StringComparison.OrdinalIgnoreCase)))



                return true;







            return false;



        }







        private bool FilterInventoryItem(object obj)



        {



            if (obj is not InventoryItemViewModel item) return false;







            // Storage Location Filter



            if (SelectedStorageFilter == "Inventory" && item.StorageLocation != ItemStorageLocation.Inventory) return false;



            if (SelectedStorageFilter == "Stash" && item.StorageLocation != ItemStorageLocation.Stash) return false;



            if (SelectedStorageFilter == "Horse" && item.StorageLocation != ItemStorageLocation.Horse) return false;







            // Category filter



            if (!MatchesCategoryFilter(item.Category, SelectedInventoryCategory))



            {



                return false;



            }







            // Equipped filter



            if (FilterEquippedOnly && !item.IsEquipped)



            {



                return false;



            }







            // Search text filter



            if (!string.IsNullOrWhiteSpace(InventorySearchFilter))



            {



                string search = InventorySearchFilter.Trim().ToLowerInvariant();



                bool matchesName = item.ItemUiName.ToLowerInvariant().Contains(search);



                bool matchesGuid = item.ItemId.ToLowerInvariant().Contains(search);



                bool matchesCat = item.Category.ToLowerInvariant().Contains(search);



                bool matchesSub = item.SubCategory.ToLowerInvariant().Contains(search);



                return matchesName || matchesGuid || matchesCat || matchesSub;



            }







            return true;



        }







        private static bool IsCryEngineSaveFile(string filePath)



        {



            try



            {



                if (!File.Exists(filePath)) return false;



                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);



                if (fs.Length < 16) return false;



                byte[] magic = new byte[4];



                if (fs.Read(magic, 0, 4) != 4) return false;



                return BitConverter.ToUInt32(magic, 0) == 0xFFFFFFFF;



            }



            catch



            {



                return false;



            }



        }







        private static List<(string virtualName, string diskPath)> ParseWgsContainer(string containerFilePath)



        {



            var entries = new List<(string virtualName, string diskPath)>();



            try



            {



                string? containerDir = Path.GetDirectoryName(containerFilePath);



                if (string.IsNullOrEmpty(containerDir) || !File.Exists(containerFilePath)) return entries;







                byte[] data = File.ReadAllBytes(containerFilePath);



                if (data.Length < 8) return entries;







                uint version = BitConverter.ToUInt32(data, 0);



                uint count = BitConverter.ToUInt32(data, 4);







                int offset = 8;



                for (int i = 0; i < count; i++)



                {



                    if (offset + 160 > data.Length) break;







                    string vname = Encoding.Unicode.GetString(data, offset, 128);



                    int nullIdx = vname.IndexOf('\0');



                    if (nullIdx >= 0) vname = vname.Substring(0, nullIdx);



                    vname = vname.Trim();







                    byte[] guidBytes = new byte[16];



                    Buffer.BlockCopy(data, offset + 128, guidBytes, 0, 16);



                    Guid guid = new Guid(guidBytes);



                    string hexGuid = guid.ToString("N").ToUpperInvariant();



                    string hexRaw = BitConverter.ToString(guidBytes).Replace("-", "").ToUpperInvariant();







                    string diskPath = Path.Combine(containerDir, hexGuid);



                    if (!File.Exists(diskPath) && File.Exists(Path.Combine(containerDir, hexRaw)))



                    {



                        diskPath = Path.Combine(containerDir, hexRaw);



                    }







                    if (File.Exists(diskPath))



                    {



                        entries.Add((vname, diskPath));



                    }







                    offset += 160;



                }



            }



            catch { }



            return entries;



        }







        private static List<string> DiscoverXboxWgsContainerDirs()



        {



            var result = new List<string>();



            try



            {



                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);



                string packagesDir = Path.Combine(localAppData, "Packages");



                if (!Directory.Exists(packagesDir)) return result;







                var packageDirs = Directory.GetDirectories(packagesDir, "*DeepSilver*")



                    .Concat(Directory.GetDirectories(packagesDir, "*kingdomcome*"))



                    .Concat(Directory.GetDirectories(packagesDir, "*KingdomCome*"));







                foreach (var pkg in packageDirs.Distinct(StringComparer.OrdinalIgnoreCase))



                {



                    string wgsRoot = Path.Combine(pkg, "SystemAppData", "wgs");



                    if (!Directory.Exists(wgsRoot)) continue;







                    foreach (var userDir in Directory.GetDirectories(wgsRoot))



                    {



                        if (string.Equals(Path.GetFileName(userDir), "t", StringComparison.OrdinalIgnoreCase)) continue;







                        var containerFiles = Directory.GetFiles(userDir, "container.*", SearchOption.AllDirectories);



                        foreach (var cFile in containerFiles)



                        {



                            string? cDir = Path.GetDirectoryName(cFile);



                            if (!string.IsNullOrEmpty(cDir) && !result.Contains(cDir, StringComparer.OrdinalIgnoreCase))



                            {



                                result.Add(cDir);



                            }



                        }







                        if (!result.Contains(userDir, StringComparer.OrdinalIgnoreCase))



                        {



                            result.Add(userDir);



                        }



                    }



                }



            }



            catch { }



            return result;



        }







        private static List<(string path, DateTime lastModified)> ProbeSavesInDirectory(string dirPath)



        {



            var list = new List<(string path, DateTime lastModified)>();



            try



            {



                if (!Directory.Exists(dirPath)) return list;







                var containerFiles = Directory.GetFiles(dirPath, "container.*", SearchOption.AllDirectories);



                foreach (var cFile in containerFiles)



                {



                    var entries = ParseWgsContainer(cFile);



                    foreach (var e in entries)



                    {



                        if (e.virtualName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase) || IsCryEngineSaveFile(e.diskPath))



                        {



                            list.Add((e.diskPath, File.GetLastWriteTime(e.diskPath)));



                        }



                    }



                }







                var whsFiles = Directory.GetFiles(dirPath, "*.whs", SearchOption.AllDirectories);



                foreach (var f in whsFiles)



                {



                    if (!list.Any(x => x.path.Equals(f, StringComparison.OrdinalIgnoreCase)))



                    {



                        list.Add((f, File.GetLastWriteTime(f)));



                    }



                }



            }



            catch { }



            return list;



        }







        public void SwitchToXbox()



        {



            if (string.IsNullOrEmpty(_detectedXboxDir) || !Directory.Exists(_detectedXboxDir))



            {



                var xboxDirs = DiscoverXboxWgsContainerDirs();



                foreach (var dir in xboxDirs)



                {



                    var saves = ProbeSavesInDirectory(dir);



                    if (saves.Count > 0)



                    {



                        _detectedXboxDir = dir;



                        XboxSavesCount = saves.Count;



                        break;



                    }



                }



            }







            if (!string.IsNullOrEmpty(_detectedXboxDir) && Directory.Exists(_detectedXboxDir))



            {



                CurrentPlatform = "Xbox";



                AppSettings.Current.LastSavePlatform = "Xbox";



                AppSettings.Current.Save();



                ScanDirectory(_detectedXboxDir);



                StatusMessage = IsGerman



                    ? $"Zu Xbox Game Pass Spielständen gewechselt ({AvailableSaves.Count} gefunden)"



                    : $"Switched to Xbox Game Pass saves ({AvailableSaves.Count} found)";



            }



            else



            {



                StatusMessage = IsGerman ? "Keine Xbox Game Pass Spielstände gefunden." : "No Xbox Game Pass saves found.";



            }



        }







        public void SwitchToSteam()



        {



            if (string.IsNullOrEmpty(_detectedSteamDir) || !Directory.Exists(_detectedSteamDir))



            {



                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);



                string[] candidates = {



                    Path.Combine(userProfile, "Saved Games", "kingdomcome2", "saves"),



                    Path.Combine(userProfile, "Saved Games", "kingdomcome2"),



                    Path.Combine(userProfile, "Saved Games", "Kingdom Come Deliverance 2", "saves")



                };



                foreach (var dir in candidates)



                {



                    var saves = ProbeSavesInDirectory(dir);



                    if (saves.Count > 0)



                    {



                        _detectedSteamDir = dir;



                        SteamSavesCount = saves.Count;



                        break;



                    }



                }



            }







            if (!string.IsNullOrEmpty(_detectedSteamDir) && Directory.Exists(_detectedSteamDir))



            {



                CurrentPlatform = "Steam";



                AppSettings.Current.LastSavePlatform = "Steam";



                AppSettings.Current.Save();



                ScanDirectory(_detectedSteamDir);



                StatusMessage = IsGerman



                    ? $"Zu Steam Spielständen gewechselt ({AvailableSaves.Count} gefunden)"



                    : $"Switched to Steam saves ({AvailableSaves.Count} found)";



            }



            else



            {



                StatusMessage = IsGerman ? "Keine Steam Spielstände gefunden." : "No Steam saves found.";



            }



        }







        public void SwitchPlatform(string? platform)



        {



            if (string.Equals(platform, "Steam", StringComparison.OrdinalIgnoreCase))



            {



                SwitchToSteam();



            }



            else



            {



                SwitchToXbox();



            }



        }







        public void ScanDirectory(string dirPath)



        {



            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath)) return;







            bool isXbox = dirPath.Contains("Packages", StringComparison.OrdinalIgnoreCase) ||



                          dirPath.Contains("wgs", StringComparison.OrdinalIgnoreCase) ||



                          dirPath.Contains("SystemAppData", StringComparison.OrdinalIgnoreCase);



            bool isSteam = dirPath.Contains("Saved Games", StringComparison.OrdinalIgnoreCase) ||



                           dirPath.Contains("steamapps", StringComparison.OrdinalIgnoreCase) ||



                           File.Exists(Path.Combine(dirPath, "steam_autocloud.vdf")) ||



                           File.Exists(Path.Combine(dirPath, "..", "steam_autocloud.vdf"));







            if (isXbox) CurrentPlatform = "Xbox";



            else if (isSteam) CurrentPlatform = "Steam";



            else CurrentPlatform = "Custom";







            CurrentDirectoryPath = dirPath;



            AppSettings.Current.LastSaveDirectory = dirPath;



            AppSettings.Current.Save();







            AvailableSaves.Clear();







            var foundItems = new List<SaveGameItemViewModel>();



            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);







            try



            {



                // 1. Scan Xbox Connected Storage (WGS) containers if any exist in dirPath



                var containerFiles = Directory.GetFiles(dirPath, "container.*", SearchOption.AllDirectories);



                foreach (var cFile in containerFiles)



                {



                    var entries = ParseWgsContainer(cFile);



                    foreach (var entry in entries)



                    {



                        if (seenPaths.Contains(entry.diskPath)) continue;







                        if (entry.virtualName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase) ||



                            IsCryEngineSaveFile(entry.diskPath))



                        {



                            string friendlyName = entry.virtualName;



                            if (!friendlyName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase))



                            {



                                friendlyName += ".whs";



                            }







                            var item = SaveGameItemViewModel.FromFile(entry.diskPath, friendlyName);



                            if (item != null && item.SaveType != "Unknown")



                            {



                                foundItems.Add(item);



                                seenPaths.Add(entry.diskPath);



                            }



                        }



                    }



                }







                // 2. Scan standard *.whs files



                var whsFiles = Directory.GetFiles(dirPath, "*.whs", SearchOption.AllDirectories);



                foreach (var file in whsFiles)



                {



                    if (seenPaths.Contains(file)) continue;



                    if (!file.EndsWith(".whs", StringComparison.OrdinalIgnoreCase)) continue;







                    var item = SaveGameItemViewModel.FromFile(file);



                    if (item != null && item.SaveType != "Unknown")



                    {



                        foundItems.Add(item);



                        seenPaths.Add(file);



                    }



                }







                // 3. Check for any CryEngine savegame files without .whs extension (e.g. directly opened WGS GUID file)



                var allFiles = Directory.GetFiles(dirPath, "*", SearchOption.TopDirectoryOnly);



                foreach (var file in allFiles)



                {



                    if (seenPaths.Contains(file)) continue;



                    string fName = Path.GetFileName(file);



                    if (fName.StartsWith("container.", StringComparison.OrdinalIgnoreCase) ||



                        fName.Equals("containers.index", StringComparison.OrdinalIgnoreCase))



                        continue;







                    if (IsCryEngineSaveFile(file))



                    {



                        string friendly = fName.Length == 32 ? $"{fName.Substring(0, 8)}.whs" : (fName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase) ? fName : $"{fName}.whs");



                        var item = SaveGameItemViewModel.FromFile(file, friendly);



                        if (item != null && item.SaveType != "Unknown")



                        {



                            foundItems.Add(item);



                            seenPaths.Add(file);



                        }



                    }



                }



            }



            catch { }







            // Sort by LastModified descending



            foreach (var save in foundItems.OrderByDescending(x => x.LastModified))



            {



                AvailableSaves.Add(save);



            }







            StatusMessage = IsGerman



                ? $"Scan abgeschlossen: {AvailableSaves.Count} .whs Spielstand/Spielstände in {dirPath} gefunden"



                : $"Scan completed: {AvailableSaves.Count} .whs savegame(s) found in {dirPath}";







            SelectedSaveItem = AvailableSaves.FirstOrDefault();



        }







        public void AutoDetectSaves()



        {



            // 1. Discover Xbox Game Pass WGS directories



            var xboxDirs = DiscoverXboxWgsContainerDirs();



            string? bestXboxDir = null;



            DateTime bestXboxTime = DateTime.MinValue;



            int maxXboxCount = 0;







            foreach (var dir in xboxDirs)



            {



                var saves = ProbeSavesInDirectory(dir);



                if (saves.Count > 0)



                {



                    DateTime maxTime = saves.Max(s => s.lastModified);



                    if (saves.Count > maxXboxCount || (saves.Count == maxXboxCount && maxTime > bestXboxTime))



                    {



                        bestXboxDir = dir;



                        bestXboxTime = maxTime;



                        maxXboxCount = saves.Count;



                    }



                }



            }



            _detectedXboxDir = bestXboxDir;



            XboxSavesCount = maxXboxCount;







            // 2. Discover Steam / GOG / Standard Saved Games directories



            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);



            string[] standardDirs = {



                Path.Combine(userProfile, "Saved Games", "kingdomcome2", "saves"),



                Path.Combine(userProfile, "Saved Games", "kingdomcome2"),



                Path.Combine(userProfile, "Saved Games", "Kingdom Come Deliverance 2", "saves"),



                Path.Combine(userProfile, "Saved Games", "Kingdom Come Deliverance 2"),



                Path.Combine(userProfile, "Saved Games", "KingdomComeDeliverance2", "saves"),



                Path.Combine(userProfile, "Saved Games", "KingdomComeDeliverance2")



            };







            string? bestSteamDir = null;



            DateTime bestSteamTime = DateTime.MinValue;



            int maxSteamCount = 0;







            foreach (var dir in standardDirs)



            {



                var saves = ProbeSavesInDirectory(dir);



                if (saves.Count > 0)



                {



                    DateTime maxTime = saves.Max(s => s.lastModified);



                    if (saves.Count > maxSteamCount || (saves.Count == maxSteamCount && maxTime > bestSteamTime))



                    {



                        bestSteamDir = dir;



                        bestSteamTime = maxTime;



                        maxSteamCount = saves.Count;



                    }



                }



            }



            _detectedSteamDir = bestSteamDir;



            SteamSavesCount = maxSteamCount;







            // 3. Determine which platform to load initially



            var settings = AppSettings.Current;



            string targetPlatform = string.Empty;







            if (!string.IsNullOrEmpty(settings.LastSavePlatform))



            {



                if (settings.LastSavePlatform == "Steam" && HasSteamSaves) targetPlatform = "Steam";



                else if (settings.LastSavePlatform == "Xbox" && HasXboxSaves) targetPlatform = "Xbox";



            }







            if (string.IsNullOrEmpty(targetPlatform))



            {



                bool isXboxGame = !string.IsNullOrEmpty(settings.GameDirectory) &&



                    (settings.GameDirectory.Contains("XboxGames", StringComparison.OrdinalIgnoreCase) ||



                     settings.GameDirectory.Contains("DeepSilver", StringComparison.OrdinalIgnoreCase) ||



                     settings.GameDirectory.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase));







                if (isXboxGame && HasXboxSaves)



                {



                    targetPlatform = "Xbox";



                }



                else if (HasXboxSaves && HasSteamSaves)



                {



                    targetPlatform = bestXboxTime >= bestSteamTime ? "Xbox" : "Steam";



                }



                else if (HasXboxSaves)



                {



                    targetPlatform = "Xbox";



                }



                else if (HasSteamSaves)



                {



                    targetPlatform = "Steam";



                }



            }







            if (targetPlatform == "Xbox" && !string.IsNullOrEmpty(_detectedXboxDir))



            {



                CurrentPlatform = "Xbox";



                ScanDirectory(_detectedXboxDir);



                return;



            }



            else if (targetPlatform == "Steam" && !string.IsNullOrEmpty(_detectedSteamDir))



            {



                CurrentPlatform = "Steam";



                ScanDirectory(_detectedSteamDir);



                return;



            }



            else if (HasXboxSaves && !string.IsNullOrEmpty(_detectedXboxDir))



            {



                CurrentPlatform = "Xbox";



                ScanDirectory(_detectedXboxDir);



                return;



            }



            else if (HasSteamSaves && !string.IsNullOrEmpty(_detectedSteamDir))



            {



                CurrentPlatform = "Steam";



                ScanDirectory(_detectedSteamDir);



                return;



            }







            // 4. Fallback developer / custom directories



            string[] fallbackDirs = {



                settings.LastSaveDirectory,



                @"D:\Projects\KCD_2_SaveGAme_Editor\SaveGames",



                Path.Combine(Environment.CurrentDirectory, "SaveGames"),



                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveGames")



            };







            foreach (var dir in fallbackDirs)



            {



                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))



                {



                    var saves = ProbeSavesInDirectory(dir);



                    if (saves.Count > 0)



                    {



                        CurrentPlatform = "Custom";



                        ScanDirectory(dir);



                        return;



                    }



                }



            }







            StatusMessage = IsGerman



                ? "Keine .whs Spielstände automatisch gefunden. Bitte 'Öffnen...' wählen."



                : "No .whs save games auto-detected. Please use 'Open...'.";



        }







        private void BrowseFolder()



        {



            var dialog = new OpenFolderDialog



            {



                Title = "Select Kingdom Come: Deliverance 2 Saves Folder (.whs)"



            };







            if (dialog.ShowDialog() == true)



            {



                ScanDirectory(dialog.FolderName);



            }



        }







        #endregion







        #region Save Actions







        private void OpenSave()



        {



            var dialog = new OpenFileDialog



            {



                Filter = "Kingdom Come: Deliverance II Save Games (*.whs;*)|*.whs;*|All Files (*.*)|*.*",



                Title = "Open Kingdom Come: Deliverance 2 Save Game (.whs)",



                DefaultExt = ".whs"



            };







            if (dialog.ShowDialog() == true)



            {



                bool isWhs = dialog.FileName.EndsWith(".whs", StringComparison.OrdinalIgnoreCase);



                bool isCry = IsCryEngineSaveFile(dialog.FileName);







                if (!isWhs && !isCry)



                {



                    StatusMessage = IsGerman



                        ? "Fehler: Es können nur gültige KCD2 .whs Spielstände geöffnet werden!"



                        : "Error: Only valid KCD2 .whs save games can be opened!";



                    return;



                }







                LoadSaveFile(dialog.FileName);



                string? folder = Path.GetDirectoryName(dialog.FileName);



                if (!string.IsNullOrEmpty(folder) && folder != CurrentDirectoryPath)



                {



                    ScanDirectory(folder);



                }



            }



        }







        public void LoadSaveFile(string path)



        {



            CloseCurrentSession();







            StatusMessage = IsGerman ? $"Lade {Path.GetFileName(path)}..." : $"Loading {Path.GetFileName(path)}...";



            var res = NativeMethods.kcd2_save_open(path, out _handle);



            if (res != Kcd2Result.Ok || _handle == IntPtr.Zero)



            {



                StatusMessage = IsGerman ? $"Laden des Spielstands fehlgeschlagen: {NativeMethods.kcd2_get_last_error_message()}" : $"Failed to load save: {NativeMethods.kcd2_get_last_error_message()}";



                IsSaveLoaded = false;



                return;



            }







            SaveFilePath = path;



            IsSaveLoaded = true;







            // Read Metadata



            if (NativeMethods.kcd2_save_get_metadata(_handle, out var meta) == Kcd2Result.Ok)



            {



                SaveName = meta.SaveName;



                LevelName = meta.PlaylineId;



                GameVersion = meta.GameVersion;



                ulong hours = meta.PlaytimeSeconds / 3600;



                ulong mins = (meta.PlaytimeSeconds % 3600) / 60;



                PlaytimeString = $"{hours} hours, {mins} mins";



            }







            // Read Player Stats



            if (NativeMethods.kcd2_save_get_player_stats(_handle, out var stats) == Kcd2Result.Ok)



            {



                Groschen = stats.Groschen;



                Health = stats.Health;



                MaxHealth = stats.MaxHealth;



                Stamina = stats.Stamina;



                MaxStamina = stats.MaxStamina;



                Energy = stats.Energy;



                Nourishment = stats.Nourishment;







                Strength = stats.Strength;



                Agility = stats.Agility;



                Vitality = stats.Vitality;



                Speech = stats.Speech;



                Charisma = stats.Charisma;



                Speed = stats.Speed;







                CharacterLevel = stats.CharacterLevel;







                Conspicuousness = stats.Conspicuousness;



                Noise = stats.Noise;



                Visibility = stats.Visibility;







                CarryWeight = stats.CarryWeight;



                MaxCarryWeight = stats.MaxCarryWeight;







                Warfare = stats.Warfare;



                Sword = stats.Sword;



                Defense = stats.Defense;



                Stealth = stats.Stealth;



                LocksPicked = stats.Lockpicking;



                Pickpocketing = stats.Pickpocketing;



                PotionsConsumed = stats.Alchemy;



                Marksmanship = stats.Marksmanship;



                Horsemanship = stats.Horsemanship;



                Houndmaster = stats.Houndmaster;



                Scholarship = stats.Scholarship;



                Blacksmithing = stats.Blacksmithing;



            }







            // Read Horse Stats



            if (NativeMethods.kcd2_save_get_horse_stats(_handle, out var horseStats) == Kcd2Result.Ok)



            {



                HorseHealth = horseStats.Health;



                HorseMaxHealth = horseStats.MaxHealth;



                HorseStamina = horseStats.Stamina;



                HorseMaxStamina = horseStats.MaxStamina;



                HorseSpeed = horseStats.Speed;



                HorseCapacity = horseStats.Capacity;



                HorseCourage = horseStats.Courage;



                HasHorse = !string.IsNullOrEmpty(horseStats.HorseName) && 



                           !horseStats.HorseName.Contains("Kein Pferd", StringComparison.OrdinalIgnoreCase) &&



                           !horseStats.HorseName.Contains("No Horse", StringComparison.OrdinalIgnoreCase);







                SyncHorseSelection(horseStats.HorseName);



            }



            else



            {



                HorseName = IsGerman ? "Kein Pferd" : "No Horse";



                HasHorse = false;



                _selectedHorseOption = null;



                OnPropertyChanged(nameof(SelectedHorseOption));



            }







            // Load Inventory Items



            LoadInventoryItems();







            // Read DLCs from Manifest XML



            LoadDlcsFromManifest();







            // Read Quests from Payload



            LoadQuestsFromSave();
            if (NativeMethods.kcd2_save_get_uint32_prop(_handle, "RaczekSwordReforged", out uint rzVal) == Kcd2Result.Ok)
            {
                _isRaczekSwordReforged = (rzVal > 0);
                OnPropertyChanged(nameof(IsRaczekSwordReforged));
            }
            OnPropertyChanged(nameof(CurrentStoryRegionDisplay));







            IsDirty = false;







            StatusMessage = IsGerman ? $"Geladen: {Path.GetFileName(path)} (MD5-Prüfsumme verifiziert)" : $"Loaded: {Path.GetFileName(path)} (Verified MD5 Checksum)";



        }







        private void SaveSave()



        {



            if (_handle == IntPtr.Zero) return;







            NativeMethods.kcd2_save_get_player_stats(_handle, out var stats);



            stats.Groschen = Groschen;



            stats.Health = Health;



            stats.MaxHealth = MaxHealth;



            stats.Stamina = Stamina;



            stats.MaxStamina = MaxStamina;



            stats.Energy = Energy;



            stats.Nourishment = Nourishment;







            stats.Strength = Strength;



            stats.Agility = Agility;



            stats.Vitality = Vitality;



            stats.Speech = Speech;



            stats.Charisma = Charisma;



            stats.Speed = Speed;







            stats.CharacterLevel = CharacterLevel;







            stats.Conspicuousness = Conspicuousness;



            stats.Noise = Noise;



            stats.Visibility = Visibility;







            stats.CarryWeight = CarryWeight;



            stats.MaxCarryWeight = MaxCarryWeight;







            stats.Warfare = Warfare;



            stats.Sword = Sword;



            stats.Defense = Defense;



            stats.Stealth = Stealth;



            stats.Lockpicking = LocksPicked;



            stats.Pickpocketing = Pickpocketing;



            stats.Alchemy = PotionsConsumed;



            stats.Marksmanship = Marksmanship;



            stats.Horsemanship = Horsemanship;



            stats.Houndmaster = Houndmaster;



            stats.Scholarship = Scholarship;



            stats.Blacksmithing = Blacksmithing;







            var groschenItem = InventoryItems.FirstOrDefault(x => x.ItemId.Equals("5ef63059-322e-4e1b-abe8-926e100c770e", StringComparison.OrdinalIgnoreCase));



            if (groschenItem != null)



            {



                groschenItem.Amount = (int)Math.Clamp(Groschen, 0, int.MaxValue);



            }







            NativeMethods.kcd2_save_set_player_stats(_handle, in stats);







            if (HasHorse)



            {



                if (SelectedHorseOption != null && _handle != IntPtr.Zero)



                {



                    NativeMethods.kcd2_save_set_horse(_handle, SelectedHorseOption.Id);



                }







                var hStats = new Kcd2HorseStats



                {



                    HorseName = HorseName,



                    Health = HorseHealth,



                    MaxHealth = HorseMaxHealth,



                    Stamina = HorseStamina,



                    MaxStamina = HorseMaxStamina,



                    Speed = HorseSpeed,



                    Capacity = HorseCapacity,



                    Courage = HorseCourage



                };



                NativeMethods.kcd2_save_set_horse_stats(_handle, in hStats);



            }







            SaveQuestsToSave();







            foreach (var item in InventoryItems)



            {



                if (item.ItemId.Equals("5ef63059-322e-4e1b-abe8-926e100c770e", StringComparison.OrdinalIgnoreCase))



                    continue;







                var native = item.ToNativeItem();



                NativeMethods.kcd2_save_set_inventory_item(_handle, in native);



            }







            UpdateManifestDlcs();







            StatusMessage = IsGerman ? "Komprimiere neu und berechne MD5-Prüfsumme..." : "Re-compressing and recalculating MD5 checksum...";



            var res = NativeMethods.kcd2_save_write(_handle, null, CreateBackup ? 1 : 0);



            if (res != Kcd2Result.Ok)



            {



                StatusMessage = IsGerman ? $"Speichern fehlgeschlagen: {NativeMethods.kcd2_get_last_error_message()}" : $"Failed to save: {NativeMethods.kcd2_get_last_error_message()}";



                MessageBox.Show(StatusMessage, IsGerman ? "Fehler beim Speichern" : "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);



                return;



            }







            SyncXboxWgsContainer(SaveFilePath);







            StatusMessage = IsGerman



                ? $"Erfolgreich gespeichert! Prüfsumme aktualisiert. {(CreateBackup ? "(Backup erstellt)" : "")}"



                : $"Saved successfully! Checksum recalculated. {(CreateBackup ? "(Backup created)" : "")}";



            MessageBox.Show(



                IsGerman ? "Spielstand erfolgreich mit gültiger CryEngine-Prüfsumme gespeichert!" : "Save game saved successfully with valid game checksum!",



                IsGerman ? "Speichern erfolgreich" : "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);







            if (!string.IsNullOrEmpty(CurrentDirectoryPath))



            {



                ScanDirectory(CurrentDirectoryPath);



            }



        }







        private void SyncXboxWgsContainer(string filePath)



        {



            try



            {



                string? containerDir = Path.GetDirectoryName(filePath);



                if (string.IsNullOrEmpty(containerDir)) return;







                string? wgsDir = Path.GetDirectoryName(containerDir);



                if (string.IsNullOrEmpty(wgsDir)) return;







                string containersIndex = Path.Combine(wgsDir, "containers.index");



                if (!File.Exists(containersIndex)) return;







                long totalSize = 0;



                var allFiles = Directory.GetFiles(containerDir);



                foreach (var f in allFiles)



                {



                    totalSize += new FileInfo(f).Length;



                }







                string containerFolderName = Path.GetFileName(containerDir);



                byte[] data = File.ReadAllBytes(containersIndex);



                int offset = 4;



                if (offset + 4 > data.Length) return;







                int count = BitConverter.ToInt32(data, offset);



                offset += 4;







                for (int i = 0; i < count; i++)



                {



                    if (offset + 16 > data.Length) break;



                    int nameLen = BitConverter.ToInt32(data, offset);



                    offset += 4;



                    offset += nameLen * 2;



                    offset += 8; // cloudId







                    if (offset + 16 > data.Length) break;



                    byte[] guidBytes = new byte[16];



                    Array.Copy(data, offset, guidBytes, 0, 16);



                    Guid cGuid = new Guid(guidBytes);



                    string cGuidHex = cGuid.ToString("N").ToUpperInvariant();



                    offset += 16;







                    int ftOffset = offset;



                    offset += 8;



                    offset += 8;



                    int sizeOffset = offset;



                    offset += 8;







                    if (cGuidHex.Equals(containerFolderName, StringComparison.OrdinalIgnoreCase))



                    {



                        byte[] sizeBytes = BitConverter.GetBytes(totalSize);



                        Array.Copy(sizeBytes, 0, data, sizeOffset, 8);







                        long fileTime = DateTime.UtcNow.ToFileTimeUtc();



                        byte[] ftBytes = BitConverter.GetBytes(fileTime);



                        Array.Copy(ftBytes, 0, data, ftOffset, 8);







                        File.WriteAllBytes(containersIndex, data);



                        break;



                    }



                }



            }



            catch {}



        }







        private void EnableAllDlcs()



        {



            foreach (var dlc in AvailableDlcs)



            {



                dlc.IsEnabled = true;



            }



            UpdateManifestDlcs();



        }







        private void DisableAllDlcs()



        {



            foreach (var dlc in AvailableDlcs)



            {



                dlc.IsEnabled = false;



            }



            UpdateManifestDlcs();



        }







        private void LoadDlcsFromManifest()



        {



            AvailableDlcs.Clear();



            if (_handle == IntPtr.Zero) return;







            string? xml = NativeMethods.GetManifestXmlString(_handle);



            if (string.IsNullOrEmpty(xml)) return;







            var declaredDlcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);



            var matches = Regex.Matches(xml, @"<C_DLCDescription\s+Id=""([^""]+)""");



            foreach (Match m in matches)



            {



                declaredDlcs.Add(m.Groups[1].Value);



            }







            var knownDlcs = new (string Id, string NameDe, string NameEn, string DescDe, string DescEn)[]
            {
                ("QuestForValor", "Das Wappen des Löwen (Quest for Valor)", "The Lion's Crest (Quest for Valor)", "Bonus-Questreihe, Brunclík-Rüstungsset, Waffen & Pferdeschabracke", "Bonus questline, Knight Brunclík armor set, weapons & horse caparison"),
                ("SeasonPassShields", "Schilde der Jahreszeiten", "Shields of Seasons Passing", "Vier einzigartige Schilde mit saisonaler Heraldik (Frühling, Sommer, Herbst, Winter)", "Four unique seasonal shield heraldries (Spring, Summer, Autumn, Winter)"),
                ("GoldEditionHuntsman", "Galantes Jäger-Set", "Gallant Huntsman's Kit", "St.-Hubertus-Jagdmütze, Meister-Armbrust & Nimrod-Jagdgewand", "St. Hubert hunting cap, Master crossbow, & Nimrod hunting coat"),
                ("BrushesWithDeath", "Pinsel mit dem Tod (Brushes with Death)", "Brushes with Death", "Story-Erweiterung 1: Eine geheimnisvolle Verschwörung im böhmischen Umland", "Story Expansion 1: A mysterious intrigue across Bohemian countryside"),
                ("LegacyOfTheForge", "Das Erbe der Schmiede (Legacy of the Forge)", "Legacy of the Forge", "Story-Erweiterung 2: Heinrichs Meisterprüfung und legendäre Meisterschmiedekunst", "Story Expansion 2: Henry's master trials and legendary master blacksmithing"),
                ("MysteriaEcclesiae", "Geheimnisse der Kirche (Mysteria Ecclesiae)", "Mysteria Ecclesiae", "Story-Erweiterung 3: Schattenhafte Mysterien in den Klöstern von Sedlec und Sasau", "Story Expansion 3: Shadowy mysteries within Sedlec and Sasau monasteries")
            };

            foreach (var (id, nameDe, nameEn, descDe, descEn) in knownDlcs)
            {
                bool isEnabled = declaredDlcs.Contains(id);
                var item = new DlcItemViewModel(id, nameDe, nameEn, descDe, descEn, isEnabled, UpdateManifestDlcs);
                AvailableDlcs.Add(item);
                declaredDlcs.Remove(id);
            }

            foreach (var id in declaredDlcs)
            {
                var item = new DlcItemViewModel(id, id, id, "Erweiterungspaket / Zusätzlicher Inhalt", "Expansion / Additional content package", true, UpdateManifestDlcs);
                AvailableDlcs.Add(item);
            }

            OnPropertyChanged(nameof(Loc_DlcActiveCountText));



        }







        private void UpdateManifestDlcs()



        {



            if (_handle == IntPtr.Zero) return;







            string? xml = NativeMethods.GetManifestXmlString(_handle);



            if (string.IsNullOrEmpty(xml)) return;







            var activeIds = AvailableDlcs.Where(x => x.IsEnabled).Select(x => x.Id).ToList();







            string updatedXml;



            if (activeIds.Count == 0)



            {



                updatedXml = Regex.Replace(xml, @"\s*<DLCs>[\s\S]*?</DLCs>", string.Empty);



            }



            else



            {



                var sb = new StringBuilder();



                sb.AppendLine("\t<DLCs>");



                foreach (var id in activeIds)



                {



                    sb.AppendLine($"\t\t<C_DLCDescription Id=\"{id}\" />");



                }



                sb.Append("\t</DLCs>");



                string dlcBlock = sb.ToString();







                if (Regex.IsMatch(xml, @"<DLCs>[\s\S]*?</DLCs>"))



                {



                    updatedXml = Regex.Replace(xml, @"<DLCs>[\s\S]*?</DLCs>", dlcBlock.Trim());



                }



                else



                {



                    int idx = xml.LastIndexOf("</C_SaveGameDescription>", StringComparison.OrdinalIgnoreCase);



                    if (idx >= 0)



                    {



                        updatedXml = xml.Insert(idx, dlcBlock + "\n");



                    }



                    else



                    {



                        updatedXml = xml;



                    }



                }



            }







            NativeMethods.kcd2_save_set_manifest_xml(_handle, updatedXml);
            IsDirty = true;
            OnPropertyChanged(nameof(Loc_DlcActiveCountText));

            StatusMessage = IsGerman
                ? $"DLC-Anforderungen aktualisiert ({activeIds.Count} aktiv). Klicke auf 'Speichern & Signieren'."
                : $"Updated DLC requirements ({activeIds.Count} active). Click 'Save & Sign' to write.";
        }

        private void AddCustomDlc()
        {
            if (string.IsNullOrWhiteSpace(CustomDlcId)) return;
            string id = CustomDlcId.Trim();
            var existing = AvailableDlcs.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.IsEnabled = true;
            }
            else
            {
                var item = new DlcItemViewModel(id, id, id, "Benutzerdefiniertes Erweiterungspaket / Mod-DLC", "Custom Expansion Package / Mod DLC", true, UpdateManifestDlcs);
                AvailableDlcs.Add(item);
            }
            CustomDlcId = string.Empty;
            UpdateManifestDlcs();
        }







        private void ImportWhs()



        {



            if (string.IsNullOrEmpty(SaveFilePath)) return;







            var dialog = new OpenFileDialog



            {



                Filter = "KCD2 Save Games (*.whs)|*.whs|All files (*.*)|*.*",



                Title = IsGerman ? "Wähle .whs Spielstand zum Importieren in den aktuellen Slot" : "Select .whs save to import into current slot"



            };







            if (dialog.ShowDialog() == true)



            {



                try



                {



                    string backupPath = SaveFilePath + $".pre_import_{DateTime.Now:yyyyMMdd_HHmmss}.bak";



                    File.Copy(SaveFilePath, backupPath, true);



                    File.Copy(dialog.FileName, SaveFilePath, true);



                    LoadSaveFile(SaveFilePath);







                    StatusMessage = IsGerman



                        ? $"'{Path.GetFileName(dialog.FileName)}' in Slot importiert. Backup: {Path.GetFileName(backupPath)}"



                        : $"Imported '{Path.GetFileName(dialog.FileName)}' into slot. Backup: {Path.GetFileName(backupPath)}";



                    MessageBox.Show(



                        IsGerman



                            ? $"'{Path.GetFileName(dialog.FileName)}' erfolgreich in diesen Slot importiert!\n\nEin Sicherheitsbackup wurde erstellt:\n{Path.GetFileName(backupPath)}"



                            : $"Successfully imported '{Path.GetFileName(dialog.FileName)}' into this slot!\n\nA safety backup was created:\n{Path.GetFileName(backupPath)}",



                        IsGerman ? "Import erfolgreich" : "Import Successful",



                        MessageBoxButton.OK,



                        MessageBoxImage.Information);



                }



                catch (Exception ex)



                {



                    MessageBox.Show(



                        IsGerman ? $"Fehler beim Importieren des Spielstands: {ex.Message}" : $"Failed to import save: {ex.Message}",



                        IsGerman ? "Importfehler" : "Import Error",



                        MessageBoxButton.OK,



                        MessageBoxImage.Error);



                }



            }



        }







        private void ExportWhs()



        {



            if (string.IsNullOrEmpty(SaveFilePath) || _handle == IntPtr.Zero) return;







            var dialog = new SaveFileDialog



            {



                Filter = "KCD2 Save Games (*.whs)|*.whs",



                FileName = !string.IsNullOrEmpty(SaveName) ? $"{SaveName}.whs" : $"{SelectedSaveItem?.FileName ?? "kcd2_exported_save.whs"}",



                Title = IsGerman ? "Spielstand als .whs exportieren" : "Export Save as .whs"



            };







            if (dialog.ShowDialog() == true)



            {



                try



                {



                    File.Copy(SaveFilePath, dialog.FileName, true);



                    StatusMessage = IsGerman ? $"Spielstand exportiert nach: {dialog.FileName}" : $"Exported save to: {dialog.FileName}";



                    MessageBox.Show(



                        IsGerman ? $"Spielstand erfolgreich exportiert nach:\n{dialog.FileName}" : $"Save exported successfully to:\n{dialog.FileName}",



                        IsGerman ? "Export erfolgreich" : "Export Successful",



                        MessageBoxButton.OK,



                        MessageBoxImage.Information);



                }



                catch (Exception ex)



                {



                    MessageBox.Show(



                        IsGerman ? $"Fehler beim Exportieren des Spielstands: {ex.Message}" : $"Failed to export save: {ex.Message}",



                        IsGerman ? "Exportfehler" : "Export Error",



                        MessageBoxButton.OK,



                        MessageBoxImage.Error);



                }



            }



        }







        private void AddGroschen(object? param)



        {



            if (param != null && long.TryParse(param.ToString(), out long add))



            {



                Groschen = Math.Max(0, Groschen + add);



            }



        }







        private void SetGroschen(object? param)



        {



            if (param != null && long.TryParse(param.ToString(), out long val))



            {



                Groschen = Math.Max(0, val);



            }



        }







        private void CloseCurrentSession()



        {



            if (_handle != IntPtr.Zero)



            {



                NativeMethods.kcd2_save_close(_handle);



                _handle = IntPtr.Zero;



            }



            AvailableDlcs.Clear();



            InventoryItems.Clear();



            HorseItems.Clear();



            HorseName = IsGerman ? "Kein Pferd" : "No Horse";



            HasHorse = false;



            _selectedHorseOption = null;



            OnPropertyChanged(nameof(SelectedHorseOption));



            InitializeQuests();



            IsDirty = false;



        }







        private void LoadInventoryItems()



        {



            InventoryItems.Clear();



            HorseItems.Clear();



            if (_handle == IntPtr.Zero) return;







            if (NativeMethods.kcd2_save_get_inventory_count(_handle, out uint count) == Kcd2Result.Ok && count > 0)



            {



                var buffer = new Kcd2InventoryItem[count];



                if (NativeMethods.kcd2_save_get_inventory_items(_handle, buffer, count, out uint retrieved) == Kcd2Result.Ok)



                {



                    for (int i = 0; i < retrieved; ++i)



                    {



                        var itemVm = new InventoryItemViewModel(buffer[i], OnInventoryItemChanged);



                        InventoryItems.Add(itemVm);



                        if (itemVm.SlotId >= 100 || itemVm.Category == "Pferd")



                        {



                            HorseItems.Add(itemVm);



                        }



                    }



                }



            }







            UpdateCategoryTabCounts();



            if (SelectedInventoryItem == null && InventoryItems.Count > 0)



            {



                SelectedInventoryItem = InventoryItems[0];



            }



        }







        private void OnInventoryItemChanged(InventoryItemViewModel item)



        {



            if (_handle == IntPtr.Zero) return;



            var native = item.ToNativeItem();



            NativeMethods.kcd2_save_set_inventory_item(_handle, in native);







            if (item.ItemId.Equals("5ef63059-322e-4e1b-abe8-926e100c770e", StringComparison.OrdinalIgnoreCase))



            {



                _groschen = item.Amount;



                OnPropertyChanged(nameof(Groschen));



            }



        }







        private void MaxHorseStats()



        {



            if (!HasHorse) return;



            HorseHealth = HorseMaxHealth;



            HorseStamina = HorseMaxStamina;



            StatusMessage = IsGerman



                ? $"Pferd ({HorseName}) Gesundheit & Ausdauer voll aufgeladen!"



                : $"Horse ({HorseName}) Health & Stamina fully restored!";



        }







        private void MaxAllStats()



        {



            Strength = 30;



            Agility = 30;



            Vitality = 30;



            Speech = 30;



            Speed = 30;



            Charisma = 20;



            CharacterLevel = 30;



            Health = 100.0f;



            MaxHealth = 100.0f;



            Stamina = 100.0f;



            MaxStamina = 100.0f;



            Energy = 100.0f;



            Nourishment = 100.0f;



            Groschen = Math.Max(Groschen, 980419);



            if (HasHorse)



            {



                HorseHealth = HorseMaxHealth;



                HorseStamina = HorseMaxStamina;



            }



            StatusMessage = IsGerman



                ? "Alle Attribute und Vitalwerte von Heinrich & Pferd maximiert!"



                : "All attributes and vitals of Henry & horse maximized!";



        }







        #endregion







        #region New Prototype Utilities & Initializers







        private void InitializeCategoryTabs()



        {



            CategoryTabs.Clear();



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Alle", LabelDe = "Alle", LabelEn = "All", Icon = "▦", AccentBrush = "#8C1D24", IsSelected = true, UiKey = "All" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Waffen", LabelDe = "Waffen", LabelEn = "Weapons", Icon = "⚔️", AccentBrush = "#3B4252", UiKey = "Weapons" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Rüstung", LabelDe = "Rüstung", LabelEn = "Armor", Icon = "🛡️", AccentBrush = "#92400E", UiKey = "Armour" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Nahrung", LabelDe = "Nahrung", LabelEn = "Food", Icon = "🍖", AccentBrush = "#065F46", UiKey = "Food" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Bücher", LabelDe = "Bücher", LabelEn = "Books", Icon = "📜", AccentBrush = "#854D0E", UiKey = "Books" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Materialien", LabelDe = "Materialien", LabelEn = "Materials", Icon = "🧪", AccentBrush = "#115E59", UiKey = "Herbs" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Quest", LabelDe = "Quest", LabelEn = "Quest", Icon = "⭐", AccentBrush = "#1E40AF", UiKey = "Quest" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Sonstiges", LabelDe = "Sonstiges", LabelEn = "Misc", Icon = "📦", AccentBrush = "#9A3412", UiKey = "Other" });



            CategoryTabs.Add(new CategoryTabViewModel { Id = "Pferd", LabelDe = "Pferd", LabelEn = "Horse", Icon = "🐎", AccentBrush = "#3F6212", UiKey = "Horse" });



        }







        public void SelectCategoryTab(string catId)



        {



            SelectedInventoryCategory = catId;



            foreach (var tab in CategoryTabs)



            {



                tab.IsSelected = tab.Id.Equals(catId, StringComparison.OrdinalIgnoreCase);



            }



            FilteredInventory.Refresh();







            // AUTOMATIC TARGET SWITCHING (from prototype v2)



            if (catId.Equals("Pferd", StringComparison.OrdinalIgnoreCase) || catId.Equals("Horse", StringComparison.OrdinalIgnoreCase))



            {



                SelectedInventoryTarget = "Pebbles";



            }



            else if (SelectedInventoryTarget == "Pebbles")



            {



                SelectedInventoryTarget = "Henry";



            }



        }







        public void UpdateCategoryTabCounts()



        {



            var baseList = SelectedStorageFilter switch



            {



                "Inventory" => InventoryItems.Where(i => i.StorageLocation == ItemStorageLocation.Inventory),



                "Stash" => InventoryItems.Where(i => i.StorageLocation == ItemStorageLocation.Stash),



                "Horse" => InventoryItems.Where(i => i.StorageLocation == ItemStorageLocation.Horse),



                _ => InventoryItems



            };







            foreach (var tab in CategoryTabs)



            {



                tab.Count = baseList.Count(i => MatchesCategoryFilter(i.Category, tab.Id));



            }







            OnPropertyChanged(nameof(CountInventoryItems));



            OnPropertyChanged(nameof(CountStashItems));



            OnPropertyChanged(nameof(CountHorseItems));



            OnPropertyChanged(nameof(CountAllItems));



        }







        private void InitializePaperdoll()
        {
            LeftPaperdollSlots.Clear();
            RightPaperdollSlots.Clear();

            // LEFT SLOTS
            var helm = new PaperdollSlotViewModel("slot_helm", "Mailänder Beckenhaube", "Milanese Bascinet", "🪖", false, true, "helm", SelectPaperdollSlot);
            helm.SetOutfitPreset(1, "Mailänder Beckenhaube", "Milanese Bascinet", "🪖", true, false, "helm");
            helm.SetOutfitPreset(2, "Schwarze Kappe", "Dark Padded Cap", "🧢", true, false, "cap");
            helm.SetOutfitPreset(3, "Roter Samthut", "Red Velvet Cap", "🎩", true, false, "hat");
            LeftPaperdollSlots.Add(helm);

            var coif = new PaperdollSlotViewModel("slot_coif", "Kettengugel", "Chainmail Coif", "🛡️", false, true, "coif", SelectPaperdollSlot);
            coif.SetOutfitPreset(1, "Kettengugel", "Chainmail Coif", "🛡️", true, false, "coif");
            coif.SetOutfitPreset(2, "Dunkle Gugel", "Dark Coif", "🧣", true, false, "dark_coif");
            coif.SetOutfitPreset(3, "— Ablegen / Leer —", "— Unequipped / Empty —", "🛡️", false, false, "");
            LeftPaperdollSlots.Add(coif);

            var hood = new PaperdollSlotViewModel("slot_hood", "Bestickte Gugel", "Embroidered Hood", "🧣", true, true, "hood", SelectPaperdollSlot);
            hood.SetOutfitPreset(1, "Bestickte Gugel", "Embroidered Hood", "🧣", true, true, "hood");
            hood.SetOutfitPreset(2, "Jäger-Kapuze", "Hunter's Hood", "🥷", true, false, "hunter_hood");
            hood.SetOutfitPreset(3, "Goldene Seidengugel", "Gold Silk Chaperon", "🧣", true, false, "silk_hood");
            LeftPaperdollSlots.Add(hood);

            var gorget = new PaperdollSlotViewModel("slot_gorget", "Halsberge", "Gorget", "⭕", false, true, "gorget", SelectPaperdollSlot);
            gorget.SetOutfitPreset(1, "Platten-Halsberge", "Plate Gorget", "⭕", true, false, "gorget");
            gorget.SetOutfitPreset(2, "Lederkragen", "Leather Collar", "⭕", true, false, "leather_collar");
            gorget.SetOutfitPreset(3, "Zierkragen", "Ornamental Collar", "⭕", true, false, "ornament_collar");
            LeftPaperdollSlots.Add(gorget);

            var ring = new PaperdollSlotViewModel("slot_ring", "Siegelring", "Signet Ring", "💍", false, true, "ring", SelectPaperdollSlot);
            ring.SetOutfitPreset(1, "Ritterlicher Siegelring", "Knightly Signet Ring", "💍", true, false, "ring");
            ring.SetOutfitPreset(2, "Schleichring", "Ring of Silence", "💍", true, false, "stealth_ring");
            ring.SetOutfitPreset(3, "Rubin-Siegelring", "Ruby Signet Ring", "💍", true, false, "ruby_ring");
            LeftPaperdollSlots.Add(ring);

            var necklace = new PaperdollSlotViewModel("slot_necklace", "Amulett", "Amulet", "🎖️", false, true, "necklace", SelectPaperdollSlot);
            necklace.SetOutfitPreset(1, "Krieger-Amulett", "Warrior's Amulet", "🎖️", true, false, "necklace");
            necklace.SetOutfitPreset(2, "Wolfszahn-Talisman", "Wolf Tooth Talisman", "🎖️", true, false, "wolf_tooth");
            necklace.SetOutfitPreset(3, "Edles Goldamulett", "Noble Gold Necklace", "🎖️", true, false, "gold_amulet");
            LeftPaperdollSlots.Add(necklace);

            var dagger = new PaperdollSlotViewModel("slot_dagger", "Dolch", "Dagger", "🗡️", false, true, "dagger", SelectPaperdollSlot);
            dagger.SetOutfitPreset(1, "Panzerbrecher-Dolch", "Misericorde", "🗡️", true, false, "dagger");
            dagger.SetOutfitPreset(2, "Jagddolch", "Hunting Dagger", "🗡️", true, false, "hunt_dagger");
            dagger.SetOutfitPreset(3, "Prunkdolch", "Ornate Dagger", "🗡️", true, false, "ornate_dagger");
            LeftPaperdollSlots.Add(dagger);

            var crossbow = new PaperdollSlotViewModel("slot_crossbow", "Armbrust", "Crossbow", "🏹", true, true, "crossbow", SelectPaperdollSlot);
            crossbow.SetOutfitPreset(1, "Schwere Kriegsarmbrust", "Heavy War Crossbow", "🏹", true, true, "crossbow");
            crossbow.SetOutfitPreset(2, "Eiben-Jagdbogen", "Yew Hunting Bow", "🏹", true, false, "bow");
            crossbow.SetOutfitPreset(3, "Verzierte Armbrust", "Decorated Crossbow", "🏹", true, false, "dec_crossbow");
            LeftPaperdollSlots.Add(crossbow);

            var sword = new PaperdollSlotViewModel("slot_sword", "Radzigs Schwert", "Radzig's Sword", "⚔️", true, true, "radzig", SelectPaperdollSlot);
            sword.SetOutfitPreset(1, "Radzigs Langschwert", "Radzig's Longsword", "⚔️", true, true, "radzig");
            sword.SetOutfitPreset(2, "Jagdschwert", "Hunting Sword", "⚔️", true, false, "hunt_sword");
            sword.SetOutfitPreset(3, "Turnierschwert", "Tournament Longsword", "⚔️", true, false, "tourney_sword");
            LeftPaperdollSlots.Add(sword);

            var shield = new PaperdollSlotViewModel("slot_shield", "Schild", "Shield", "🛡️", false, true, "shield", SelectPaperdollSlot);
            shield.SetOutfitPreset(1, "Skalitzer Wappenschild", "Skalitz Heraldic Shield", "🛡️", true, false, "shield");
            shield.SetOutfitPreset(2, "— Ablegen / Leer —", "— Unequipped / Empty —", "🛡️", false, false, "");
            shield.SetOutfitPreset(3, "Prunkschild", "Ceremonial Shield", "🛡️", true, false, "ceremonial_shield");
            LeftPaperdollSlots.Add(shield);

            var handgonne = new PaperdollSlotViewModel("slot_handgonne", "Handkanone", "Hand Cannon", "💥", false, true, "handgonne", SelectPaperdollSlot);
            handgonne.SetOutfitPreset(1, "Gusseiserne Handkanone", "Cast Iron Handgonne", "💥", true, false, "handgonne");
            handgonne.SetOutfitPreset(2, "— Ablegen / Leer —", "— Unequipped / Empty —", "💥", false, false, "");
            handgonne.SetOutfitPreset(3, "Prunk-Handkanone", "Ornate Handgonne", "💥", true, false, "ornate_handgonne");
            LeftPaperdollSlots.Add(handgonne);

            var bullet = new PaperdollSlotViewModel("slot_bullet", "Kugel (24)", "Bullets (24)", "⚪", false, true, "bullet", SelectPaperdollSlot);
            bullet.SetOutfitPreset(1, "Blei-Kugeln (24)", "Lead Bullets (24)", "⚪", true, false, "bullet");
            bullet.SetOutfitPreset(2, "Jagdpfeile (30)", "Hunting Arrows (30)", "🏹", true, false, "arrow");
            bullet.SetOutfitPreset(3, "Silberkugeln (12)", "Silver Bullets (12)", "⚪", true, false, "silver_bullet");
            LeftPaperdollSlots.Add(bullet);

            // RIGHT SLOTS
            var coat = new PaperdollSlotViewModel("slot_coat", "Waffenrock", "Noble Jupon", "🥋", true, true, "coat", SelectPaperdollSlot);
            coat.SetOutfitPreset(1, "Skalitzer Waffenrock", "Skalitz Jupon", "🥋", true, true, "coat");
            coat.SetOutfitPreset(2, "Dunkler Wams", "Dark Padded Doublet", "🥋", true, false, "dark_wams");
            coat.SetOutfitPreset(3, "Brokat-Waffenrock", "Brocade Jupon", "🥋", true, false, "brocade_jupon");
            RightPaperdollSlots.Add(coat);

            var gambeson = new PaperdollSlotViewModel("slot_gambeson", "Gambeson", "Gambeson", "🧥", false, true, "gambeson", SelectPaperdollSlot);
            gambeson.SetOutfitPreset(1, "Gepolsterter Gambeson", "Padded Gambeson", "🧥", true, false, "gambeson");
            gambeson.SetOutfitPreset(2, "Leichtes Steppwams", "Light Gambeson", "🧥", true, false, "light_gambeson");
            gambeson.SetOutfitPreset(3, "Seiden-Pourpoint", "Silk Pourpoint", "🧥", true, false, "silk_pourpoint");
            RightPaperdollSlots.Add(gambeson);

            var cuirass = new PaperdollSlotViewModel("slot_cuirass", "Mailänder Kürass", "Milanese Cuirass", "🛡️", true, true, "cuirass", SelectPaperdollSlot);
            cuirass.SetOutfitPreset(1, "Mailänder Kürass", "Milanese Cuirass", "🛡️", true, true, "cuirass");
            cuirass.SetOutfitPreset(2, "Gehärteter Lederharnisch", "Hardened Leather Jerkin", "🛡️", true, false, "leather_cuirass");
            cuirass.SetOutfitPreset(3, "Gravierter Kürass", "Engraved Cuirass", "🛡️", true, false, "engraved_cuirass");
            RightPaperdollSlots.Add(cuirass);

            var arms = new PaperdollSlotViewModel("slot_arms", "Armkacheln", "Arm Vambraces", "🦾", false, true, "arms", SelectPaperdollSlot);
            arms.SetOutfitPreset(1, "Mailänder Armkacheln", "Milanese Arm Vambraces", "🦾", true, false, "arms");
            arms.SetOutfitPreset(2, "Lederarmschützer", "Leather Vambraces", "🦾", true, false, "leather_arms");
            arms.SetOutfitPreset(3, "Zier-Armschützer", "Decorated Vambraces", "🦾", true, false, "dec_arms");
            RightPaperdollSlots.Add(arms);

            var gauntlets = new PaperdollSlotViewModel("slot_gauntlets", "Handschuhe", "Gauntlets", "🧤", true, true, "gauntlets", SelectPaperdollSlot);
            gauntlets.SetOutfitPreset(1, "Plattenhandschuhe", "Plate Gauntlets", "🧤", true, true, "gauntlets");
            gauntlets.SetOutfitPreset(2, "Lederhandschuhe", "Leather Gloves", "🧤", true, false, "leather_gloves");
            gauntlets.SetOutfitPreset(3, "Samthandschuhe", "Velvet-lined Gloves", "🧤", true, false, "velvet_gloves");
            RightPaperdollSlots.Add(gauntlets);

            var hose = new PaperdollSlotViewModel("slot_hose", "Beinlinge", "Hose", "👖", false, true, "hose", SelectPaperdollSlot);
            hose.SetOutfitPreset(1, "Wollene Beinlinge", "Woolen Hose", "👖", true, false, "hose");
            hose.SetOutfitPreset(2, "Dunkle Beinkleider", "Dark Tight Hose", "👖", true, false, "dark_hose");
            hose.SetOutfitPreset(3, "Rote Seidenhosen", "Crimson Noble Hose", "👖", true, false, "silk_hose");
            RightPaperdollSlots.Add(hose);

            var cuisse = new PaperdollSlotViewModel("slot_cuisse", "Plattenbeine", "Plate Leggings", "🦵", true, true, "cuisse", SelectPaperdollSlot);
            cuisse.SetOutfitPreset(1, "Mailänder Plattenbeine", "Milanese Plate Leggings", "🦵", true, true, "cuisse");
            cuisse.SetOutfitPreset(2, "— Ablegen / Leer —", "— Unequipped / Empty —", "🦵", false, false, "");
            cuisse.SetOutfitPreset(3, "Polierte Beinplatten", "Polished Greaves", "🦵", true, false, "polished_cuisse");
            RightPaperdollSlots.Add(cuisse);

            var paddedLegs = new PaperdollSlotViewModel("slot_padded_legs", "Polsterbeine", "Padded Chausses", "🦿", true, true, "padded", SelectPaperdollSlot);
            paddedLegs.SetOutfitPreset(1, "Gepolsterte Chausses", "Padded Chausses", "🦿", true, true, "padded");
            paddedLegs.SetOutfitPreset(2, "Geräuscharme Polster", "Silent Padded Hose", "🦿", true, false, "silent_padded");
            paddedLegs.SetOutfitPreset(3, "Feine Polsterbeine", "Fine Chausses", "🦿", true, false, "fine_padded");
            RightPaperdollSlots.Add(paddedLegs);

            var boots = new PaperdollSlotViewModel("slot_boots", "Adelsstiefel", "Noble Boots", "👢", true, true, "boots", SelectPaperdollSlot);
            boots.SetOutfitPreset(1, "Ritterliche Reitstiefel", "Knightly Riding Boots", "👢", true, true, "boots");
            boots.SetOutfitPreset(2, "Pirschstiefel", "Silent Stalker Boots", "👢", true, false, "stalker_boots");
            boots.SetOutfitPreset(3, "Corduan-Reiterstiefel", "Cordovan Riding Boots", "👢", true, false, "cordovan_boots");
            RightPaperdollSlots.Add(boots);

            var spurs = new PaperdollSlotViewModel("slot_spurs", "Goldsporen", "Golden Spurs", "✨", false, true, "spurs", SelectPaperdollSlot);
            spurs.SetOutfitPreset(1, "Goldsporen", "Golden Spurs", "✨", true, false, "spurs");
            spurs.SetOutfitPreset(2, "— Ablegen / Leer —", "— Unequipped / Empty —", "✨", false, false, "");
            spurs.SetOutfitPreset(3, "Goldene Turniersporen", "Knightly Gold Spurs", "✨", true, false, "tourney_spurs");
            RightPaperdollSlots.Add(spurs);

            var torch = new PaperdollSlotViewModel("slot_torch", "Fackel", "Torch", "🔥", false, true, "torch", SelectPaperdollSlot);
            torch.SetOutfitPreset(1, "Pechfackel", "Pitch Torch", "🔥", true, false, "torch");
            torch.SetOutfitPreset(2, "Dunkle Laterne", "Dark Lantern", "🔥", true, false, "lantern");
            torch.SetOutfitPreset(3, "Wachsfackel", "Wax Torch", "🔥", true, false, "wax_torch");
            RightPaperdollSlots.Add(torch);

            var pouch = new PaperdollSlotViewModel("slot_pouch", "Retterschnaps", "Saviour Schnapps", "🧪", false, true, "schnapps", SelectPaperdollSlot);
            pouch.SetOutfitPreset(1, "Retterschnaps", "Saviour Schnapps", "🧪", true, false, "schnapps");
            pouch.SetOutfitPreset(2, "Schlafgift-Fläschchen", "Bane Poison Vial", "🧪", true, false, "poison");
            pouch.SetOutfitPreset(3, "Lazarus-Trank", "Lazarus Potion", "🧪", true, false, "lazarus");
            RightPaperdollSlots.Add(pouch);

            ApplyOutfit(ActiveOutfit);
            RecalculateCharacterStats();
        }

        private void InitializeHorseEquipment()
        {
            HorseEquipmentSlots.Clear();

            // 1. Sattel / Saddle
            var saddle = new PaperdollSlotViewModel("horse_saddle", "Rittersattel (4 Taschen)", "Knight's Saddle (4 Bags)", "🏇", false, true, "saddle_knight", SelectPaperdollSlot);
            saddle.SetOutfitPreset(1, "Rittersattel (4 Taschen)", "Knight's Saddle (4 Bags)", "🏇", true, false, "saddle_knight");
            saddle.SetOutfitPreset(2, "Leichter Rennsattel", "Light Racing Saddle", "🏇", true, false, "saddle_racing");
            saddle.SetOutfitPreset(3, "Edler Prunksattel (4 Taschen)", "Noble Splendid Saddle (4 Bags)", "🏇", true, false, "82242945-dcc8-4b6d-aec9-e12b80ff6f29");
            HorseEquipmentSlots.Add(saddle);

            // 2. Zaumzeug / Bridle
            var bridle = new PaperdollSlotViewModel("horse_bridle", "Ritterzaumzeug", "Knight's Bridle", "🐴", false, true, "bridle_knight", SelectPaperdollSlot);
            bridle.SetOutfitPreset(1, "Ritterzaumzeug", "Knight's Bridle", "🐴", true, false, "bridle_knight");
            bridle.SetOutfitPreset(2, "Leichtes Lederzaumzeug", "Light Leather Bridle", "🐴", true, false, "596212ea-ce9b-4b8c-af09-278ae2d834bb");
            bridle.SetOutfitPreset(3, "Edles Zaumzeug", "Noble Bridle", "🐴", true, false, "68204d36-5d4d-412e-9b37-8bce360952f3");
            HorseEquipmentSlots.Add(bridle);

            // 3. Schabracke / Caparison
            var caparison = new PaperdollSlotViewModel("horse_caparison", "Gesteppte Schabracke", "Padded Caparison", "🛡️", false, true, "36694031-e85d-4547-8054-5e67b51aa8fe", SelectPaperdollSlot);
            caparison.SetOutfitPreset(1, "Gesteppte Schabracke", "Padded Caparison", "🛡️", true, false, "36694031-e85d-4547-8054-5e67b51aa8fe");
            caparison.SetOutfitPreset(2, "Leichte Reisedecke", "Light Travel Blanket", "🛡️", true, false, "caparison_light");
            caparison.SetOutfitPreset(3, "Bestickte Prunkschabracke", "Embroidered Noble Caparison", "🛡️", true, false, "caparison_noble");
            HorseEquipmentSlots.Add(caparison);

            // 4. Hufeisen / Horseshoes
            var shoes = new PaperdollSlotViewModel("horse_horseshoes", "Militär-Hufeisen", "Military Horseshoes", "🧲", false, true, "0faf833f-8e88-40a0-87b8-2669c0e64c03", SelectPaperdollSlot);
            shoes.SetOutfitPreset(1, "Militär-Hufeisen", "Military Horseshoes", "🧲", true, false, "0faf833f-8e88-40a0-87b8-2669c0e64c03");
            shoes.SetOutfitPreset(2, "Edle Hufeisen", "Noble Horseshoes", "🧲", true, false, "5ded1ff4-9f81-4179-bb65-f786e6e80560");
            shoes.SetOutfitPreset(3, "Vergoldete Hufeisen", "Gilded Horseshoes", "🧲", true, false, "horseshoes_gilded");
            HorseEquipmentSlots.Add(shoes);

            // 5. Rossharnisch / Chanfron
            var chanfron = new PaperdollSlotViewModel("horse_chanfron", "Ketten-Rossharnisch", "Mail Chanfron", "🪖", false, true, "911df0af-e8c8-4ff6-b487-a13f29cefe38", SelectPaperdollSlot);
            chanfron.SetOutfitPreset(1, "Ketten-Rossharnisch", "Mail Chanfron", "🪖", true, false, "911df0af-e8c8-4ff6-b487-a13f29cefe38");
            chanfron.SetOutfitPreset(2, "— Ablegen / Leer —", "— Unequipped / Empty —", "🪖", false, false, "");
            chanfron.SetOutfitPreset(3, "Gravierte Rossstirn", "Engraved Chanfron", "🪖", true, false, "chanfron_engraved");
            HorseEquipmentSlots.Add(chanfron);

            // 6. Satteltaschen / Saddlebags
            var saddlebags = new PaperdollSlotViewModel("horse_saddlebags", "Schwere Streitross-Packtaschen", "Heavy Warhorse Saddlebags", "🧳", false, true, "bags_heavy", SelectPaperdollSlot);
            saddlebags.SetOutfitPreset(1, "Schwere Streitross-Packtaschen", "Heavy Warhorse Saddlebags", "🧳", true, false, "bags_heavy");
            saddlebags.SetOutfitPreset(2, "Leichte Jagdtaschen", "Light Hunting Bags", "🧳", true, false, "bags_light");
            saddlebags.SetOutfitPreset(3, "Edle Leder-Doppeltaschen", "Noble Leather Saddlebags", "🧳", true, false, "bags_noble");
            HorseEquipmentSlots.Add(saddlebags);

            ApplyHorseOutfit(ActiveHorseOutfit);
            RecalculateHorseStats();
        }

        private void SelectPaperdollSlot(PaperdollSlotViewModel slot)
        {
            if (slot == null) return;

            foreach (var s in LeftPaperdollSlots) s.IsSelected = (s == slot);
            foreach (var s in RightPaperdollSlots) s.IsSelected = (s == slot);
            foreach (var s in HorseEquipmentSlots) s.IsSelected = (s == slot);

            SelectedPaperdollSlot = slot;
            SlotPickerSearchText = string.Empty;
            SlotPickerSelectedSource = "All";

            EnsureCodexItemsLoaded();
            PopulateSlotPickerItems();

            IsSlotPickerModalOpen = true;
            StatusMessage = IsGerman
                ? $"Ausrüstungsslot '{slot.DisplayLabel}' geöffnet."
                : $"Equipment slot '{slot.DisplayLabel}' opened.";
        }

        private void PopulateSlotPickerItems()
        {
            SlotPickerItems.Clear();
            if (SelectedPaperdollSlot == null) return;

            string slotId = SelectedPaperdollSlot.Id;
            string search = (SlotPickerSearchText ?? string.Empty).Trim().ToLowerInvariant();
            bool sourceAll = SlotPickerSelectedSource.Equals("All", StringComparison.OrdinalIgnoreCase);

            var addedGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Items from Player or Horse Inventory
            bool isHorseSlot = slotId.StartsWith("horse_");
            foreach (var item in InventoryItems)
            {
                if (isHorseSlot)
                {
                    if (item.StorageLocation != ItemStorageLocation.Inventory && item.StorageLocation != ItemStorageLocation.Horse)
                        continue;
                }
                else
                {
                    if (item.StorageLocation != ItemStorageLocation.Inventory)
                        continue;
                }

                if (!IsItemMatchingSlot(slotId, item.ItemUiName, item.Category, item.SubCategory, item.Category, string.Empty))
                    continue;

                if (!string.IsNullOrEmpty(search) &&
                    !item.ItemUiName.ToLowerInvariant().Contains(search) &&
                    !item.SubCategory.ToLowerInvariant().Contains(search) &&
                    !item.Category.ToLowerInvariant().Contains(search))
                    continue;

                addedGuids.Add(item.ItemId);
                SlotPickerItems.Add(new SlotPickerItemViewModel
                {
                    ItemId = item.ItemId,
                    Name = item.ItemUiName,
                    Category = item.Category,
                    SubCategory = item.SubCategory,
                    Icon = item.Glyph,
                    Weight = item.Weight,
                    Price = item.Price,
                    Power = item.Power,
                    Tier = item.Tier,
                    Condition = item.Condition,
                    IsInPlayerInventory = true,
                    Amount = item.Amount
                });
            }

            // 2. Items from Game Codex
            if (sourceAll)
            {
                EnsureCodexItemsLoaded();
                foreach (var codex in AllCodexItems)
                {
                    if (addedGuids.Contains(codex.Guid)) continue;

                    if (!IsItemMatchingSlot(slotId, codex.DisplayName, codex.Category, string.Empty, codex.RawCategory, codex.Tag))
                        continue;

                    if (!string.IsNullOrEmpty(search) &&
                        !codex.DisplayName.ToLowerInvariant().Contains(search) &&
                        !codex.Category.ToLowerInvariant().Contains(search) &&
                        !codex.Tag.ToLowerInvariant().Contains(search))
                        continue;

                    SlotPickerItems.Add(new SlotPickerItemViewModel
                    {
                        ItemId = codex.Guid,
                        Name = codex.DisplayName,
                        Category = codex.Category,
                        SubCategory = codex.RawCategory,
                        Icon = codex.Icon,
                        Weight = codex.Weight,
                        Price = codex.Price,
                        Power = 100,
                        Tier = 2,
                        Condition = 1.0f,
                        IsInPlayerInventory = false,
                        Amount = 1
                    });
                }
            }

            int count = SlotPickerItems.Count;
            SlotPickerItemCountText = IsGerman
                ? $"{count} Gegenstände verfügbar"
                : $"{count} items available";

            if (SlotPickerItems.Count > 0)
            {
                SelectedSlotPickerItem = SlotPickerItems.FirstOrDefault(i =>
                    !string.IsNullOrEmpty(SelectedPaperdollSlot.LinkedItemId) &&
                    i.ItemId.Equals(SelectedPaperdollSlot.LinkedItemId, StringComparison.OrdinalIgnoreCase))
                    ?? SlotPickerItems[0];
            }
            else
            {
                SelectedSlotPickerItem = null;
            }
        }

        private bool IsItemMatchingSlot(string slotId, string name, string category, string subCategory, string rawCategory, string tag)
        {
            string lowerName = (name ?? string.Empty).ToLowerInvariant();
            string lowerTag = (tag ?? string.Empty).ToLowerInvariant();
            string lowerCat = (category ?? string.Empty).ToLowerInvariant();
            string lowerSub = (subCategory ?? string.Empty).ToLowerInvariant();

            return slotId switch
            {
                "slot_helm" => lowerCat.Contains("rüst") || lowerCat.Contains("armour")
                    ? (lowerSub.Contains("kopf") || lowerSub.Contains("head") || lowerName.Contains("helm") || lowerName.Contains("helmet") || lowerName.Contains("bascinet") || lowerName.Contains("beckenhaube") || lowerName.Contains("kettle") || lowerName.Contains("kesselhut") || lowerName.Contains("schaller") || lowerName.Contains("sallet") || lowerName.Contains("armet") || lowerName.Contains("hut") || lowerName.Contains("hat") || lowerName.Contains("cap") || lowerName.Contains("mütze") || lowerTag.Contains("helm"))
                    : (lowerName.Contains("helm") || lowerName.Contains("helmet") || lowerName.Contains("bascinet")),

                "slot_coif" => lowerName.Contains("coif") || lowerName.Contains("kettengugel") || lowerName.Contains("ringelgugel") || lowerName.Contains("mail coif") || lowerTag.Contains("coif"),

                "slot_hood" => lowerName.Contains("hood") || lowerName.Contains("gugel") || lowerName.Contains("kapuze") || lowerName.Contains("chaperon") || lowerName.Contains("bundhaube") || lowerTag.Contains("hood"),

                "slot_gorget" => lowerName.Contains("gorget") || lowerName.Contains("halsberge") || lowerName.Contains("kragen") || lowerName.Contains("collar") || lowerName.Contains("bavière") || lowerName.Contains("neck") || lowerTag.Contains("gorget"),

                "slot_ring" => lowerName.Contains("ring") || lowerName.Contains("siegelring") || lowerName.Contains("signet") || lowerTag.Contains("ring"),

                "slot_necklace" => lowerName.Contains("necklace") || lowerName.Contains("amulet") || lowerName.Contains("amulett") || lowerName.Contains("halskette") || lowerName.Contains("kette") || lowerName.Contains("cross") || lowerName.Contains("kreuz") || lowerName.Contains("talisman") || lowerName.Contains("medaillon") || lowerTag.Contains("necklace"),

                "slot_dagger" => lowerName.Contains("dagger") || lowerName.Contains("dolch") || lowerName.Contains("knife") || lowerName.Contains("messer") || lowerName.Contains("misericorde") || lowerName.Contains("stiletto") || lowerTag.Contains("dagger"),

                "slot_crossbow" => lowerName.Contains("crossbow") || lowerName.Contains("armbrust") || lowerName.Contains("bow") || lowerName.Contains("bogen") || lowerTag.Contains("bow"),

                "slot_sword" => (lowerCat.Contains("waff") || lowerCat.Contains("weapon"))
                    && !lowerName.Contains("arrow") && !lowerName.Contains("pfeil") && !lowerName.Contains("bullet") && !lowerName.Contains("kugel") && !lowerName.Contains("fackel") && !lowerName.Contains("torch") && !lowerName.Contains("bow") && !lowerName.Contains("bogen") && !lowerName.Contains("armbrust")
                    || lowerName.Contains("sword") || lowerName.Contains("schwert") || lowerName.Contains("sabre") || lowerName.Contains("säbel") || lowerName.Contains("blade") || lowerName.Contains("axe") || lowerName.Contains("axt") || lowerName.Contains("mace") || lowerName.Contains("kolben") || lowerName.Contains("hammer") || lowerName.Contains("speer") || lowerName.Contains("spear") || lowerName.Contains("halberd") || lowerName.Contains("hellebarde"),

                "slot_shield" => lowerName.Contains("shield") || lowerName.Contains("schild") || lowerName.Contains("buckler") || lowerName.Contains("tartsche") || lowerName.Contains("pavise") || lowerTag.Contains("shield"),

                "slot_handgonne" => lowerName.Contains("handgonne") || lowerName.Contains("handkanone") || lowerName.Contains("cannon") || lowerName.Contains("kanone") || lowerName.Contains("büchse") || lowerName.Contains("gun") || lowerTag.Contains("gun"),

                "slot_bullet" => lowerName.Contains("bullet") || lowerName.Contains("kugel") || lowerName.Contains("arrow") || lowerName.Contains("pfeil") || lowerName.Contains("bolt") || lowerName.Contains("bolzen") || lowerName.Contains("powder") || lowerName.Contains("pulver") || lowerName.Contains("munition") || lowerName.Contains("ammo") || lowerTag.Contains("ammo"),

                "slot_coat" => lowerName.Contains("coat") || lowerName.Contains("jupon") || lowerName.Contains("waffenrock") || lowerName.Contains("wappenrock") || lowerName.Contains("tabard") || lowerName.Contains("surcoat") || lowerName.Contains("mantel") || lowerName.Contains("cape") || lowerName.Contains("robe") || lowerTag.Contains("coat"),

                "slot_gambeson" => lowerName.Contains("gambeson") || lowerName.Contains("doublet") || lowerName.Contains("wams") || lowerName.Contains("pourpoint") || lowerName.Contains("aketon") || lowerName.Contains("stepp") || lowerTag.Contains("gambeson"),

                "slot_cuirass" => lowerName.Contains("cuirass") || lowerName.Contains("kürass") || lowerName.Contains("harnisch") || lowerName.Contains("brigandine") || lowerName.Contains("hauberk") || lowerName.Contains("kettenhemd") || lowerName.Contains("brustplatte") || lowerName.Contains("breastplate") || lowerName.Contains("panzer") || (lowerCat.Contains("rüst") && (lowerSub.Contains("körper") || lowerSub.Contains("body"))),

                "slot_arms" => lowerName.Contains("arms") || lowerName.Contains("vambrace") || lowerName.Contains("armkacheln") || lowerName.Contains("armschienen") || lowerName.Contains("couter") || lowerName.Contains("armzeug") || (lowerCat.Contains("rüst") && (lowerSub.Contains("arm"))),

                "slot_gauntlets" => lowerName.Contains("gauntlet") || lowerName.Contains("handschuh") || lowerName.Contains("handschuhe") || lowerName.Contains("gloves") || lowerName.Contains("mittens") || lowerTag.Contains("gloves"),

                "slot_hose" => lowerName.Contains("hose") || lowerName.Contains("beinlinge") || lowerName.Contains("pants") || lowerName.Contains("trousers") || lowerName.Contains("hosen") || lowerName.Contains("bruche") || lowerTag.Contains("pants"),

                "slot_cuisse" => lowerName.Contains("cuisse") || lowerName.Contains("greaves") || lowerName.Contains("plattenbeine") || lowerName.Contains("beinschienen") || lowerName.Contains("beinplatten") || lowerName.Contains("poleyn") || lowerName.Contains("beinzeug") || (lowerCat.Contains("rüst") && (lowerSub.Contains("bein") || lowerSub.Contains("leg"))),

                "slot_padded_legs" => lowerName.Contains("padded") || lowerName.Contains("polster") || lowerName.Contains("chausses") || lowerName.Contains("polsterbeine") || lowerName.Contains("steppbeine"),

                "slot_boots" => lowerName.Contains("boot") || lowerName.Contains("boots") || lowerName.Contains("stiefel") || lowerName.Contains("shoe") || lowerName.Contains("schuhe") || lowerName.Contains("sabaton") || lowerName.Contains("reiterstiefel") || lowerTag.Contains("boots"),

                "slot_spurs" => lowerName.Contains("spur") || lowerName.Contains("spurs") || lowerName.Contains("sporen") || lowerName.Contains("goldsporen") || lowerName.Contains("turniersporen"),

                "slot_torch" => lowerName.Contains("torch") || lowerName.Contains("fackel") || lowerName.Contains("laterne") || lowerName.Contains("lantern") || lowerName.Contains("licht"),

                "slot_pouch" => lowerName.Contains("schnapps") || lowerName.Contains("schnaps") || lowerName.Contains("potion") || lowerName.Contains("trank") || lowerName.Contains("retter") || lowerName.Contains("lazarus") || lowerName.Contains("salbe") || lowerName.Contains("beutel") || lowerName.Contains("tasche") || lowerName.Contains("pouch") || lowerName.Contains("gift") || lowerName.Contains("elixier"),

                "horse_saddle" => lowerName.Contains("saddle") || lowerName.Contains("sattel") || lowerTag.Contains("saddle"),

                "horse_bridle" => lowerName.Contains("bridle") || lowerName.Contains("zaum") || lowerTag.Contains("bridle"),

                "horse_caparison" => lowerName.Contains("caparison") || lowerName.Contains("schabracke") || lowerName.Contains("decke") || lowerTag.Contains("caparison"),

                "horse_horseshoes" => lowerName.Contains("horseshoe") || lowerName.Contains("hufeisen") || lowerTag.Contains("horseshoe"),

                "horse_chanfron" => lowerName.Contains("chanfron") || lowerName.Contains("chamfron") || lowerName.Contains("rossharnisch") || lowerName.Contains("rossstirn") || lowerName.Contains("pferdepanzer") || lowerName.Contains("harness") || lowerTag.Contains("horse_armor"),

                "horse_saddlebags" => lowerName.Contains("tasche") || lowerName.Contains("bag") || lowerTag.Contains("bag"),

                _ => true
            };
        }

        private void EquipSelectedSlotItem()
        {
            if (SelectedPaperdollSlot == null || SelectedSlotPickerItem == null) return;

            string name = SelectedSlotPickerItem.Name;
            string icon = SelectedSlotPickerItem.Icon;
            string itemId = SelectedSlotPickerItem.ItemId;
            float condition = SelectedSlotPickerItem.Condition;
            bool isHorseSlot = SelectedPaperdollSlot.Id.StartsWith("horse_");
            int activePreset = isHorseSlot ? ActiveHorseOutfit : ActiveOutfit;

            SelectedPaperdollSlot.EquipItem(activePreset, name, name, icon, itemId, false, condition);

            var existing = InventoryItems.FirstOrDefault(i => i.ItemId.Equals(itemId, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (isHorseSlot)
                {
                    existing.StorageLocation = ItemStorageLocation.Horse;
                }
                else
                {
                    if (ActiveOutfit == 1) existing.Outfit1 = true;
                    else if (ActiveOutfit == 2) existing.Outfit2 = true;
                    else if (ActiveOutfit == 3) existing.Outfit3 = true;
                    existing.SlotId = GetSlotNumericId(SelectedPaperdollSlot.Id);
                }
            }
            else
            {
                var storage = isHorseSlot ? ItemStorageLocation.Horse : ItemStorageLocation.Inventory;
                SpawnItemIntoInventory(SelectedSlotPickerItem, GetSlotNumericId(SelectedPaperdollSlot.Id), storage);
            }

            if (isHorseSlot)
            {
                RecalculateHorseStats();
            }
            else
            {
                RecalculateCharacterStats();
            }

            string presetLabel = isHorseSlot
                ? (IsGerman ? $"Sattelzeug {activePreset}" : $"Tack {activePreset}")
                : (IsGerman ? $"Outfit {activePreset}" : $"Outfit {activePreset}");

            StatusMessage = IsGerman
                ? $"Gegenstand '{name}' in Slot '{SelectedPaperdollSlot.DisplayLabel}' ({presetLabel}) ausgerüstet."
                : $"Equipped '{name}' in slot '{SelectedPaperdollSlot.DisplayLabel}' ({presetLabel}).";

            IsSlotPickerModalOpen = false;
        }

        private void UnequipSlot()
        {
            if (SelectedPaperdollSlot == null) return;

            bool isHorseSlot = SelectedPaperdollSlot.Id.StartsWith("horse_");
            int activePreset = isHorseSlot ? ActiveHorseOutfit : ActiveOutfit;

            SelectedPaperdollSlot.Unequip(activePreset);

            if (isHorseSlot)
            {
                RecalculateHorseStats();
            }
            else
            {
                RecalculateCharacterStats();
            }

            string presetLabel = isHorseSlot
                ? (IsGerman ? $"Sattelzeug {activePreset}" : $"Tack {activePreset}")
                : (IsGerman ? $"Outfit {activePreset}" : $"Outfit {activePreset}");

            StatusMessage = IsGerman
                ? $"Slot '{SelectedPaperdollSlot.DisplayLabel}' geleert ({presetLabel})."
                : $"Unequipped slot '{SelectedPaperdollSlot.DisplayLabel}' ({presetLabel}).";

            IsSlotPickerModalOpen = false;
        }

        private void SelectSlotPickerSource(string? source)
        {
            if (string.IsNullOrEmpty(source)) return;
            SlotPickerSelectedSource = source;
        }

        private void SpawnItemIntoInventory(SlotPickerItemViewModel item, int slotId, ItemStorageLocation storage = ItemStorageLocation.Inventory)
        {
            var native = new Kcd2InventoryItem
            {
                ItemId = item.ItemId,
                ItemUiName = item.Name,
                UniqueId = (ulong)DateTime.UtcNow.Ticks,
                Amount = 1,
                Condition = Math.Clamp(item.Condition, 0.0f, 1.0f),
                SlotId = slotId,
                Flags = 0x00u
            };

            if (_handle != IntPtr.Zero)
            {
                NativeMethods.kcd2_save_add_inventory_item(_handle, in native);
            }

            var itemVm = new InventoryItemViewModel(native, OnInventoryItemChanged)
            {
                Category = item.Category,
                SubCategory = item.SubCategory,
                Weight = item.Weight,
                Price = item.Price,
                Power = item.Power,
                Tier = item.Tier,
                Condition = item.Condition,
                Glyph = item.Icon,
                StorageLocation = storage,
                Outfit1 = (storage == ItemStorageLocation.Inventory && ActiveOutfit == 1),
                Outfit2 = (storage == ItemStorageLocation.Inventory && ActiveOutfit == 2),
                Outfit3 = (storage == ItemStorageLocation.Inventory && ActiveOutfit == 3)
            };

            InventoryItems.Add(itemVm);
            UpdateCategoryTabCounts();
        }

        private int GetSlotNumericId(string slotId)
        {
            return slotId switch
            {
                "slot_helm" => 1,
                "slot_coif" => 2,
                "slot_hood" => 3,
                "slot_gorget" => 4,
                "slot_ring" => 5,
                "slot_necklace" => 6,
                "slot_dagger" => 7,
                "slot_crossbow" => 8,
                "slot_sword" => 9,
                "slot_shield" => 10,
                "slot_handgonne" => 11,
                "slot_bullet" => 12,
                "slot_coat" => 13,
                "slot_gambeson" => 14,
                "slot_cuirass" => 15,
                "slot_arms" => 16,
                "slot_gauntlets" => 17,
                "slot_hose" => 18,
                "slot_cuisse" => 19,
                "slot_padded_legs" => 20,
                "slot_boots" => 21,
                "slot_spurs" => 22,
                "slot_torch" => 23,
                "slot_pouch" => 24,
                "horse_saddle" => 25,
                "horse_bridle" => 26,
                "horse_caparison" => 27,
                "horse_horseshoes" => 28,
                "horse_chanfron" => 29,
                "horse_saddlebags" => 30,
                _ => 1
            };
        }

        public void RecalculateHorseStats()
        {
            if (SelectedHorseOption == null) return;

            float baseSpeed = SelectedHorseOption.DefaultSpeed;
            float baseCapacity = SelectedHorseOption.DefaultCapacity;
            float baseCourage = SelectedHorseOption.DefaultCourage;
            float baseStamina = SelectedHorseOption.DefaultStamina;

            float addSpeed = 0;
            float addCapacity = 0;
            float addCourage = 0;
            float addStamina = 0;

            foreach (var slot in HorseEquipmentSlots)
            {
                if (!slot.IsEquipped) continue;
                string name = (slot.DisplayLabel ?? string.Empty).ToLowerInvariant();
                string id = slot.Id;

                if (id == "horse_saddle")
                {
                    if (name.Contains("4") || name.Contains("edel") || name.Contains("noble") || name.Contains("prunk"))
                        addCapacity += 160f;
                    else if (name.Contains("renn") || name.Contains("racing") || name.Contains("leicht"))
                    {
                        addCapacity += 60f;
                        addSpeed += 3f;
                    }
                    else
                        addCapacity += 100f;
                }
                else if (id == "horse_horseshoes")
                {
                    if (name.Contains("renn") || name.Contains("edel") || name.Contains("noble") || name.Contains("gold"))
                        addSpeed += 12f;
                    else if (name.Contains("militär") || name.Contains("military"))
                        addSpeed += 8f;
                    else
                        addSpeed += 5f;
                }
                else if (id == "horse_bridle")
                {
                    if (name.Contains("prunk") || name.Contains("edel") || name.Contains("ritter") || name.Contains("knight") || name.Contains("noble"))
                        addCourage += 4f;
                    else
                        addCourage += 2f;
                }
                else if (id == "horse_caparison")
                {
                    if (name.Contains("prunk") || name.Contains("edel") || name.Contains("gesteppt"))
                        addCourage += 3f;
                    else
                        addCourage += 1f;
                }
                else if (id == "horse_chanfron")
                {
                    if (name.Contains("ketten") || name.Contains("graviert") || name.Contains("harnisch"))
                        addCourage += 4f;
                    else
                        addCourage += 2f;
                }
                else if (id == "horse_saddlebags")
                {
                    if (name.Contains("edel") || name.Contains("noble") || name.Contains("schwer") || name.Contains("heavy"))
                        addCapacity += 120f;
                    else
                        addCapacity += 70f;
                }
            }

            HorseSpeed = MathF.Min(60f, baseSpeed + addSpeed);
            HorseCapacity = MathF.Min(600f, baseCapacity + addCapacity);
            HorseCourage = MathF.Min(25f, baseCourage + addCourage);
            HorseStamina = MathF.Min(500f, baseStamina + addStamina);
        }

        private void RecalculateCharacterStats()
        {
            if (ActiveOutfit == 1)
            {
                ArmorHeadStab = 12; ArmorHeadSlash = 12; ArmorHeadBlunt = 14;
                ArmorBodyStab = 140; ArmorBodySlash = 138; ArmorBodyBlunt = 75;
                ArmorArmsStab = 13; ArmorArmsSlash = 16; ArmorArmsBlunt = 18;
                ArmorLegsStab = 157; ArmorLegsSlash = 137; ArmorLegsBlunt = 73;
                WeaponMeleeStab = 138; WeaponMeleeSlash = 131; WeaponMeleeBlunt = 26;
                WeaponRangedAttack = 704;
                WeaponUnarmedAttack = 133;
                Noise = 61;
                Conspicuousness = 41;
                Visibility = 45;
            }
            else if (ActiveOutfit == 2)
            {
                ArmorHeadStab = 4; ArmorHeadSlash = 4; ArmorHeadBlunt = 6;
                ArmorBodyStab = 45; ArmorBodySlash = 40; ArmorBodyBlunt = 35;
                ArmorArmsStab = 6; ArmorArmsSlash = 8; ArmorArmsBlunt = 8;
                ArmorLegsStab = 35; ArmorLegsSlash = 30; ArmorLegsBlunt = 25;
                WeaponMeleeStab = 95; WeaponMeleeSlash = 110; WeaponMeleeBlunt = 20;
                WeaponRangedAttack = 640;
                WeaponUnarmedAttack = 110;
                Noise = 14;
                Conspicuousness = 18;
                Visibility = 22;
            }
            else if (ActiveOutfit == 3)
            {
                ArmorHeadStab = 2; ArmorHeadSlash = 2; ArmorHeadBlunt = 4;
                ArmorBodyStab = 65; ArmorBodySlash = 60; ArmorBodyBlunt = 40;
                ArmorArmsStab = 8; ArmorArmsSlash = 10; ArmorArmsBlunt = 10;
                ArmorLegsStab = 45; ArmorLegsSlash = 40; ArmorLegsBlunt = 30;
                WeaponMeleeStab = 120; WeaponMeleeSlash = 125; WeaponMeleeBlunt = 22;
                WeaponRangedAttack = 580;
                WeaponUnarmedAttack = 120;
                Noise = 32;
                Conspicuousness = 55;
                Visibility = 60;
            }

            OnPropertyChanged(nameof(CurrentOutfitPedestal));
        }

        private void EnsureCodexItemsLoaded()
        {
            if (AllCodexItems.Count == 0)
            {
                var items = GameDataService.Instance.GetAllCodexItems();
                foreach (var it in items)
                {
                    AllCodexItems.Add(it);
                }
            }
        }







        private void UpdatePaperdollSelection(InventoryItemViewModel? selectedItem)



        {



            if (selectedItem == null) return;



            foreach (var slot in LeftPaperdollSlots)



            {



                slot.IsSelected = (!string.IsNullOrEmpty(slot.LinkedItemId) && selectedItem.ItemId.Contains(slot.LinkedItemId, StringComparison.OrdinalIgnoreCase)) ||



                                  selectedItem.ItemUiName.Contains(slot.Label, StringComparison.OrdinalIgnoreCase);



            }



            foreach (var slot in RightPaperdollSlots)



            {



                slot.IsSelected = (!string.IsNullOrEmpty(slot.LinkedItemId) && selectedItem.ItemId.Contains(slot.LinkedItemId, StringComparison.OrdinalIgnoreCase)) ||



                                  selectedItem.ItemUiName.Contains(slot.Label, StringComparison.OrdinalIgnoreCase);



            }



        }







        private void InitializeFactionsAndPerks()



        {



            Factions.Clear();



            Factions.Add(new FactionRepViewModel("rep_kuttenberg", "Kuttenberg Bürgerschaft & Gilden", "Kuttenberg Burgers & Guilds", 88));



            Factions.Add(new FactionRepViewModel("rep_trosky", "Burg Trosky Garnison", "Trosky Castle Garrison", 72));



            Factions.Add(new FactionRepViewModel("rep_suchdol", "Suchdol & Umland", "Suchdol & Countryside", 95));



            Factions.Add(new FactionRepViewModel("rep_royal", "Getreue von König Wenzel IV.", "King Wenceslas IV Loyalists", 100));







            Perks.Clear();



            Perks.Add(new PerkItemViewModel("prk_master_strike", "Meisterschlag", "Master Strike", true));



            Perks.Add(new PerkItemViewModel("prk_blacksmith", "Meisterschmied von Skalitz", "Master Blacksmith of Skalitz", true));



            Perks.Add(new PerkItemViewModel("prk_gunner", "Böhmischer Büchsenmeister", "Bohemian Gunsmith", true));



            Perks.Add(new PerkItemViewModel("prk_mule", "Packesel I, II & III (+120 Traglast)", "Pack Mule I, II & III (+120 Carry Capacity)", true));



            Perks.Add(new PerkItemViewModel("prk_alchemist", "Routine II (Auto-Brauen)", "Routine II (Auto-Brew)", true));



            Perks.Add(new PerkItemViewModel("prk_nightcrawler", "Nachtschwärmer (+Schleichen)", "Nightcrawler (+Stealth)", false));



        }







        private bool FilterQuestItem(object obj)
        {
            if (obj is not QuestItemViewModel quest) return false;

            if (!string.Equals(SelectedQuestFilter, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(quest.Type, SelectedQuestFilter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(SelectedQuestRegionFilter, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(quest.Region, SelectedQuestRegionFilter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(QuestSearchFilter))
            {
                string query = QuestSearchFilter.Trim();
                bool match = quest.TitleDe.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             quest.TitleEn.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             quest.LocationDe.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             quest.LocationEn.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                             quest.Id.Contains(query, StringComparison.OrdinalIgnoreCase);
                if (!match) return false;
            }

            return true;
        }

        private bool FilterCharacterFate(object obj)
        {
            if (obj is not GameCharacterFateViewModel fate) return false;

            if (SelectedFateCategoryFilter == "vip" && fate.Category != CharacterCategory.StoryVip) return false;
            if (SelectedFateCategoryFilter == "decision" && fate.Category != CharacterCategory.DecisionChoice) return false;
            if (SelectedFateCategoryFilter == "craftsman" && fate.Category != CharacterCategory.CraftsmanMaster) return false;

            if (!string.IsNullOrWhiteSpace(FateSearchFilter))
            {
                string q = FateSearchFilter.Trim();
                bool match = fate.NameDe.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                             fate.NameEn.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                             fate.RoleDe.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                             fate.RoleEn.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                             fate.LocationDe.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                             fate.LocationEn.Contains(q, StringComparison.OrdinalIgnoreCase);
                if (!match) return false;
            }

            return true;
        }

        private void InitializeHorses()



        {



            AvailableHorses.Clear();



            AvailableHorses.Add(new HorseOptionViewModel("tsem_sedivka", "Šedivka (Pebbles)", "Pebbles", "Trosky-Schimmel (Cob)", "Trosky Cob", 1, 40f, 210f, 11f, 410f, 100f, "char_KUN_SEDIVKA_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kkut_horseForSale_1", "Kincsem", "Kincsem", "Kuttenberger Rennpferd", "Kuttenberg Courser", 4, 39f, 388f, 19f, 490f, 150f, "char_KUN_PRODEJNI_KUTNA_HORA_1_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kkut_horseForSale_2", "Gavora", "Gavora", "Böhmischer Renner", "Bohemian Courser", 5, 44f, 290f, 15f, 530f, 130f, "char_KUN_PRODEJNI_KUTNA_HORA_2_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kkut_horseForSale_3", "Napoli", "Napoli", "Östlicher Vollbluthengst", "Eastern Thoroughbred", 5, 42f, 320f, 20f, 510f, 140f, "char_KUN_PRODEJNI_KUTNA_HORA_3_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kkut_horseForSale_4", "Piseker Bursche", "Pisek Lad", "Arabischer Schimmel", "Arabian Steed", 5, 43f, 310f, 18f, 520f, 135f, "char_KUN_PRODEJNI_KUTNA_HORA_4_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_1", "Kluger Hans", "Clever Hans", "Kaltblut-Lasttier", "Heavy Draught Horse", 3, 37f, 410f, 14f, 470f, 160f, "char_KUN_PRODEJNI_GRUNTA_1_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_2", "Lutz (Bill)", "Bill", "Polnischer Schimmel", "Polish Cob", 3, 40f, 300f, 16f, 450f, 125f, "char_KUN_PRODEJNI_GRUNTA_2_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_3", "Svadilfari", "Svadilfari", "Schwerer Festungshengst", "Heavy Siege Stallion", 4, 36f, 440f, 20f, 500f, 180f, "char_KUN_PRODEJNI_GRUNTA_3_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_4", "Windwurf", "Windwurf", "Schwarzer Rappe", "Black Courser", 4, 42f, 330f, 17f, 480f, 130f, "char_KUN_PRODEJNI_GRUNTA_4_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_5", "Veillantif", "Veillantif", "Sumpfstute von Maleschau", "Maleschau Mare", 3, 39f, 340f, 15f, 460f, 120f, "char_KUN_PRODEJNI_GRUNTA_5_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kgru_horseForSale_6", "Wilhelmine (Mark)", "Mark", "Ausdauerndes Reitpferd", "Endurance Riding Horse", 2, 38f, 280f, 13f, 440f, 110f, "char_KUN_PRODEJNI_GRUNTA_6_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_1", "Khartoum", "Khartoum", "Silberner Renner", "Silver Courser", 5, 45f, 305f, 19f, 550f, 145f, "char_KUN_PRODEJNI_MALESOV_1_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_2", "Artax", "Artax", "Alter Wallach", "Old Gelding", 1, 35f, 240f, 10f, 380f, 90f, "char_KUN_PRODEJNI_MALESOV_2_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_3", "Kasztanka", "Kasztanka", "Schmiede-Arbeitspferd", "Smithy Workhorse", 2, 36f, 360f, 14f, 430f, 140f, "char_KUN_PRODEJNI_MALESOV_3_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_4", "Shlamhak", "Shlamhak", "Taktisches Kavalleriepferd", "Cavalry Charger", 4, 41f, 350f, 18f, 490f, 150f, "char_KUN_PRODEJNI_MALESOV_4_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_5", "Erdel", "Erdel", "Sächsisches Streitross", "Saxon Warhorse", 4, 39f, 370f, 19f, 480f, 155f, "char_KUN_PRODEJNI_MALESOV_5_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("kmal_horseForSale_6", "Rabe (Crow)", "Crow", "Keltische Stute", "Celtic Mare", 3, 41f, 310f, 16f, 460f, 125f, "char_KUN_PRODEJNI_MALESOV_6_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("ksem_horseForSale_1", "Tarant", "Tarant", "Semíner Kaltblut", "Semin Draught", 3, 40f, 380f, 16f, 460f, 130f, "char_KUN_PRODEJNI_SEMIN_1_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("ksem_horseForSale_2", "Kämpfer (Soldier)", "Soldier", "Festungs-Streitross", "Siege Warhorse", 4, 38f, 420f, 18f, 480f, 150f, "char_KUN_PRODEJNI_SEMIN_2_uiName"));



            AvailableHorses.Add(new HorseOptionViewModel("ksem_horseForSale_3", "Bitterklee", "Bitterklee", "Böhmisches Reitpferd", "Bohemian Riding Horse", 3, 42f, 310f, 15f, 450f, 120f, "char_KUN_PRODEJNI_SEMIN_3_uiName"));







            _selectedHorseOption = AvailableHorses.FirstOrDefault();



        }







                private void AddQuest(
            string id,
            string type,
            string titleDe,
            string titleEn,
            string locationDe,
            string locationEn,
            string defaultStatus,
            string? uiTitleKey,
            string? locationKey,
            string region,
            bool isPointOfNoReturn,
            string? ponrWarningDe,
            string? ponrWarningEn,
            params (string de, string en, bool isDone, bool isActive, string? uiKey)[] objectives)
        {
            int doneCount = objectives.Count(o => o.isDone);
            var q = new QuestItemViewModel(id, type, titleDe, titleEn, locationDe, locationEn, defaultStatus, doneCount, Math.Max(1, objectives.Length), uiTitleKey, locationKey, region, isPointOfNoReturn, ponrWarningDe, ponrWarningEn);
            foreach (var obj in objectives)
            {
                q.Objectives.Add(new QuestObjectiveViewModel(obj.de, obj.en, obj.isDone, obj.isActive, () =>
                {
                    q.UpdateStageFromObjectives();
                    if (_handle != IntPtr.Zero)
                    {
                        NativeMethods.kcd2_save_set_quest_status(_handle, q.Id, q.Status);
                    }
                    IsDirty = true;
                }, obj.uiKey));
            }
            Quests.Add(q);
        }

        private void AddQuest(
            string id,
            string type,
            string titleDe,
            string titleEn,
            string locationDe,
            string locationEn,
            string defaultStatus,
            string region,
            bool isPointOfNoReturn,
            string? ponrWarningDe,
            string? ponrWarningEn,
            params (string de, string en, bool isDone, bool isActive)[] objectives)
        {
            var mapped = objectives.Select(o => (o.de, o.en, o.isDone, o.isActive, (string?)null)).ToArray();
            AddQuest(id, type, titleDe, titleEn, locationDe, locationEn, defaultStatus, null, null, region, isPointOfNoReturn, ponrWarningDe, ponrWarningEn, mapped);
        }

        private void AddQuest(
            string id,
            string type,
            string titleDe,
            string titleEn,
            string locationDe,
            string locationEn,
            string defaultStatus,
            string region,
            params (string de, string en, bool isDone, bool isActive)[] objectives)
        {
            AddQuest(id, type, titleDe, titleEn, locationDe, locationEn, defaultStatus, region, false, null, null, objectives);
        }

        private void AddQuest(
            string id,
            string type,
            string titleDe,
            string titleEn,
            string locationDe,
            string locationEn,
            string defaultStatus,
            params (string de, string en, bool isDone, bool isActive)[] objectives)
        {
            AddQuest(id, type, titleDe, titleEn, locationDe, locationEn, defaultStatus, "Trosecko", false, null, null, objectives);
        }

        private void InitializeQuests()
        {
            Quests.Clear();

            // Prologue / Act 1 Main Quests
            AddQuest("_posledniPomazani", "haupt", "Sterbesakramente", "Last Rites", "Suchdol", "Suchdol", "aktiv",
                "@qname_poslednipomazani_1DR8", "location_suchdol", "Kutnohorsko", false, null, null,
                ("Nimm die Treppen zu den vorderen Wällen.", "Go up the stairs to the front ramparts.", false, true, "@poslednip_objective1_9O8s"),
                ("Verteidige den vorderen Wall.", "Defend the front ramparts!", false, false, "@poslednip_bran_hradbu_y3Yz"),
                ("Stoß die Leiter um.", "Knock down the ladder!", false, false, "@poslednip_shod_zebriky_8Vh2"),
                ("Töte alle Gegner auf den Wällen!", "Wipe out all the enemy forces on the rampart!", false, false, "@poslednip_odraz_nepritele_COTb"),
                ("Folge dem Kommandanten Knochenteufel.", "Follow the commander Dry Devil.", false, false, "@poslednip_dostan_se_na_bo_MLRL"),
                ("Hilf dabei, das Tor im Burghof zu verteidigen.", "Help defend the gate in the courtyard!", false, false, "@poslednip_pomoz_na_nadvor_Asak"));

            AddQuest("_prepadeni", "haupt", "Der Weg voraus", "Easy Riders", "Trosky", "Trosky", "nicht_gestartet",
                "@qname_prepadeni_KsSs", "location_trosky", "Trosecko", false, null, null,
                ("Wehre Angriffe ab.", "Block attacks.", false, false, "@prepadeni_blokuj_ptackovi_1jzM"),
                ("Statte dich mit Rüstung und Schwert aus und sprich mit Herrn Hans.", "Get kitted out with armour and a sword and talk to Sir Hans.", false, false, "@prepadeni_nasad_si_zbroj__xQPy"),
                ("Begleite Herrn Hans' Gefolge.", "Follow Sir Hans' company.", false, false, "@prepadeni_nasleduj_pana_p_VQdv"),
                ("Überlebe!", "Survive!", false, false, "@prepadeni_prezij_1wJE"));

            AddQuest("_sedmStatecnych", "haupt", "Die Sieben Wackeren", "The Magnificent Seven", "Burg Trosky", "Trosky Castle", "nicht_gestartet", "Trosecko",
                ("Finde die sieben ehemaligen Söldner auf Trosky", "Find the seven former mercenaries at Trosky", false, false),
                ("Überzeuge Jan Žižka von deiner Treue", "Convince Jan Zizka of your loyalty", false, false),
                ("Triff dich mit den Gefährten beim Lagerfeuer", "Meet the companions at the campfire", false, false));

            AddQuest("_sedmStatecnych2", "haupt", "Die Sieben Wackeren II", "The Magnificent Seven II", "Burg Semín", "Semín Castle", "nicht_gestartet", "Trosecko",
                ("Reite mit Žižkas Trupp nach Semín", "Ride with Zizka's band to Semin", false, false),
                ("Befrage Herrn von Semín über den Überfall", "Question Lord of Semin about the raid", false, false),
                ("Sichere das Feldlager vor Spionen", "Secure camp against spies", false, false));

            AddQuest("_svatba", "haupt", "Die Hochzeit auf Burg Semín", "The Wedding at Semín Castle", "Burg Semín", "Semín Castle", "nicht_gestartet",
                "Trosecko", true,
                "Achtung: Dies ist ein Haupt-Ereignis auf Burg Semín. Das Vorantreiben leitet den finalen Akt in Trosecko ein!",
                "Warning: Semín Castle Wedding initiates the climax of the Trosecko story arc!",
                ("Besorge ein standesgemäßes Hochzeitsgewand", "Obtain noble wedding attire", false, false),
                ("Sprich mit der Braut und den Hochzeitsgästen", "Talk with bride and guests", false, false),
                ("Schlichte den Streit zwischen den Zechern", "Settle dispute among carousers", false, false),
                ("Tanze mit der Tochter des Vogts", "Dance with bailiff's daughter", false, false));

            AddQuest("_korenarkaZachrana", "haupt", "Die Rettung der Kräuterfrau Božena", "Rescuing Herbalist Božena", "Trosky-Wälder", "Trosky Woods", "nicht_gestartet", "Trosecko",
                ("Folge den Hufspuren in die dichten Wälder", "Follow horse tracks into deep woods", false, false),
                ("Befreie Kräuterfrau Božena aus der Gefangenschaft", "Free Herbalist Bozena from captivity", false, false),
                ("Eskortiere Božena sicher zu ihrer Waldhütte", "Escort Bozena safely to woodland hut", false, false));

            AddQuest("_tarasMura", "haupt", "Taras Mura und die Banditen", "Taras Mura and the Bandits", "Kuttenberg-Vorstadt", "Kuttenberg Suburbs", "nicht_gestartet", "Kutnohorsko",
                ("Spüre Taras Mura in den Vorstadt-Gassen auf", "Track down Taras Mura in suburban alleys", false, false),
                ("Besiege die Banditen im Hinterhof", "Defeat bandits in courtyard", false, false),
                ("Hole die gestohlenen Bergwerksunterlagen zurück", "Recover stolen mining ledgers", false, false));

            AddQuest("_zaby", "neben", "Der Froschteich & die Wilderer", "The Frog Pond & Poachers", "Želejov", "Zelejov", "nicht_gestartet", "Trosecko",
                ("Untersuche die illegalen Reusen am Froschteich", "Investigate illegal fish traps at pond", false, false),
                ("Stelle die Wilderer auf frischer Tat", "Catch poachers in the act", false, false),
                ("Berichte dem Hege-Aufseher von Želejov", "Report back to Zelejov game warden", false, false));

            AddQuest("_kumaniNaTrosecku", "haupt", "Kumanen auf dem Trosecko", "Cumans on Trosecko", "Burg Trosky", "Trosky Castle", "nicht_gestartet", "Trosecko",
                ("Erkunde das verlassene Kumanen-Feldlager", "Scout abandoned Cuman encampment", false, false),
                ("Erbeute die geheime Kumanen-Marschroute", "Seize secret Cuman marching route", false, false),
                ("Warne die Burgwache vor dem Angriff", "Warn castle guard of attack", false, false));

            AddQuest("_mysi1", "neben", "Die Mäuseplage von Suchdol", "The Suchdol Rat Infestation", "Suchdol", "Suchdol", "nicht_gestartet", "Kutnohorsko",
                ("Finde die Ursache des Getreideverlusts in den Silos", "Find source of grain spoilage in silos", false, false),
                ("Braue einen wirksamen Kräutersud gegen die Nager", "Brew herbal rodent repellent", false, false),
                ("Verteile den Sud in allen Vorratskellern", "Distribute remedy across food cellars", false, false));

            AddQuest("_poklad", "neben", "Der versunkene Schatz von Nebákov", "The Sunken Treasure of Nebákov", "Nebákov", "Nebákov", "nicht_gestartet", "Trosecko",
                ("Finde die alte Schatzkarte in der Wassermühle", "Find old treasure map inside watermill", false, false),
                ("Tauche nach der versunkenen Schatztruhe im Mühlteich", "Dive for sunken chest in millpond", false, false),
                ("Öffne die Truhe mit Dietrich oder Brecheisen", "Unlock chest using lockpick or crowbar", false, false));

            AddQuest("_poustevnik", "neben", "Das Geheimnis des Einsiedlers", "The Hermit's Secret", "Apolena-Felsen", "Apolena Rocks", "nicht_gestartet", "Trosecko",
                ("Suche die Einsiedler-Höhle in den Apolena-Felsen", "Search for hermit's cave in Apolena Rocks", false, false),
                ("Löse das Rätsel der eingemeißelten Steine", "Solve riddle of carved stones", false, false),
                ("Bringe dem Einsiedler seine Gebetskette", "Return lost rosary to hermit", false, false));

            AddQuest("_sermiri", "neben", "Die Fechtmeister von Kuttenberg", "The Fencing Masters of Kuttenberg", "Kuttenberg", "Kuttenberg", "nicht_gestartet", "Kutnohorsko",
                ("Fordere die Gesellen der Fechtschule heraus", "Challenge fencing school journeymen", false, false),
                ("Meistere den Meisterschlag vor dem Richter", "Execute master strike before judge", false, false),
                ("Erringe das Kuttenberger Meister-Privileg", "Win Kuttenberg fencing master's certificate", false, false));

            AddQuest("_utokNaNebakov", "haupt", "Der Angriff auf die Festung Nebákov", "The Assault on Nebákov Fortress", "Festung Nebákov", "Nebákov Fortress", "nicht_gestartet",
                "Trosecko", true,
                "Kritischer Wendepunkt: Der Sturm auf Nebákov sperrt unvollendete Nebenquests in Trosecko!",
                "Critical Point of No Return: The assault on Nebákov Fortress will lock open Trosecko side quests!",
                ("Bereite die Sturmleitern und Belagerungsgeräte vor", "Prepare scaling ladders and siege weapons", false, false),
                ("Erstürme die Palisade mit Žižkas Vorhut", "Storm palisade alongside Zizka's vanguard", false, false),
                ("Schalte die feindlichen Schützen auf dem Bergfried aus", "Neutralize enemy archers on keep", false, false));

            AddQuest("_nebakovObrana", "haupt", "Die Verteidigung von Nebákov", "The Defense of Nebákov", "Festung Nebákov", "Nebákov Fortress", "nicht_gestartet",
                "Trosecko", true,
                "Kritischer Wendepunkt: Nach dieser Schlacht reist Heinrich weiter nach Kuttenberg!",
                "Critical Point of No Return: Following this siege, Henry departs for Kuttenberg!",
                ("Sichere das Burgtor vor den ungarischen Rammböcken", "Secure main gate against Hungarian rams", false, false),
                ("Gieße heißes Pech über die Mauerbrecher", "Pour boiling pitch over wall-breachers", false, false),
                ("Halte die Stellung bis zum Eintreffen der Entsatzung", "Hold position until relief force arrives", false, false));

            AddQuest("_nebakovPruzkum", "haupt", "Aufklärung im Nebákov-Tal", "Reconnaissance in Nebákov Valley", "Nebákov-Tal", "Nebákov Valley", "nicht_gestartet", "Trosecko",
                ("Schleiche unbemerkt an den Vorposten vorbei", "Slip unnoticed past enemy outposts", false, false),
                ("Zähle Zelte und Vorräte im feindlichen Hauptlager", "Count tents and provisions in main camp", false, false),
                ("Kehre unentdeckt zu Radzig zurück", "Return to Sir Radzig undetected", false, false));

            AddQuest("_mucirna", "haupt", "Die Folterkammer von Malešov", "The Torture Chamber of Malešov", "Malešov", "Malešov", "nicht_gestartet", "Kutnohorsko",
                ("Infiltriere die Untergeschosse der Burg Malešov", "Infiltrate lower levels of Malesov castle", false, false),
                ("Finde den Schlüssel zu den Folterkerkern", "Find key to dungeon torture chambers", false, false),
                ("Befreie die gefangenen Bürgersöhne von Kuttenberg", "Rescue captured Kuttenberg burghers", false, false));

            AddQuest("_bohutovaVlozka", "haupt", "Pater Bohutas Sendung", "Father Bohuta's Mission", "Suchdol", "Suchdol", "nicht_gestartet",
                "Kutnohorsko", true,
                "Kritischer Wendepunkt: Entscheidende Phase während der Belagerung von Suchdol!",
                "Critical Point of No Return: Crucial stage during the Siege of Suchdol!",
                ("Halte eine Predigt zur Hebung der Moral", "Deliver stirring sermon to raise morale", false, false),
                ("Hole Beichte von den verwundeten Soldaten ein", "Hear confessions from wounded soldiers", false, false),
                ("Besorge Messwein für das Abendmahl", "Obtain communion wine for the sacrament", false, false));

            AddQuest("_zbranePanaSemina", "neben", "Die Waffenschmiede des Herrn von Semín", "Lord Semín's Armory", "Burg Semín", "Semín Castle", "nicht_gestartet", "Trosecko",
                ("Beschaffe hochwertiges böhmisches Eisenerz", "Acquire high-grade Bohemian iron ore", false, false),
                ("Schmiede drei meisterhafte Harnisch-Klingen", "Forge three masterwork armor-piercing blades", false, false),
                ("Liefere die Waffen an den Waffenmeister von Semín", "Deliver arms to Semin's armorer", false, false));

            AddQuest("_fightClubZelejov", "aktiv", "Die Faustkämpfer von Želejov", "The Brawlers of Želejov", "Želejov", "Želejov", "nicht_gestartet", "Trosecko",
                ("Besiege den Mühlenburschen im Ring", "Defeat mill apprentice in ring", false, false),
                ("Schlage den Hufschmied ohne Treffer einzustecken", "Defeat blacksmith without taking hits", false, false),
                ("Werde der Faustkampf-Champion von Želejov", "Become brawl champion of Zelejov", false, false));

            AddQuest("_kubaParalu", "neben", "Kuba Paralu und die Schenke", "Kuba Paralu and the Tavern", "Želejov", "Želejov", "nicht_gestartet", "Trosecko",
                ("Finde den sturzbetrunkenen Kuba im Stall", "Locate drunk Kuba sleeping in stall", false, false),
                ("Begleiche seine aufgelaufenen Zechschulden", "Settle his tavern tab with innkeeper", false, false),
                ("Bringe Kuba wohlbehalten zu seinem Meister", "Escort Kuba safely back to master", false, false));

            AddQuest("_socky", "aktiv", "Die Bettler von Kuttenberg", "The Beggars of Kuttenberg", "Kuttenberg", "Kuttenberg", "nicht_gestartet", "Kutnohorsko",
                ("Verteile Almosen an die Bedürftigen am Marktplatz", "Distribute alms to needy at market square", false, false),
                ("Entlarve die falschen Bettler vor dem Rathaus", "Expose counterfeit beggars by town hall", false, false),
                ("Erfahre geheime Gerüchte über die Silberhändler", "Gather secret rumors about silver cartels", false, false));

            AddQuest("_plesnivec", "neben", "Der Schimmelreiter der Trosky-Heide", "The Phantom of Trosky Heath", "Trosky-Heide", "Trosky Heath", "nicht_gestartet", "Trosecko",
                ("Befrage die Holzfäller über die nächtliche Gestalt", "Question woodcutters about night rider", false, false),
                ("Lege dich bei Mitternacht auf der Heide auf die Lauer", "Lie in wait at midnight upon heath", false, false),
                ("Decke die Wahrheit hinter der Erscheinung auf", "Uncover truth behind ghost sighting", false, false));

            AddQuest("_drak", "neben", "Die Drachenhöhle im Böhmischen Paradies", "The Dragon's Lair in Bohemian Paradise", "Prachov-Felsen", "Prachov Rocks", "nicht_gestartet", "Trosecko",
                ("Erkunde die Höhle des 'Drachen' in den Felsen", "Explore dragon's cave deep in rocks", false, false),
                ("Finde heraus, was das unheimliche Fauchen erzeugt", "Discover what creates eerie hissing sound", false, false),
                ("Sichere den verborgenen Raubritter-Schatz", "Secure hidden robber baron's stash", false, false));

            AddQuest("_falesnaPosadka", "haupt", "Die falsche Burgbesatzung", "The False Garrison", "Burg Trosky", "Trosky Castle", "nicht_gestartet", "Trosecko",
                ("Überprüfe die Wachstuben der äußeren Vorburg", "Inspect guard rooms of outer bailey", false, false),
                ("Enttarne die verkleideten Verräter unter den Wachen", "Unmask disguised traitors among garrison", false, false),
                ("Verhindere das heimliche Öffnen des Fallgitters", "Prevent secret opening of portcullis", false, false));

            AddQuest("_viktorka", "neben", "Das Schicksal der armen Viktorka", "The Fate of Poor Viktorka", "Grund", "Grund", "nicht_gestartet", "Kutnohorsko",
                ("Finde Viktorkas Spuren am Waldrand von Grund", "Find Viktorka's tracks at edge of woods", false, false),
                ("Beruhige die verwirrte Frau und höre ihre Klage an", "Calm distraught woman and hear plea", false, false),
                ("Bringe ihr Amulett zurück zur Dorfkapelle", "Return family amulet to village chapel", false, false));

            AddQuest("_relikvie", "neben", "Die gestohlene Reliquie von St. Barbara", "The Stolen Relic of St. Barbara", "St. Barbara Kuttenberg", "St. Barbara Kuttenberg", "nicht_gestartet", "Kutnohorsko",
                ("Untersuche den aufgebrochenen Reliquienschrein", "Examine broken reliquary shrine", false, false),
                ("Befrage die Steinmetze an der Dombaustelle", "Question stonemasons at cathedral site", false, false),
                ("Stelle den Dieb auf dem Kuttenberger Silbermarkt", "Confront thief at silver market", false, false));

            AddQuest("_stareKosti", "neben", "Alte Knochen im Beinhaus", "Old Bones in the Ossuary", "Sedletz", "Sedlec", "nicht_gestartet", "Kutnohorsko",
                ("Sprich mit dem Totengräber des Klosters Sedletz", "Speak with gravedigger at Sedlec monastery", false, false),
                ("Ordne die entweihten Gebeine in der Krypta", "Arrange desecrated bones back in crypt", false, false),
                ("Enthülle das Geheimnis der verschwundenen Urkunde", "Reveal secret behind missing charter", false, false));

            AddQuest("_spizovaciOddil", "haupt", "Der Requirierungstrupp von König Sigismund", "King Sigismund's Foraging Party", "Suchdol", "Suchdol", "nicht_gestartet", "Kutnohorsko",
                ("Fange den feindlichen Fouragiertrupp vor Suchdol ab", "Intercept enemy foraging party near Suchdol", false, false),
                ("Schütze die Bauernhöfe vor Plünderung", "Protect farmsteads from looting", false, false),
                ("Bringe das requirierte Vieh zurück ins Dorf", "Bring requisitioned livestock back to village", false, false));

            AddQuest("_poPytlackeStezce", "aktiv", "Auf der Fährte des Wildschützen", "On the Poacher's Trail", "Trosky-Wälder", "Trosky Woods", "nicht_gestartet", "Trosecko",
                ("Folge den Spuren erlegten Rotwilds im Unterholz", "Follow tracks of poached deer in undergrowth", false, false),
                ("Stelle den berüchtigten Wildschützen im Hochwald", "Confront notorious poacher in high forest", false, false),
                ("Entscheide über das Schicksal des Wildschützen", "Decide poacher's ultimate fate", false, false));

            SelectedQuest = Quests.FirstOrDefault();
        }

        private void InitializeCharacterFates()
        {
            CharacterFates.Clear();

            // 1. Story VIPs (Unsterbliche Kern-Figuren mit Schutzklasse)
            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_jan_ptacek", "Herr Hans Capon (Jan Ptáček)", "Sir Hans Capon (Jan Ptacek)",
                "Herr von Pirkstein & Heinrichs Gefährte", "Lord of Pirkstein & Henry's companion",
                "Trosecko / Kuttenberg", "Trosecko / Kuttenberg",
                "Unsterblicher junger Adliger und Heinrichs ältester Reisegefährte. Begleitet Heinrich auf heiklen Gesandtschaften.",
                "Immortal young noble and Henry's closest companion. Accompanies Henry on perilous diplomatic missions.",
                "👑", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_jan_zizka", "Jan Žižka von Trocnov", "Jan Zizka of Trocnov",
                "Hauptmann der Rebellen & Hussitenführer", "Captain of the Rebels & Hussite leader",
                "Feldlager / Trosky", "Camp / Trosky",
                "Legendärer Söldnerführer und genialer Taktiker. Schart loyale Brüder um sich im Widerstand gegen König Sigismund.",
                "Legendary mercenary commander and brilliant tactician. Leads loyal brothers in resistance against King Sigismund.",
                "⚔️", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_radzig", "Herr Radzig Kobyla", "Sir Radzig Kobyla",
                "Königlicher Hetman & Heinrichs Vater", "Royal Hetman & Henry's father",
                "Kuttenberg / Trosky", "Kuttenberg / Trosky",
                "Ehrenhafter königlicher Burggraf von Skalitz. Entsandte Heinrich und Hans Capon auf ihre folgenschwere Mission.",
                "Honorable Royal Burgrave of Skalitz. Dispatched Henry and Hans Capon on their fateful expedition.",
                "🛡️", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_bohuta", "Pater Bohuta", "Father Bohuta",
                "Krieger-Priester von Suchdol", "Warrior-Priest of Suchdol",
                "Suchdol", "Suchdol",
                "Ehemaliger Krieger und jetziger Seelsorger von Suchdol. Greift bei Belagerungen selbst zu Schwert und Schild.",
                "Former soldier turned parish priest of Suchdol. Takes up arms to protect his flock during sieges.",
                "✝️", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_katerina", "Katharina (Kateřina)", "Katherine",
                "Žižkas Vertraute & Widerstandskämpferin", "Zizka's confidante & resistance fighter",
                "Trosky / Kuttenberg", "Trosky / Kuttenberg",
                "Kluge und furchtlose Gefährtin im Kreis der Rebellen. Steht Heinrich in brenzligen Lagen treu zur Seite.",
                "Clever, fearless companion among the rebel company. Stands firmly alongside Henry in tight spots.",
                "🏹", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_ota_bergov", "Otto von Bergow (Ota z Bergova)", "Otto of Bergow",
                "Herr von Burg Trosky", "Lord of Trosky Castle",
                "Burg Trosky", "Trosky Castle",
                "Mächtiger böhmischer Magnat auf Sigismunds Seite. Residiert auf der majestätischen Felsenburg Trosky.",
                "Powerful Bohemian lord aligned with Sigismund. Rules from the majestic twin towers of Trosky Castle.",
                "🏰", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_pan_semin", "Herr von Semín", "Lord of Semin",
                "Burgherr der Feste Semín", "Lord of Semin Keep",
                "Burg Semín", "Semin Castle",
                "Ehrwürdiger Patriarch von Semín. Versucht sein Lehen und seine Untertanen vor Überfällen zu bewahren.",
                "Aging patriarch of Semin Keep. Tries to protect his lands and people amidst escalating raiding.",
                "🦅", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_suchy_cert", "Knochenteufel (Suchý Čert)", "Dry Devil",
                "Hauptmann der Suchdol-Verteidiger", "Captain of Suchdol Defenders",
                "Suchdol", "Suchdol",
                "Grimmiger, unerschrockener Söldnerveteran. Leitet die vorderste Verteidigung der Wehrmauern von Suchdol.",
                "Grim, battle-hardened veteran commander leading front-line defense on Suchdol ramparts.",
                "🔥", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_musa", "Musa von Mali", "Musa of Mali",
                "Gelehrter, Arzt & Fechter", "Scholar, Physician & Duelist",
                "Kuttenberg", "Kuttenberg",
                "Weiser afrikanischer Reisender und Medicus. Meisterhaft im Fechten und gelehrt in Philosophie und Arznei.",
                "Wise African physician and scholar. Accomplished in swordsmanship and profound in medicinal lore.",
                "📜", CharacterCategory.StoryVip, "Immortal", () => IsDirty = true));

            // 2. Story Decisions (Schicksals-Wahl: Verschonen vs Hinrichten / Leben vs Tod)
            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_brabant", "Brabant", "Brabant",
                "Ehemaliger Burgvogt & Informant", "Former Bailiff & Informant",
                "Semín / Kuttenberg", "Semin / Kuttenberg",
                "Wurde als doppelzüngiger Spion entlarvt. Heinrich entscheidet, ob er verschont oder als Verräter gerichtet wird.",
                "Exposed as a double agent. Henry decides whether to grant mercy or execute him for treason.",
                "⚖️", CharacterCategory.DecisionChoice, "Spared", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_taras_mura", "Taras Mura", "Taras Mura",
                "Banditenführer der Kuttenberger Vorstadt", "Outlaw Boss of Kuttenberg Suburbs",
                "Kuttenberg-Vorstadt", "Kuttenberg Suburbs",
                "Befehligt eine Diebesbande in den Gassen. Heinrich kann verhandeln oder die Räuberbande zur Strecke bringen.",
                "Commands a gang in the suburbs. Henry can negotiate with him or eliminate the bandit hideout.",
                "🗡️", CharacterCategory.DecisionChoice, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_kuba_paralu", "Kuba Paralu", "Kuba Paralu",
                "Zechbruder & Mühlenknecht", "Tavern Regular & Mill Hand",
                "Želejov", "Zelejov",
                "Verstrickte sich in Zechschulden und Schlägereien. Kann aus dem Stall gerettet werden oder im Frost verenden.",
                "Ensnared in heavy tavern tabs and brawls. Can be rescued safely or perish in neglect.",
                "🍺", CharacterCategory.DecisionChoice, "Rescued", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_poacher_trosky", "Wildschütze der Trosky-Wälder", "Poacher of Trosky Woods",
                "Gesuchter Meisterschütze", "Wanted Master Marksman",
                "Trosky-Wälder", "Trosky Woods",
                "Erlegte verbotenes Rotwild in herzoglichen Forsten. Heinrich entscheidet: Gnade oder Auslieferung an den Vogt.",
                "Poached noble stag in lordly preserves. Henry chooses between showing mercy or handing him to the bailiff.",
                "🦌", CharacterCategory.DecisionChoice, "Spared", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_false_garrison", "Verräter der Burgwache Trosky", "Traitorous Trosky Garrison",
                "Infiltrierte Wachposten", "Infiltrated Sentries",
                "Burg Trosky", "Trosky Castle",
                "Plant heimliches Öffnen der Fallgitter. Heinrichs Ermittlung bestimmt, ob sie inhaftiert oder erschlagen werden.",
                "Plotted secret opening of castle portcullis. Henry's investigation determines capture or demise.",
                "🚪", CharacterCategory.DecisionChoice, "Spared", () => IsDirty = true));

            // 3. Master Craftsmen & Merchants (Sterbliche Meister mit Wiederbeleben / Revive Button)
            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_blacksmith_semin", "Waffenschmied von Semín", "Master Blacksmith of Semin",
                "Meisterschmied für Klingen & Harnische", "Master Bladesmith & Armorer",
                "Burg Semín", "Semin Castle",
                "Schmiedet erlesene böhmische Langschwerter und Rüstungen. Unverzichtbar für hochwertige Reparaturen.",
                "Forges renowned Bohemian longswords and plate armor. Essential for high-tier gear and repairs.",
                "⚒️", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_armorer_kuttenberg", "Rüstungsschmied von Kuttenberg", "Master Armorer of Kuttenberg",
                "Obermeister der Kuttenberger Plattnerzunft", "Guildmaster of Kuttenberg Armorers",
                "Kuttenberg", "Kuttenberg",
                "Führender Rüstungsmeister Böhmens mit den widerstandsfähigsten Mailänder und Nürnberger Harnischen.",
                "Renowned armor craftsman in Bohemia supplying resilient Milanese and Nuremberg plate suits.",
                "🛡️", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_herbalist_bozena", "Kräuterfrau Božena", "Herbalist Bozena",
                "Waldheilerin & Meister-Alchemistin", "Woodland Healer & Master Alchemist",
                "Trosky-Wälder", "Trosky Woods",
                "Kennerin der seltensten Waldkräuter und lebensrettender Arzneitränke. Verkauft wertvolle Rezepte.",
                "Keeper of rare botanical lore and life-saving medicinal brews. Sells precious recipes.",
                "🌿", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_fencing_master", "Fechtmeister Peter", "Fencing Master Peter",
                "Lehrer der Kuttenberger Fechtschule", "Teacher of Kuttenberg Fencing Guild",
                "Kuttenberg", "Kuttenberg",
                "Lehrt Gesellen die gefährlichsten Meisterschläge, Finten und Halbschwert-Techniken.",
                "Trains journeymen in deadly master strikes, feints, and half-sword combat techniques.",
                "🤺", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_gravedigger_sedlec", "Totengräber von Sedletz", "Gravedigger of Sedlec",
                "Hüter des Sedletzer Beinhauses", "Caretaker of Sedlec Ossuary",
                "Sedletz", "Sedlec",
                "Verwahrt historische Urkunden, Krypta-Schlüssel und Gebeine des Klosters Sedletz.",
                "Safeguards historic charters, crypt keys, and ancient ossuary relics of Sedlec Monastery.",
                "💀", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            CharacterFates.Add(new GameCharacterFateViewModel(
                "c_horse_breeder", "Pferdezüchter von Semín", "Horse Breeder of Semin",
                "Gestütsmeister edler Schlachtrösser", "Stud Master of Warhorses",
                "Burg Semín", "Semin Castle",
                "Züchtet die ausdauerndsten böhmischen Kaltblüter und Reitpferde der Trosky-Region.",
                "Breeds the most resilient Bohemian draught and warhorses in the Trosky realm.",
                "🐎", CharacterCategory.CraftsmanMaster, "Alive", () => IsDirty = true));

            SelectedCharacterFate = CharacterFates.FirstOrDefault();
        }


        private void LoadQuestsFromSave()
        {
            if (_handle == IntPtr.Zero) return;

            string? questData = NativeMethods.GetAllQuestsString(_handle);
            if (string.IsNullOrEmpty(questData)) return;

            var pairs = questData.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var questDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in pairs)
            {
                int eqIdx = pair.IndexOf('=');
                if (eqIdx > 0)
                {
                    string id = pair.Substring(0, eqIdx).Trim();
                    string val = pair.Substring(eqIdx + 1).Trim();
                    questDict[id] = val;
                }
            }

            // Extract active quest and objective from SelectedSaveItem manifest if available
            string? activeQuestKey = null;
            string? activeObjectiveKey = null;
            if (SelectedSaveItem != null && !string.IsNullOrEmpty(SelectedSaveItem.RawQuestKey))
            {
                var parts = SelectedSaveItem.RawQuestKey.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0) activeQuestKey = parts[0].Trim();
                if (parts.Length > 1) activeObjectiveKey = parts[1].Trim();
            }

            foreach (var q in Quests)
            {
                string? matchedStatus = null;
                if (questDict.TryGetValue(q.Id, out var s1)) matchedStatus = s1;
                else if (questDict.TryGetValue("_" + q.Id.TrimStart('_'), out var s2)) matchedStatus = s2;
                else if (questDict.TryGetValue(q.Id.TrimStart('_'), out var s3)) matchedStatus = s3;

                bool isActiveInHeader = false;
                if (!string.IsNullOrEmpty(activeQuestKey))
                {
                    if (!string.IsNullOrEmpty(q.UiTitleKey) && q.UiTitleKey.Equals(activeQuestKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isActiveInHeader = true;
                    }
                    else if (activeQuestKey.Contains(q.Id.TrimStart('_'), StringComparison.OrdinalIgnoreCase))
                    {
                        isActiveInHeader = true;
                    }
                }

                if (isActiveInHeader && (matchedStatus == null || matchedStatus.Equals("NotStarted", StringComparison.OrdinalIgnoreCase)))
                {
                    matchedStatus = "Active";
                }

                if (!string.IsNullOrEmpty(matchedStatus))
                {
                    string uiStatus = matchedStatus.ToLowerInvariant() switch
                    {
                        "active" => "aktiv",
                        "done" or "completed" => "abgeschlossen",
                        "failed" => "fehlgeschlagen",
                        "available" or "available_nomarker" => "verfügbar",
                        "notstarted" => "nicht_gestartet",
                        _ => matchedStatus
                    };
                    q.Status = uiStatus;
                    if (uiStatus == "abgeschlossen")
                    {
                        foreach (var obj in q.Objectives)
                        {
                            obj.IsDone = true;
                            obj.IsActive = false;
                        }
                        q.Stage = q.MaxStage;
                    }
                    else if (uiStatus == "aktiv")
                    {
                        bool markedActive = false;
                        foreach (var obj in q.Objectives)
                        {
                            if (!string.IsNullOrEmpty(activeObjectiveKey) &&
                                !string.IsNullOrEmpty(obj.UiKey) &&
                                obj.UiKey.Equals(activeObjectiveKey, StringComparison.OrdinalIgnoreCase))
                            {
                                obj.IsActive = true;
                                obj.IsDone = false;
                                markedActive = true;
                            }
                            else if (obj.IsDone)
                            {
                                obj.IsActive = false;
                            }
                        }
                        if (!markedActive && q.Objectives.Count > 0 && !q.Objectives.Any(o => o.IsActive || o.IsDone))
                        {
                            q.Objectives[0].IsActive = true;
                        }
                        q.UpdateStageFromObjectives();
                    }
                    else // "nicht_gestartet", "verfügbar"
                    {
                        foreach (var obj in q.Objectives)
                        {
                            obj.IsDone = false;
                            obj.IsActive = false;
                        }
                        q.Stage = 0;
                    }
                }
                else
                {
                    q.Status = "nicht_gestartet";
                    foreach (var obj in q.Objectives)
                    {
                        obj.IsDone = false;
                        obj.IsActive = false;
                    }
                    q.Stage = 0;
                }
            }

            foreach (var kvp in questDict)
            {
                string rawId = kvp.Key;
                string normId = rawId.TrimStart('_');
                bool exists = Quests.Any(q => q.Id.Equals(rawId, StringComparison.OrdinalIgnoreCase) ||
                                              q.Id.TrimStart('_').Equals(normId, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    string uiStatus = kvp.Value.ToLowerInvariant() switch
                    {
                        "active" => "aktiv",
                        "done" or "completed" => "abgeschlossen",
                        "failed" => "fehlgeschlagen",
                        "available" or "available_nomarker" => "verfügbar",
                        "notstarted" => "nicht_gestartet",
                        _ => kvp.Value
                    };
                    if (uiStatus != "nicht_gestartet")
                    {
                        string prettyName = HumanizeQuestId(rawId);
                        var dynamicQ = new QuestItemViewModel(rawId, "haupt", prettyName, prettyName, "Böhmen", "Bohemia", uiStatus, uiStatus == "abgeschlossen" ? 1 : 0, 1, null, null, "Trosecko", false, null, null);
                        dynamicQ.Objectives.Add(new QuestObjectiveViewModel(
                            IsGerman ? "Aktuellen Quest-Fortschritt verfolgen" : "Track current quest progress",
                            "Track current quest progress",
                            uiStatus == "abgeschlossen",
                            uiStatus == "aktiv",
                            () =>
                            {
                                dynamicQ.UpdateStageFromObjectives();
                                if (_handle != IntPtr.Zero)
                                {
                                    NativeMethods.kcd2_save_set_quest_status(_handle, dynamicQ.Id, dynamicQ.Status);
                                }
                                IsDirty = true;
                            }));
                        Quests.Add(dynamicQ);
                    }
                }
            }

            FilteredQuests.Refresh();
            var activeQuest = Quests.FirstOrDefault(q => q.Status == "aktiv") ?? Quests.FirstOrDefault();
            if (activeQuest != null)
            {
                SelectedQuest = activeQuest;
            }
        }

        private void SaveQuestsToSave()



        {



            if (_handle == IntPtr.Zero) return;



            foreach (var q in Quests)



            {



                NativeMethods.kcd2_save_set_quest_status(_handle, q.Id, q.Status);



            }



        }







        private static string HumanizeQuestId(string rawId)



        {



            string clean = rawId.TrimStart('_');



            var sb = new StringBuilder();



            for (int i = 0; i < clean.Length; i++)



            {



                if (i > 0 && char.IsUpper(clean[i]) && !char.IsUpper(clean[i - 1]))



                {



                    sb.Append(' ');



                }



                sb.Append(clean[i]);



            }



            return sb.ToString();



        }







        private void InitializeCompanionAndHerbs()



        {



            DogPerks.Clear();



            DogPerks.Add(new DogPerkItemViewModel("dp_hunt", "Jagdhund (Wild aufspüren)", "Hunting Dog (Track game)", true));



            DogPerks.Add(new DogPerkItemViewModel("dp_alarm", "Wachhund (Warnt vor Hinterhalten)", "Watchdog (Warns of ambushes)", true));



            DogPerks.Add(new DogPerkItemViewModel("dp_bite", "Verbeißen (Hält Feinde im Kampf)", "Clinch (Holds enemies in combat)", true));



            DogPerks.Add(new DogPerkItemViewModel("dp_heal", "Gefährten-Trost (+Charisma für Heinrich)", "Companion Solace (+Charisma for Henry)", true));







            Herbs.Clear();



            Herbs.Add(new HerbItemViewModel("h_belladonna", "Tollkirsche (Belladonna)", "Belladonna (Deadly Nightshade)", "🌸", 45));



            Herbs.Add(new HerbItemViewModel("h_nettle", "Brennnessel (Nettle)", "Stinging Nettle", "🌿", 120));



            Herbs.Add(new HerbItemViewModel("h_marigold", "Ringelblume (Marigold)", "Marigold", "🌼", 85));



            Herbs.Add(new HerbItemViewModel("h_chamomile", "Kamille (Chamomile)", "Chamomile", "☀️", 95));



            Herbs.Add(new HerbItemViewModel("h_valerian", "Baldrian (Valerian)", "Valerian", "✨", 38));



            Herbs.Add(new HerbItemViewModel("h_st_john", "Johanniskraut (St. John)", "St. John's Wort", "🌸", 64));



            Herbs.Add(new HerbItemViewModel("h_sage", "Salbei (Sage)", "Sage", "🍃", 42));



            Herbs.Add(new HerbItemViewModel("h_mint", "Minze (Mint)", "Mint", "💨", 77));



            Herbs.Add(new HerbItemViewModel("h_wormwood", "Wermut (Wormwood)", "Wormwood", "🔥", 29));



            Herbs.Add(new HerbItemViewModel("h_eyebright", "Augentrost (Eyebright)", "Eyebright", "👁️", 50));







            CraftingRecipes.Clear();



            AlchemyRecipes.Clear();



            foreach (var meta in GameDataService.Instance.LoadedRecipes)



            {



                var rec = new CraftingRecipeViewModel(



                    meta.Id,



                    meta.RecipeType,



                    meta.Category,



                    meta.UiNameKey,



                    meta.UiDescKey,



                    meta.FallbackNameDe,



                    meta.FallbackNameEn,



                    meta.IngredientsDe,



                    meta.IngredientsEn,



                    meta.BaseMaterial,



                    meta.MinSkillLevel,



                    meta.PerkId,



                    meta.Icon,



                    true);







                rec.OnUnlockedChanged = () =>



                {



                    IsDirty = true;



                    StatusMessage = IsGerman



                        ? $"Rezept '{rec.DisplayName}': {(rec.IsUnlocked ? "Gelernt" : "Verlernt")}"



                        : $"Recipe '{rec.DisplayName}': {(rec.IsUnlocked ? "Learned" : "Unlearned")}";



                };







                CraftingRecipes.Add(rec);



                if (meta.RecipeType == "Alchemy")



                {



                    AlchemyRecipes.Add(new AlchemyRecipeViewModel(meta.Id, meta.FallbackNameDe, meta.FallbackNameEn, meta.IngredientsDe, meta.IngredientsEn, true));



                }



            }







            CraftingPerks.Clear();



            foreach (var meta in GameDataService.Instance.LoadedCraftingPerks)



            {



                var perk = new CraftingSkillPerkViewModel(



                    meta.PerkId,



                    meta.SkillType,



                    meta.UiNameKey,



                    meta.UiDescKey,



                    meta.FallbackNameDe,



                    meta.FallbackNameEn,



                    meta.FallbackDescDe,



                    meta.FallbackDescEn,



                    meta.Level,



                    meta.Icon,



                    true);







                perk.OnUnlockedChanged = () =>



                {



                    IsDirty = true;



                    StatusMessage = IsGerman



                        ? $"Handwerkstalent '{perk.DisplayName}': {(perk.IsUnlocked ? "Freigeschaltet" : "Gesperrt")}"



                        : $"Craft perk '{perk.DisplayName}': {(perk.IsUnlocked ? "Unlocked" : "Locked")}";



                };







                CraftingPerks.Add(perk);



            }



        }







        private void UnlockAllCrafting()



        {



            foreach (var r in CraftingRecipes)



            {



                if (SelectedCraftFilter == "Alchemy" && r.RecipeType != "Alchemy") continue;



                if (SelectedCraftFilter == "Blacksmithing" && r.RecipeType != "Blacksmithing") continue;



                r.IsUnlocked = true;



            }



            FilteredCraftingRecipes?.Refresh();



            IsDirty = true;



            StatusMessage = IsGerman



                ? "Alle Rezepte im ausgewählten Filter erfolgreich gelernt!"



                : "All recipes in selected filter successfully learned!";



        }







        private void LockAllCrafting()



        {



            foreach (var r in CraftingRecipes)



            {



                if (SelectedCraftFilter == "Alchemy" && r.RecipeType != "Alchemy") continue;



                if (SelectedCraftFilter == "Blacksmithing" && r.RecipeType != "Blacksmithing") continue;



                r.IsUnlocked = false;



            }



            FilteredCraftingRecipes?.Refresh();



            IsDirty = true;



            StatusMessage = IsGerman



                ? "Alle Rezepte im ausgewählten Filter verlernt (gesperrt)!"



                : "All recipes in selected filter locked (unlearned)!";



        }







        private void UnlockAllCraftingPerks()



        {



            foreach (var p in CraftingPerks) p.IsUnlocked = true;



            FilteredCraftingPerks?.Refresh();



            IsDirty = true;



            StatusMessage = IsGerman



                ? "Alle Schmiede- & Alchemie-Talente freigeschaltet!"



                : "All blacksmith and alchemy perks unlocked!";



        }







        private void LockAllCraftingPerks()



        {



            foreach (var p in CraftingPerks) p.IsUnlocked = false;



            FilteredCraftingPerks?.Refresh();



            IsDirty = true;



            StatusMessage = IsGerman



                ? "Alle Schmiede- & Alchemie-Talente zurückgesetzt!"



                : "All blacksmith and alchemy perks reset!";



        }







        private void RepairAllItems()



        {



            foreach (var item in InventoryItems)



            {



                item.Condition = 1.0f;



                item.Blood = 0;



            }



            foreach (var s in LeftPaperdollSlots) s.HasBlood = false;



            foreach (var s in RightPaperdollSlots) s.HasBlood = false;



            StatusMessage = IsGerman ? "Alle Gegenstände auf 100% Zustand repariert & gereinigt!" : "All items restored to 100% condition & cleaned!";



        }







        private void ClearStolenFlags()



        {



            foreach (var item in InventoryItems)



            {



                item.IsStolen = false;



            }



            StatusMessage = IsGerman ? "Diebesgut-Markierung von allen Gegenständen entfernt!" : "Removed stolen mark from all inventory items!";



        }







        private void CleanCharacterDirt()



        {



            foreach (var s in LeftPaperdollSlots) s.HasBlood = false;



            foreach (var s in RightPaperdollSlots) s.HasBlood = false;



            foreach (var item in InventoryItems)



            {



                if (item.IsEquipped) item.Blood = 0;



            }



            StatusMessage = IsGerman ? "Blut & Schmutz von Heinrichs Rüstung gewaschen!" : "Washed blood & dirt from Henry's armour!";



        }







        private void SwitchOutfit(object? param)
        {
            if (param != null && int.TryParse(param.ToString(), out int num) && num >= 1 && num <= 3)
            {
                ActiveOutfit = num;
                ApplyOutfit(num);
                RecalculateCharacterStats();

                string name = num switch
                {
                    1 => IsGerman ? "Outfit I (Kampf)" : "Outfit I (Combat)",
                    2 => IsGerman ? "Outfit II (Heimlichkeit)" : "Outfit II (Stealth)",
                    3 => IsGerman ? "Outfit III (Adelig)" : "Outfit III (Noble)",
                    _ => $"Outfit {num}"
                };

                StatusMessage = IsGerman ? $"Auf {name} gewechselt." : $"Switched to {name}.";
            }
        }

        private void ApplyOutfit(int num)
        {
            foreach (var s in LeftPaperdollSlots) s.ApplyOutfit(num);
            foreach (var s in RightPaperdollSlots) s.ApplyOutfit(num);

            OnPropertyChanged(nameof(CurrentOutfitTitle));
            OnPropertyChanged(nameof(CurrentOutfitPedestal));
            OnPropertyChanged(nameof(IsOutfit1Active));
            OnPropertyChanged(nameof(IsOutfit2Active));
            OnPropertyChanged(nameof(IsOutfit3Active));
        }

        private void SwitchHorseOutfit(object? param)
        {
            if (param != null && int.TryParse(param.ToString(), out int num) && num >= 1 && num <= 3)
            {
                ActiveHorseOutfit = num;
                ApplyHorseOutfit(num);
                RecalculateHorseStats();

                string name = num switch
                {
                    1 => IsGerman ? "Sattelzeug I (Streitross)" : "Tack I (Warhorse)",
                    2 => IsGerman ? "Sattelzeug II (Reise & Tempo)" : "Tack II (Travel & Speed)",
                    3 => IsGerman ? "Sattelzeug III (Prunk & Adel)" : "Tack III (Noble Courser)",
                    _ => $"Tack {num}"
                };

                StatusMessage = IsGerman ? $"Auf {name} gewechselt." : $"Switched to {name}.";
            }
        }

        private void ApplyHorseOutfit(int num)
        {
            foreach (var s in HorseEquipmentSlots) s.ApplyOutfit(num);

            OnPropertyChanged(nameof(CurrentHorseOutfitTitle));
            OnPropertyChanged(nameof(CurrentHorseOutfitBadge));
            OnPropertyChanged(nameof(IsHorseOutfit1Active));
            OnPropertyChanged(nameof(IsHorseOutfit2Active));
            OnPropertyChanged(nameof(IsHorseOutfit3Active));
        }







        private void SetRightView(object? param)



        {



            if (param != null)



            {



                SelectedRightView = param.ToString() ?? "Split";



            }



        }







        private void DuplicateSelectedItem()



        {



            if (SelectedInventoryItem == null) return;



            var orig = SelectedInventoryItem.ToNativeItem();



            orig.UniqueId = (ulong)DateTime.UtcNow.Ticks;



            if (_handle != IntPtr.Zero)



            {



                NativeMethods.kcd2_save_add_inventory_item(_handle, in orig);



            }



            var copyVm = new InventoryItemViewModel(orig, OnInventoryItemChanged)



            {



                ItemUiName = SelectedInventoryItem.ItemUiName + (IsGerman ? " (Kopie)" : " (Copy)"),



                Amount = SelectedInventoryItem.Amount,



                Condition = SelectedInventoryItem.Condition,



                Price = SelectedInventoryItem.Price,



                Power = SelectedInventoryItem.Power,



                Tier = SelectedInventoryItem.Tier,



                Blood = SelectedInventoryItem.Blood,



                IsStolen = SelectedInventoryItem.IsStolen



            };



            InventoryItems.Insert(0, copyVm);



            SelectedInventoryItem = copyVm;



            UpdateCategoryTabCounts();



            StatusMessage = IsGerman ? $"Item '{copyVm.ItemUiName}' dupliziert!" : $"Item '{copyVm.ItemUiName}' duplicated!";



        }







        private void DeleteSelectedItem()



        {



            if (SelectedInventoryItem == null) return;



            var item = SelectedInventoryItem;



            if (_handle != IntPtr.Zero)



            {



                NativeMethods.kcd2_save_remove_inventory_item(_handle, item.UniqueId);



            }



            InventoryItems.Remove(item);



            SelectedInventoryItem = InventoryItems.FirstOrDefault();



            UpdateCategoryTabCounts();



            StatusMessage = IsGerman ? $"Item '{item.ItemUiName}' entfernt." : $"Item '{item.ItemUiName}' removed.";



        }







        private void SelectCodexPreset(string? preset)



        {



            if (string.IsNullOrEmpty(preset)) return;



            SelectedCodexPreset = preset;



            switch (preset)



            {



                case "heinrich_sword":



                    NewItemName = IsGerman ? "Heinrichs Meister-Langschwert" : "Henry's Master Longsword";



                    NewItemCategory = "Waffen";



                    NewItemQuantity = 1;



                    NewItemPower = 165;



                    NewItemWeight = 3.1f;



                    NewItemPrice = 2450;



                    break;



                case "kuttenberg_plate":



                    NewItemName = IsGerman ? "Kuttenberger Fürsten-Kürass" : "Kuttenberg Prince's Cuirass";



                    NewItemCategory = "Rüstung";



                    NewItemQuantity = 1;



                    NewItemPower = 168;



                    NewItemWeight = 16.0f;



                    NewItemPrice = 4200;



                    break;



                case "saviour_schnapps":



                    NewItemName = "Retterschnaps (Saviour Schnapps)";



                    NewItemCategory = "Nahrung";



                    NewItemQuantity = 25;



                    NewItemPower = 100;



                    NewItemWeight = 0.5f;



                    NewItemPrice = 50;



                    break;



                case "black_powder":



                    NewItemName = IsGerman ? "Feines Büchsenpulver & Kugeln" : "Fine Gunpowder & Bullets";



                    NewItemCategory = "Waffen";



                    NewItemQuantity = 50;



                    NewItemPower = 300;



                    NewItemWeight = 0.1f;



                    NewItemPrice = 65;



                    break;



                case "capon_bow":



                    NewItemName = IsGerman ? "Hans Capons Jagdbogen" : "Sir Hans Capon's Hunting Bow";



                    NewItemCategory = "Waffen";



                    NewItemQuantity = 1;



                    NewItemPower = 140;



                    NewItemWeight = 1.8f;



                    NewItemPrice = 890;



                    break;



            }



        }







        private void OpenAddItemModal()



        {



            if (AllCodexItems.Count == 0)



            {



                var items = GameDataService.Instance.GetAllCodexItems();



                foreach (var it in items)



                {



                    AllCodexItems.Add(it);



                }



            }







            if (SelectedCodexItem == null && AllCodexItems.Count > 0)



            {



                SelectedCodexItem = AllCodexItems[0];



            }







            SpawnQuantity = 1;



            SpawnCondition = 100;



            SpawnIsStolen = false;



            IsAddItemModalOpen = true;



        }







        private bool FilterCodexItem(object obj)



        {



            if (obj is not GameItemCodexDefinition item) return false;







            if (CodexCategoryFilter != "All")



            {



                if (!item.RawCategory.Equals(CodexCategoryFilter, StringComparison.OrdinalIgnoreCase))



                {



                    return false;



                }



            }







            switch (CodexWorthFilter)



            {



                case "Under50":



                    if (item.Price >= 50) return false;



                    break;



                case "50to500":



                    if (item.Price < 50 || item.Price > 500) return false;



                    break;



                case "500to2500":



                    if (item.Price < 500 || item.Price > 2500) return false;



                    break;



                case "Above2500":



                    if (item.Price <= 2500) return false;



                    break;



            }







            if (!string.IsNullOrWhiteSpace(CodexSearchFilter))



            {



                string s = CodexSearchFilter.Trim().ToLowerInvariant();



                bool mName = item.DisplayName.ToLowerInvariant().Contains(s);



                bool mGuid = item.Guid.ToLowerInvariant().Contains(s);



                bool mInternal = item.InternalName.ToLowerInvariant().Contains(s);



                bool mTag = item.Tag.ToLowerInvariant().Contains(s);



                return mName || mGuid || mInternal || mTag;



            }







            return true;



        }







        private void ApplyCodexSorting()



        {



            if (FilteredCodexItems == null) return;



            FilteredCodexItems.SortDescriptions.Clear();



            switch (CodexSortOption)



            {



                case "NameDesc":



                    FilteredCodexItems.SortDescriptions.Add(new SortDescription(nameof(GameItemCodexDefinition.DisplayName), ListSortDirection.Descending));



                    break;



                case "PriceDesc":



                    FilteredCodexItems.SortDescriptions.Add(new SortDescription(nameof(GameItemCodexDefinition.Price), ListSortDirection.Descending));



                    break;



                case "PriceAsc":



                    FilteredCodexItems.SortDescriptions.Add(new SortDescription(nameof(GameItemCodexDefinition.Price), ListSortDirection.Ascending));



                    break;



                case "WeightDesc":



                    FilteredCodexItems.SortDescriptions.Add(new SortDescription(nameof(GameItemCodexDefinition.Weight), ListSortDirection.Descending));



                    break;



                default:



                    FilteredCodexItems.SortDescriptions.Add(new SortDescription(nameof(GameItemCodexDefinition.DisplayName), ListSortDirection.Ascending));



                    break;



            }



        }







        private void ConfirmAddItem()



        {



            if (SelectedCodexItem == null) return;







            int targetSlot = SpawnTargetStorage switch



            {



                "Stash" => 200,



                "Horse" => 100,



                _ => 0



            };







            var native = new Kcd2InventoryItem



            {



                ItemId = SelectedCodexItem.Guid,



                ItemUiName = SelectedCodexItem.DisplayName,



                UniqueId = (ulong)DateTime.UtcNow.Ticks,



                Amount = Math.Max(1, SpawnQuantity),



                Condition = Math.Clamp(SpawnCondition / 100.0f, 0.0f, 1.0f),



                SlotId = targetSlot,



                Flags = SpawnIsStolen ? 0x02u : 0x00u



            };







            if (_handle != IntPtr.Zero)



            {



                NativeMethods.kcd2_save_add_inventory_item(_handle, in native);



            }







            var itemVm = new InventoryItemViewModel(native, OnInventoryItemChanged)



            {



                Category = SelectedCodexItem.Category,



                Weight = SelectedCodexItem.Weight,



                Price = SelectedCodexItem.Price,



                Tier = 3



            };







            InventoryItems.Insert(0, itemVm);



            SelectedInventoryItem = itemVm;



            UpdateCategoryTabCounts();



            FilteredInventory.Refresh();



            IsDirty = true;



            IsAddItemModalOpen = false;







            string targetName = SpawnTargetStorage switch



            {



                "Stash" => IsGerman ? "Spieler-Truhe (Stash)" : "Player Stash",



                "Horse" => IsGerman ? "Pferd" : "Horse",



                _ => IsGerman ? "Spieler-Inventar" : "Player Inventory"



            };







            StatusMessage = IsGerman 



                ? $"'{itemVm.ItemUiName}' ({itemVm.Amount}x) zu {targetName} hinzugefügt!" 



                : $"Added '{itemVm.ItemUiName}' ({itemVm.Amount}x) to {targetName}!";



        }







        #endregion







        public void Dispose()



        {



            CloseCurrentSession();



        }



    }



}




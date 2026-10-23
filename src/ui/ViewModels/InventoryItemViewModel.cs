using System;
using System.Windows.Input;
using KCD2SaveEditor.UI.Interop;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public enum ItemStorageLocation
    {
        Inventory, // SlotId < 100
        Horse,     // SlotId >= 100 && SlotId < 200
        Stash      // SlotId >= 200
    }

    public class InventoryItemViewModel : ViewModelBase
    {
        private readonly Action<InventoryItemViewModel>? _onChanged;
        private string _itemId;
        private string _itemUiName;
        private ulong _uniqueId;
        private int _amount;
        private float _condition;
        private int _slotId;
        private uint _flags;
        private string _category;
        private string _subCategory;
        private string _glyph;

        private float _weight;
        private float _price;
        private int _power;
        private int _tier;
        private int _blood;
        private bool _isStolen;
        private bool _outfit1;
        private bool _outfit2;
        private bool _outfit3;

        public InventoryItemViewModel(Kcd2InventoryItem item, Action<InventoryItemViewModel>? onChanged = null)
        {
            _onChanged = onChanged;
            _itemId = item.ItemId ?? string.Empty;
            _uniqueId = item.UniqueId;
            _amount = item.Amount;
            _condition = item.Condition;
            _slotId = item.SlotId;
            _flags = item.Flags;

            if (_slotId >= 100 && _slotId < 200)
            {
                _category = "Pferd";
                _itemUiName = GameDataService.Instance.GetItemName(_itemId, item.ItemUiName);
            }
            else
            {
                _itemUiName = GameDataService.Instance.GetItemName(_itemId, item.ItemUiName);
                _category = GameDataService.Instance.GetItemCategory(_itemId, DetermineCategory(_itemId, _itemUiName, _slotId));
            }

            _subCategory = DetermineSubCategory(_category, _itemUiName);
            _glyph = DetermineGlyph(_category, _itemUiName);

            var meta = GameDataService.Instance.GetItemMetadata(_itemId);
            if (meta != null)
            {
                _weight = meta.Weight > 0 ? meta.Weight : 1.0f;
                _price = meta.Price > 0 ? meta.Price : 50f;
            }
            else
            {
                _weight = 1.0f;
                _price = 50f;
            }

            _tier = DetermineTier(_price, _category);
            _power = DeterminePower(_category, _subCategory, _tier);
            _blood = (int)Math.Clamp((1.0f - _condition) * 80f, 0, 100);
            _isStolen = (_flags & 0x02) != 0;
            _outfit1 = _slotId != 0;

            AddAmountCommand = new RelayCommand(param => AddAmount(param));
            SetAmountCommand = new RelayCommand(param => SetAmount(param));
            MoveToStashCommand = new RelayCommand(() => StorageLocation = ItemStorageLocation.Stash);
            MoveToInventoryCommand = new RelayCommand(() => StorageLocation = ItemStorageLocation.Inventory);
            MoveToHorseCommand = new RelayCommand(() => StorageLocation = ItemStorageLocation.Horse);
        }

        public void RefreshLocalization()
        {
            ItemUiName = GameDataService.Instance.GetItemName(ItemId, ItemUiName);
            if (SlotId >= 100 && SlotId < 200)
            {
                Category = "Pferd";
            }
            else
            {
                Category = GameDataService.Instance.GetItemCategory(ItemId, DetermineCategory(ItemId, ItemUiName, SlotId));
            }
            SubCategory = DetermineSubCategory(Category, ItemUiName);
            Glyph = DetermineGlyph(Category, ItemUiName);

            var meta = GameDataService.Instance.GetItemMetadata(ItemId);
            if (meta != null)
            {
                Weight = meta.Weight > 0 ? meta.Weight : Weight;
                Price = meta.Price > 0 ? meta.Price : Price;
            }

            OnPropertyChanged(nameof(ItemUiName));
            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(SubCategory));
            OnPropertyChanged(nameof(FormattedAmount));
            OnPropertyChanged(nameof(ConditionPercent));
            OnPropertyChanged(nameof(ConditionPercentInt));
            OnPropertyChanged(nameof(FormattedWeight));
            OnPropertyChanged(nameof(FormattedPrice));
            OnPropertyChanged(nameof(StorageLocationName));
            OnPropertyChanged(nameof(StorageLocationBadge));
        }

        public string ItemId
        {
            get => _itemId;
            set => SetProperty(ref _itemId, value);
        }

        public string ShortUuid => _uniqueId != 0 ? $"0x{_uniqueId:X8}" : (_itemId.Length >= 8 ? $"0x{_itemId[..8].ToUpperInvariant()}" : "0x00000000");

        public string ItemUiName
        {
            get => _itemUiName;
            set => SetProperty(ref _itemUiName, value);
        }

        public ulong UniqueId
        {
            get => _uniqueId;
            set
            {
                if (SetProperty(ref _uniqueId, value))
                {
                    OnPropertyChanged(nameof(ShortUuid));
                }
            }
        }

        public int Amount
        {
            get => _amount;
            set
            {
                int clamped = Math.Max(0, value);
                if (SetProperty(ref _amount, clamped))
                {
                    OnPropertyChanged(nameof(FormattedAmount));
                    _onChanged?.Invoke(this);
                }
            }
        }

        public string FormattedAmount => Amount > 1 ? Amount.ToString("N0") : string.Empty;

        public float Condition
        {
            get => _condition;
            set
            {
                float clamped = Math.Clamp(value, 0.0f, 1.0f);
                if (SetProperty(ref _condition, clamped))
                {
                    OnPropertyChanged(nameof(ConditionPercent));
                    OnPropertyChanged(nameof(ConditionPercentInt));
                    OnPropertyChanged(nameof(ConditionColor));
                    _onChanged?.Invoke(this);
                }
            }
        }

        public int ConditionPercentInt
        {
            get => (int)Math.Round(Condition * 100);
            set => Condition = value / 100.0f;
        }

        public string ConditionPercent => $"{(int)Math.Round(Condition * 100)}";

        public string ConditionColor
        {
            get
            {
                int pct = (int)Math.Round(Condition * 100);
                if (pct >= 85) return "#4ADE80"; // Emerald 400
                if (pct >= 65) return "#FCD34D"; // Amber 300
                return "#FB7185"; // Rose 400
            }
        }

        public int SlotId
        {
            get => _slotId;
            set
            {
                if (SetProperty(ref _slotId, value))
                {
                    _outfit1 = value > 0 && value < 100;
                    if (_slotId >= 100 && _slotId < 200)
                    {
                        Category = "Pferd";
                    }
                    else
                    {
                        Category = GameDataService.Instance.GetItemCategory(ItemId, DetermineCategory(ItemId, ItemUiName, _slotId));
                    }
                    SubCategory = DetermineSubCategory(Category, ItemUiName);
                    Glyph = DetermineGlyph(Category, ItemUiName);
                    OnPropertyChanged(nameof(Outfit1));
                    OnPropertyChanged(nameof(IsEquipped));
                    OnPropertyChanged(nameof(StorageLocation));
                    OnPropertyChanged(nameof(StorageLocationName));
                    OnPropertyChanged(nameof(StorageLocationBadge));
                    OnPropertyChanged(nameof(StorageBadgeBackground));
                    OnPropertyChanged(nameof(StorageBadgeBorder));
                    OnPropertyChanged(nameof(StorageBadgeForeground));
                    OnPropertyChanged(nameof(IsInStash));
                    OnPropertyChanged(nameof(IsInInventory));
                    OnPropertyChanged(nameof(IsInHorse));
                    _onChanged?.Invoke(this);
                }
            }
        }

        public ItemStorageLocation StorageLocation
        {
            get
            {
                if (_slotId >= 200) return ItemStorageLocation.Stash;
                if (_slotId >= 100) return ItemStorageLocation.Horse;
                return ItemStorageLocation.Inventory;
            }
            set
            {
                int newSlot = value switch
                {
                    ItemStorageLocation.Stash => 200,
                    ItemStorageLocation.Horse => 100,
                    _ => 0
                };
                SlotId = newSlot;
            }
        }

        public string StorageLocationName => StorageLocation switch
        {
            ItemStorageLocation.Stash => GameDataService.Instance.GetUiText("Storage_Stash", "📦 Truhe (Stash)", "📦 Stash"),
            ItemStorageLocation.Horse => GameDataService.Instance.GetUiText("Storage_Horse", "🐎 Pferd", "🐎 Horse"),
            _ => GameDataService.Instance.GetUiText("Storage_Inventory", "🎒 Spieler-Inventar", "🎒 Player Inventory")
        };

        public string StorageLocationBadge => StorageLocation switch
        {
            ItemStorageLocation.Stash => GameDataService.Instance.GetUiText("Badge_Stash", "📦 Truhe", "📦 Stash"),
            ItemStorageLocation.Horse => GameDataService.Instance.GetUiText("Badge_Horse", "🐎 Pferd", "🐎 Horse"),
            _ => GameDataService.Instance.GetUiText("Badge_Henry", "🎒 Heinrich", "🎒 Henry")
        };

        public string StorageBadgeBackground => StorageLocation switch
        {
            ItemStorageLocation.Stash => "#281D3B",
            ItemStorageLocation.Horse => "#2A2318",
            _ => "#162821"
        };

        public string StorageBadgeBorder => StorageLocation switch
        {
            ItemStorageLocation.Stash => "#A855F7",
            ItemStorageLocation.Horse => "#F59E0B",
            _ => "#10B981"
        };

        public string StorageBadgeForeground => StorageLocation switch
        {
            ItemStorageLocation.Stash => "#E9D5FF",
            ItemStorageLocation.Horse => "#FDE68A",
            _ => "#A7F3D0"
        };

        public bool IsInStash => StorageLocation == ItemStorageLocation.Stash;
        public bool IsInInventory => StorageLocation == ItemStorageLocation.Inventory;
        public bool IsInHorse => StorageLocation == ItemStorageLocation.Horse;

        public ICommand MoveToStashCommand { get; }
        public ICommand MoveToInventoryCommand { get; }
        public ICommand MoveToHorseCommand { get; }

        public uint Flags
        {
            get => _flags;
            set => SetProperty(ref _flags, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string SubCategory
        {
            get => _subCategory;
            set => SetProperty(ref _subCategory, value);
        }

        public string Glyph
        {
            get => _glyph;
            set => SetProperty(ref _glyph, value);
        }

        public float Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        public string FormattedWeight => _weight > 0 ? _weight.ToString("0.0") : "—";

        public float Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public string FormattedPrice => _price > 0 ? _price.ToString("N0") : "—";

        public int Power
        {
            get => _power;
            set => SetProperty(ref _power, value);
        }

        public string FormattedPower => _power > 0 ? _power.ToString() : "—";

        public int Tier
        {
            get => _tier;
            set
            {
                if (SetProperty(ref _tier, value))
                {
                    OnPropertyChanged(nameof(TierDisplay));
                }
            }
        }

        public string TierDisplay => _tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "M",
            _ => string.Empty
        };

        public int Blood
        {
            get => _blood;
            set
            {
                if (SetProperty(ref _blood, Math.Clamp(value, 0, 100)))
                {
                    OnPropertyChanged(nameof(HasBlood));
                }
            }
        }

        public bool HasBlood => _blood > 0;

        public bool IsStolen
        {
            get => _isStolen;
            set
            {
                if (SetProperty(ref _isStolen, value))
                {
                    if (value) _flags |= 0x02;
                    else _flags &= ~0x02u;
                    _onChanged?.Invoke(this);
                }
            }
        }

        public bool IsEquipped => _outfit1 || _outfit2 || _outfit3 || _slotId != 0;

        public bool Outfit1
        {
            get => _outfit1;
            set
            {
                if (SetProperty(ref _outfit1, value))
                {
                    OnPropertyChanged(nameof(IsEquipped));
                }
            }
        }

        public bool Outfit2
        {
            get => _outfit2;
            set
            {
                if (SetProperty(ref _outfit2, value))
                {
                    OnPropertyChanged(nameof(IsEquipped));
                }
            }
        }

        public bool Outfit3
        {
            get => _outfit3;
            set
            {
                if (SetProperty(ref _outfit3, value))
                {
                    OnPropertyChanged(nameof(IsEquipped));
                }
            }
        }

        public ICommand AddAmountCommand { get; }
        public ICommand SetAmountCommand { get; }

        public void AddAmount(object? param)
        {
            if (param != null && int.TryParse(param.ToString(), out int val))
            {
                Amount = Math.Max(0, Amount + val);
            }
        }

        public void SetAmount(object? param)
        {
            if (param != null && int.TryParse(param.ToString(), out int val))
            {
                Amount = Math.Max(0, val);
            }
        }

        public Kcd2InventoryItem ToNativeItem()
        {
            return new Kcd2InventoryItem
            {
                ItemId = _itemId,
                ItemUiName = _itemUiName,
                UniqueId = _uniqueId,
                Amount = _amount,
                Condition = _condition,
                SlotId = _slotId,
                Flags = _flags
            };
        }

        private static string DetermineCategory(string guid, string name, int slotId = 0)
        {
            if (slotId >= 100 && slotId < 200) return "Pferd";

            string lowerName = name.ToLowerInvariant();

            // Horse Tack & Equipment
            if (lowerName.Contains("pferd") || lowerName.Contains("horse") ||
                lowerName.Contains("sattel") || lowerName.Contains("saddle") ||
                lowerName.Contains("zaumzeug") || lowerName.Contains("bridle") ||
                lowerName.Contains("hufeisen") || lowerName.Contains("horseshoe") ||
                lowerName.Contains("schabracke") || lowerName.Contains("caparison") ||
                lowerName.Contains("harness") || lowerName.Contains("pferdedecke") ||
                lowerName.Contains("pferdepanzer") || lowerName.Contains("sporen") ||
                lowerName.Contains("spurs"))
            {
                return "Pferd";
            }

            // Weapons
            if (lowerName.Contains("schwert") || lowerName.Contains("sword") ||
                lowerName.Contains("kanone") || lowerName.Contains("cannon") ||
                lowerName.Contains("gewehr") || lowerName.Contains("rifle") ||
                lowerName.Contains("bogen") || lowerName.Contains("bow") ||
                lowerName.Contains("pfeil") || lowerName.Contains("arrow") ||
                lowerName.Contains("kugel") || lowerName.Contains("bullet") ||
                lowerName.Contains("dolch") || lowerName.Contains("dagger") ||
                lowerName.Contains("fackel") || lowerName.Contains("torch") ||
                lowerName.Contains("streitaxt") || lowerName.Contains("axe") ||
                lowerName.Contains("streitkolben") || lowerName.Contains("mace"))
            {
                return "Waffen";
            }

            // Armor
            if (lowerName.Contains("harnisch") || lowerName.Contains("cuirass") ||
                lowerName.Contains("helm") || lowerName.Contains("helmet") ||
                lowerName.Contains("armschienen") || lowerName.Contains("vambraces") ||
                lowerName.Contains("beinlinge") || lowerName.Contains("chausses") ||
                lowerName.Contains("wappenrock") || lowerName.Contains("jupon") ||
                lowerName.Contains("schuhe") || lowerName.Contains("stiefel") || lowerName.Contains("boots") ||
                lowerName.Contains("hosen") || lowerName.Contains("hose") ||
                lowerName.Contains("gugel") || lowerName.Contains("coif") ||
                lowerName.Contains("schild") || lowerName.Contains("shield") ||
                lowerName.Contains("ring") || lowerName.Contains("halskette"))
            {
                return "Rüstung";
            }

            // Food & Drink
            if (lowerName.Contains("bier") || lowerName.Contains("beer") ||
                lowerName.Contains("wein") || lowerName.Contains("wine") ||
                lowerName.Contains("fleisch") || lowerName.Contains("meat") ||
                lowerName.Contains("brot") || lowerName.Contains("bread") ||
                lowerName.Contains("wurst") || lowerName.Contains("sausage") ||
                lowerName.Contains("käse") || lowerName.Contains("kaese") || lowerName.Contains("cheese") ||
                lowerName.Contains("apfel") || lowerName.Contains("apple") ||
                lowerName.Contains("trank") || lowerName.Contains("potion") ||
                lowerName.Contains("schnaps"))
            {
                return "Nahrung";
            }

            // Materials & Tools
            if (lowerName.Contains("set") || lowerName.Contains("kit") ||
                lowerName.Contains("werkzeug") || lowerName.Contains("schiesspulver") ||
                lowerName.Contains("gunpowder") || lowerName.Contains("erz") || lowerName.Contains("ore") ||
                lowerName.Contains("barren") || lowerName.Contains("ingot"))
            {
                return "Materialien";
            }

            // Books & Documents
            if (lowerName.Contains("buch") || lowerName.Contains("book") ||
                lowerName.Contains("schrift") || lowerName.Contains("brief") ||
                lowerName.Contains("rezept") || lowerName.Contains("recipe"))
            {
                return "Bücher";
            }

            // Quest
            if (lowerName.Contains("quest") || lowerName.Contains("siegel") || lowerName.Contains("brief von"))
            {
                return "Quest";
            }

            return "Sonstiges";
        }

        private static string DetermineSubCategory(string category, string name)
        {
            bool isGerman = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
            string lowerName = name.ToLowerInvariant();
            if (category == "Waffen")
            {
                if (lowerName.Contains("bogen") || lowerName.Contains("bow") ||
                    lowerName.Contains("armbrust") || lowerName.Contains("crossbow") ||
                    lowerName.Contains("kanone") || lowerName.Contains("handgonne") ||
                    lowerName.Contains("pistole") || lowerName.Contains("gewehr"))
                    return isGerman ? "Fernkampfwaffen" : "Ranged Weapons";

                if (lowerName.Contains("pfeil") || lowerName.Contains("arrow") ||
                    lowerName.Contains("bolzen") || lowerName.Contains("bolt") ||
                    lowerName.Contains("kugel") || lowerName.Contains("bullet") ||
                    lowerName.Contains("pulver"))
                    return isGerman ? "Munition" : "Ammunition";

                if (lowerName.Contains("dolch") || lowerName.Contains("dagger") ||
                    lowerName.Contains("fackel") || lowerName.Contains("torch") ||
                    lowerName.Contains("streitaxt") || lowerName.Contains("axe") ||
                    lowerName.Contains("streitkolben") || lowerName.Contains("mace"))
                    return isGerman ? "Andere Waffen" : "Other Weapons";

                return isGerman ? "Schwerter" : "Swords";
            }

            if (category == "Rüstung")
            {
                if (lowerName.Contains("helm") || lowerName.Contains("helmet") ||
                    lowerName.Contains("gugel") || lowerName.Contains("coif") ||
                    lowerName.Contains("hood") || lowerName.Contains("hut") || lowerName.Contains("cap"))
                    return isGerman ? "Kopf" : "Head";

                if (lowerName.Contains("arm") || lowerName.Contains("handschuh") ||
                    lowerName.Contains("gauntlet") || lowerName.Contains("vambrace"))
                    return isGerman ? "Arme" : "Arms";

                if (lowerName.Contains("bein") || lowerName.Contains("hose") ||
                    lowerName.Contains("chausses") || lowerName.Contains("stiefel") ||
                    lowerName.Contains("boots") || lowerName.Contains("schuhe") ||
                    lowerName.Contains("sporen"))
                    return isGerman ? "Beine" : "Legs";

                return isGerman ? "Körper" : "Body";
            }

            if (category == "Nahrung")
            {
                if (lowerName.Contains("trank") || lowerName.Contains("potion") ||
                    lowerName.Contains("schnaps") || lowerName.Contains("saviour") ||
                    lowerName.Contains("tee") || lowerName.Contains("decoction") ||
                    lowerName.Contains("bier") || lowerName.Contains("wein"))
                    return isGerman ? "Tränke" : "Potions";

                return isGerman ? "Nahrung" : "Food";
            }

            if (category == "Pferd") return isGerman ? "Pferdeausrüstung" : "Horse Tack";
            if (category == "Bücher") return isGerman ? "Bücher & Dokumente" : "Books & Documents";
            if (category == "Materialien") return isGerman ? "Kräuter & Alchemie" : "Herbs & Alchemy";
            if (category == "Quest") return isGerman ? "Quest-Gegenstände" : "Quest Items";

            return isGerman ? "Sonstiges" : "Miscellaneous";
        }

        private static int DetermineTier(float price, string category)
        {
            if (price >= 1500) return 4;
            if (price >= 750) return 3;
            if (price >= 200) return 2;
            if (price >= 50) return 1;
            return 0;
        }

        private static int DeterminePower(string category, string subCategory, int tier)
        {
            if (category == "Waffen")
            {
                if (subCategory == "Fernkampfwaffen" || subCategory == "Ranged Weapons") return 120 + tier * 125;
                if (subCategory == "Munition" || subCategory == "Ammunition") return 100 + tier * 50;
                return 80 + tier * 25;
            }
            if (category == "Rüstung")
            {
                if (subCategory == "Körper" || subCategory == "Body") return 75 + tier * 25;
                if (subCategory == "Beine" || subCategory == "Legs") return 70 + tier * 22;
                return 15 + tier * 20;
            }
            return 0;
        }

        private static string DetermineGlyph(string category, string name)
        {
            string lowerName = name.ToLowerInvariant();
            if (lowerName.Contains("groschen")) return "💰";
            if (lowerName.Contains("sattel") || lowerName.Contains("saddle") ||
                lowerName.Contains("zaumzeug") || lowerName.Contains("bridle") ||
                lowerName.Contains("hufeisen") || lowerName.Contains("horseshoe") ||
                lowerName.Contains("schabracke") || lowerName.Contains("caparison") ||
                lowerName.Contains("pferd") || lowerName.Contains("horse")) return "🐎";
            if (lowerName.Contains("schwert") || lowerName.Contains("sword")) return "⚔️";
            if (lowerName.Contains("kanone") || lowerName.Contains("gewehr")) return "💥";
            if (lowerName.Contains("bogen") || lowerName.Contains("pfeil")) return "🏹";
            if (lowerName.Contains("kugel")) return "⚪";
            if (lowerName.Contains("trank") || lowerName.Contains("decoction") || lowerName.Contains("schnaps") || lowerName.Contains("tee")) return "🧪";
            if (lowerName.Contains("dietrich") || lowerName.Contains("lockpick")) return "🗝️";
            if (lowerName.Contains("verband") || lowerName.Contains("bandage") || lowerName.Contains("salve")) return "🩹";
            if (lowerName.Contains("buch") || lowerName.Contains("book")) return "📜";
            if (lowerName.Contains("fackel")) return "🔥";

            return category switch
            {
                "Waffen" => "⚔️",
                "Rüstung" => "🛡️",
                "Nahrung" => "🍖",
                "Materialien" => "🔨",
                "Bücher" => "📜",
                "Quest" => "⭐",
                "Pferd" => "🐎",
                _ => "📦"
            };
        }
    }
}

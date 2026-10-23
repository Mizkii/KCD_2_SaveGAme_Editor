using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class HerbItemViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private string _icon = "🌿";
        private int _quantity;

        public HerbItemViewModel(string id, string nameDe, string nameEn, string icon, int quantity)
        {
            _id = id;
            _nameDe = nameDe;
            _nameEn = nameEn;
            _icon = icon;
            _quantity = quantity;
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string NameDe
        {
            get => _nameDe;
            set => SetProperty(ref _nameDe, value);
        }

        public string NameEn
        {
            get => _nameEn;
            set => SetProperty(ref _nameEn, value);
        }

        public string DisplayName => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? NameDe : NameEn;

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, Math.Max(0, value));
        }

        public void NotifyLanguageChanged() => OnPropertyChanged(nameof(DisplayName));
    }

    public class CraftingRecipeViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _recipeType = "Alchemy"; // "Alchemy" or "Blacksmithing"
        private string _category = "All";
        private string _uiNameKey = string.Empty;
        private string _uiDescKey = string.Empty;
        private string _fallbackNameDe = string.Empty;
        private string _fallbackNameEn = string.Empty;
        private string _ingredientsDe = string.Empty;
        private string _ingredientsEn = string.Empty;
        private string _baseMaterial = string.Empty;
        private int _minSkillLevel = 1;
        private string _perkId = string.Empty;
        private string _icon = "🧪";
        private bool _isUnlocked;

        public Action? OnUnlockedChanged { get; set; }

        public CraftingRecipeViewModel() { }

        public CraftingRecipeViewModel(
            string id,
            string recipeType,
            string category,
            string uiNameKey,
            string uiDescKey,
            string fallbackNameDe,
            string fallbackNameEn,
            string ingredientsDe,
            string ingredientsEn,
            string baseMaterial,
            int minSkillLevel,
            string perkId,
            string icon,
            bool isUnlocked)
        {
            _id = id;
            _recipeType = recipeType;
            _category = category;
            _uiNameKey = uiNameKey;
            _uiDescKey = uiDescKey;
            _fallbackNameDe = fallbackNameDe;
            _fallbackNameEn = fallbackNameEn;
            _ingredientsDe = ingredientsDe;
            _ingredientsEn = ingredientsEn;
            _baseMaterial = baseMaterial;
            _minSkillLevel = minSkillLevel;
            _perkId = perkId;
            _icon = icon;
            _isUnlocked = isUnlocked;
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string RecipeType
        {
            get => _recipeType;
            set => SetProperty(ref _recipeType, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string UiNameKey
        {
            get => _uiNameKey;
            set => SetProperty(ref _uiNameKey, value);
        }

        public string UiDescKey
        {
            get => _uiDescKey;
            set => SetProperty(ref _uiDescKey, value);
        }

        public string FallbackNameDe
        {
            get => _fallbackNameDe;
            set => SetProperty(ref _fallbackNameDe, value);
        }

        public string FallbackNameEn
        {
            get => _fallbackNameEn;
            set => SetProperty(ref _fallbackNameEn, value);
        }

        public string IngredientsDe
        {
            get => _ingredientsDe;
            set => SetProperty(ref _ingredientsDe, value);
        }

        public string IngredientsEn
        {
            get => _ingredientsEn;
            set => SetProperty(ref _ingredientsEn, value);
        }

        public string BaseMaterial
        {
            get => _baseMaterial;
            set => SetProperty(ref _baseMaterial, value);
        }

        public int MinSkillLevel
        {
            get => _minSkillLevel;
            set => SetProperty(ref _minSkillLevel, value);
        }

        public string PerkId
        {
            get => _perkId;
            set => SetProperty(ref _perkId, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (SetProperty(ref _isUnlocked, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                    OnUnlockedChanged?.Invoke();
                }
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiNameKey) && GameDataService.Instance.HasString(_uiNameKey))
                {
                    return GameDataService.Instance.GetString(_uiNameKey);
                }
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                return isDe ? (!string.IsNullOrEmpty(_fallbackNameDe) ? _fallbackNameDe : _id)
                            : (!string.IsNullOrEmpty(_fallbackNameEn) ? _fallbackNameEn : _id);
            }
        }

        public string DisplayDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiDescKey) && GameDataService.Instance.HasString(_uiDescKey))
                {
                    return GameDataService.Instance.GetString(_uiDescKey);
                }
                return string.Empty;
            }
        }

        public string DisplayIngredients
        {
            get
            {
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                string str = isDe ? _ingredientsDe : _ingredientsEn;
                if (string.IsNullOrEmpty(str)) str = _ingredientsDe;
                if (string.IsNullOrEmpty(str)) return string.Empty;

                var parts = str.Split(new[] { ", " }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    int xIdx = part.IndexOf("x ");
                    if (xIdx >= 0)
                    {
                        string prefix = part.Substring(0, xIdx + 2);
                        string itemKey = part.Substring(xIdx + 2).Trim();
                        if (GameDataService.Instance.HasString(itemKey))
                        {
                            parts[i] = prefix + GameDataService.Instance.GetString(itemKey);
                        }
                        else if (Guid.TryParse(itemKey, out _))
                        {
                            string resolvedName = GameDataService.Instance.GetItemName(itemKey, "");
                            if (!string.IsNullOrEmpty(resolvedName))
                            {
                                parts[i] = prefix + resolvedName;
                            }
                        }
                        else
                        {
                            parts[i] = prefix + itemKey;
                        }
                    }
                    else
                    {
                        if (part.Equals("Water", StringComparison.OrdinalIgnoreCase)) parts[i] = isDe ? "Wasser" : "Water";
                        else if (part.Equals("Wine", StringComparison.OrdinalIgnoreCase)) parts[i] = isDe ? "Wein" : "Wine";
                        else if (part.Equals("Oil", StringComparison.OrdinalIgnoreCase)) parts[i] = isDe ? "Öl" : "Oil";
                        else if (part.Equals("Spiritus", StringComparison.OrdinalIgnoreCase)) parts[i] = isDe ? "Weingeist" : "Spirits";
                    }
                }
                return string.Join(", ", parts);
            }
        }

        public string CategoryDisplayName
        {
            get
            {
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                return _category switch
                {
                    "LongSword" => isDe ? "Langschwert" : "Longsword",
                    "ShortSword" => isDe ? "Kurzschwert / Säbel" : "Shortsword / Sabre",
                    "Axe" => isDe ? "Streitaxt" : "Battle Axe",
                    "Other" => isDe ? "Hufeisen / Zubehör" : "Horseshoes / Misc",
                    "Healing" => isDe ? "Heilung & Elixiere" : "Healing & Elixirs",
                    "Poison" => isDe ? "Gifte & Klingenöle" : "Poisons & Blade Oils",
                    "Special" => isDe ? "Spezialgebräue" : "Special Brews",
                    _ => _category
                };
            }
        }

        public string CategoryBadgeColor
        {
            get
            {
                return _category switch
                {
                    "LongSword" => "#E5C478", // Gold
                    "ShortSword" => "#38BDF8", // Sky Blue
                    "Axe" => "#F97316", // Amber Orange
                    "Other" => "#A78BFA", // Light Purple
                    "Healing" => "#34D399", // Emerald Green
                    "Poison" => "#E11D48", // Crimson Rose
                    "Special" => "#FBBF24", // Yellow
                    _ => "#94A3B8"
                };
            }
        }

        public string StatusText
        {
            get
            {
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                if (IsUnlocked) return isDe ? "Gelernt" : "Learned";
                return isDe ? "Nicht gelernt" : "Not learned";
            }
        }

        public string StatusColor => IsUnlocked ? "#34D399" : "#64748B";

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayDescription));
            OnPropertyChanged(nameof(DisplayIngredients));
            OnPropertyChanged(nameof(CategoryDisplayName));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    public class CraftingSkillPerkViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _skillType = "Blacksmithing"; // "Blacksmithing" or "Alchemy"
        private string _uiNameKey = string.Empty;
        private string _uiDescKey = string.Empty;
        private string _fallbackNameDe = string.Empty;
        private string _fallbackNameEn = string.Empty;
        private string _fallbackDescDe = string.Empty;
        private string _fallbackDescEn = string.Empty;
        private int _level = 1;
        private string _icon = "⚒️";
        private bool _isUnlocked;

        public Action? OnUnlockedChanged { get; set; }

        public CraftingSkillPerkViewModel(
            string id,
            string skillType,
            string uiNameKey,
            string uiDescKey,
            string fallbackNameDe,
            string fallbackNameEn,
            string fallbackDescDe,
            string fallbackDescEn,
            int level,
            string icon,
            bool isUnlocked)
        {
            _id = id;
            _skillType = skillType;
            _uiNameKey = uiNameKey;
            _uiDescKey = uiDescKey;
            _fallbackNameDe = fallbackNameDe;
            _fallbackNameEn = fallbackNameEn;
            _fallbackDescDe = fallbackDescDe;
            _fallbackDescEn = fallbackDescEn;
            _level = level;
            _icon = icon;
            _isUnlocked = isUnlocked;
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string SkillType
        {
            get => _skillType;
            set => SetProperty(ref _skillType, value);
        }

        public string UiNameKey
        {
            get => _uiNameKey;
            set => SetProperty(ref _uiNameKey, value);
        }

        public string UiDescKey
        {
            get => _uiDescKey;
            set => SetProperty(ref _uiDescKey, value);
        }

        public string FallbackNameDe
        {
            get => _fallbackNameDe;
            set => SetProperty(ref _fallbackNameDe, value);
        }

        public string FallbackNameEn
        {
            get => _fallbackNameEn;
            set => SetProperty(ref _fallbackNameEn, value);
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (SetProperty(ref _isUnlocked, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                    OnUnlockedChanged?.Invoke();
                }
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiNameKey) && GameDataService.Instance.HasString(_uiNameKey))
                {
                    return GameDataService.Instance.GetString(_uiNameKey);
                }
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                return isDe ? (!string.IsNullOrEmpty(_fallbackNameDe) ? _fallbackNameDe : _id)
                            : (!string.IsNullOrEmpty(_fallbackNameEn) ? _fallbackNameEn : _id);
            }
        }

        public string DisplayDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiDescKey) && GameDataService.Instance.HasString(_uiDescKey))
                {
                    return GameDataService.Instance.GetString(_uiDescKey);
                }
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                return isDe ? _fallbackDescDe : _fallbackDescEn;
            }
        }

        public string StatusText
        {
            get
            {
                bool isDe = GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase);
                return IsUnlocked ? (isDe ? "Gelernt" : "Learned") : (isDe ? "Nicht gelernt" : "Not learned");
            }
        }

        public string StatusColor => IsUnlocked ? "#34D399" : "#64748B";

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayDescription));
            OnPropertyChanged(nameof(StatusText));
        }
    }

    // Backward-compatible alias for existing references
    public class AlchemyRecipeViewModel : CraftingRecipeViewModel
    {
        public AlchemyRecipeViewModel(string id, string nameDe, string nameEn, string ingredientsDe, string ingredientsEn, bool isUnlocked)
            : base(id, "Alchemy", "Special", "", "", nameDe, nameEn, ingredientsDe, ingredientsEn, "Water", 1, "", "🧪", isUnlocked)
        {
        }
    }
}

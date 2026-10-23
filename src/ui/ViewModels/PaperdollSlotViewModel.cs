using System;
using System.Collections.Generic;
using System.Windows.Input;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class SlotOutfitEntry
    {
        public string ItemId { get; set; } = string.Empty;
        public string NameDe { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool HasBlood { get; set; }
        public bool IsEquipped { get; set; } = true;
        public float Condition { get; set; } = 1.0f;
    }

    public class PaperdollSlotViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _slotCategory = string.Empty;
        private string _labelDe = string.Empty;
        private string _labelEn = string.Empty;
        private string _defaultLabelDe = string.Empty;
        private string _defaultLabelEn = string.Empty;
        private string _defaultIcon = string.Empty;
        private string? _uiKey;
        private string _icon = string.Empty;
        private bool _hasBlood;
        private bool _isEquipped = true;
        private string? _linkedItemId;
        private bool _isSelected;
        private float _condition = 1.0f;

        private readonly Dictionary<int, SlotOutfitEntry> _outfitEntries = new();

        public PaperdollSlotViewModel(string id, string labelDe, string labelEn, string icon, bool hasBlood, bool isEquipped, string? linkedItemId, Action<PaperdollSlotViewModel>? onClick = null, string? uiKey = null)
        {
            _id = id;
            _slotCategory = id.StartsWith("slot_") ? id.Substring(5) : id;
            _labelDe = labelDe;
            _labelEn = labelEn;
            _defaultLabelDe = labelDe;
            _defaultLabelEn = labelEn;
            _defaultIcon = icon;
            _icon = icon;
            _hasBlood = hasBlood;
            _isEquipped = isEquipped;
            _linkedItemId = linkedItemId;
            _uiKey = uiKey;

            _outfitEntries[1] = new SlotOutfitEntry
            {
                ItemId = linkedItemId ?? string.Empty,
                NameDe = labelDe,
                NameEn = labelEn,
                Icon = icon,
                HasBlood = hasBlood,
                IsEquipped = isEquipped,
                Condition = 1.0f
            };

            ClickCommand = new RelayCommand(() => onClick?.Invoke(this));
        }

        // Backward compatibility constructor
        public PaperdollSlotViewModel(string id, string label, string icon, bool hasBlood, bool isEquipped, string? linkedItemId, Action<PaperdollSlotViewModel>? onClick = null)
            : this(id, label, label, icon, hasBlood, isEquipped, linkedItemId, onClick)
        {
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string SlotCategory
        {
            get => _slotCategory;
            set => SetProperty(ref _slotCategory, value);
        }

        public string DefaultLabelDe => _defaultLabelDe;
        public string DefaultLabelEn => _defaultLabelEn;
        public string DefaultLabel => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? _defaultLabelDe : _defaultLabelEn;
        public string DefaultIcon => _defaultIcon;

        public string? UiKey
        {
            get => _uiKey;
            set => SetProperty(ref _uiKey, value);
        }

        public string LabelDe
        {
            get => _labelDe;
            set => SetProperty(ref _labelDe, value);
        }

        public string LabelEn
        {
            get => _labelEn;
            set => SetProperty(ref _labelEn, value);
        }

        public string Label => DisplayLabel;

        public string DisplayLabel
        {
            get
            {
                if (!_isEquipped)
                {
                    return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase)
                        ? $"[{_defaultLabelDe} (Leer)]"
                        : $"[{_defaultLabelEn} (Empty)]";
                }
                if (!string.IsNullOrEmpty(_uiKey) && GameDataService.Instance.HasString(_uiKey))
                {
                    return GameDataService.Instance.GetString(_uiKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? _labelDe : _labelEn;
            }
        }

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayLabel));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(DefaultLabel));
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool HasBlood
        {
            get => _hasBlood;
            set => SetProperty(ref _hasBlood, value);
        }

        public bool IsEquipped
        {
            get => _isEquipped;
            set => SetProperty(ref _isEquipped, value);
        }

        public float Condition
        {
            get => _condition;
            set => SetProperty(ref _condition, value);
        }

        public string? LinkedItemId
        {
            get => _linkedItemId;
            set => SetProperty(ref _linkedItemId, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ICommand ClickCommand { get; }

        public void SetOutfitPreset(int outfitIndex, string nameDe, string nameEn, string icon, bool isEquipped, bool hasBlood, string? itemId = null, float condition = 1.0f)
        {
            _outfitEntries[outfitIndex] = new SlotOutfitEntry
            {
                ItemId = itemId ?? string.Empty,
                NameDe = nameDe,
                NameEn = nameEn,
                Icon = icon,
                IsEquipped = isEquipped,
                HasBlood = hasBlood,
                Condition = condition
            };
        }

        public void ApplyOutfit(int outfitIndex)
        {
            if (_outfitEntries.TryGetValue(outfitIndex, out var entry))
            {
                _labelDe = entry.NameDe;
                _labelEn = entry.NameEn;
                _icon = entry.Icon;
                _hasBlood = entry.HasBlood;
                _isEquipped = entry.IsEquipped;
                _linkedItemId = entry.ItemId;
                _condition = entry.Condition;
            }
            NotifyLanguageChanged();
            OnPropertyChanged(nameof(Icon));
            OnPropertyChanged(nameof(HasBlood));
            OnPropertyChanged(nameof(IsEquipped));
            OnPropertyChanged(nameof(LinkedItemId));
            OnPropertyChanged(nameof(Condition));
        }

        public void EquipItem(int outfitIndex, string nameDe, string nameEn, string icon, string itemId, bool hasBlood = false, float condition = 1.0f)
        {
            _outfitEntries[outfitIndex] = new SlotOutfitEntry
            {
                ItemId = itemId,
                NameDe = nameDe,
                NameEn = nameEn,
                Icon = icon,
                IsEquipped = true,
                HasBlood = hasBlood,
                Condition = condition
            };
            ApplyOutfit(outfitIndex);
        }

        public void Unequip(int outfitIndex)
        {
            string emptyDe = "— Ablegen / Leer —";
            string emptyEn = "— Unequipped / Empty —";
            _outfitEntries[outfitIndex] = new SlotOutfitEntry
            {
                ItemId = string.Empty,
                NameDe = emptyDe,
                NameEn = emptyEn,
                Icon = _defaultIcon,
                IsEquipped = false,
                HasBlood = false,
                Condition = 1.0f
            };
            ApplyOutfit(outfitIndex);
        }

        public SlotOutfitEntry? GetOutfitEntry(int outfitIndex)
        {
            return _outfitEntries.TryGetValue(outfitIndex, out var entry) ? entry : null;
        }
    }
}

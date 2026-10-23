using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class CategoryTabViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _labelDe = string.Empty;
        private string _labelEn = string.Empty;
        private string? _uiKey;
        private string _icon = string.Empty;
        private int _count;
        private bool _isSelected;
        private string _accentBrush = "#D4AF37";

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

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

        public string DisplayLabel
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiKey) && GameDataService.Instance.HasString(_uiKey))
                {
                    return GameDataService.Instance.GetString(_uiKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? LabelDe : LabelEn;
            }
        }

        public void NotifyLanguageChanged() => OnPropertyChanged(nameof(DisplayLabel));

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string AccentBrush
        {
            get => _accentBrush;
            set => SetProperty(ref _accentBrush, value);
        }
    }
}

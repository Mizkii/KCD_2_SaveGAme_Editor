using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class FactionRepViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private int _reputation;

        public FactionRepViewModel(string id, string nameDe, string nameEn, int reputation)
        {
            _id = id;
            _nameDe = nameDe;
            _nameEn = nameEn;
            _reputation = reputation;
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

        public void NotifyLanguageChanged() => OnPropertyChanged(nameof(DisplayName));

        public int Reputation
        {
            get => _reputation;
            set => SetProperty(ref _reputation, Math.Clamp(value, -100, 100));
        }

        public string FormattedReputation => $"{_reputation} / 100";
    }

    public class PerkItemViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private bool _isEnabled;

        public PerkItemViewModel(string id, string nameDe, string nameEn, bool isEnabled)
        {
            _id = id;
            _nameDe = nameDe;
            _nameEn = nameEn;
            _isEnabled = isEnabled;
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

        public void NotifyLanguageChanged() => OnPropertyChanged(nameof(DisplayName));

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}

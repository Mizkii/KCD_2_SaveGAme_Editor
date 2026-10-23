using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class DogPerkItemViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private bool _isEnabled;

        public DogPerkItemViewModel(string id, string nameDe, string nameEn, bool isEnabled)
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

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public void NotifyLanguageChanged() => OnPropertyChanged(nameof(DisplayName));
    }
}

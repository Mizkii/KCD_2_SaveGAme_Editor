using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class DlcItemViewModel : ViewModelBase
    {
        private bool _isEnabled;
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private string _descDe = string.Empty;
        private string _descEn = string.Empty;
        private string _displayName = string.Empty;
        private string _description = string.Empty;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_nameDe) || !string.IsNullOrEmpty(_nameEn))
                {
                    return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase)
                        ? (!string.IsNullOrEmpty(_nameDe) ? _nameDe : _nameEn)
                        : (!string.IsNullOrEmpty(_nameEn) ? _nameEn : _nameDe);
                }
                return _displayName;
            }
            set => SetProperty(ref _displayName, value);
        }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(_descDe) || !string.IsNullOrEmpty(_descEn))
                {
                    return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase)
                        ? (!string.IsNullOrEmpty(_descDe) ? _descDe : _descEn)
                        : (!string.IsNullOrEmpty(_descEn) ? _descEn : _descDe);
                }
                return _description;
            }
            set => SetProperty(ref _description, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    OnChanged?.Invoke();
                }
            }
        }

        public Action? OnChanged { get; set; }

        public DlcItemViewModel(string id, string nameDe, string nameEn, string descDe, string descEn, bool isEnabled, Action? onChanged = null)
        {
            _id = id;
            _nameDe = nameDe;
            _nameEn = nameEn;
            _descDe = descDe;
            _descEn = descEn;
            _displayName = nameEn;
            _description = descEn;
            _isEnabled = isEnabled;
            OnChanged = onChanged;
        }

        public DlcItemViewModel(string id, string displayName, string description, bool isEnabled, Action? onChanged = null)
            : this(id, displayName, displayName, description, description, isEnabled, onChanged)
        {
        }

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Description));
        }
    }
}

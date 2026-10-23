using System;
using System.Windows.Media;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public enum CharacterCategory
    {
        StoryVip,       // Unsterbliche Story-Hauptfiguren (Hans Capon, Jan Žižka, etc.)
        DecisionChoice, // Schicksals-Entscheidungen (Brabant, Taras Mura, Kuba Paralu, etc.)
        CraftsmanMaster // Sterbliche Meister-Händler & Handwerker (Schmiede, Alchemisten, etc.)
    }

    public class GameCharacterFateViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _nameDe = string.Empty;
        private string _nameEn = string.Empty;
        private string _roleDe = string.Empty;
        private string _roleEn = string.Empty;
        private string _locationDe = string.Empty;
        private string _locationEn = string.Empty;
        private string _descriptionDe = string.Empty;
        private string _descriptionEn = string.Empty;
        private string _icon = "👤";
        private CharacterCategory _category;
        private string _status = "Alive"; // "Alive", "Dead", "Immortal", "Spared", "Executed", "Rescued"
        private Action? _onStatusChanged;

        public GameCharacterFateViewModel(
            string id,
            string nameDe,
            string nameEn,
            string roleDe,
            string roleEn,
            string locationDe,
            string locationEn,
            string descriptionDe,
            string descriptionEn,
            string icon,
            CharacterCategory category,
            string initialStatus = "Alive",
            Action? onStatusChanged = null)
        {
            _id = id;
            _nameDe = nameDe;
            _nameEn = nameEn;
            _roleDe = roleDe;
            _roleEn = roleEn;
            _locationDe = locationDe;
            _locationEn = locationEn;
            _descriptionDe = descriptionDe;
            _descriptionEn = descriptionEn;
            _icon = icon;
            _category = category;
            _status = initialStatus;
            _onStatusChanged = onStatusChanged;
        }

        public string Id => _id;
        public string Icon => _icon;
        public CharacterCategory Category => _category;

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
        public string Name => DisplayName;

        public string RoleDe
        {
            get => _roleDe;
            set => SetProperty(ref _roleDe, value);
        }

        public string RoleEn
        {
            get => _roleEn;
            set => SetProperty(ref _roleEn, value);
        }

        public string DisplayRole => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? RoleDe : RoleEn;
        public string Role => DisplayRole;

        public string LocationDe
        {
            get => _locationDe;
            set => SetProperty(ref _locationDe, value);
        }

        public string LocationEn
        {
            get => _locationEn;
            set => SetProperty(ref _locationEn, value);
        }

        public string DisplayLocation => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? LocationDe : LocationEn;
        public string Location => DisplayLocation;

        public string DescriptionDe
        {
            get => _descriptionDe;
            set => SetProperty(ref _descriptionDe, value);
        }

        public string DescriptionEn
        {
            get => _descriptionEn;
            set => SetProperty(ref _descriptionEn, value);
        }

        public string DisplayDescription => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? DescriptionDe : DescriptionEn;
        public string Description => DisplayDescription;

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusBadgeText));
                    OnPropertyChanged(nameof(StatusBadgeBackground));
                    OnPropertyChanged(nameof(StatusBadgeForeground));
                    OnPropertyChanged(nameof(StatusBadgeBorder));
                    OnPropertyChanged(nameof(IsDead));
                    OnPropertyChanged(nameof(CanBeRevived));
                    OnPropertyChanged(nameof(IsDecisionCharacter));
                    OnPropertyChanged(nameof(IsStoryVip));
                    _onStatusChanged?.Invoke();
                }
            }
        }

        public bool IsDead => Status.Equals("Dead", StringComparison.OrdinalIgnoreCase) ||
                              Status.Equals("Executed", StringComparison.OrdinalIgnoreCase);

        public bool CanBeRevived => Category == CharacterCategory.CraftsmanMaster && IsDead;
        public bool IsDecisionCharacter => Category == CharacterCategory.DecisionChoice;
        public bool IsStoryVip => Category == CharacterCategory.StoryVip;
        public bool IsCraftsman => Category == CharacterCategory.CraftsmanMaster;
        public bool IsVip => IsStoryVip;
        public bool IsDecision => IsDecisionCharacter;

        public string CategoryBadgeText
        {
            get
            {
                return Category switch
                {
                    CharacterCategory.StoryVip => GameDataService.Instance.GetUiText("StoryVip", "STORY-VIP (UNSTERBLICH)", "STORY VIP (IMMORTAL)"),
                    CharacterCategory.DecisionChoice => GameDataService.Instance.GetUiText("DecisionChoice", "SCHICKSALS-WAHL", "STORY DECISION"),
                    _ => GameDataService.Instance.GetUiText("MasterCraftsman", "MEISTER-HANDWERKER", "MASTER CRAFTSMAN")
                };
            }
        }

        public Brush CategoryBadgeBackground
        {
            get
            {
                return Category switch
                {
                    CharacterCategory.StoryVip => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20F59E0B")),
                    CharacterCategory.DecisionChoice => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20A855F7")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#200EA5E9"))
                };
            }
        }

        public Brush CategoryBadgeForeground
        {
            get
            {
                return Category switch
                {
                    CharacterCategory.StoryVip => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D")),
                    CharacterCategory.DecisionChoice => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8B4FE")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"))
                };
            }
        }

        public string StatusBadgeText
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "alive" => GameDataService.Instance.GetUiText("Alive", "Lebendig", "Alive"),
                    "dead" => GameDataService.Instance.GetUiText("Dead", "Tot", "Dead"),
                    "immortal" => GameDataService.Instance.GetUiText("Immortal", "Unsterblich", "Immortal"),
                    "spared" => GameDataService.Instance.GetUiText("Spared", "Verschont", "Spared"),
                    "executed" => GameDataService.Instance.GetUiText("Executed", "Hingerichtet", "Executed"),
                    "rescued" => GameDataService.Instance.GetUiText("Rescued", "Gerettet", "Rescued"),
                    _ => Status
                };
            }
        }

        public Brush StatusBadgeBackground
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "alive" or "rescued" or "spared" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2510B981")),
                    "dead" or "executed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25EF4444")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25F59E0B"))
                };
            }
        }

        public Brush StatusBadgeForeground
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "alive" or "rescued" or "spared" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6EE7B7")),
                    "dead" or "executed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D"))
                };
            }
        }

        public Brush StatusBadgeBorder
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "alive" or "rescued" or "spared" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5010B981")),
                    "dead" or "executed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50EF4444")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50F59E0B"))
                };
            }
        }

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayRole));
            OnPropertyChanged(nameof(Role));
            OnPropertyChanged(nameof(DisplayLocation));
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(DisplayDescription));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(CategoryBadgeText));
            OnPropertyChanged(nameof(StatusBadgeText));
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class QuestObjectiveViewModel : ViewModelBase
    {
        private string _textDe = string.Empty;
        private string _textEn = string.Empty;
        private string? _uiKey;
        private bool _isDone;
        private bool _isActive;
        private Action? _onChanged;

        public QuestObjectiveViewModel(string textDe, string textEn, bool isDone, bool isActive = false, Action? onChanged = null, string? uiKey = null)
        {
            _textDe = textDe;
            _textEn = textEn;
            _isDone = isDone;
            _isActive = isActive;
            _onChanged = onChanged;
            _uiKey = uiKey;
        }

        public Action? OnChanged
        {
            get => _onChanged;
            set => _onChanged = value;
        }

        public string? UiKey
        {
            get => _uiKey;
            set => SetProperty(ref _uiKey, value);
        }

        public string TextDe
        {
            get => _textDe;
            set => SetProperty(ref _textDe, value);
        }

        public string TextEn
        {
            get => _textEn;
            set => SetProperty(ref _textEn, value);
        }

        public string DisplayText
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiKey) && GameDataService.Instance.HasString(_uiKey))
                {
                    return GameDataService.Instance.GetString(_uiKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? TextDe : TextEn;
            }
        }

        public string StatusActiveText => GameDataService.Instance.IsGerman ? "Status: Aktiv / In Bearbeitung" : "Status: Active / In Progress";

        public bool IsDone
        {
            get => _isDone;
            set
            {
                if (SetProperty(ref _isDone, value))
                {
                    _onChanged?.Invoke();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(StatusActiveText));
        }
    }

    public class QuestItemViewModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _type = "haupt"; // "haupt", "neben", "aktiv"
        private string _titleDe = string.Empty;
        private string _titleEn = string.Empty;
        private string? _uiTitleKey;
        private string _locationDe = string.Empty;
        private string _locationEn = string.Empty;
        private string? _locationKey;
        private string _status = "aktiv"; // "aktiv", "abgeschlossen", "fehlgeschlagen"
        private int _stage = 1;
        private int _maxStage = 1;
        private string _region = "Trosecko"; // "Trosecko", "Kutnohorsko"
        private bool _isPointOfNoReturn;
        private string? _pointOfNoReturnWarningDe;
        private string? _pointOfNoReturnWarningEn;

        public ObservableCollection<QuestObjectiveViewModel> Objectives { get; } = new();

        public QuestItemViewModel(
            string id,
            string type,
            string titleDe,
            string titleEn,
            string locationDe,
            string locationEn,
            string status,
            int stage,
            int maxStage,
            string? uiTitleKey = null,
            string? locationKey = null,
            string region = "Trosecko",
            bool isPointOfNoReturn = false,
            string? ponrWarningDe = null,
            string? ponrWarningEn = null)
        {
            _id = id;
            _type = type;
            _titleDe = titleDe;
            _titleEn = titleEn;
            _locationDe = locationDe;
            _locationEn = locationEn;
            _status = status;
            _stage = stage;
            _maxStage = maxStage;
            _uiTitleKey = uiTitleKey;
            _locationKey = locationKey;
            _region = region;
            _isPointOfNoReturn = isPointOfNoReturn;
            _pointOfNoReturnWarningDe = ponrWarningDe;
            _pointOfNoReturnWarningEn = ponrWarningEn;
        }

        public string Region
        {
            get => _region;
            set
            {
                if (SetProperty(ref _region, value))
                {
                    OnPropertyChanged(nameof(RegionBadgeText));
                    OnPropertyChanged(nameof(RegionBadgeBackground));
                    OnPropertyChanged(nameof(RegionBadgeForeground));
                }
            }
        }

        public bool IsPointOfNoReturn
        {
            get => _isPointOfNoReturn;
            set => SetProperty(ref _isPointOfNoReturn, value);
        }

        public string? PointOfNoReturnWarningDe
        {
            get => _pointOfNoReturnWarningDe;
            set
            {
                if (SetProperty(ref _pointOfNoReturnWarningDe, value))
                {
                    OnPropertyChanged(nameof(PointOfNoReturnWarning));
                }
            }
        }

        public string? PointOfNoReturnWarningEn
        {
            get => _pointOfNoReturnWarningEn;
            set
            {
                if (SetProperty(ref _pointOfNoReturnWarningEn, value))
                {
                    OnPropertyChanged(nameof(PointOfNoReturnWarning));
                }
            }
        }

        public string PointOfNoReturnWarning => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase)
            ? (_pointOfNoReturnWarningDe ?? "Achtung: Dies ist ein kritischer Wendepunkt der Handlung!")
            : (_pointOfNoReturnWarningEn ?? "Warning: This is a critical Point of No Return!");

        public string RegionBadgeText => Region.ToUpperInvariant();

        public Brush RegionBadgeBackground => Region.Equals("Trosecko", StringComparison.OrdinalIgnoreCase)
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#200284C7"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#207C3AED"));

        public Brush RegionBadgeForeground => Region.Equals("Trosecko", StringComparison.OrdinalIgnoreCase)
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C084FC"));

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string? UiTitleKey
        {
            get => _uiTitleKey;
            set => SetProperty(ref _uiTitleKey, value);
        }

        public string TitleDe
        {
            get => _titleDe;
            set => SetProperty(ref _titleDe, value);
        }

        public string TitleEn
        {
            get => _titleEn;
            set => SetProperty(ref _titleEn, value);
        }

        public string DisplayTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(_uiTitleKey) && GameDataService.Instance.HasString(_uiTitleKey))
                {
                    return GameDataService.Instance.GetString(_uiTitleKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? TitleDe : TitleEn;
            }
        }

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

        public string DisplayLocation
        {
            get
            {
                if (!string.IsNullOrEmpty(_locationKey) && GameDataService.Instance.HasString(_locationKey))
                {
                    return GameDataService.Instance.GetString(_locationKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? LocationDe : LocationEn;
            }
        }

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
                }
            }
        }

        public int Stage
        {
            get => _stage;
            set
            {
                if (SetProperty(ref _stage, value))
                {
                    OnPropertyChanged(nameof(StageFormatted));
                }
            }
        }

        public int MaxStage
        {
            get => _maxStage;
            set
            {
                if (SetProperty(ref _maxStage, value))
                {
                    OnPropertyChanged(nameof(StageFormatted));
                }
            }
        }

        public string StageFormatted => GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase)
            ? $"Stufe {Stage} von {MaxStage} Zielen"
            : $"Stage {Stage} of {MaxStage} Objectives";

        public void UpdateStageFromObjectives()
        {
            int doneCount = 0;
            foreach (var obj in Objectives)
            {
                if (obj.IsDone) doneCount++;
            }
            Stage = doneCount;
            MaxStage = Math.Max(1, Objectives.Count);

            if (Objectives.Count > 0 && doneCount == Objectives.Count && Status != "fehlgeschlagen")
            {
                Status = "abgeschlossen";
            }
            else if (doneCount < Objectives.Count && Status == "abgeschlossen")
            {
                Status = "aktiv";
            }
        }

        public string TypeBadgeText
        {
            get
            {
                return Type.ToLowerInvariant() switch
                {
                    "haupt" => GameDataService.Instance.GetUiText("Main", "HAUPTQUEST", "MAIN QUEST"),
                    "neben" => GameDataService.Instance.GetUiText("Side", "NEBENQUEST", "SIDE QUEST"),
                    "aktiv" => GameDataService.Instance.GetUiText("Activity", "AKTIVITÄT", "ACTIVITY"),
                    _ => GameDataService.Instance.GetUiText("Quest", "QUEST", "QUEST")
                };
            }
        }

        public string StatusBadgeText
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "abgeschlossen" => GameDataService.Instance.GetUiText("Completed", "Abgeschlossen", "Completed"),
                    "fehlgeschlagen" => GameDataService.Instance.GetUiText("Failed", "Fehlgeschlagen", "Failed"),
                    "aktiv" => GameDataService.Instance.GetUiText("Active", "Aktiv", "Active"),
                    "verfuegbar" or "verfügbar" or "available" => GameDataService.Instance.GetUiText("Available", "Verfügbar", "Available"),
                    _ => GameDataService.Instance.GetUiText("NotStarted", "Nicht begonnen", "Not Started")
                };
            }
        }

        public Brush StatusBadgeBackground
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "abgeschlossen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2510B981")),
                    "fehlgeschlagen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25EF4444")),
                    "aktiv" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#25F59E0B")),
                    "verfuegbar" or "verfügbar" or "available" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#250EA5E9")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#20475569"))
                };
            }
        }

        public Brush StatusBadgeForeground
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "abgeschlossen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6EE7B7")),
                    "fehlgeschlagen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5")),
                    "aktiv" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D")),
                    "verfuegbar" or "verfügbar" or "available" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"))
                };
            }
        }

        public Brush StatusBadgeBorder
        {
            get
            {
                return Status.ToLowerInvariant() switch
                {
                    "abgeschlossen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5010B981")),
                    "fehlgeschlagen" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50EF4444")),
                    "aktiv" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#50F59E0B")),
                    "verfuegbar" or "verfügbar" or "available" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#500EA5E9")),
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#40475569"))
                };
            }
        }

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayTitle));
            OnPropertyChanged(nameof(DisplayLocation));
            OnPropertyChanged(nameof(TypeBadgeText));
            OnPropertyChanged(nameof(StatusBadgeText));
            OnPropertyChanged(nameof(StageFormatted));
            OnPropertyChanged(nameof(PointOfNoReturnWarning));
            foreach (var obj in Objectives)
            {
                obj.NotifyLanguageChanged();
            }
        }
    }
}

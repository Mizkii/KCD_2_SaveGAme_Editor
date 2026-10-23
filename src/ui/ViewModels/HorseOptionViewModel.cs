using System;
using KCD2SaveEditor.UI.Services;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class HorseOptionViewModel : ViewModelBase
    {
        public string Id { get; set; }
        public string? UiKey { get; set; }
        public string? BreedKey { get; set; }
        public string NameDe { get; set; }
        public string NameEn { get; set; }
        public string BreedDe { get; set; }
        public string BreedEn { get; set; }
        public int Tier { get; set; }
        public float DefaultSpeed { get; set; }
        public float DefaultCapacity { get; set; }
        public float DefaultCourage { get; set; }
        public float DefaultStamina { get; set; }
        public float DefaultMaxHealth { get; set; }

        public HorseOptionViewModel(
            string id,
            string nameDe,
            string nameEn,
            string breedDe,
            string breedEn,
            int tier,
            float speed,
            float capacity,
            float courage,
            float stamina,
            float maxHealth,
            string? uiKey = null,
            string? breedKey = null)
        {
            Id = id;
            NameDe = nameDe;
            NameEn = nameEn;
            BreedDe = breedDe;
            BreedEn = breedEn;
            Tier = tier;
            DefaultSpeed = speed;
            DefaultCapacity = capacity;
            DefaultCourage = courage;
            DefaultStamina = stamina;
            DefaultMaxHealth = maxHealth;
            UiKey = uiKey;
            BreedKey = breedKey;
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(UiKey) && GameDataService.Instance.HasString(UiKey))
                {
                    return GameDataService.Instance.GetString(UiKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? NameDe : NameEn;
            }
        }

        public string DisplayBreed
        {
            get
            {
                if (!string.IsNullOrEmpty(BreedKey) && GameDataService.Instance.HasString(BreedKey))
                {
                    return GameDataService.Instance.GetString(BreedKey);
                }
                return GameDataService.Instance.ActiveLanguage.Equals("German", StringComparison.OrdinalIgnoreCase) ? BreedDe : BreedEn;
            }
        }

        public string TierDisplay => $"Tier {Tier}";

        public void NotifyLanguageChanged()
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(DisplayBreed));
        }
    }
}

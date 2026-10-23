using System;

namespace KCD2SaveEditor.UI.ViewModels
{
    public class SlotPickerItemViewModel : ViewModelBase
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Icon { get; set; } = "📦";
        public float Weight { get; set; }
        public float Price { get; set; }
        public int Power { get; set; }
        public int Tier { get; set; }
        public float Condition { get; set; } = 1.0f;
        public bool IsInPlayerInventory { get; set; }
        public int Amount { get; set; } = 1;

        public string TierDisplay => Tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "M",
            _ => string.Empty
        };

        public string FormattedWeight => Weight > 0 ? $"{Weight:0.0} kg" : "—";
        public string FormattedPrice => Price > 0 ? $"{Price:N0} Gr." : "—";
        public string ConditionDisplay => $"{Math.Round(Condition * 100)}%";
        public string SourceBadge => IsInPlayerInventory ? "🎒 Inventar" : "📖 Spiel-Katalog";
        public string SourceBadgeBackground => IsInPlayerInventory ? "#1B3A2C" : "#2A2312";
        public string SourceBadgeBorder => IsInPlayerInventory ? "#4ADE80" : "#FCD34D";
        public string SourceBadgeForeground => IsInPlayerInventory ? "#86EFAC" : "#FDE68A";
    }
}

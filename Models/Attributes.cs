using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace DnDToolkit.Models
{
    public partial class Attributes : ObservableObject
    {
        private static int CalcMod(int score) => (int)Math.Floor((score - 10) / 2.0);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StrengthMod))]
        private int strength = 10;

        [JsonIgnore]
        public int StrengthMod => CalcMod(Strength);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DexterityMod))]
        private int dexterity = 10;

        [JsonIgnore]
        public int DexterityMod => CalcMod(Dexterity);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConstitutionMod))]
        private int constitution = 10;

        [JsonIgnore]
        public int ConstitutionMod => CalcMod(Constitution);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IntelligenceMod))]
        private int intelligence = 10;

        [JsonIgnore]
        public int IntelligenceMod => CalcMod(Intelligence);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WisdomMod))]
        private int wisdom = 10;

        [JsonIgnore]
        public int WisdomMod => CalcMod(Wisdom);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CharismaMod))]
        private int charisma = 10;

        [JsonIgnore]
        public int CharismaMod => CalcMod(Charisma);
    }
}

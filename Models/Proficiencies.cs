using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Proficiencies : ObservableObject
    {
        // --- 豁免熟练 (Saving Throws) ---
        [ObservableProperty] private bool strengthSave = false;
        [ObservableProperty] private bool dexteritySave = false;
        [ObservableProperty] private bool constitutionSave = false;
        [ObservableProperty] private bool intelligenceSave = false;
        [ObservableProperty] private bool wisdomSave = false;
        [ObservableProperty] private bool charismaSave = false;

        // --- 技能熟练 (Skills) ---
        // 力量
        [ObservableProperty] private bool athletics = false;

        // 敏捷
        [ObservableProperty] private bool acrobatics = false;
        [ObservableProperty] private bool sleightOfHand = false;
        [ObservableProperty] private bool stealth = false;

        // 智力
        [ObservableProperty] private bool arcana = false;
        [ObservableProperty] private bool history = false;
        [ObservableProperty] private bool investigation = false;
        [ObservableProperty] private bool nature = false;
        [ObservableProperty] private bool religion = false;

        // 感知
        [ObservableProperty] private bool animalHandling = false;
        [ObservableProperty] private bool insight = false;
        [ObservableProperty] private bool medicine = false;
        [ObservableProperty] private bool perception = false;
        [ObservableProperty] private bool survival = false;

        // 魅力
        [ObservableProperty] private bool deception = false;
        [ObservableProperty] private bool intimidation = false;
        [ObservableProperty] private bool performance = false;
        [ObservableProperty] private bool persuasion = false;

        // 其他熟练项 & 语言 (Other Proficiencies & Languages)
        [ObservableProperty] private string otherProficienciesAndLanguages = "";
    }
}

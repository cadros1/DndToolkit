using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Profile : ObservableObject
    {
        // ... 原有的 Name, Race, Class 等保持不变 ...
        [ObservableProperty] private string characterName = "";
        [ObservableProperty] private string playerName = "";
        [ObservableProperty] private string race = "";
        [ObservableProperty] private string classAndLevel = "";
        [ObservableProperty] private string background = "";
        [ObservableProperty] private string alignment = "";
        [ObservableProperty] private int experiencePoints = 0;
        [ObservableProperty] private string inspiration = ""; // 灵感
        [ObservableProperty] private int proficiencyBonus = 2; // 熟练加值 (通常由等级决定，但允许手动填)

        // --- P1 左下角: 被动感知 (Passive Wisdom) ---
        [ObservableProperty] private int passivePerception = 10;

        // --- 外观 (P2 顶部) ---
        [ObservableProperty] private string age = "";
        [ObservableProperty] private string height = "";
        [ObservableProperty] private string weight = "";
        [ObservableProperty] private string eyes = "";
        [ObservableProperty] private string skin = "";
        [ObservableProperty] private string hair = "";

        [ObservableProperty] private string portraitBase64 = "";
    }
}

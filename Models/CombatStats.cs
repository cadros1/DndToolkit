using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class CombatStats : ObservableObject
    {
        // --- 顶部槽位 ---
        [ObservableProperty] private int armorClass = 10; // 使用string以允许输入 "16+2" 等备注
        [ObservableProperty] private int initiative = 0;
        [ObservableProperty] private string speed = "";

        // --- 生命值 ---
        [ObservableProperty] private int hitPointsMax = 0;
        [ObservableProperty] private int hitPointsCurrent = 0;
        [ObservableProperty] private int hitPointsTemp = 0;

        // --- 生命骰 (Hit Dice) ---
        [ObservableProperty] private string hitDiceTotal = "";
        [ObservableProperty] private string hitDiceCurrent = "";

        // --- 死亡豁免 (Death Saves) ---
        // 0 = 未勾选, 1 = 勾选. 界面上绑定三个CheckBox
        [ObservableProperty] private bool deathSuccess1;
        [ObservableProperty] private bool deathSuccess2;
        [ObservableProperty] private bool deathSuccess3;

        [ObservableProperty] private bool deathFail1;
        [ObservableProperty] private bool deathFail2;
        [ObservableProperty] private bool deathFail3;

        // --- 攻击&施法 下方的备注文本框 ---
        [ObservableProperty] private string attacksAndSpellcastingNotes = "";
    }
}

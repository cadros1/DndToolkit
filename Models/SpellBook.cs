using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Spellbook : ObservableObject
    {
        [ObservableProperty] private string spellcastingClass = "";
        [ObservableProperty] private string spellcastingAbility = "";
        [ObservableProperty] private int spellSaveDC = 0;
        [ObservableProperty] private int spellAttackBonus = 0;

        [ObservableProperty]
        private ObservableCollection<SpellLevelGroup> allSpells;

        public Spellbook()
        {
            AllSpells = new ObservableCollection<SpellLevelGroup>();
            // 这里初始化默认结构是好的。
            // 当从 JSON 反序列化时，System.Text.Json 会创建一个新 List 覆盖这个属性，
            // 所以不会出现“默认数据”和“读取数据”重复的问题。
            for (int i = 0; i <= 9; i++)
            {
                AllSpells.Add(new SpellLevelGroup(i));
            }
        }
    }

    public partial class SpellLevelGroup : ObservableObject
    {
        [ObservableProperty]
        private int level;
        [JsonIgnore]
        private readonly int[] defaultSpellCounts = [8, 12, 13, 13, 13, 9, 9, 9, 7, 7];

        // 忽略纯 UI 显示用的属性
        [JsonIgnore]
        public string LevelLabel => Level == 0 ? "戏法" : $"{Level}环法术";

        [ObservableProperty] private int totalSlots = 0;
        [ObservableProperty] private int remainSlots = 0;

        [ObservableProperty]
        private ObservableCollection<Spell> spells = new();

        // 必须提供一个无参构造函数给序列化器使用 (可以是 public 也可以是 private，STJ 都能用)
        public SpellLevelGroup()
        {
            // 反序列化时会调用这里，Spells 列表会被 JSON 数据填充
        }

        // 你的逻辑构造函数
        public SpellLevelGroup(int level) : this()
        {
            Level = level;
            // 只有新建角色时才填充空行，反序列化时 JSON 会覆盖 Spells 属性，所以不冲突
            int defaultCount = defaultSpellCounts[level];
            for (int i = 0; i < defaultCount; i++) Spells.Add(new Spell());
        }
    }

    public partial class Spell : ObservableObject
    {
        [ObservableProperty] private string name = "";
        [ObservableProperty] private bool isPrepared = false;
    }
}

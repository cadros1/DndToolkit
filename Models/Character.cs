using CommunityToolkit.Mvvm.ComponentModel;
using DnDToolkit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DnDToolkit.Models
{
    public partial class Character : ObservableObject
    {
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        [JsonIgnore]
        public string? FilePath { get; set; }

        // P1: 顶部基础信息 (名字, 职业, 种族等)
        [ObservableProperty] private Profile profile = new();

        // P1: 左侧属性 (六维)
        [ObservableProperty] private Attributes attributes = new();

        // P1: 战斗核心数据 (AC, HP, 速度, 死亡豁免) -> 新增
        [ObservableProperty] private CombatStats combat = new();

        // P1: 技能与豁免
        [ObservableProperty] private Proficiencies proficiencies = new();

        // P1 右侧 & P2: 个性, 理想, 纽带, 缺陷, 背景故事 -> 新增
        [ObservableProperty] private Roleplay roleplay = new();

        // P3: 魔法书
        [ObservableProperty] private Spellbook spellbook = new();

        // P1 中下: 武器攻击列表
        [ObservableProperty] private ObservableCollection<Weapon> weapons = new();

        // P1 底部 & P2: 物品与金钱 -> 重构
        [ObservableProperty] private Inventory inventory = new();

        public Character()
        {
            weapons = [new(), new(), new()];
        }
    }
}

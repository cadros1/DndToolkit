using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DnDToolkit.Helpers;
using DnDToolkit.Messages;
using DnDToolkit.Models;
using DnDToolkit.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.ViewModels
{
    // 用于列表显示的日志模型
    public class RollLogEntry
    {
        public string Title { get; set; } = "";
        public int Result { get; set; } = 0;
        public string Details { get; set; } = "";
        public string Time { get; set; } = "";
        public string Color { get; set; } = "Black"; // 简单区分大成功/失败颜色
    }

    public partial class AdventureViewModel : ObservableObject
    {
        private readonly ICharacterService _characterService;

        // === 角色选择 ===
        [ObservableProperty] private ObservableCollection<Character> allCharacters = new();

        // 注意：这里使用了 SetProperty 的回调写法，当角色改变时刷新动作列表
        private Character _currentCharacter;
        public Character CurrentCharacter
        {
            get => _currentCharacter;
            set
            {
                if (SetProperty(ref _currentCharacter, value))
                {
                    UpdateActionGroups(); // 角色变了，动作列表也要变
                }
            }
        }

        // === 掷骰参数 ===
        [ObservableProperty] private int selectedDiceSides = 20;
        [ObservableProperty] private int diceCount = 1;
        [ObservableProperty] private int customModifier = 0; // 额外加值
        [ObservableProperty] private bool isAdvantage = false;
        [ObservableProperty] private bool isDisadvantage = false;

        // === 动作源数据 (分类) ===
        public ObservableCollection<RollOption> AttributeOptions { get; private set; } = new();
        public ObservableCollection<RollOption> SaveOptions { get; private set; } = new();
        public ObservableCollection<RollOption> SkillOptions { get; private set; } = new();
        public ObservableCollection<RollOption> OtherOptions { get; private set; } = new();
        public RollOption FreeRollOption { get; } = new RollOption { Name = "自由检定", Type = "Free", IsFixedDice = false, BaseModifier = 0 };

        private RollOption _selectedAction;
        public RollOption SelectedAction
        {
            get => _selectedAction;
            set
            {
                if (SetProperty(ref _selectedAction, value))
                {
                    OnActionChanged();
                }
            }
        }

        // === 界面状态 ===
        [ObservableProperty] private bool isDiceSelectionVisible = true; // 控制骰子选择器是否显示
        [ObservableProperty] private int currentBaseModifier = 0;   // 只读显示

        public ObservableCollection<int> DiceOptions { get; } = new() { 4, 6, 8, 10, 12, 20, 100 };
        [ObservableProperty] private ObservableCollection<RollLogEntry> rollLogs = new();

        public AdventureViewModel(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public async Task RefreshDataAsync()
        {
            var list = await _characterService.GetAllCharactersAsync();
            AllCharacters = new ObservableCollection<Character>(list);

            if (!AllCharacters.Any()) CurrentCharacter = null;
            else if (CurrentCharacter == null || !AllCharacters.Any(c => c.Id == CurrentCharacter.Id))
            {
                CurrentCharacter = AllCharacters.First();
            }
            // 刷新动作列表
            UpdateActionGroups();
        }

        private void UpdateActionGroups()
        {
            // 清空旧数据
            AttributeOptions.Clear();
            SaveOptions.Clear();
            SkillOptions.Clear();
            OtherOptions.Clear();

            if (CurrentCharacter == null) return;

            // 1. 属性 & 豁免
            string[] attrs = { "力量", "敏捷", "体质", "智力", "感知", "魅力" };
            foreach (var a in attrs)
            {
                AttributeOptions.Add(new RollOption { Name = a + "检定", Type = "Attribute", BaseModifier = GetAttributeMod(a), IsFixedDice = true });
                SaveOptions.Add(new RollOption { Name = a + "豁免", Type = "Save", BaseModifier = GetSaveMod(a), IsFixedDice = true });
            }

            // 2. 技能
            AddSkillOption("运动", "力量", CurrentCharacter.Proficiencies.Athletics);
            AddSkillOption("体操", "敏捷", CurrentCharacter.Proficiencies.Acrobatics);
            AddSkillOption("巧手", "敏捷", CurrentCharacter.Proficiencies.SleightOfHand);
            AddSkillOption("隐匿", "敏捷", CurrentCharacter.Proficiencies.Stealth);
            AddSkillOption("奥秘", "智力", CurrentCharacter.Proficiencies.Arcana);
            AddSkillOption("历史", "智力", CurrentCharacter.Proficiencies.History);
            AddSkillOption("调查", "智力", CurrentCharacter.Proficiencies.Investigation);
            AddSkillOption("自然", "智力", CurrentCharacter.Proficiencies.Nature);
            AddSkillOption("宗教", "智力", CurrentCharacter.Proficiencies.Religion);
            AddSkillOption("驯兽", "感知", CurrentCharacter.Proficiencies.AnimalHandling);
            AddSkillOption("洞悉", "感知", CurrentCharacter.Proficiencies.Insight);
            AddSkillOption("医药", "感知", CurrentCharacter.Proficiencies.Medicine);
            AddSkillOption("察觉", "感知", CurrentCharacter.Proficiencies.Perception);
            AddSkillOption("生存", "感知", CurrentCharacter.Proficiencies.Survival);
            AddSkillOption("欺瞒", "魅力", CurrentCharacter.Proficiencies.Deception);
            AddSkillOption("威吓", "魅力", CurrentCharacter.Proficiencies.Intimidation);
            AddSkillOption("表演", "魅力", CurrentCharacter.Proficiencies.Performance);
            AddSkillOption("游说", "魅力", CurrentCharacter.Proficiencies.Persuasion);

            // 3. 战斗 (先攻 + 武器)
            OtherOptions.Add(new RollOption { Name = "先攻检定", Type = "Other", BaseModifier = CurrentCharacter.Combat.Initiative, IsFixedDice = true });
            foreach (var w in CurrentCharacter.Weapons)
            {
                OtherOptions.Add(new RollOption { Name = "武器攻击检定：" + w.Name, Type = "Other", BaseModifier = w.AttackBonus, IsFixedDice = true, Context = w });
            }
            OtherOptions.Add(new RollOption { Name = "死亡豁免", Type = "Other", BaseModifier = 0, IsFixedDice = true });

            // 默认选中自由掷骰
            SelectedAction = FreeRollOption;
        }

        private void AddSkillOption(string skillName, string attrName, bool isProf)
        {
            int pb = CurrentCharacter.Profile.ProficiencyBonus;
            int mod = GetAttributeMod(attrName) + (isProf ? pb : 0);
            SkillOptions.Add(new RollOption { Name = skillName, Type = "Skill", BaseModifier = mod, IsFixedDice = true });
        }

        // === 选中项改变时的逻辑 ===
        private void OnActionChanged()
        {
            if (SelectedAction == null) return;

            // 更新显示的基础加值
            CurrentBaseModifier = SelectedAction.BaseModifier;

            // 如果是固定骰子（预设检定），隐藏骰子选择器，强制设为 1d20
            if (SelectedAction.IsFixedDice)
            {
                IsDiceSelectionVisible = false;
                SelectedDiceSides = 20;
                DiceCount = 1;
            }
            else
            {
                IsDiceSelectionVisible = true; // 自由模式下显示
            }
        }

        // === 执行掷骰 (大一统方法) ===
        [RelayCommand]
        private void ExecuteRoll()
        {
            if (SelectedAction == null) return;

            // 最终加值 = 动作自带的基础加值 + 用户输入的临时调整值
            int totalMod = SelectedAction.BaseModifier + CustomModifier;

            // 优劣势判断
            RollType type = RollType.Normal;
            if (IsAdvantage) type = RollType.Advantage;
            if (IsDisadvantage) type = RollType.Disadvantage;

            // 掷骰
            var result = DiceHelper.Roll(SelectedDiceSides, DiceCount, totalMod, type);

            // 颜色
            string color = "Black";
            if (result.IsCritical) color = "Gold";
            if (result.IsFumble) color = "#FF6B6B";

            // 记录日志
            string title = SelectedAction.Name;
            if (CustomModifier != 0) title += $" (Extra {CustomModifier:+0;-0})";

            RollLogs.Insert(0, new RollLogEntry
            {
                Title = title,
                Result = result.Total,
                Details = result.Details,
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Color = color
            });
        }

        [RelayCommand]
        private void ClearLogs() => RollLogs.Clear();

        // === 辅助命令：按钮直接设置 Action ===
        [RelayCommand]
        private void SetAction(RollOption option)
        {
            SelectedAction = option;
        }

        // 用于切换回自由模式
        [RelayCommand]
        private void SetFreeMode()
        {
            SelectedAction = FreeRollOption;
        }

        // 辅助方法：获取属性调整值
        private int GetAttributeMod(string name)
        {
            return name switch
            {
                "力量" => CurrentCharacter.Attributes.StrengthMod,
                "敏捷" => CurrentCharacter.Attributes.DexterityMod,
                "体质" => CurrentCharacter.Attributes.ConstitutionMod,
                "智力" => CurrentCharacter.Attributes.IntelligenceMod,
                "感知" => CurrentCharacter.Attributes.WisdomMod,
                "魅力" => CurrentCharacter.Attributes.CharismaMod,
                _ => 0
            };
        }

        private int GetSaveMod(string name)
        {
            int pb = CurrentCharacter.Profile.ProficiencyBonus;
            int attrMod = GetAttributeMod(name);
            bool isProf = name switch
            {
                "力量" => CurrentCharacter.Proficiencies.StrengthSave,
                "敏捷" => CurrentCharacter.Proficiencies.DexteritySave,
                "体质" => CurrentCharacter.Proficiencies.ConstitutionSave,
                "智力" => CurrentCharacter.Proficiencies.IntelligenceSave,
                "感知" => CurrentCharacter.Proficiencies.WisdomSave,
                "魅力" => CurrentCharacter.Proficiencies.CharismaSave,
                _ => false
            };
            return attrMod + (isProf ? pb : 0);
        }

        private int ParseInt(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            string clean = new string(input.Where(c => char.IsDigit(c) || c == '-').ToArray());
            return int.TryParse(clean, out int result) ? result : 0;
        }
    }

    public class RollOption
    {
        public string Name { get; set; } = "";
        public string Group { get; set; } = "通用"; // 用于分组显示(可选)
        public int BaseModifier { get; set; } = 0;  // 预设的加值 (如力量+3)
        public bool IsFixedDice { get; set; } = false; // 是否固定骰子类型(预设检定为True)
        public int FixedSides { get; set; } = 20;   // 固定的骰子面数

        public string Type { get; set; } = "Free"; // Free, Attribute, Save, Skill, Attack, Initiative, DeathSave
        public object? Context { get; set; } // 上下文数据 (比如Weapon对象)
    }
}

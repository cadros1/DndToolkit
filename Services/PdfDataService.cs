using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32; // 用于文件选择对话框
using DnDToolkit.Helpers;
using DnDToolkit.Models;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;

namespace DnDToolkit.Services
{
    public class PdfDataService
    {
        // 技能中文名到属性名的映射
        private readonly Dictionary<string, string> _skillMap = new()
        {
            { "运动", nameof(Proficiencies.Athletics) },
            { "杂技", nameof(Proficiencies.Acrobatics) }, // PDF中叫杂技，对应体操
            { "巧手", nameof(Proficiencies.SleightOfHand) },
            { "躲藏", nameof(Proficiencies.Stealth) }, // PDF中叫躲藏，对应隐匿
            { "奥秘", nameof(Proficiencies.Arcana) },
            { "历史", nameof(Proficiencies.History) },
            { "调查", nameof(Proficiencies.Investigation) },
            { "自然", nameof(Proficiencies.Nature) },
            { "宗教", nameof(Proficiencies.Religion) },
            { "驯兽", nameof(Proficiencies.AnimalHandling) },
            { "洞悉", nameof(Proficiencies.Insight) },
            { "医药", nameof(Proficiencies.Medicine) },
            { "察觉", nameof(Proficiencies.Perception) },
            { "生存", nameof(Proficiencies.Survival) },
            { "欺瞒", nameof(Proficiencies.Deception) },
            { "威吓", nameof(Proficiencies.Intimidation) },
            { "表演", nameof(Proficiencies.Performance) },
            { "游说", nameof(Proficiencies.Persuasion) }
        };

        // 属性简写映射
        private readonly Dictionary<string, string> _attrMap = new()
        {
            { "STR", nameof(Proficiencies.StrengthSave) },
            { "DEX", nameof(Proficiencies.DexteritySave) },
            { "CON", nameof(Proficiencies.ConstitutionSave) },
            { "INT", nameof(Proficiencies.IntelligenceSave) },
            { "WIS", nameof(Proficiencies.WisdomSave) },
            { "CHA", nameof(Proficiencies.CharismaSave) }
        };

        /// <summary>
        /// 从PDF导入角色数据
        /// </summary>
        public async Task<Character?> ImportCharacterPdfAsync()
        {
            // 1. 选择文件
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "导入角色卡 PDF"
            };

            if (openFileDialog.ShowDialog() != true) return null;

            string filePath = openFileDialog.FileName;

            return await Task.Run(() =>
            {
                try
                {
                    var character = new Character();

                    using (PdfReader reader = new PdfReader(filePath))
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);
                        if (form == null) return null;

                        IDictionary<string, PdfFormField> fields = form.GetAllFormFields();

                        // --- 基础信息 ---
                        character.Profile.CharacterName = GetString(fields, "CharacterName");
                        character.Profile.PlayerName = GetString(fields, "PlayerName");
                        character.Profile.Race = GetString(fields, "Race"); // PDF字段中有个空格 "Race "，需要Trim
                        character.Profile.ClassAndLevel = GetString(fields, "ClassLevel");
                        character.Profile.Background = GetString(fields, "Background");
                        character.Profile.Alignment = GetString(fields, "Alignment");
                        character.Profile.ExperiencePoints = ParseInt(GetString(fields, "XP"));

                        character.Profile.Age = GetString(fields, "Age");
                        character.Profile.Height = GetString(fields, "Height");
                        character.Profile.Weight = GetString(fields, "Weight");
                        character.Profile.Eyes = GetString(fields, "Eyes");
                        character.Profile.Skin = GetString(fields, "Skin");
                        character.Profile.Hair = GetString(fields, "Hair");
                        character.Roleplay.PersonalityTraits = GetString(fields, "PersonalityTraits"); // PDF结尾有空格
                        character.Roleplay.Ideals = GetString(fields, "Ideals");
                        character.Roleplay.Bonds = GetString(fields, "Bonds");
                        character.Roleplay.Flaws = GetString(fields, "Flaws");
                        character.Roleplay.CharacterExperience = GetString(fields, "角色经历");
                        character.Roleplay.CharacterBackstory = GetString(fields, "Backstory");
                        character.Roleplay.AlliesAndOrganizations = GetString(fields, "Allies");
                        character.Roleplay.Treasure = GetString(fields, "Treasure");
                        character.Roleplay.FeaturesAndTraits = GetString(fields, "Feat+Traits");

                        // --- 属性 (读取值) ---
                        character.Attributes.Strength = ParseInt(GetString(fields, "STR"));
                        character.Attributes.Dexterity = ParseInt(GetString(fields, "DEX"));
                        character.Attributes.Constitution = ParseInt(GetString(fields, "CON"));
                        character.Attributes.Intelligence = ParseInt(GetString(fields, "INT"));
                        character.Attributes.Wisdom = ParseInt(GetString(fields, "WIS"));
                        character.Attributes.Charisma = ParseInt(GetString(fields, "CHA"));

                        // --- 熟练项 (Check Box) ---
                        // 1. 技能
                        foreach (var kvp in _skillMap)
                        {
                            bool isProf = GetBool(fields, $"Check Box {kvp.Key}");
                            // 反射赋值
                            var prop = typeof(Proficiencies).GetProperty(kvp.Value);
                            if (prop != null) prop.SetValue(character.Proficiencies, isProf);
                        }
                        // 2. 豁免
                        foreach (var kvp in _attrMap)
                        {
                            bool isProf = GetBool(fields, $"Check Box {kvp.Key}");
                            var prop = typeof(Proficiencies).GetProperty(kvp.Value);
                            if (prop != null) prop.SetValue(character.Proficiencies, isProf);
                        }
                        // 3. 其他
                        character.Profile.ProficiencyBonus = ParseInt(GetString(fields, "ProfBonus"));
                        character.Profile.PassivePerception = ParseInt(GetString(fields, "Passive Perception"));
                        character.Profile.Inspiration = GetString(fields, "Inspiration");
                        character.Proficiencies.OtherProficienciesAndLanguages = GetString(fields, "ProficienciesLang");

                        // --- 战斗数据 ---
                        character.Combat.ArmorClass = ParseInt(GetString(fields, "AC"));
                        character.Combat.Initiative = ParseInt(GetString(fields, "Initiative"));
                        character.Combat.Speed = GetString(fields, "Speed");
                        character.Combat.AttacksAndSpellcastingNotes = GetString(fields, "AttacksAndSpellcasting");
                        character.Combat.Ability = GetString(fields, "Ability");

                        // 生命值 (Current 读取 Max)
                        int hpMax = ParseInt(GetString(fields, "HPMax"));
                        character.Combat.HitPointsMax = hpMax;
                        character.Combat.HitPointsCurrent = hpMax;
                        character.Combat.HitPointsTemp = ParseInt(GetString(fields, "HPTemp"));

                        // 生命骰 (Current 读取 Total)
                        string hdTotal = GetString(fields, "HDTotal");
                        character.Combat.HitDiceTotal = hdTotal;
                        character.Combat.HitDiceCurrent = hdTotal;

                        character.Inventory.EquipmentText = GetString(fields, "Equipment");
                        character.Inventory.CP = ParseInt(GetString(fields, "CP"));
                        character.Inventory.SP = ParseInt(GetString(fields, "SP"));
                        character.Inventory.EP = ParseInt(GetString(fields, "EP"));
                        character.Inventory.GP = ParseInt(GetString(fields, "GP"));
                        character.Inventory.PP = ParseInt(GetString(fields, "PP"));

                        // --- 武器 ---
                        character.Weapons.Clear();
                        for (int i = 1; i <= 3; i++)
                        {
                            string name = GetString(fields, $"Wpn Name {i}");
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                character.Weapons.Add(new Weapon
                                {
                                    Name = name,
                                    AttackBonus = ParseInt(GetString(fields, $"Wpn1 AtkBonus".Replace("1", i.ToString()))), // PDF命名不一致修正? 假设逻辑
                                    Damage = GetString(fields, $"Wpn1 Damage".Replace("1", i.ToString()))
                                });
                            }
                        }
                        // PDF 命名 quirk: Wpn1, Wpn2, Wpn3
                        // 你的列表显示: Wpn Name 1, Wpn1 AtkBonus... Wpn2 AtkBonus...

                        // --- 法术 ---
                        character.Spellbook.SpellcastingClass = GetString(fields, "Spellcasting Class");
                        character.Spellbook.SpellcastingAbility = GetString(fields, "SpellcastingAbility");
                        character.Spellbook.SpellSaveDC = ParseInt(GetString(fields, "SpellSaveDC"));
                        character.Spellbook.SpellAttackBonus = ParseInt(GetString(fields, "SpellAtkBonus"));

                        // 读取法术列表
                        // 0环: 001-008, 1环: 101-112 ... 

                        // 实际遍历PDF字段
                        for (int level = 0; level <= 9; level++)
                        {
                            var levelGroup = character.Spellbook.AllSpells[level];
                            // 读取法术位总量 (用于Current)
                            if (level > 0)
                            {
                                string totalSlots = GetString(fields, $"SlotsTotal {level}");
                                levelGroup.TotalSlots = ParseInt(totalSlots);
                                levelGroup.RemainSlots = levelGroup.TotalSlots;
                            }

                            // 读取每一个法术行
                            int count = levelGroup.Spells.Count;
                            for (int i = 0; i < count; i++)
                            {
                                int pdfIndex = i + 1;
                                string suffix = $"{level}{pdfIndex:D2}"; // e.g. 001, 112, 203

                                string spellName = GetString(fields, $"Spells {suffix}");
                                bool prepared = GetBool(fields, $"Check Box S{suffix}");

                                if (!string.IsNullOrWhiteSpace(spellName))
                                {
                                    levelGroup.Spells[i].Name = spellName;
                                    levelGroup.Spells[i].IsPrepared = prepared;
                                }
                            }
                        }

                        // --- 图片提取 (高级) ---
                        try
                        {
                            ExtractImageFromField(fields, "Character Image", character);
                        }
                        catch { /* 图片提取失败不影响整体 */ }
                    }

                    return character;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取 PDF 失败: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// 将角色导出到PDF
        /// </summary>
        public async Task ExportCharacterPdfAsync(Character character)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"{character.Profile.CharacterName}-{character.Profile.Race}-{character.Profile.ClassAndLevel}.pdf"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string outputPath = saveFileDialog.FileName;

            await Task.Run(() =>
            {
                // 1. 获取嵌入的模板
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("Character.pdf", StringComparison.OrdinalIgnoreCase)) ?? throw new FileNotFoundException("Character.pdf template not found.");
                using Stream templateStream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException("读取嵌入模板失败");
                using PdfReader reader = new PdfReader(templateStream);
                using PdfWriter writer = new PdfWriter(outputPath);
                using PdfDocument pdfDoc = new PdfDocument(reader, writer);
                PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
                IDictionary<string, PdfFormField> fields = form.GetAllFormFields();

                // --- 基础 ---
                SetField(fields, "CharacterName", character.Profile.CharacterName);
                SetField(fields, "CharacterName 2", character.Profile.CharacterName);
                SetField(fields, "Character Image Name", character.Profile.CharacterName);
                SetField(fields, "PlayerName", character.Profile.PlayerName);
                SetField(fields, "Race ", character.Profile.Race); // 注意空格
                SetField(fields, "ClassLevel", character.Profile.ClassAndLevel);
                SetField(fields, "Background", character.Profile.Background);
                SetField(fields, "Alignment", character.Profile.Alignment);
                SetField(fields, "XP", character.Profile.ExperiencePoints.ToString());

                SetField(fields, "Age", character.Profile.Age);
                SetField(fields, "Height", character.Profile.Height);
                SetField(fields, "Weight", character.Profile.Weight);
                SetField(fields, "Eyes", character.Profile.Eyes);
                SetField(fields, "Skin", character.Profile.Skin);
                SetField(fields, "Hair", character.Profile.Hair);
                SetField(fields, "PersonalityTraits ", character.Roleplay.PersonalityTraits); // 注意空格
                SetField(fields, "Ideals", character.Roleplay.Ideals);
                SetField(fields, "Bonds", character.Roleplay.Bonds);
                SetField(fields, "Flaws", character.Roleplay.Flaws);
                SetField(fields, "角色经历", character.Roleplay.CharacterExperience);
                SetField(fields, "Backstory", character.Roleplay.CharacterBackstory);
                SetField(fields, "Allies", character.Roleplay.AlliesAndOrganizations);
                SetField(fields, "Treasure", character.Roleplay.Treasure);
                SetField(fields, "Feat+Traits", character.Roleplay.FeaturesAndTraits);

                // --- 属性 ---
                SetField(fields, "STR", character.Attributes.Strength.ToString());
                SetField(fields, "STRmod", character.Attributes.StrengthMod.ToString());
                SetField(fields, "DEX", character.Attributes.Dexterity.ToString());
                SetField(fields, "DEXmod ", character.Attributes.DexterityMod.ToString()); // 注意空格
                SetField(fields, "CON", character.Attributes.Constitution.ToString());
                SetField(fields, "CONmod", character.Attributes.ConstitutionMod.ToString());
                SetField(fields, "INT", character.Attributes.Intelligence.ToString());
                SetField(fields, "INTmod", character.Attributes.IntelligenceMod.ToString());
                SetField(fields, "WIS", character.Attributes.Wisdom.ToString());
                SetField(fields, "WISmod", character.Attributes.WisdomMod.ToString());
                SetField(fields, "CHA", character.Attributes.Charisma.ToString());
                SetField(fields, "CHAmod", character.Attributes.CharismaMod.ToString());

                // --- 熟练项 ---
                foreach (var kvp in _skillMap)
                {
                    var prop = typeof(Proficiencies).GetProperty(kvp.Value);
                    if (prop != null && (bool)prop.GetValue(character.Proficiencies))
                    {
                        SetCheckBox(fields, $"Check Box {kvp.Key}", true);
                    }
                }
                foreach (var kvp in _attrMap)
                {
                    var prop = typeof(Proficiencies).GetProperty(kvp.Value);
                    if (prop != null && (bool)prop.GetValue(character.Proficiencies))
                    {
                        SetCheckBox(fields, $"Check Box {kvp.Key}", true);
                    }
                }
                SetField(fields, "ProfBonus", "" + character.Profile.ProficiencyBonus);
                SetField(fields, "Passive Perception", character.Profile.PassivePerception.ToString());
                SetField(fields, "Inspiration", character.Profile.Inspiration);
                SetField(fields, "ProficienciesLang", character.Proficiencies.OtherProficienciesAndLanguages);

                // --- 战斗 ---
                SetField(fields, "AC", "" + character.Combat.ArmorClass);
                SetField(fields, "Initiative", "" + character.Combat.Initiative);
                SetField(fields, "Speed", character.Combat.Speed);
                SetField(fields, "HPMax", "" + character.Combat.HitPointsMax);
                SetField(fields, "HPCurrent", "" + character.Combat.HitPointsMax); // 导出时填满
                SetField(fields, "HDTotal", character.Combat.HitDiceTotal);
                SetField(fields, "HDCurrent", character.Combat.HitDiceTotal);
                SetField(fields, "AttacksAndSpellcasting", character.Combat.AttacksAndSpellcastingNotes);
                SetField(fields, "Ability", character.Combat.Ability);

                SetField(fields, "Equipment", character.Inventory.EquipmentText);
                SetField(fields, "CP", "" + character.Inventory.CP);
                SetField(fields, "SP", "" + character.Inventory.SP);
                SetField(fields, "EP", "" + character.Inventory.EP);
                SetField(fields, "GP", "" + character.Inventory.GP);
                SetField(fields, "PP", "" + character.Inventory.PP);

                // --- 武器 ---
                for (int i = 0; i < Math.Min(3, character.Weapons.Count); i++)
                {
                    int id = i + 1;
                    var wpn = character.Weapons[i];
                    SetField(fields, $"Wpn Name {id}", wpn.Name);
                    // PDF 命名: Wpn1 AtkBonus, Wpn2 AtkBonus...
                    SetField(fields, $"Wpn{id} AtkBonus", wpn.AttackBonus.ToString());
                    SetField(fields, $"Wpn{id} Damage", wpn.Damage);
                }

                // --- 法术 ---
                SetField(fields, "Spellcasting Class", "" + character.Spellbook.SpellcastingClass);
                SetField(fields, "SpellcastingAbility", character.Spellbook.SpellcastingAbility);
                SetField(fields, "SpellSaveDC", character.Spellbook.SpellSaveDC.ToString());
                SetField(fields, "SpellAtkBonus", character.Spellbook.SpellAttackBonus.ToString());

                for (int level = 0; level <= 9; level++)
                {
                    var group = character.Spellbook.AllSpells[level];
                    if (level > 0)
                    {
                        SetField(fields, $"SlotsTotal {level}", group.TotalSlots.ToString());
                        SetField(fields, $"SlotsRemaining {level}", "0");
                    }

                    for (int i = 0; i < group.Spells.Count; i++)
                    {
                        var spell = group.Spells[i];
                        int pdfIndex = i + 1;
                        string suffix = $"{level}{pdfIndex:D2}";

                        if (!string.IsNullOrWhiteSpace(spell.Name))
                        {
                            SetField(fields, $"Spells {suffix}", spell.Name);
                            if (spell.IsPrepared) SetCheckBox(fields, $"Check Box S{suffix}", true);
                        }
                    }
                }

                // --- 图片导出 ---
                if (!string.IsNullOrEmpty(character.Profile.PortraitBase64))
                {
                    try
                    {
                        //byte[] imgBytes = Convert.FromBase64String(character.Profile.PortraitBase64);
                        //ImageData imageData = ImageDataFactory.Create(imgBytes);
                        var formField = fields["Character Image"];
                        if (formField != null)
                        {
                            // iText7 设置按钮图标的方式
                            // 注意：这可能需要具体的 PDF Widget 结构支持，如果是标准 PushButton 应该有效
                            formField.SetValue(""); // 清空可能的文本
                            // 设置图片通常涉及修改 Appearance Dictionary (AP)
                            // 简单方式：使用 iText 的 API 如果支持，或者留白
                            // 由于 iText7 设置 Button Image 比较复杂，这里尝试通用方法：
                            // 在许多 AcroForm 实现中，直接设置图片比较困难，需要重建 Widget。
                            // 这里为了代码稳定性，暂不执行极其复杂的 AP 重写，
                            // 如果需要，可以使用 PdfButtonFormField.SetImage(image) 
                            if (formField is PdfButtonFormField buttonField)
                            {
                                buttonField.SetImage(character.Profile.PortraitBase64);
                            }
                        }
                    }
                    catch { /* 图片写入失败忽略 */ }
                }
            });
        }

        // === 辅助方法 ===

        private static string GetString(IDictionary<string, PdfFormField> fields, string key)
        {
            // 模糊匹配：去除首尾空格
            var field = fields.FirstOrDefault(f => f.Key.Trim() == key.Trim()).Value;
            return field?.GetValueAsString() ?? "";
        }

        private static bool GetBool(IDictionary<string, PdfFormField> fields, string key)
        {
            var field = fields.FirstOrDefault(f => f.Key.Trim() == key.Trim()).Value;
            if (field == null) return false;

            // Checkbox 选中时通常值为 "Yes" 或 "/Yes"，未选中为 "Off" 或 "/Off"
            var value = field.GetValueAsString();
            return value != null && (value.Equals("Yes", StringComparison.OrdinalIgnoreCase) || value.Equals("On", StringComparison.OrdinalIgnoreCase));
        }

        private static void SetField(IDictionary<string, PdfFormField> fields, string key, string value)
        {
            var field = fields.FirstOrDefault(f => f.Key.Trim() == key.Trim()).Value;
            field?.SetValue(value ?? "");
        }

        private static void SetCheckBox(IDictionary<string, PdfFormField> fields, string key, bool isChecked)
        {
            var field = fields.FirstOrDefault(f => f.Key.Trim() == key.Trim()).Value;
            if (field is PdfButtonFormField checkBox)
            {
                // 设置为 "Yes" 通常是选中状态，取决于 PDF 定义的 Export Value
                if (isChecked)
                {
                    // 尝试获取 Checkbox 的选中状态值，通常是 "Yes"
                    string[] states = checkBox.GetAppearanceStates();
                    string onState = states.FirstOrDefault(s => !s.Equals("Off", StringComparison.OrdinalIgnoreCase)) ?? "Yes";
                    checkBox.SetValue(onState);
                }
                else
                {
                    checkBox.SetValue("Off");
                }
            }
        }

        private static int ParseInt(string val)
        {
            if (int.TryParse(val, out int result)) return result;
            return 0;
        }

        private static void ExtractImageFromField(IDictionary<string, PdfFormField> fields, string key, Character character)
        {
            if (!fields.TryGetValue(key, out PdfFormField? field)) return;
            var widgets = field.GetWidgets();
            if (widgets == null || widgets.Count == 0) return;

            var widget = widgets[0];
            var apDict = widget.GetAppearanceDictionary();
            if (apDict == null) return;

            // 获取 Normal Appearance
            var normalAp = apDict.GetAsStream(PdfName.N);
            if (normalAp == null) return;

            // 解析 XObject 资源
            var resources = normalAp.GetAsDictionary(PdfName.Resources);
            if (resources == null) return;

            var xObjects = resources.GetAsDictionary(PdfName.XObject);
            if (xObjects == null) return;

            foreach (var xObjectKey in xObjects.KeySet())
            {
                var xObjectStream = xObjects.GetAsStream(xObjectKey);
                if (xObjectStream != null)
                {
                    var subtype = xObjectStream.GetAsName(PdfName.Subtype);
                    if (subtype != null && subtype.Equals(PdfName.Image))
                    {
                        // 找到图片了！
                        var imageXObject = new PdfImageXObject(xObjectStream);
                        byte[] imageBytes = imageXObject.GetImageBytes();
                        character.Profile.PortraitBase64 = Convert.ToBase64String(imageBytes);
                        break; // 只取第一张
                    }
                }
            }
        }
    }
}

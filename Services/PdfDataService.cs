using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using DnDToolkit.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;

namespace DnDToolkit.Services
{
    public class PdfDataService
    {
        // 技能映射表
        private readonly Dictionary<string, string> _skillMap = new()
        {
            { "运动", nameof(Proficiencies.Athletics) },
            { "杂技", nameof(Proficiencies.Acrobatics) },
            { "巧手", nameof(Proficiencies.SleightOfHand) },
            { "躲藏", nameof(Proficiencies.Stealth) },
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

        // 豁免属性映射表
        private readonly Dictionary<string, string> _saveMap = new()
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

                    // 只读模式打开
                    using (PdfDocument document = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify))
                    {
                        var form = document.AcroForm;
                        if (form == null) return null;

                        // --- 基础信息 ---
                        var p = character.Profile;
                        p.CharacterName = GetText(form, "CharacterName");
                        p.PlayerName = GetText(form, "PlayerName");
                        p.Race = GetText(form, "Race ");
                        p.ClassAndLevel = GetText(form, "ClassLevel");
                        p.Background = GetText(form, "Background");
                        p.Alignment = GetText(form, "Alignment");
                        p.ExperiencePoints = ParseInt(GetText(form, "XP"));

                        p.Age = GetText(form, "Age");
                        p.Height = GetText(form, "Height");
                        p.Weight = GetText(form, "Weight");
                        p.Eyes = GetText(form, "Eyes");
                        p.Skin = GetText(form, "Skin");
                        p.Hair = GetText(form, "Hair");

                        var r = character.Roleplay;
                        r.PersonalityTraits = GetText(form, "PersonalityTraits ");
                        r.Ideals = GetText(form, "Ideals");
                        r.Bonds = GetText(form, "Bonds");
                        r.Flaws = GetText(form, "Flaws");
                        r.CharacterExperience = GetText(form, "角色经历");
                        r.CharacterBackstory = GetText(form, "Backstory");
                        r.AlliesAndOrganizations = GetText(form, "Allies");
                        r.Treasure = GetText(form, "Treasure");
                        r.FeaturesAndTraits = GetText(form, "Feat+Traits");

                        // --- 属性 ---
                        var a = character.Attributes;
                        a.Strength = ParseInt(GetText(form, "STR"));
                        a.Dexterity = ParseInt(GetText(form, "DEX"));
                        a.Constitution = ParseInt(GetText(form, "CON"));
                        a.Intelligence = ParseInt(GetText(form, "INT"));
                        a.Wisdom = ParseInt(GetText(form, "WIS"));
                        a.Charisma = ParseInt(GetText(form, "CHA"));

                        // --- 熟练项 ---
                        foreach (var kvp in _skillMap)
                        {
                            bool isProf = GetBool(form, $"Check Box {kvp.Key}");
                            typeof(Proficiencies).GetProperty(kvp.Value)?.SetValue(character.Proficiencies, isProf);
                        }
                        foreach (var kvp in _saveMap)
                        {
                            bool isProf = GetBool(form, $"Check Box {kvp.Key}");
                            typeof(Proficiencies).GetProperty(kvp.Value)?.SetValue(character.Proficiencies, isProf);
                        }

                        // 其他
                        // 熟练加值
                        p.ProficiencyBonus = ParseInt(GetText(form, "ProfBonus"));

                        // 被动感知
                        p.PassivePerception = ParseInt(GetText(form, "Passive Perception"));
                        p.Inspiration = GetText(form, "Inspiration");
                        character.Proficiencies.OtherProficienciesAndLanguages = GetText(form, "ProficienciesLang");

                        // --- 战斗数据 ---
                        var c = character.Combat;
                        // AC, 先攻
                        c.ArmorClass = ParseInt(GetText(form, "AC"));
                        c.Initiative = ParseInt(GetText(form, "Initiative"));
                        c.Speed = GetText(form, "Speed");
                        c.AttacksAndSpellcastingNotes = GetText(form, "AttacksAndSpellcasting");
                        c.Ability = GetText(form, "Ability");

                        // 生命值/生命骰
                        int hpMax = ParseInt(GetText(form, "HPMax"));
                        c.HitPointsMax = hpMax;
                        c.HitPointsCurrent = hpMax; // 导入时默认填满
                        c.HitPointsTemp = ParseInt(GetText(form, "HPTemp"));
                        c.HitDiceTotal = GetText(form, "HDTotal");
                        c.HitDiceCurrent = GetText(form, "HDTotal");

                        // 钱币
                        var i = character.Inventory;
                        i.EquipmentText = GetText(form, "Equipment");
                        i.CP = ParseInt(GetText(form, "CP"));
                        i.SP = ParseInt(GetText(form, "SP"));
                        i.EP = ParseInt(GetText(form, "EP"));
                        i.GP = ParseInt(GetText(form, "GP"));
                        i.PP = ParseInt(GetText(form, "PP"));

                        // --- 武器 ---
                        character.Weapons.Clear();
                        for (int idx = 1; idx <= 3; idx++)
                        {
                            string name = GetText(form, $"Wpn Name {idx}");
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                character.Weapons.Add(new Weapon
                                {
                                    Name = name,
                                    AttackBonus = ParseInt(GetText(form, $"Wpn{idx} AtkBonus")),
                                    Damage = GetText(form, $"Wpn{idx} Damage")
                                });
                            }
                        }

                        // --- 法术 ---
                        var s = character.Spellbook;
                        s.SpellcastingClass = GetText(form, "Spellcasting Class");
                        s.SpellcastingAbility = GetText(form, "SpellcastingAbility");
                        s.SpellSaveDC = ParseInt(GetText(form, "SpellSaveDC"));
                        s.SpellAttackBonus = ParseInt(GetText(form, "SpellAtkBonus"));

                        for (int level = 0; level <= 9; level++)
                        {
                            var group = s.AllSpells[level];
                            if (level > 0)
                            {
                                int totalSlots = ParseInt(GetText(form, $"SlotsTotal {level}"));
                                group.TotalSlots = totalSlots;
                                group.RemainSlots = totalSlots; // 导入时填满
                            }

                            // 法术列表
                            for (int k = 0; k < group.Spells.Count; k++)
                            {
                                int pdfIndex = k + 1;
                                string suffix = $"{level}{pdfIndex:D2}";

                                string spellName = GetText(form, $"Spells {suffix}");
                                bool prepared = GetBool(form, $"Check Box S{suffix}");

                                if (!string.IsNullOrWhiteSpace(spellName))
                                {
                                    group.Spells[k].Name = spellName;
                                    group.Spells[k].IsPrepared = prepared;
                                }
                            }
                        }
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
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("Character.pdf", StringComparison.OrdinalIgnoreCase));

                if (resourceName == null) throw new FileNotFoundException("Character.pdf template not found.");

                using Stream templateStream = assembly.GetManifestResourceStream(resourceName)!;
                using PdfDocument document = PdfReader.Open(templateStream, PdfDocumentOpenMode.Modify);

                var form = document.AcroForm;

                // --- 关键：设置 NeedAppearances ---
                if (form.Elements.ContainsKey("/NeedAppearances"))
                {
                    form.Elements["/NeedAppearances"] = new PdfBoolean(true);
                }
                else
                {
                    form.Elements.Add("/NeedAppearances", new PdfBoolean(true));
                }

                // --- 基础 ---
                var p = character.Profile;
                SetText(form, "CharacterName", p.CharacterName);
                SetText(form, "CharacterName 2", p.CharacterName);
                SetText(form, "Character Image Name", p.CharacterName);
                SetText(form, "PlayerName", p.PlayerName);
                SetText(form, "Race ", p.Race);
                SetText(form, "ClassLevel", p.ClassAndLevel);
                SetText(form, "Background", p.Background);
                SetText(form, "Alignment", p.Alignment);
                SetText(form, "XP", p.ExperiencePoints.ToString());

                SetText(form, "Age", p.Age);
                SetText(form, "Height", p.Height);
                SetText(form, "Weight", p.Weight);
                SetText(form, "Eyes", p.Eyes);
                SetText(form, "Skin", p.Skin);
                SetText(form, "Hair", p.Hair);

                var r = character.Roleplay;
                SetText(form, "PersonalityTraits ", r.PersonalityTraits);
                SetText(form, "Ideals", r.Ideals);
                SetText(form, "Bonds", r.Bonds);
                SetText(form, "Flaws", r.Flaws);
                SetText(form, "角色经历", r.CharacterExperience);
                SetText(form, "Backstory", r.CharacterBackstory);
                SetText(form, "Allies", r.AlliesAndOrganizations);
                SetText(form, "Treasure", r.Treasure);
                SetText(form, "Feat+Traits", r.FeaturesAndTraits);

                // --- 属性 & 调整值 ---
                var a = character.Attributes;
                SetText(form, "STR", a.Strength.ToString());
                SetText(form, "STRmod", a.StrengthMod.ToString());
                SetText(form, "DEX", a.Dexterity.ToString());
                SetText(form, "DEXmod ", a.DexterityMod.ToString());
                SetText(form, "CON", a.Constitution.ToString());
                SetText(form, "CONmod", a.ConstitutionMod.ToString());
                SetText(form, "INT", a.Intelligence.ToString());
                SetText(form, "INTmod", a.IntelligenceMod.ToString());
                SetText(form, "WIS", a.Wisdom.ToString());
                SetText(form, "WISmod", a.WisdomMod.ToString());
                SetText(form, "CHA", a.Charisma.ToString());
                SetText(form, "CHAmod", a.CharismaMod.ToString());

                // --- 熟练项 ---
                foreach (var kvp in _skillMap)
                {
                    bool val = (bool)(typeof(Proficiencies).GetProperty(kvp.Value)?.GetValue(character.Proficiencies) ?? false);
                    SetCheck(form, $"Check Box {kvp.Key}", val);
                }
                foreach (var kvp in _saveMap)
                {
                    bool val = (bool)(typeof(Proficiencies).GetProperty(kvp.Value)?.GetValue(character.Proficiencies) ?? false);
                    SetCheck(form, $"Check Box {kvp.Key}", val);
                }

                SetText(form, "ProfBonus", p.ProficiencyBonus.ToString());
                SetText(form, "Passive Perception", p.PassivePerception.ToString());
                SetText(form, "Inspiration", p.Inspiration);
                SetText(form, "ProficienciesLang", character.Proficiencies.OtherProficienciesAndLanguages);

                // --- 战斗 ---
                var c = character.Combat;
                SetText(form, "AC", c.ArmorClass.ToString());
                SetText(form, "Initiative", c.Initiative.ToString());
                SetText(form, "Speed", c.Speed);

                SetText(form, "HPMax", c.HitPointsMax.ToString());
                SetText(form, "HPCurrent", c.HitPointsMax.ToString());
                SetText(form, "HDTotal", c.HitDiceTotal);
                SetText(form, "HDCurrent", c.HitDiceTotal);

                SetText(form, "AttacksAndSpellcasting", c.AttacksAndSpellcastingNotes);
                SetText(form, "Ability", c.Ability);

                var i = character.Inventory;
                SetText(form, "Equipment", i.EquipmentText);
                SetText(form, "CP", i.CP.ToString());
                SetText(form, "SP", i.SP.ToString());
                SetText(form, "EP", i.EP.ToString());
                SetText(form, "GP", i.GP.ToString());
                SetText(form, "PP", i.PP.ToString());

                // --- 武器 ---
                for (int idx = 0; idx < Math.Min(3, character.Weapons.Count); idx++)
                {
                    int id = idx + 1;
                    var wpn = character.Weapons[idx];
                    SetText(form, $"Wpn Name {id}", wpn.Name);
                    SetText(form, $"Wpn{id} AtkBonus", wpn.AttackBonus.ToString());
                    SetText(form, $"Wpn{id} Damage", wpn.Damage);
                }

                // --- 法术 ---
                var s = character.Spellbook;
                SetText(form, "Spellcasting Class", s.SpellcastingClass);
                SetText(form, "SpellcastingAbility", s.SpellcastingAbility);
                SetText(form, "SpellSaveDC", s.SpellSaveDC.ToString());
                SetText(form, "SpellAtkBonus", s.SpellAttackBonus.ToString());

                for (int level = 0; level <= 9; level++)
                {
                    var group = s.AllSpells[level];
                    if (level > 0)
                    {
                        SetText(form, $"SlotsTotal {level}", group.TotalSlots.ToString());
                        SetText(form, $"SlotsRemaining {level}", group.TotalSlots.ToString());
                    }

                    for (int k = 0; k < group.Spells.Count; k++)
                    {
                        var spell = group.Spells[k];
                        int pdfIndex = k + 1;
                        string suffix = $"{level}{pdfIndex:D2}";

                        if (!string.IsNullOrWhiteSpace(spell.Name))
                        {
                            SetText(form, $"Spells {suffix}", spell.Name);
                            SetCheck(form, $"Check Box S{suffix}", spell.IsPrepared);
                        }
                    }
                }

                document.Save(outputPath);
            });
        }

        // ==================== 辅助方法 ====================

        private static string GetText(PdfAcroForm form, string key)
        {
            var fieldName = form.Fields.Names.FirstOrDefault(n => n.Trim() == key.Trim());
            if (fieldName == null) return "";

            if (form.Fields[fieldName] is PdfTextField textField)
            {
                if (textField.Value is PdfString pdfString)
                {
                    return pdfString.Value;
                }

                return textField.Value?.ToString() ?? "";
            }

            return "";
        }

        private static bool GetBool(PdfAcroForm form, string key)
        {
            var fieldName = form.Fields.Names.FirstOrDefault(n => n.Trim() == key.Trim());
            if (fieldName == null) return false;
            if (form.Fields[fieldName] is PdfCheckBoxField checkBox)
            {
                return checkBox.Checked;
            }
            return false;
        }

        private static void SetText(PdfAcroForm form, string key, string value)
        {
            var fieldName = form.Fields.Names.FirstOrDefault(n => n.Trim() == key.Trim());
            if (fieldName == null) return;
            if (form.Fields[fieldName] is PdfTextField textField)
            {
                textField.Value = new PdfString(value ?? "");
            }
        }

        private static void SetCheck(PdfAcroForm form, string key, bool isChecked)
        {
            var fieldName = form.Fields.Names.FirstOrDefault(n => n.Trim() == key.Trim());
            if (fieldName == null) return;
            if (form.Fields[fieldName] is PdfCheckBoxField checkBox)
            {
                checkBox.Checked = isChecked;
            }
        }

        private static int ParseInt(string val)
        {
            if (int.TryParse(val, out int result)) return result;
            return 0;
        }
    }
}
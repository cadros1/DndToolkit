using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Roleplay : ObservableObject
    {
        // --- P1 右侧小框 ---
        [ObservableProperty] private string personalityTraits = "";
        [ObservableProperty] private string ideals = "";
        [ObservableProperty] private string bonds = "";
        [ObservableProperty] private string flaws = "";

        // --- P2 文本框 ---
        [ObservableProperty] private string characterBackstory = ""; // 角色经历
        [ObservableProperty] private string alliesAndOrganizations = ""; // 同盟&组织
        [ObservableProperty] private string additionalFeaturesAndTraits = ""; // 附加特征 (P2中间)
        [ObservableProperty] private string treasure = ""; // 所持物/宝藏 (P2底部)

        // P1 右下角: 特殊能力 (Features & Traits)
        [ObservableProperty] private string featuresAndTraits = "";
    }
}

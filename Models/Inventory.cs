using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Inventory : ObservableObject
    {
        // --- 货币 (Currency) ---
        [ObservableProperty] private int cP = 0; // 铜币
        [ObservableProperty] private int sP = 0; // 银币
        [ObservableProperty] private int eP = 0; // 银金币
        [ObservableProperty] private int gP = 0; // 金币
        [ObservableProperty] private int pP = 0; // 铂金币

        // --- 装备文本框 (Equipment) ---
        // PDF中这里是一个大文本框，如果你想结构化，可以保留List，
        // 但为了忠实还原PDF，通常提供一个大字符串，或者混合使用。
        [ObservableProperty] private string equipmentText = "";
    }
}

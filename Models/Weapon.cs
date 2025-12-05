using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Models
{
    public partial class Weapon : ObservableObject
    {
        [ObservableProperty] private string name = "";
        [ObservableProperty] private int attackBonus = 0;
        [ObservableProperty] private string damage = ""; // 例如 "1d8 + 3 Slashing"
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDToolkit.Helpers
{
    public enum RollType
    {
        Normal,
        Advantage,    // 优势
        Disadvantage  // 劣势
    }

    public class DiceResult
    {
        public int Total { get; set; }
        public string Details { get; set; } = "";
        public bool IsCritical { get; set; } // 大成功 (自然20)
        public bool IsFumble { get; set; }   // 大失败 (自然1)
    }

    public static class DiceHelper
    {
        private static readonly Random _random = new Random();

        public static DiceResult Roll(int sides, int count, int modifier, RollType type = RollType.Normal)
        {
            // 内部函数：投掷一次 count * d(sides)
            (int sum, int[] rolls) RollOnce()
            {
                var rolls = new int[count];
                for (int i = 0; i < count; i++)
                    rolls[i] = _random.Next(1, sides + 1);
                return (rolls.Sum(), rolls);
            }

            var result1 = RollOnce();
            var result2 = RollOnce();

            int finalSum = result1.sum;
            string rollStr = $"[{string.Join(",", result1.rolls)}]";
            bool crit = false;
            bool fumble = false;

            // 只有 d20 才有大成功/大失败的概念 (通常规则)
            if (sides == 20 && count == 1)
            {
                if (result1.rolls[0] == 20) crit = true;
                if (result1.rolls[0] == 1) fumble = true;
            }

            if (type == RollType.Advantage)
            {
                // 优势：取高
                finalSum = Math.Max(result1.sum, result2.sum);
                rollStr = $"Adv: {result1.sum}[{string.Join(",", result1.rolls)}] vs {result2.sum}[{string.Join(",", result2.rolls)}]";

                // 优势下的大成功判定 (如果最终结果来源于那次投掷)
                if (sides == 20 && count == 1)
                {
                    int val = finalSum; // 取高的那个
                    crit = val == 20;
                    fumble = val == 1;
                }
            }
            else if (type == RollType.Disadvantage)
            {
                // 劣势：取低
                finalSum = Math.Min(result1.sum, result2.sum);
                rollStr = $"Dis: {result1.sum}[{string.Join(",", result1.rolls)}] vs {result2.sum}[{string.Join(",", result2.rolls)}]";

                if (sides == 20 && count == 1)
                {
                    int val = finalSum;
                    crit = val == 20;
                    fumble = val == 1;
                }
            }

            int total = finalSum + modifier;
            string modStr = modifier >= 0 ? $"+{modifier}" : $"{modifier}";

            return new DiceResult
            {
                Total = total,
                Details = $"{rollStr} {modStr} = {total}",
                IsCritical = crit,
                IsFumble = fumble
            };
        }
    }
}

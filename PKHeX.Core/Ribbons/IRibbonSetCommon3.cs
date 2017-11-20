﻿namespace PKHeX.Core
{
    /// <summary> Common Ribbons introduced in Generation 3 </summary>
    internal interface IRibbonSetCommon3
    {
        bool RibbonChampionG3Hoenn { get; set; }
        bool RibbonArtist { get; set; }
        bool RibbonEffort { get; set; }
    }

    internal static partial class RibbonExtensions
    {
        private static readonly string[] RibbonSetNamesCommon3 =
        {
            nameof(IRibbonSetCommon3.RibbonChampionG3Hoenn), nameof(IRibbonSetCommon3.RibbonArtist), nameof(IRibbonSetCommon3.RibbonEffort)
        };
        internal static bool[] RibbonBits(this IRibbonSetCommon3 set)
        {
            if (set == null)
                return new bool[3];
            return new[]
            {
                set.RibbonChampionG3Hoenn,
                set.RibbonArtist,
                set.RibbonEffort,
            };
        }
        internal static string[] RibbonNames(this IRibbonSetCommon3 set) => RibbonSetNamesCommon3;
    }
}

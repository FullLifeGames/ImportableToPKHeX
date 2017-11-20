﻿namespace PKHeX.Core
{
    /// <summary>
    /// Common Encounter Properties base interface.
    /// </summary>
    public interface IEncounterable
    {
        int Species { get; }
        string Name { get; }
        bool EggEncounter { get; }
        int LevelMin { get; }
        int LevelMax { get; }
    }

    public static partial class Extensions
    {
        private static bool IsWithinRange(this IEncounterable encounter, int lvl)
        {
            return encounter.LevelMin <= lvl && lvl <= encounter.LevelMax;
        }
        public static bool IsWithinRange(this IEncounterable encounter, PKM pkm)
        {
            if (!pkm.HasOriginalMetLocation)
                return encounter.IsWithinRange(pkm.CurrentLevel);
            if (encounter.EggEncounter)
                return pkm.CurrentLevel == Legal.GetEggHatchLevel(pkm);
            if (encounter is MysteryGift g)
                return pkm.CurrentLevel == g.Level;
            return pkm.CurrentLevel == pkm.Met_Level;
        }
        internal static string GetEncounterTypeName(this IEncounterable Encounter) => Encounter?.Name ?? "Unknown";
    }
}

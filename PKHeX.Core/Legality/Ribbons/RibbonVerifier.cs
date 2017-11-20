﻿using System.Collections.Generic;
using System.Linq;
using static PKHeX.Core.LegalityCheckStrings;

namespace PKHeX.Core
{
    internal static class RibbonVerifier
    {
        internal static List<string> GetIncorrectRibbons(PKM pkm, object encounterContent, int gen)
        {
            List<string> missingRibbons = new List<string>();
            List<string> invalidRibbons = new List<string>();
            IEnumerable<RibbonResult> ribs = GetRibbonResults(pkm, encounterContent, gen);
            foreach (var bad in ribs)
                (bad.Invalid ? invalidRibbons : missingRibbons).Add(bad.Name);

            var result = new List<string>();
            if (missingRibbons.Count > 0)
                result.Add(string.Format(V600, string.Join(", ", missingRibbons.Select(z => z.Replace("Ribbon", "")))));
            if (invalidRibbons.Count > 0)
                result.Add(string.Format(V601, string.Join(", ", invalidRibbons.Select(z => z.Replace("Ribbon", "")))));
            return result;
        }
        internal static bool GetIncorrectRibbonsEgg(PKM pkm, object encounterContent)
        {
            var event3 = encounterContent as IRibbonSetEvent3;
            var event4 = encounterContent as IRibbonSetEvent4;
            var RibbonNames = ReflectUtil.GetPropertiesStartWithPrefix(pkm.GetType(), "Ribbon");
            if (event3 != null)
                RibbonNames = RibbonNames.Except(event3.RibbonNames());
            if (event4 != null)
                RibbonNames = RibbonNames.Except(event4.RibbonNames());

            foreach (object RibbonValue in RibbonNames.Select(RibbonName => ReflectUtil.GetValue(pkm, RibbonName)))
            {
                if (HasFlag(RibbonValue) || HasCount(RibbonValue))
                    return true;

                bool HasFlag(object o) => o is bool z && z;
                bool HasCount(object o) => o is int z && z > 0;
            }
            return false;
        }
        private static IEnumerable<RibbonResult> GetRibbonResults(PKM pkm, object encounterContent, int gen)
        {
            return GetInvalidRibbons(pkm, gen)
                .Concat(GetInvalidRibbonsEvent1(pkm, encounterContent))
                .Concat(GetInvalidRibbonsEvent2(pkm, encounterContent));
        }
        
        private static IEnumerable<RibbonResult> GetInvalidRibbons(PKM pkm, int gen)
        {
            bool artist = false;
            if (pkm is IRibbonSetOnly3 o3)
            {
                artist = o3.RibbonCounts().Any(z => z == 4);
            }
            if (pkm is IRibbonSetUnique3 u3)
            {
                if (gen != 3 || !IsAllowedBattleFrontier(pkm.Species))
                {
                    if (u3.RibbonWinning)
                        yield return new RibbonResult(nameof(u3.RibbonWinning));
                    if (u3.RibbonVictory)
                        yield return new RibbonResult(nameof(u3.RibbonVictory));
                }
            }
            if (pkm is IRibbonSetUnique4 u4)
            {
                if (!IsAllowedBattleFrontier(pkm.Species, pkm.AltForm, 4))
                    foreach (var z in GetInvalidRibbonsNone(u4.RibbonBitsAbility(), u4.RibbonNamesAbility()))
                        yield return z;

                var c3 = u4.RibbonBitsContest3(); var c3n = u4.RibbonNamesContest3();
                var c4 = u4.RibbonBitsContest4(); var c4n = u4.RibbonNamesContest4();
                var iter3 = gen == 3 ? getMissingContestRibbons(c3, c3n) : GetInvalidRibbonsNone(c3, c3n);
                var iter4 = (gen == 3 || gen == 4) && IsAllowedInContest4(pkm.Species) ? getMissingContestRibbons(c4, c4n) : GetInvalidRibbonsNone(c4, c4n);
                foreach (var z in iter3.Concat(iter4))
                    yield return z;

                for (int i = 0; i < 5; ++i)
                    artist |= c3[3 | i << 2]; // any master rank ribbon

                IEnumerable<RibbonResult> getMissingContestRibbons(IReadOnlyList<bool> bits, IReadOnlyList<string> names)
                {
                    for (int i = 0; i < bits.Count; i += 4)
                    {
                        bool required = false;
                        for (int j = i + 3; j >= i; j--)
                            if (bits[j])
                                required = true;
                            else if (required)
                                yield return new RibbonResult(names[j], false);
                    }
                }
            }
            if (pkm is IRibbonSetCommon4 s4)
            {
                bool inhabited4 = 3 <= gen && gen <= 4;
                IEnumerable<RibbonResult> iterate = GetInvalidRibbons4Any(pkm, s4, gen);
                if (!inhabited4)
                    iterate = iterate.Concat(GetInvalidRibbonsNone(s4.RibbonBitsOnly(), s4.RibbonNamesOnly()));
                foreach (var z in iterate)
                    yield return z;
            }
            if (pkm is IRibbonSetCommon6 s6)
            {
                artist = s6.RibbonCountMemoryContest > 4;
                bool inhabited6 = 3 <= gen && gen <= 6;

                var iterate = inhabited6
                    ? GetInvalidRibbons6Any(pkm, s6, gen)
                    : GetInvalidRibbonsNone(s6.RibbonBits(), s6.RibbonNamesBool());
                foreach (var z in iterate)
                    yield return z;

                if (!inhabited6)
                {
                    if (s6.RibbonCountMemoryContest > 0)
                        yield return new RibbonResult(nameof(s6.RibbonCountMemoryContest));
                    if (s6.RibbonCountMemoryBattle > 0)
                        yield return new RibbonResult(nameof(s6.RibbonCountMemoryBattle));
                }

                if (s6.RibbonBestFriends && pkm.OT_Affection < 255 && pkm.IsUntraded) // can't lower affection
                    yield return new RibbonResult(nameof(s6.RibbonBestFriends));
            }
            if (pkm is IRibbonSetCommon7 s7)
            {
                bool inhabited7 = gen <= 7;
                var iterate = inhabited7 ? GetInvalidRibbons7Any(pkm, s7) : GetInvalidRibbonsNone(s7.RibbonBits(), s7.RibbonNames());
                foreach (var z in iterate)
                    yield return z;
            }
            if (pkm is IRibbonSetCommon3 s3)
            {
                if (s3.RibbonChampionG3Hoenn && gen != 3)
                    yield return new RibbonResult(nameof(s3.RibbonChampionG3Hoenn)); // RSE HoF
                if (s3.RibbonArtist && (gen != 3 || !artist))
                    yield return new RibbonResult(nameof(s3.RibbonArtist)); // RSE Master Rank Portrait
                if (s3.RibbonEffort && gen == 5 && pkm.Format == 5) // unobtainable in Gen 5
                    yield return new RibbonResult(nameof(s3.RibbonEffort));
            }
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons4Any(PKM pkm, IRibbonSetCommon4 s4, int gen)
        {
            if (s4.RibbonRecord)
                yield return new RibbonResult(nameof(s4.RibbonRecord)); // Unobtainable
            if (s4.RibbonFootprint && (pkm.Format < 6 && gen == 5 || gen >= 6 && pkm.CurrentLevel - pkm.Met_Level < 30))
                yield return new RibbonResult(nameof(s4.RibbonFootprint));

            bool gen34 = gen == 3 || gen == 4;
            bool not6 = pkm.Format < 6 || gen > 6 || gen < 3;
            bool noDaily = !gen34 && not6;
            bool noCosmetic = !gen34 && (not6 || pkm.XY && pkm.IsUntraded);

            if (noDaily)
                foreach (var z in GetInvalidRibbonsNone(s4.RibbonBitsDaily(), s4.RibbonNamesDaily()))
                    yield return z;
            if (noCosmetic)
                foreach (var z in GetInvalidRibbonsNone(s4.RibbonBitsCosmetic(), s4.RibbonNamesCosmetic()))
                    yield return z;
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons6Any(PKM pkm, IRibbonSetCommon6 s6, int gen)
        {
            foreach (var p in GetInvalidRibbons6Memory(pkm, s6, gen))
                yield return p;

            bool untraded = pkm.IsUntraded;
            var iter = untraded ? GetInvalidRibbons6Untraded(pkm, s6) : GetInvalidRibbons6Traded(pkm, s6);
            foreach (var p in iter)
                yield return p;

            bool allContest = s6.RibbonBitsContest().All(z => z);
            if (allContest ^ s6.RibbonContestStar && !(untraded && pkm.XY)) // if not already checked
                yield return new RibbonResult(nameof(s6.RibbonContestStar), s6.RibbonContestStar);

            const int mem_Chatelaine = 30;
            bool hasChampMemory = pkm.HT_Memory == mem_Chatelaine || pkm.OT_Memory == mem_Chatelaine;
            if (!hasChampMemory || s6.RibbonBattlerSkillful || s6.RibbonBattlerExpert)
                yield break;

            var result = new RibbonResult(nameof(s6.RibbonBattlerSkillful), false);
            result.Combine(new RibbonResult(nameof(s6.RibbonBattlerExpert)));
            yield return result;
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons6Memory(PKM pkm, IRibbonSetCommon6 s6, int gen)
        {
            int contest = 0;
            int battle = 0;
            switch (gen)
            {
                case 3:
                    contest = IsAllowedInContest4(pkm.Species) ? 40 : 20;
                    battle = IsAllowedBattleFrontier(pkm.Species) ? 8 : 0;
                    break;
                case 4:
                    contest = IsAllowedInContest4(pkm.Species) ? 20 : 0;
                    battle = IsAllowedBattleFrontier(pkm.Species) ? 6 : 0;
                    break;
            }
            if (s6.RibbonCountMemoryContest > contest)
                yield return new RibbonResult(nameof(s6.RibbonCountMemoryContest));
            if (s6.RibbonCountMemoryBattle > battle)
                yield return new RibbonResult(nameof(s6.RibbonCountMemoryBattle));
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons6Untraded(PKM pkm, IRibbonSetCommon6 s6)
        {
            if (pkm.XY)
            {
                if (s6.RibbonChampionG6Hoenn)
                    yield return new RibbonResult(nameof(s6.RibbonChampionG6Hoenn));

                if (s6.RibbonContestStar)
                    yield return new RibbonResult(nameof(s6.RibbonContestStar));
                if (s6.RibbonMasterCoolness)
                    yield return new RibbonResult(nameof(s6.RibbonMasterCoolness));
                if (s6.RibbonMasterBeauty)
                    yield return new RibbonResult(nameof(s6.RibbonMasterBeauty));
                if (s6.RibbonMasterCuteness)
                    yield return new RibbonResult(nameof(s6.RibbonMasterCuteness));
                if (s6.RibbonMasterCleverness)
                    yield return new RibbonResult(nameof(s6.RibbonMasterCleverness));
                if (s6.RibbonMasterToughness)
                    yield return new RibbonResult(nameof(s6.RibbonMasterToughness));
            }
            else if (pkm.AO)
            {
                if (s6.RibbonChampionKalos)
                    yield return new RibbonResult(nameof(s6.RibbonChampionKalos));
            }
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons6Traded(PKM pkm, IRibbonSetCommon6 s6)
        {
            if (s6.RibbonTraining)
            {
                const int req = 12; // only first 12
                int count = pkm.SuperTrainingMedalCount(req);
                if (count < req)
                    yield return new RibbonResult(nameof(s6.RibbonTraining));
            }

            const int mem_Champion = 27;
            bool hasChampMemory = pkm.HT_Memory == mem_Champion || pkm.OT_Memory == mem_Champion;
            if (!hasChampMemory || s6.RibbonChampionKalos || s6.RibbonChampionG6Hoenn)
                yield break;

            var result = new RibbonResult(nameof(s6.RibbonChampionKalos), false);
            result.Combine(new RibbonResult(nameof(s6.RibbonChampionG6Hoenn)));
            yield return result;
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbons7Any(PKM pkm, IRibbonSetCommon7 s7)
        {
            if (!IsAllowedBattleFrontier(pkm.Species))
            {
                if (s7.RibbonBattleRoyale)
                    yield return new RibbonResult(nameof(s7.RibbonBattleRoyale));
                if (s7.RibbonBattleTreeGreat)
                    yield return new RibbonResult(nameof(s7.RibbonBattleTreeGreat));
                if (s7.RibbonBattleTreeMaster)
                    yield return new RibbonResult(nameof(s7.RibbonBattleTreeMaster));
            }
        }

        private static IEnumerable<RibbonResult> GetInvalidRibbonsEvent1(PKM pkm, object encounterContent)
        {
            if (!(pkm is IRibbonSetEvent3 set1))
                yield break;
            var names = set1.RibbonNames();
            var sb = set1.RibbonBits();
            var eb = (encounterContent as IRibbonSetEvent3).RibbonBits();

            if (pkm.Gen3)
            {
                eb[0] = sb[0]; // permit Earth Ribbon
                if (pkm.Version == 15 && encounterContent is EncounterStaticShadow s)
                {
                    // only require national ribbon if no longer on origin game
                    bool xd = !Encounters3.Encounter_Colo.Contains(s);
                    eb[1] = !(xd && pkm is XK3 x && !x.RibbonNational || !xd && pkm is CK3 c && !c.RibbonNational);
                }
            }

            for (int i = 0; i < sb.Length; i++)
                if (sb[i] != eb[i])
                    yield return new RibbonResult(names[i], !eb[i]); // only flag if invalid
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbonsEvent2(PKM pkm, object encounterContent)
        {
            if (!(pkm is IRibbonSetEvent4 set2))
                yield break;
            var names = set2.RibbonNames();
            var sb = set2.RibbonBits();
            var eb = (encounterContent as IRibbonSetEvent4).RibbonBits();

            if (encounterContent is EncounterStatic s && s.RibbonWishing)
                eb[1] = true; // require Wishing Ribbon

            for (int i = 0; i < sb.Length; i++)
                if (sb[i] != eb[i])
                    yield return new RibbonResult(names[i], !eb[i]); // only flag if invalid
        }
        private static IEnumerable<RibbonResult> GetInvalidRibbonsNone(IReadOnlyList<bool> bits, IReadOnlyList<string> names)
        {
            for (int i = 0; i < bits.Count; i++)
                if (bits[i])
                    yield return new RibbonResult(names[i]);
        }

        private static bool IsAllowedInContest4(int species) => species != 201 && species != 132; // Disallow Unown and Ditto
        private static bool IsAllowedBattleFrontier(int species, int form = 0, int gen = 0)
        {
            if (gen == 4 && species == 172 && form == 1) // spiky
                return false;

            return !Legal.BattleFrontierBanlist.Contains(species);
        }
    }
}

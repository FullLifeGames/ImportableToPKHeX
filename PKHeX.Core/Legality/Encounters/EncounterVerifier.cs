﻿using System;
using System.Collections.Generic;
using System.Linq;
using static PKHeX.Core.LegalityCheckStrings;

namespace PKHeX.Core
{
    public static class EncounterVerifier
    {
        /// <summary>
        /// Gets the method to verify the <see cref="IEncounterable"/> data.
        /// </summary>
        /// <param name="pkm">Source data to verify</param>
        /// <returns>Returns the verification method appropriate for the input PKM</returns>
        public static Func<PKM, LegalInfo, CheckResult> GetEncounterVerifierMethod(PKM pkm)
        {
            switch (pkm.GenNumber)
            {
                case 1:
                case 2:
                    return VerifyEncounterG12;
                default:
                    return VerifyEncounter;
            }
        }

        private static CheckResult VerifyEncounter(PKM pkm, LegalInfo info)
        {
            switch (info.EncounterMatch)
            {
                case EncounterEgg e:
                    pkm.WasEgg = true;
                    return VerifyEncounterEgg(pkm, e);
                case EncounterLink l:
                    return VerifyEncounterLink(pkm, l);
                case EncounterTrade t:
                    return VerifyEncounterTrade(pkm, t);
                case EncounterSlot w:
                    return VerifyEncounterWild(pkm, w);
                case EncounterStatic s:
                    return VerifyEncounterStatic(pkm, s);
                case MysteryGift g:
                    return VerifyEncounterEvent(pkm, g);
                default:
                    return new CheckResult(Severity.Invalid, V80, CheckIdentifier.Encounter);
            }
        }
        private static CheckResult VerifyEncounterG12(PKM pkm, LegalInfo info)
        {
            var encounter = info.EncounterMatch;
            var EncounterMatch = encounter is GBEncounterData g ? g.Encounter : encounter;
            if (encounter.EggEncounter)
            {
                pkm.WasEgg = true;
                return VerifyEncounterEgg(pkm, EncounterMatch);
            }
            if (EncounterMatch is EncounterSlot1 l)
            {
                if (info.Generation == 2)
                    return VerifyWildEncounterGen2(pkm, l);
                return new CheckResult(Severity.Valid, V68, CheckIdentifier.Encounter);
            }
            if (EncounterMatch is EncounterStatic s)
                return VerifyEncounterStatic(pkm, s);
            if (EncounterMatch is EncounterTrade t)
                return VerifyEncounterTrade(pkm, t);

            return new CheckResult(Severity.Invalid, V80, CheckIdentifier.Encounter);
        }

        // Gen2 Wild Encounters
        private static CheckResult VerifyWildEncounterGen2(PKM pkm, EncounterSlot1 encounter)
        {
            switch (encounter.Type)
            {
                // Fishing in the beta gen 2 Safari Zone
                case SlotType.Old_Rod_Safari:
                case SlotType.Good_Rod_Safari:
                case SlotType.Super_Rod_Safari:
                    return new CheckResult(Severity.Invalid, V609, CheckIdentifier.Encounter);
            }

            if (encounter.Version == GameVersion.C)
                return VerifyWildEncounterCrystal(pkm, encounter);

            return new CheckResult(Severity.Valid, V68, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyWildEncounterCrystal(PKM pkm, EncounterSlot encounter)
        {
            switch (encounter.Type)
            {
                case SlotType.Headbutt:
                case SlotType.Headbutt_Special:
                    return VerifyWildEncounterCrystalHeadbutt(pkm, encounter);

                case SlotType.Old_Rod:
                case SlotType.Good_Rod:
                case SlotType.Super_Rod:
                    switch (encounter.Location)
                    {
                        case 19: // National Park
                            return new CheckResult(Severity.Invalid, V608, CheckIdentifier.Encounter);
                        case 76: // Route 14
                            return new CheckResult(Severity.Invalid, V607, CheckIdentifier.Encounter);
                    }
                    break;
            }

            return new CheckResult(Severity.Valid, V68, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyWildEncounterCrystalHeadbutt(PKM pkm, EncounterSlot encounter)
        {
            var Area = Legal.GetCrystalTreeArea(encounter);
            if (Area == null)  // Failsafe, every area with headbutt encounters has a tree area
                return new CheckResult(Severity.Invalid, V605, CheckIdentifier.Encounter);

            var table = Area.GetTrees(encounter.Type);
            var trainerpivot = pkm.TID % 10;
            switch (table[trainerpivot])
            {
                case TreeEncounterAvailable.ValidTree:
                    return new CheckResult(Severity.Valid, V604, CheckIdentifier.Encounter);
                case TreeEncounterAvailable.InvalidTree:
                    return new CheckResult(Severity.Invalid, V605, CheckIdentifier.Encounter);
                default: // Impossible
                    return new CheckResult(Severity.Invalid, V606, CheckIdentifier.Encounter);
            }
        }

        // Eggs
        private static CheckResult VerifyEncounterEgg(PKM pkm, IEncounterable egg)
        {
            // Check Species
            if (Legal.NoHatchFromEgg.Contains(pkm.Species))
                return new CheckResult(Severity.Invalid, V50, CheckIdentifier.Encounter);
            switch (pkm.GenNumber)
            {
                case 1:
                case 2: return new CheckResult(CheckIdentifier.Encounter); // no met location info
                case 3: return pkm.Format != 3 ? VerifyEncounterEgg3Transfer(pkm) : VerifyEncounterEgg3(pkm);
                case 4: return pkm.IsEgg ? VerifyUnhatchedEgg(pkm, 02002) : VerifyEncounterEgg4(pkm);
                case 5: return pkm.IsEgg ? VerifyUnhatchedEgg(pkm, 30003) : VerifyEncounterEgg5(pkm);
                case 6: return pkm.IsEgg ? VerifyUnhatchedEgg(pkm, 30002) : VerifyEncounterEgg6(pkm);
                case 7: return pkm.IsEgg ? VerifyUnhatchedEgg(pkm, 30002) : VerifyEncounterEgg7(pkm);

                default: // none of the above
                    return new CheckResult(Severity.Invalid, V51, CheckIdentifier.Encounter);
            }
        }
        private static CheckResult VerifyEncounterEgg3(PKM pkm)
        {
            return pkm.Format == 3 ? VerifyEncounterEgg3Native(pkm) : VerifyEncounterEgg3Transfer(pkm);
        }
        private static CheckResult VerifyEncounterEgg3Native(PKM pkm)
        {
            if (pkm.Met_Level != 0)
                return new CheckResult(Severity.Invalid, string.Format(V52, 0), CheckIdentifier.Encounter);
            if (pkm.IsEgg)
            {
                var loc = pkm.FRLG ? Legal.ValidEggMet_FRLG : Legal.ValidEggMet_RSE;
                if (!loc.Contains(pkm.Met_Location))
                    return new CheckResult(Severity.Invalid, V55, CheckIdentifier.Encounter);
            }
            else
            {
                var locs = pkm.FRLG ? Legal.ValidMet_FRLG : pkm.E ? Legal.ValidMet_E : Legal.ValidMet_RS;
                if (locs.Contains(pkm.Met_Location))
                    return new CheckResult(Severity.Valid, V53, CheckIdentifier.Encounter);
                if (Legal.ValidMet_FRLG.Contains(pkm.Met_Location) || Legal.ValidMet_E.Contains(pkm.Met_Location) || Legal.ValidMet_RS.Contains(pkm.Met_Location))
                    return new CheckResult(Severity.Valid, V56, CheckIdentifier.Encounter);
                return new CheckResult(Severity.Invalid, V54, CheckIdentifier.Encounter);
            }
            return new CheckResult(Severity.Valid, V53, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterEgg3Transfer(PKM pkm)
        {
            if (pkm.IsEgg)
                return new CheckResult(Severity.Invalid, V57, CheckIdentifier.Encounter);
            if (pkm.Met_Level < 5)
                return new CheckResult(Severity.Invalid, V58, CheckIdentifier.Encounter);
            if (pkm.Egg_Location != 0)
                return new CheckResult(Severity.Invalid, V59, CheckIdentifier.Encounter);
            if (pkm.Format == 4 && pkm.Met_Location != 0x37) // Pal Park
                return new CheckResult(Severity.Invalid, V60, CheckIdentifier.Encounter);
            if (pkm.Format != 4 && pkm.Met_Location != 30001)
                return new CheckResult(Severity.Invalid, V61, CheckIdentifier.Encounter);

            return new CheckResult(Severity.Valid, V53, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterEgg4(PKM pkm)
        {
            if (pkm.Format == 4)
                return VerifyEncounterEggLevelLoc(pkm, 0, Legal.Met_HGSS_Hatch);
            if (pkm.IsEgg)
                return new CheckResult(Severity.Invalid, V57, CheckIdentifier.Encounter);
            // transferred
            if (pkm.Met_Level < 1)
                return new CheckResult(Severity.Invalid, V58, CheckIdentifier.Encounter);

            if (pkm.Met_Location != 30001)
                return new CheckResult(Severity.Invalid, V61, CheckIdentifier.Encounter);
            return new CheckResult(Severity.Valid, V53, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterEgg5(PKM pkm)
        {
            return VerifyEncounterEggLevelLoc(pkm, 1, pkm.B2W2 ? Legal.ValidMet_B2W2 : Legal.ValidMet_BW);
        }
        private static CheckResult VerifyEncounterEgg6(PKM pkm)
        {
            if (pkm.AO)
                return VerifyEncounterEggLevelLoc(pkm, 1, Legal.ValidMet_AO);

            if (pkm.Egg_Location == 318)
                return new CheckResult(Severity.Invalid, V55, CheckIdentifier.Encounter);

            return VerifyEncounterEggLevelLoc(pkm, 1, Legal.ValidMet_XY);
        }
        private static CheckResult VerifyEncounterEgg7(PKM pkm)
        {
            if (pkm.SM)
                return VerifyEncounterEggLevelLoc(pkm, 1, Legal.ValidMet_SM);
            if (pkm.USUM)
                return VerifyEncounterEggLevelLoc(pkm, 1, Legal.ValidMet_USUM);

            // no other games
            return new CheckResult(Severity.Invalid, V51, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterEggLevelLoc(PKM pkm, int eggLevel, ICollection<int> MetLocations)
        {
            if (pkm.Met_Level != eggLevel)
                return new CheckResult(Severity.Invalid, string.Format(V52, eggLevel), CheckIdentifier.Encounter);
            return MetLocations.Contains(pkm.Met_Location)
                ? new CheckResult(Severity.Valid, V53, CheckIdentifier.Encounter)
                : new CheckResult(Severity.Invalid, V54, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyUnhatchedEgg(PKM pkm, int tradeLoc)
        {
            var eggLevel = pkm.Format < 5 ? 0 : 1;
            if (pkm.Met_Level != eggLevel)
                return new CheckResult(Severity.Invalid, string.Format(V52, eggLevel), CheckIdentifier.Encounter);
            if (pkm.Egg_Location == tradeLoc)
                return new CheckResult(Severity.Invalid, V62, CheckIdentifier.Encounter);

            if (pkm.Met_Location == tradeLoc)
                return new CheckResult(Severity.Valid, V56, CheckIdentifier.Encounter);
            return pkm.Met_Location == 0
                ? new CheckResult(Severity.Valid, V63, CheckIdentifier.Encounter)
                : new CheckResult(Severity.Invalid, V59, CheckIdentifier.Encounter);
        }

        // Other
        private static CheckResult VerifyEncounterWild(PKM pkm, EncounterSlot slot)
        {
            // Check for Unreleased Encounters / Collisions
            switch (pkm.GenNumber)
            {
                case 4:
                    if (slot.Location == 193 && slot.Type == SlotType.Surf) // surfing in Johto Route 45
                        return new CheckResult(Severity.Invalid, V384, CheckIdentifier.Encounter);
                    break;
            }

            if (slot.Permissions.IsNormalLead)
                return slot.Permissions.Pressure
                    ? new CheckResult(Severity.Valid, V67, CheckIdentifier.Encounter)
                    : new CheckResult(Severity.Valid, V68, CheckIdentifier.Encounter);

            // Decreased Level Encounters
            if (slot.Permissions.WhiteFlute)
                return slot.Permissions.Pressure
                    ? new CheckResult(Severity.Valid, V69, CheckIdentifier.Encounter)
                    : new CheckResult(Severity.Valid, V70, CheckIdentifier.Encounter);

            // Increased Level Encounters
            if (slot.Permissions.BlackFlute)
                return slot.Permissions.Pressure
                    ? new CheckResult(Severity.Valid, V71, CheckIdentifier.Encounter)
                    : new CheckResult(Severity.Valid, V72, CheckIdentifier.Encounter);

            if (slot.Permissions.Pressure)
                return new CheckResult(Severity.Valid, V67, CheckIdentifier.Encounter);

            return new CheckResult(Severity.Valid, V73, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterStatic(PKM pkm, EncounterStatic s)
        {
            // Check for Unreleased Encounters / Collisions
            switch (pkm.GenNumber)
            {
                case 3:
                    if (s is EncounterStaticShadow w && w.EReader && pkm.Language != (int)LanguageID.Japanese) // Non-JP E-reader Pokemon 
                        return new CheckResult(Severity.Invalid, V406, CheckIdentifier.Encounter);
                    if (pkm.Species == 151 && s.Location == 201 && pkm.Language != (int)LanguageID.Japanese) // Non-JP Mew (Old Sea Map)
                        return new CheckResult(Severity.Invalid, V353, CheckIdentifier.Encounter);
                    break;
                case 4:
                    if (pkm.Species == 493 && s.Location == 086) // Azure Flute Arceus
                        return new CheckResult(Severity.Invalid, V352, CheckIdentifier.Encounter);
                    if (pkm.Species == 491 && s.Location == 079 && !pkm.Pt) // DP Darkrai
                        return new CheckResult(Severity.Invalid, V383, CheckIdentifier.Encounter);
                    if (pkm.Species == 492 && s.Location == 063 && !pkm.Pt) // DP Shaymin
                        return new CheckResult(Severity.Invalid, V354, CheckIdentifier.Encounter);
                    if (s.Location == 193 && (s as EncounterStaticTyped)?.TypeEncounter == EncounterType.Surfing_Fishing) // Roaming pokemon surfing in Johto Route 45
                        return new CheckResult(Severity.Invalid, V384, CheckIdentifier.Encounter);
                    break;
                case 7:
                    if (s.EggLocation == 60002 && pkm.RelearnMoves.Any(m => m != 0))
                        return new CheckResult(Severity.Invalid, V74, CheckIdentifier.RelearnMove); // not gift egg
                    break;
            }
            if (s.EggEncounter && !pkm.IsEgg) // hatched
            {
                var hatchCheck = VerifyEncounterEgg(pkm, null);
                if (!hatchCheck.Valid)
                    return hatchCheck;
            }

            return new CheckResult(Severity.Valid, V75, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterTrade(PKM pkm, EncounterTrade trade)
        {
            if (trade.Species == pkm.Species && trade.EvolveOnTrade)
            {
                // Pokemon that evolve on trade can not be in the phase evolution after the trade
                // If the trade holds an everstone EvolveOnTrade will be false for the encounter
                var species = LegalityAnalysis.SpeciesStrings;
                var unevolved = species[pkm.Species];
                var evolved = species[pkm.Species + 1];
                return new CheckResult(Severity.Invalid, string.Format(V401, unevolved, evolved), CheckIdentifier.Encounter);
            }
            return new CheckResult(Severity.Valid, V76, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterLink(PKM pkm, EncounterLink enc)
        {
            // Should NOT be Fateful, and should be in Database
            if (enc == null)
                return new CheckResult(Severity.Invalid, V43, CheckIdentifier.Encounter);

            if (pkm.XY && !enc.XY)
                return new CheckResult(Severity.Invalid, V44, CheckIdentifier.Encounter);
            if (pkm.AO && !enc.ORAS)
                return new CheckResult(Severity.Invalid, V45, CheckIdentifier.Encounter);

            if (enc.Shiny != null && (bool)enc.Shiny ^ pkm.IsShiny)
                return new CheckResult(Severity.Invalid, V47, CheckIdentifier.Encounter);

            return pkm.FatefulEncounter
                ? new CheckResult(Severity.Invalid, V48, CheckIdentifier.Encounter)
                : new CheckResult(Severity.Valid, V49, CheckIdentifier.Encounter);
        }
        private static CheckResult VerifyEncounterEvent(PKM pkm, MysteryGift MatchedGift)
        {
            switch (MatchedGift)
            {
                case PCD pcd:
                    if (!pcd.CanBeReceivedBy(pkm.Version))
                        return new CheckResult(Severity.Invalid, string.Format(V21, MatchedGift.CardHeader, $"-- {V416}"), CheckIdentifier.Encounter);
                    break;
            }
            if (!pkm.IsEgg && MatchedGift.IsEgg) // hatched
            {
                var hatchCheck = VerifyEncounterEgg(pkm, null);
                if (!hatchCheck.Valid)
                    return hatchCheck;
            }

            // Strict matching already performed by EncounterGenerator. May be worth moving some checks here to better flag invalid gifts.
            return new CheckResult(Severity.Valid, string.Format(V21, MatchedGift.CardHeader, string.Empty), CheckIdentifier.Encounter);
        }
    }
}

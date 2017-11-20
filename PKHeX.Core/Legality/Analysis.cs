﻿#define SUPPRESS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static PKHeX.Core.LegalityCheckStrings;

namespace PKHeX.Core
{
    /// <summary>
    /// Legality Check object containing the <see cref="CheckResult"/> data and overview values from the parse.
    /// </summary>
    public partial class LegalityAnalysis
    {
        private PKM pkm;
        private readonly bool Error;
        private readonly List<CheckResult> Parse = new List<CheckResult>();

        private IEncounterable EncounterOriginalGB;
        private IEncounterable EncounterMatch => Info.EncounterMatch;
        private Type Type; // Parent class when applicable (EncounterStatic / MysteryGift)

        private CheckResult Encounter, History;

        public readonly bool Parsed;
        public readonly bool Valid;
        private readonly PersonalInfo PersonalInfo;
        public LegalInfo Info { get; private set; }
        public bool ParsedValid => Parsed && Valid;
        public bool ParsedInvalid => Parsed && !Valid;
        public string Report(bool verbose = false) => verbose ? GetVerboseLegalityReport() : GetLegalityReport();
        private IEnumerable<int> AllSuggestedMoves
        {
            get
            {
                if (_allSuggestedMoves != null)
                    return _allSuggestedMoves;
                if (Error || pkm == null || !pkm.IsOriginValid)
                    return new int[4];
                return _allSuggestedMoves = GetSuggestedMoves(true, true, true);
            }
        }
        private IEnumerable<int> AllSuggestedRelearnMoves
        {
            get
            {
                if (_allSuggestedRelearnMoves != null)
                    return _allSuggestedRelearnMoves;
                if (Error || pkm == null || !pkm.IsOriginValid)
                    return new int[4];
                var gender = pkm.PersonalInfo.Gender;
                var inheritLvlMoves = gender > 0 && gender < 255 || Legal.MixedGenderBreeding.Contains(Info.EncounterMatch.Species);
                return _allSuggestedRelearnMoves = Legal.GetValidRelearn(pkm, Info.EncounterMatch.Species, inheritLvlMoves).ToArray();
            }
        }
        private int[] _allSuggestedMoves, _allSuggestedRelearnMoves;
        public int[] AllSuggestedMovesAndRelearn => AllSuggestedMoves.Concat(AllSuggestedRelearnMoves).ToArray();
        private string EncounterName
        {
            get
            {
                var enc = EncounterOriginalGB ?? EncounterMatch;
                return $"{enc.GetEncounterTypeName()} ({SpeciesStrings[enc.Species]})";
            }
        }

        /// <summary>
        /// Checks the input <see cref="PKM"/> data for legality.
        /// </summary>
        /// <param name="pk">Input data to check</param>
        /// <param name="table"><see cref="SaveFile"/> specific personal data</param>
        public LegalityAnalysis(PKM pk, PersonalTable table = null)
        {
#if SUPPRESS
            try
#endif
            {
                PersonalInfo = table?.GetFormeEntry(pk.Species, pk.AltForm) ?? pk.PersonalInfo;
                switch (pk.Format) // prior to storing GameVersion
                {
                    case 1: ParsePK1(pk); break;
                    case 2: ParsePK1(pk); break;
                }

                if (!Parse.Any())
                switch (pk.GenNumber)
                {
                    case 3: ParsePK3(pk); break;
                    case 4: ParsePK4(pk); break;
                    case 5: ParsePK5(pk); break;
                    case 6: ParsePK6(pk); break;

                    case 1: case 2:
                    case 7: ParsePK7(pk); break;
                }

                if (Parse.Count > 0)
                {
                    if (Parse.Any(chk => !chk.Valid))
                        Valid = false;
                    else if (Info.Moves.Any(m => m.Valid != true))
                        Valid = false;
                    else if (Info.Relearn.Any(m => m.Valid != true))
                        Valid = false;
                    else
                        Valid = true;

                    if (pkm.FatefulEncounter && Info.Relearn.Any(chk => !chk.Valid) && EncounterMatch == null)
                        AddLine(Severity.Indeterminate, V188, CheckIdentifier.Fateful);
                }
            }
#if SUPPRESS
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                Valid = false;
                AddLine(Severity.Invalid, V190, CheckIdentifier.Misc);
                pkm = pk;
                Error = true;
            }
#endif
            Parsed = true;
        }

        private void AddLine(Severity s, string c, CheckIdentifier i)
        {
            AddLine(new CheckResult(s, c, i));
        }
        private void AddLine(CheckResult chk)
        {
            Parse.Add(chk);
        }

        private void ParsePK1(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }
            UpdateTradebackG12();

            UpdateInfo();
            UpdateTypeInfo();
            VerifyNickname();
            VerifyDVs();
            VerifyEVs();
            VerifyG1OT();
            VerifyMiscG1();
        }
        private void ParsePK3(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }

            UpdateInfo();
            UpdateTypeInfo();
            UpdateChecks();
            if (pkm.Format > 3)
                VerifyTransferLegalityG3();

            if (pkm.Version == 15)
                VerifyCXD();

            if (Info.EncounterMatch is WC3 z && z.NotDistributed)
                AddLine(Severity.Invalid, V413, CheckIdentifier.Encounter);
        }
        private void ParsePK4(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }

            UpdateInfo();
            UpdateTypeInfo();
            UpdateChecks();
            if (pkm.Format > 4)
                VerifyTransferLegalityG4();
        }
        private void ParsePK5(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }

            UpdateInfo();
            UpdateTypeInfo();
            UpdateChecks();
        }
        private void ParsePK6(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }

            UpdateInfo();
            UpdateTypeInfo();
            UpdateChecks();
        }
        private void ParsePK7(PKM pk)
        {
            pkm = pk;
            if (!pkm.IsOriginValid)
            { AddLine(Severity.Invalid, V187, CheckIdentifier.GameOrigin); return; }

            UpdateInfo();
            if (pkm.VC)
                UpdateVCTransferInfo();
            UpdateTypeInfo();
            UpdateChecks();
        }

        private void UpdateVCTransferInfo()
        {
            EncounterOriginalGB = EncounterMatch;
            Info.EncounterMatch = EncounterGenerator.GetVCStaticTransferEncounter(pkm);
            EncounterStatic s = Info.EncounterMatch as EncounterStatic;
            if (s == null || !EncounterGenerator.IsVCStaticTransferEncounterValid(pkm, s))
            { AddLine(Severity.Invalid, V80, CheckIdentifier.Encounter); return; }

            foreach (var z in VerifyVCEncounter(pkm, EncounterOriginalGB.Species, EncounterOriginalGB as GBEncounterData, s))
                AddLine(z);
        }
        private void UpdateInfo()
        {
            Info = EncounterFinder.FindVerifiedEncounter(pkm);
            Encounter = Info.Parse[0];
            Parse.AddRange(Info.Parse);
        }
        private void UpdateTradebackG12()
        {
            if (pkm.Format == 1)
            {
                Legal.SetTradebackStatusRBY(pkm);
                return;
            }

            if (pkm.Format == 2 || pkm.VC2)
            {
                // Check for impossible tradeback scenarios
                // Korean Gen2 games can't tradeback because there are no Gen1 Korean games released
                bool g2only = pkm.Korean || pkm.IsEgg || pkm.HasOriginalMetLocation ||
                              pkm.Species > Legal.MaxSpeciesID_1 && !Legal.FutureEvolutionsGen1.Contains(pkm.Species);
                pkm.TradebackStatus = g2only ? TradebackType.Gen2_NotTradeback : TradebackType.Any;
                return;
            }

            // VC2 is released, we can assume it will be TradebackType.Any.
            // Is impossible to differentiate a VC1 pokemon traded to Gen7 after VC2 is available.
            // Met Date cannot be used definitively as the player can change their system clock.
            pkm.TradebackStatus = TradebackType.Any;
        }
        private void UpdateTypeInfo()
        {
            if (pkm.GenNumber <= 2 && pkm.TradebackStatus == TradebackType.Any && (EncounterMatch as GBEncounterData)?.Generation != pkm.GenNumber)
                // Example: GSC Pokemon with only possible encounters in RBY, like the legendary birds
                pkm.TradebackStatus = TradebackType.WasTradeback;

            Type = (EncounterOriginalGB ?? EncounterMatch)?.GetType();
            var bt = Type.GetTypeInfo().BaseType;
            if (bt != null && !(bt == typeof(Array) || bt == typeof(object) || bt.GetTypeInfo().IsPrimitive)) // a parent exists
                Type = bt; // use base type
        }
        private void UpdateChecks()
        {
            VerifyECPID();
            VerifyNickname();
            VerifyOT();
            VerifyIVs();
            VerifyEVs();
            VerifyLevel();
            VerifyRibbons();
            VerifyAbility();
            VerifyBall();
            VerifyForm();
            VerifyMisc();
            VerifyGender();
            VerifyItem();
            if (pkm.Format >= 4)
                VerifyEncounterType();
            if (pkm.Format >= 6)
            {
                History = VerifyHistory();
                AddLine(History);
                VerifyOTMemory();
                VerifyHTMemory();
                VerifyHyperTraining();
                VerifyMedals();
                VerifyConsoleRegion();
                VerifyVersionEvolution();
            }

            // SecondaryChecked = true;
        }
        private string GetLegalityReport()
        {
            if (!Parsed || pkm == null || Info == null)
                return V189;
            
            var lines = new List<string>();
            var vMoves = Info.Moves;
            var vRelearn = Info.Relearn;
            for (int i = 0; i < 4; i++)
                if (!vMoves[i].Valid)
                    lines.Add(string.Format(V191, vMoves[i].Judgement.Description(), i + 1, vMoves[i].Comment));

            if (pkm.Format >= 6)
            for (int i = 0; i < 4; i++)
                if (!vRelearn[i].Valid)
                    lines.Add(string.Format(V192, vRelearn[i].Judgement.Description(), i + 1, vRelearn[i].Comment));

            if (lines.Count == 0 && Parse.All(chk => chk.Valid) && Valid)
                return V193;
            
            // Build result string...
            var outputLines = Parse.Where(chk => !chk.Valid); // Only invalid
            lines.AddRange(outputLines.Select(chk => string.Format(V196, chk.Judgement.Description(), chk.Comment)));

            if (lines.Count == 0)
                return V190;

            return string.Join(Environment.NewLine, lines);
        }
        private string GetVerboseLegalityReport()
        {
            if (!Parsed || pkm == null || Info == null)
                return V189;

            const string separator = "===";
            string[] br = {separator, ""};
            var lines = new List<string> {br[1]};
            lines.AddRange(br);
            int rl = lines.Count;

            var vMoves = Info.Moves;
            var vRelearn = Info.Relearn;
            for (int i = 0; i < 4; i++)
                if (vMoves[i].Valid)
                    lines.Add(string.Format(V191, vMoves[i].Judgement.Description(), i + 1, vMoves[i].Comment));

            if (pkm.Format >= 6)
            for (int i = 0; i < 4; i++)
                if (vRelearn[i].Valid)
                    lines.Add(string.Format(V192, vRelearn[i].Judgement.Description(), i + 1, vRelearn[i].Comment));

            if (rl != lines.Count) // move info added, break for next section
                lines.Add(br[1]);
            
            var outputLines = Parse.Where(chk => chk != null && chk.Valid && chk.Comment != V).OrderBy(chk => chk.Judgement); // Fishy sorted to top
            lines.AddRange(outputLines.Select(chk => string.Format(V196, chk.Judgement.Description(), chk.Comment)));

            lines.AddRange(br);
            lines.Add(string.Format(V195, EncounterName));
            if (pkm.VC)
                lines.Add(string.Format(V196, nameof(GameVersion), Info.Game));
            var pidiv = Info.PIDIV ?? MethodFinder.Analyze(pkm);
            if (pidiv != null)
            {
                if (!pidiv.NoSeed)
                    lines.Add(string.Format(V248, pidiv.OriginSeed.ToString("X8")));
                lines.Add(string.Format(V249, pidiv.Type));
            }
            if (!Valid && Info.InvalidMatches != null)
            {
                lines.Add("Other match(es):");
                lines.AddRange(Info.InvalidMatches.Select(z => $"{z.Name}: {z.Reason}"));
            }
            
            return GetLegalityReport() + string.Join(Environment.NewLine, lines);
        }

        // Suggestions
        public int[] GetSuggestedRelearn()
        {
            if (Info.RelearnBase == null || pkm.GenNumber < 6 || !pkm.IsOriginValid)
                return new int[4];

            if (!pkm.WasEgg)
                return Info.RelearnBase;

            List<int> window = new List<int>(Info.RelearnBase.Where(z => z != 0));
            window.AddRange(pkm.Moves.Where((v, i) => !Info.Moves[i].Valid || Info.Moves[i].Flag));
            window = window.Distinct().ToList();
            int[] moves = new int[4];
            int start = Math.Max(0, window.Count - 4);
            int count = Math.Min(4, window.Count);
            window.CopyTo(start, moves, 0, count);
            return moves;
        }
        public int[] GetSuggestedMoves(bool tm, bool tutor, bool reminder)
        {
            if (pkm == null || !pkm.IsOriginValid)
                return null;
            if (!Parsed)
                return new int[4];
            return Legal.GetValidMoves(pkm, Info.EvoChainsAllGens, Tutor: tutor, Machine: tm, MoveReminder: reminder).Skip(1).ToArray(); // skip move 0
        }
        public EncounterStatic GetSuggestedMetInfo() => EncounterSuggestion.GetSuggestedMetInfo(pkm);
    }
}

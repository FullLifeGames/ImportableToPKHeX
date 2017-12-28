﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary>
    /// Generation specific Evolution Tree data.
    /// </summary>
    /// <remarks>
    /// Used to determine if a <see cref="PKM.Species"/> can evolve from prior steps in its evolution branch.
    /// </remarks>
    public class EvolutionTree
    {
        private static readonly EvolutionTree Evolves1;
        private static readonly EvolutionTree Evolves2;
        private static readonly EvolutionTree Evolves3;
        private static readonly EvolutionTree Evolves4;
        private static readonly EvolutionTree Evolves5;
        private static readonly EvolutionTree Evolves6;
        private static readonly EvolutionTree Evolves7;

        static EvolutionTree()
        {
            // Evolution tables need Personal Tables initialized beforehand, hence why the EvolutionTree data is initialized here.
            byte[] get(string resource) => Util.GetBinaryResource($"evos_{resource}.pkl");
            byte[][] unpack(string resource) => Data.UnpackMini(get(resource), resource);

            Evolves1 = new EvolutionTree(new[] { get("rby") }, GameVersion.RBY, PersonalTable.Y, Legal.MaxSpeciesID_1);
            Evolves2 = new EvolutionTree(new[] { get("gsc") }, GameVersion.GSC, PersonalTable.C, Legal.MaxSpeciesID_2);
            Evolves3 = new EvolutionTree(new[] { get("g3") }, GameVersion.RS, PersonalTable.RS, Legal.MaxSpeciesID_3);
            Evolves4 = new EvolutionTree(new[] { get("g4") }, GameVersion.DP, PersonalTable.DP, Legal.MaxSpeciesID_4);
            Evolves5 = new EvolutionTree(new[] { get("g5") }, GameVersion.BW, PersonalTable.BW, Legal.MaxSpeciesID_5);
            Evolves6 = new EvolutionTree(unpack("ao"), GameVersion.ORAS, PersonalTable.AO, Legal.MaxSpeciesID_6);
            Evolves7 = new EvolutionTree(unpack("uu"), GameVersion.USUM, PersonalTable.USUM, Legal.MaxSpeciesID_7_USUM);
        }
        internal static EvolutionTree GetEvolutionTree(int generation)
        {
            switch (generation)
            {
                case 1:
                    return Evolves1;
                case 2:
                    return Evolves2;
                case 3:
                    return Evolves3;
                case 4:
                    return Evolves4;
                case 5:
                    return Evolves5;
                case 6:
                    return Evolves6;
                default:
                    return Evolves7;
            }
        }

        private List<EvolutionSet> Entries { get; } = new List<EvolutionSet>();
        private readonly EvolutionLineage[] Lineage;
        private readonly GameVersion Game;
        private readonly PersonalTable Personal;
        private readonly int MaxSpeciesTree;

        public EvolutionTree(byte[][] data, GameVersion game, PersonalTable personal, int maxSpeciesTree)
        {
            Game = game;
            Personal = personal;
            MaxSpeciesTree = maxSpeciesTree;
            switch (game)
            {
                case GameVersion.RBY:
                    Entries = EvolutionSet1.GetArray(data[0], maxSpeciesTree);
                    break;
                case GameVersion.GSC:
                    Entries = EvolutionSet2.GetArray(data[0], maxSpeciesTree);
                    break;
                case GameVersion.RS:
                    Entries = EvolutionSet3.GetArray(data[0]);
                    break;
                case GameVersion.DP:
                    Entries = EvolutionSet4.GetArray(data[0]);
                    break;
                case GameVersion.BW:
                    Entries = EvolutionSet5.GetArray(data[0]);
                    break;
                case GameVersion.ORAS:
                    Entries.AddRange(data.Select(d => new EvolutionSet6(d)));
                    break;
                case GameVersion.USUM:
                    Entries.AddRange(data.Select(d => new EvolutionSet7(d)));
                    break;
            }
            
            // Create Lineages
            Lineage = new EvolutionLineage[Entries.Count];
            for (int i = 0; i < Entries.Count; i++)
                Lineage[i] = new EvolutionLineage();
            if (Game == GameVersion.ORAS)
                Array.Resize(ref Lineage, MaxSpeciesTree + 1);

            // Populate Lineages
            for (int i = 1; i < Lineage.Length; i++)
            {
                // Iterate over all possible evolutions
                var s = Entries[i];
                foreach (EvolutionMethod evo in s.PossibleEvolutions)
                {
                    int index = GetIndex(evo);
                    if (index < 0)
                        continue;

                    var sourceEvo = evo.Copy(i);

                    Lineage[index].Insert(sourceEvo);
                    // If current entries has a pre-evolution, propagate to evolution as well
                    if (Lineage[i].Chain.Count > 0)
                        Lineage[index].Insert(Lineage[i].Chain[0]);

                    if (index >= i) continue;
                    // If destination species evolves into something (ie a 'baby' Pokemon like Cleffa)
                    // Add it to the corresponding parent chains
                    foreach (EvolutionMethod mid in Entries[index].PossibleEvolutions)
                    {
                        int newIndex = GetIndex(mid);
                        if (newIndex < 0)
                            continue;

                        Lineage[newIndex].Insert(sourceEvo);
                    }
                }
            }
            FixEvoTreeManually();
        }

        // There's always oddballs.
        private void FixEvoTreeManually()
        {
            if (Game == GameVersion.USUM)
                FixEvoTreeSM();
        }
        private void FixEvoTreeSM()
        {
            // Wormadam -- Copy Burmy 0 to Wormadam-1/2
            Lineage[Personal.GetFormeIndex(413, 1)].Chain.Insert(0, Lineage[413].Chain[0]);
            Lineage[Personal.GetFormeIndex(413, 2)].Chain.Insert(0, Lineage[413].Chain[0]);

            // Shellos -- Move Shellos-1 evo from Gastrodon-0 to Gastrodon-1
            Lineage[Personal.GetFormeIndex(422 + 1, 1)].Chain.Insert(0, Lineage[422 + 1].Chain[0]);
            Lineage[422+1].Chain.RemoveAt(0);

            // Meowstic -- Meowstic-1 (F) should point back to Espurr, copy Meowstic-0 (M)
            Lineage[Personal.GetFormeIndex(678, 1)].Chain.Insert(0, Lineage[678].Chain[0]);

            // Floette doesn't contain evo info for forms 1-4, copy. Florges points to form 0, no fix needed.
            var fbb = Lineage[669+1].Chain[0];
            for (int i = 1; i <= 4; i++) // NOT AZ
                Lineage[Personal.GetFormeIndex(669+1, i)].Chain.Insert(0, fbb);
            // Clear forme chains from Florges
            Lineage[671].Chain.RemoveRange(0, Lineage[671].Chain.Count - 2);

            // Gourgeist -- Sizes are still relevant. Formes are in reverse order.
            for (int i = 1; i <= 3; i++)
            {
                Lineage[Personal.GetFormeIndex(711, i)].Chain.Clear();
                Lineage[Personal.GetFormeIndex(711, i)].Chain.Add(Lineage[711].Chain[3-i]);
            }
            Lineage[711].Chain.RemoveRange(0, 3);

            // Ban Raichu Evolution on SM
            Lineage[Personal.GetFormeIndex(26, 0)]
                .Chain[1].StageEntryMethods[0]
                .Banlist = EvolutionMethod.BanSM;
            // Ban Exeggutor Evolution on SM
            Lineage[Personal.GetFormeIndex(103, 0)]
                .Chain[0].StageEntryMethods[0]
                .Banlist = EvolutionMethod.BanSM;
            // Ban Marowak Evolution on SM
            Lineage[Personal.GetFormeIndex(105, 0)]
                .Chain[0].StageEntryMethods[0]
                .Banlist = EvolutionMethod.BanSM;
        }

        private int GetIndex(PKM pkm)
        {
            if (pkm.Format < 7)
                return pkm.Species;
            return Personal.GetFormeIndex(pkm.Species, pkm.AltForm);
        }
        private int GetIndex(EvolutionMethod evo)
        {
            int evolvesToSpecies = evo.Species;
            if (evolvesToSpecies == 0)
                return -1;

            if (Personal == null)
                return evolvesToSpecies;

            int evolvesToForm = evo.Form;
            if (evolvesToForm < 0)
                evolvesToForm = 0;

            return Personal.GetFormeIndex(evolvesToSpecies, evolvesToForm);
        }
        public IEnumerable<DexLevel> GetValidPreEvolutions(PKM pkm, int maxLevel, int maxSpeciesOrigin = -1, bool skipChecks = false, int minLevel = 1)
        {
            int index = GetIndex(pkm);
            if (maxSpeciesOrigin <= 0)
                maxSpeciesOrigin = Legal.GetMaxSpeciesOrigin(pkm);
            return Lineage[index].GetExplicitLineage(pkm, maxLevel, skipChecks, MaxSpeciesTree, maxSpeciesOrigin, minLevel);
        }
    }

    /// <summary>
    /// Table of Evolution Branch Entries
    /// </summary>
    public abstract class EvolutionSet
    {
        public EvolutionMethod[] PossibleEvolutions;
    }
    /// <summary>
    /// Generation 1 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet1 : EvolutionSet
    {
        private static EvolutionMethod GetMethod(byte[] data, ref int offset)
        {
            switch (data[offset])
            {
                case 1: // Level
                    var m1 = new EvolutionMethod
                    {
                        Method = 1, // Level Up
                        Level = data[offset + 1],
                        Species = data[offset + 2]
                    };
                    offset += 3;
                    return m1;
                case 2: // Use Item
                    var m2 = new EvolutionMethod
                    {
                        Method = 8, // Use Item
                        Argument = data[offset + 1],
                        // 1
                        Species = data[offset + 3],
                    };
                    offset += 4;
                    return m2;
                case 3: // Trade
                    var m3 = new EvolutionMethod
                    {
                        Method = 5, // Trade
                        // 1
                        Species = data[offset + 2]
                    };
                    offset += 3;
                    return m3;
            }
            return null;
        }
        public static List<EvolutionSet> GetArray(byte[] data, int maxSpecies)
        {
            var evos = new List<EvolutionSet>();
            int offset = 0;
            for (int i = 0; i <= maxSpecies; i++)
            {
                var m = new List<EvolutionMethod>();
                while (data[offset] != 0)
                    m.Add(GetMethod(data, ref offset));
                ++offset;
                evos.Add(new EvolutionSet1 { PossibleEvolutions = m.ToArray() });
            }
            return evos;
        }
    }
    /// <summary>
    /// Generation 2 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet2 : EvolutionSet
    {
        private static EvolutionMethod GetMethod(byte[] data, ref int offset)
        {
            int method = data[offset];
            int arg = data[offset + 1];
            int species = data[offset + 2];
            offset += 3;

            switch (method)
            {
                case 1: /* Level Up */ return new EvolutionMethod { Method = 1, Species = species, Level = arg };
                case 2: /* Use Item */ return new EvolutionMethod { Method = 8, Species = species, Argument = arg };
                case 3: /*  Trade   */ return new EvolutionMethod { Method = 5, Species = species };
                case 4: /*Friendship*/ return new EvolutionMethod { Method = 1, Species = species };
                case 5: /*  Stats   */
                    // species is currently stat ID, we don't care about evo type as stats can be changed after evo
                    return new EvolutionMethod { Method = 1, Species = data[offset++], Level = arg }; // Tyrogue stats
            }
            return null;
        }
        public static List<EvolutionSet> GetArray(byte[] data, int maxSpecies)
        {
            var evos = new List<EvolutionSet>();
            int offset = 0;
            for (int i = 0; i <= maxSpecies; i++)
            {
                var m = new List<EvolutionMethod>();
                while (data[offset] != 0)
                    m.Add(GetMethod(data, ref offset));
                ++offset;
                evos.Add(new EvolutionSet2 { PossibleEvolutions = m.ToArray() });
            }
            return evos;
        }
    }
    /// <summary>
    /// Generation 3 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet3 : EvolutionSet
    {
        private static EvolutionMethod GetMethod(byte[] data, int offset)
        {
            int method = BitConverter.ToUInt16(data, offset + 0);
            int arg =  BitConverter.ToUInt16(data, offset + 2);
            int species = SpeciesConverter.GetG4Species(BitConverter.ToUInt16(data, offset + 4));
            //2 bytes padding

            switch (method)
            {
                case 1: /* Friendship*/
                case 2: /* Friendship day*/
                case 3: /* Friendship night*/
                case 5: /* Trade   */
                case 6: /* Trade while holding */
                    return new EvolutionMethod { Method = method, Species = species, Argument = arg };
                case 4: /* Level Up */
                    return new EvolutionMethod { Method = 4, Species = species, Level = arg, Argument = arg };
                case 7: /* Use item */
                case 15: /* Beauty evolution*/
                    return new EvolutionMethod { Method = method + 1, Species = species, Argument = arg };
                case 8: /* Tyrogue -> Hitmonchan */
                case 9: /* Tyrogue -> Hitmonlee */
                case 10: /* Tyrogue -> Hitmontop*/
                case 11: /* Wurmple -> Silcoon evolution */
                case 12: /* Wurmple -> Cascoon evolution */
                case 13: /* Nincada -> Ninjask evolution */
                case 14: /* Shedinja spawn in Nincada -> Ninjask evolution */
                    return new EvolutionMethod { Method = method + 1, Species = species, Level = arg, Argument = arg };
            }
            return null;
        }
        public static List<EvolutionSet> GetArray(byte[] data)
        {
            EvolutionSet[] evos = new EvolutionSet[Legal.MaxSpeciesID_3 + 1];
            evos[0] = new EvolutionSet3 { PossibleEvolutions = new EvolutionMethod[0] };
            for (int i = 0; i <= Legal.MaxSpeciesIndex_3; i++)
            {
                int g4species = SpeciesConverter.GetG4Species(i);
                if (g4species == 0)
                    continue;
                
                int offset = i * 40;
                var m_list = new List<EvolutionMethod>();
                for (int j = 0; j < 5; j++)
                {
                    EvolutionMethod m = GetMethod(data,  offset);
                    if (m != null)
                        m_list.Add(m);
                    else
                        break;
                    offset += 8;
                }
                evos[g4species] = new EvolutionSet3 { PossibleEvolutions = m_list.ToArray() };
            }
            return evos.ToList();
        }
    }
    /// <summary>
    /// Generation 4 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet4 : EvolutionSet
    {
        private static EvolutionMethod GetMethod(byte[] data, int offset)
        {
            int[] argEvos = { 6, 8, 16, 17, 18, 19, 20, 21, 22 };
            int method = BitConverter.ToUInt16(data, offset + 0);
            int arg = BitConverter.ToUInt16(data, offset + 2);
            int species = BitConverter.ToUInt16(data, offset + 4);

            if (method == 0)
                return null;
            // To have the same estructure as gen 6
            // Gen 4 Method 6 is Gen 6 Method 7, G4 7 = G6 8, and so on
            if (method > 6)
                method++;

            var evo = new EvolutionMethod
            {
                Method = method,
                Argument = arg,
                Species = species,
                Level = arg,
            };
            
            if (argEvos.Contains(evo.Method))
                evo.Level = 0;
            return evo;
        }
        public static List<EvolutionSet> GetArray(byte[] data)
        {
            var evos = new List<EvolutionSet>();
            const int bpe = 6; // bytes per evolution entry
            const int entries = 7; // 7 * 6 = 42, + 2 alignment bytes
            const int size = 44; // bytes per species entry

            int count = data.Length / size;
            for (int i = 0; i < count; i++)
            {
                int offset = i * size;
                var m_list = new List<EvolutionMethod>();
                for (int j = 0; j < entries; j++)
                {
                    EvolutionMethod m = GetMethod(data, offset);
                    if (m != null)
                        m_list.Add(m);
                    else
                        break;
                    offset += bpe;
                }
                evos.Add(new EvolutionSet4 { PossibleEvolutions = m_list.ToArray() });
            }
            return evos;
        }
    }
    /// <summary>
    /// Generation 5 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet5 : EvolutionSet
    {
        private static EvolutionMethod GetMethod(byte[] data, int offset)
        {
            int[] argEvos = { 6, 8, 16, 17, 18, 19, 20, 21, 22 };
            int method = BitConverter.ToUInt16(data, offset + 0);
            int arg = BitConverter.ToUInt16(data, offset + 2);
            int species = BitConverter.ToUInt16(data, offset + 4);

            if (method == 0)
                return null;

            var evo = new EvolutionMethod
            {
                Method = method,
                Argument = arg,
                Species = species,
                Level = arg,
            };

            if (argEvos.Contains(evo.Method))
                evo.Level = 0;
            return evo;
        }
        public static List<EvolutionSet> GetArray(byte[] data)
        {
            var evos = new List<EvolutionSet>();
            for (int i = 0; i <= Legal.MaxSpeciesIndex_5_B2W2; i++)
            {
                /* 42 bytes per species, 
                 * for every species 7 evolutions with 6 bytes per evolution*/
                int offset = i * 42;
                var m_list = new List<EvolutionMethod>();
                for (int j = 0; j < 7; j++)
                {
                    EvolutionMethod m = GetMethod(data, offset);
                    if (m != null)
                        m_list.Add(m);
                    else
                        break;
                    offset += 6;
                }
                evos.Add(new EvolutionSet5 { PossibleEvolutions = m_list.ToArray() });
            }
            return evos;
        }
    }
    /// <summary>
    /// Generation 6 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet6 : EvolutionSet
    {
        private static readonly HashSet<int> argEvos = new HashSet<int> {6, 8, 16, 17, 18, 19, 20, 21, 22, 29, 30, 32, 33, 34};
        private const int SIZE = 6;
        public EvolutionSet6(byte[] data)
        {
            PossibleEvolutions = new EvolutionMethod[data.Length / SIZE];
            for (int i = 0; i < data.Length; i += SIZE)
            {
                var evo = new EvolutionMethod
                {
                    Method = BitConverter.ToUInt16(data, i + 0),
                    Argument = BitConverter.ToUInt16(data, i + 2),
                    Species = BitConverter.ToUInt16(data, i + 4),

                    // Copy
                    Level = BitConverter.ToUInt16(data, i + 2),
                };

                // Argument is used by both Level argument and Item/Move/etc. Clear if appropriate.
                if (argEvos.Contains(evo.Method))
                    evo.Level = 0;

                PossibleEvolutions[i/SIZE] = evo;
            }
        }
    }
    /// <summary>
    /// Generation 7 Evolution Branch Entries
    /// </summary>
    public class EvolutionSet7 : EvolutionSet
    {
        private const int SIZE = 8;
        public EvolutionSet7(byte[] data)
        {
            PossibleEvolutions = new EvolutionMethod[data.Length / SIZE];
            for (int i = 0; i < data.Length; i += SIZE)
            {
                PossibleEvolutions[i / SIZE] = new EvolutionMethod
                {
                    Method = BitConverter.ToUInt16(data, i + 0),
                    Argument = BitConverter.ToUInt16(data, i + 2),
                    Species = BitConverter.ToUInt16(data, i + 4),
                    Form = (sbyte)data[i + 6],
                    Level = data[i + 7],
                };
            }
        }
    }

    /// <summary>
    /// Criteria for evolving to this branch in the <see cref="EvolutionTree"/>
    /// </summary>
    public class EvolutionMethod
    {
        public int Method;
        public int Species;
        public int Argument;
        public int Form = -1;
        public int Level;

        public bool RequiresLevelUp;

        internal static readonly HashSet<int> TradeMethods = new HashSet<int> {5, 6, 7};
        private static readonly IReadOnlyCollection<GameVersion> NoBanlist = new GameVersion[0];
        internal static readonly IReadOnlyCollection<GameVersion> BanSM = new[] {GameVersion.SN, GameVersion.MN};
        internal IReadOnlyCollection<GameVersion> Banlist = NoBanlist;

        public bool Valid(PKM pkm, int lvl, bool skipChecks)
        {
            RequiresLevelUp = false;
            if (Form > -1)
                if (!skipChecks && pkm.AltForm != Form)
                    return false;

            if (!skipChecks && Banlist.Contains((GameVersion)pkm.Version) && pkm.IsUntraded) // sm lacks usum kantonian evos
                return false;

            switch (Method)
            {
                case 8: // Use Item
                case 42:
                    return true;
                case 17: // Male
                    return pkm.Gender == 0;
                case 18: // Female
                    return pkm.Gender == 1;

                case 5: // Trade Evolution
                case 6: // Trade while Holding
                case 7: // Trade for Opposite Species
                    return !pkm.IsUntraded || skipChecks;
                
                    // Special Levelup Cases
                case 16:
                    if (pkm.CNT_Beauty < Argument)
                        return skipChecks;
                    goto default;
                case 23: // Gender = Male
                    if (pkm.Gender != 0)
                        return false;
                    goto default;
                case 24: // Gender = Female
                    if (pkm.Gender != 1)
                        return false;
                    goto default;
                case 34: // Gender = Female, out Form1
                    if (pkm.Gender != 1 || pkm.AltForm != 1)
                        return false;
                    goto default;

                case 36: // Any Time on Version
                case 37: // Daytime on Version
                case 38: // Nighttime on Version
                    // Version checks come in pairs, check for any pair match
                    if ((pkm.Version & 1) != (Argument & 1) && pkm.IsUntraded || skipChecks)
                        return skipChecks;
                    goto default;

                default:
                    if (Level == 0 && lvl < 2)
                        return false;
                    if (lvl < Level)
                        return false;

                    RequiresLevelUp = true;
                    if (skipChecks)
                        return lvl >= Level;

                    // Check Met Level for extra validity
                    switch (pkm.GenNumber)
                    {
                        case 1: // No metdata in RBY
                        case 2: // No metdata in GS, Crystal metdata can be reset
                            return true;
                        case 3:
                        case 4:
                            if (pkm.Format > pkm.GenNumber) // Pal Park / PokeTransfer updates Met Level
                                return true;
                            return pkm.Met_Level < lvl;

                        case 5: // Bank keeps current level
                        case 6:
                        case 7:
                            return lvl >= Level && (!pkm.IsNative || pkm.Met_Level < lvl);
                    }
                    return false;
            }
        }

        public DexLevel GetDexLevel(int species, int lvl)
        {
            return new DexLevel
            {
                Species = species,
                Level = lvl,
                Form = Form,
                Flag = Method,
            };
        }

        public EvolutionMethod Copy(int species = -1)
        {
            if (species < 0)
                species = Species;
            return new EvolutionMethod
            {
                Method = Method,
                Species = species,
                Argument = Argument,
                Form = Form,
                Level = Level
            };
        }
    }

    /// <summary>
    /// Informatics pertaining to a <see cref="PKM"/>'s evolution lineage.
    /// </summary>
    public class EvolutionLineage
    {
        public readonly List<EvolutionStage> Chain = new List<EvolutionStage>();

        public void Insert(EvolutionMethod entry)
        {
            int matchChain = -1;
            for (int i = 0; i < Chain.Count; i++)
                if (Chain[i].StageEntryMethods.Any(e => e.Species == entry.Species))
                    matchChain = i;

            if (matchChain != -1)
                Chain[matchChain].StageEntryMethods.Add(entry);
            else
                Chain.Insert(0, new EvolutionStage { StageEntryMethods = new List<EvolutionMethod> {entry}});
        }
        public void Insert(EvolutionStage evo)
        {
            Chain.Insert(0, evo);
        }

        public IEnumerable<DexLevel> GetExplicitLineage(PKM pkm, int maxLevel, bool skipChecks, int maxSpeciesTree, int maxSpeciesOrigin, int minLevel)
        {
            int lvl = maxLevel;
            List<DexLevel> dl = new List<DexLevel> { new DexLevel { Species = pkm.Species, Level = lvl, Form = pkm.AltForm } };
            for (int i = Chain.Count - 1; i >= 0; i--) // reverse evolution!
            {
                bool oneValid = false;
                foreach (var evo in Chain[i].StageEntryMethods)
                {
                    if (!evo.Valid(pkm, lvl, skipChecks))
                        continue;

                    if (evo.RequiresLevelUp && minLevel >= lvl)
                        break; // impossible evolution

                    oneValid = true;
                    UpdateMinValues(dl, evo);
                    int species = evo.Species;

                    // Gen7 Personal Formes -- unmap the forme personal entry ID to the actual species ID since species are consecutive
                    if (evo.Species > maxSpeciesTree)
                        species = pkm.Species - Chain.Count + i;

                    dl.Add(evo.GetDexLevel(species, lvl));
                    if (evo.RequiresLevelUp)
                        lvl--;
                    break;
                }
                if (!oneValid)
                    break;
            }

            // Remove future gen preevolutions, no munchlax in a gen3 snorlax, no pichu in a gen1 vc raichu, etc
            if (dl.Any(d => d.Species <= maxSpeciesOrigin) && dl.Last().Species > maxSpeciesOrigin)
                dl.RemoveAt(dl.Count - 1); 

            // Last species is the wild/hatched species, the minimum is 1 because it has not evolved from previous species
            dl.Last().MinLevel = 1;
            dl.Last().RequiresLvlUp = false;
            return dl;
        }
        private static void UpdateMinValues(IReadOnlyList<DexLevel> dl, EvolutionMethod evo)
        {
            var last = dl.Last();
            if (evo.Level == 0 || !evo.RequiresLevelUp) // Evolutions like elemental stones, trade, etc
            {
                if (!evo.RequiresLevelUp)
                    last.MinLevel = 1;
                else
                {
                    // Evolutions like frienship, pichu -> pikachu, eevee -> umbreon, etc
                    last.MinLevel = 2;

                    var first = dl[0];
                    if (dl.Count > 1 && !first.RequiresLvlUp)
                        first.MinLevel = 2; // Raichu from Pikachu would have minimum level 1, but with Pichu included Raichu minimum level is 2
                }
            }
            else // level up evolutions
            {
                last.MinLevel = evo.Level;

                var first = dl[0];
                if (dl.Count > 1)
                {
                    if (first.MinLevel < evo.Level && !first.RequiresLvlUp)
                        first.MinLevel = evo.Level; // Pokemon like Nidoqueen, its minimum level is Nidorina minimum level
                    if (first.MinLevel <= evo.Level && first.RequiresLvlUp)
                        first.MinLevel = evo.Level + 1; // Pokemon like Crobat, its minimum level is Golbat minimum level + 1
                }
            }
            last.RequiresLvlUp = evo.RequiresLevelUp;
        }
    }
    /// <summary>
    /// Evolution Stage Entries
    /// </summary>
    public struct EvolutionStage
    {
        public List<EvolutionMethod> StageEntryMethods;
    }
}

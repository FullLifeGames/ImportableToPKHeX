﻿using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core
{
    public static partial class Legal
    {
        internal const int MaxSpeciesID_1 = 151;
        internal const int MaxMoveID_1 = 165;
        internal const int MaxItemID_1 = 255; 
        internal const int MaxAbilityID_1 = 0;
        
        internal static readonly ushort[] Pouch_Items_RBY =
        {
            000,001,002,003,004,005,006,            010,011,012,013,014,015,
            016,017,018,019,020,                                029,030,031,
            032,033,034,035,036,037,038,039,040,041,042,043,    045,046,047,
            048,049,    051,052,053,054,055,056,057,058,    060,061,062,063,
            064,065,066,067,068,069,070,071,072,073,074,075,076,077,078,079,
            080,081,082,083,

            // ...

            196,197,198,199,200,201,202,203,204,205,206,207,
            208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,
            224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,
            240,241,242,243,244,245,246,247,248,249,250,
        };

        internal static readonly int[] MovePP_RBY =
        {
            0,
            35, 25, 10, 15, 20, 20, 15, 15, 15, 35, 30, 05, 10, 30, 30, 35, 35, 20, 15, 20, 20, 10, 20, 30, 05, 25, 15, 15, 15, 25, 20, 05, 35, 15, 20, 20, 20, 15, 30, 35, 20, 20, 30, 25, 40, 20, 15, 20, 20, 20,
            30, 25, 15, 30, 25, 05, 15, 10, 05, 20, 20, 20, 05, 35, 20, 25, 20, 20, 20, 15, 20, 10, 10, 40, 25, 10, 35, 30, 15, 20, 40, 10, 15, 30, 15, 20, 10, 15, 10, 05, 10, 10, 25, 10, 20, 40, 30, 30, 20, 20,
            15, 10, 40, 15, 20, 30, 20, 20, 10, 40, 40, 30, 30, 30, 20, 30, 10, 10, 20, 05, 10, 30, 20, 20, 20, 05, 15, 10, 20, 15, 15, 35, 20, 15, 10, 20, 30, 15, 40, 20, 15, 10, 05, 10, 30, 10, 15, 20, 15, 40,
            40, 10, 05, 15, 10, 10, 10, 15, 30, 30, 10, 10, 20, 10, 10, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00, 00,
            00, 00, 00, 00, 00, 00
        };

        internal static readonly HashSet<int> TransferSpeciesDefaultAbility_1 = new HashSet<int> {92, 93, 94, 109, 110, 151};

        internal static readonly int[] TMHM_RBY =
        {
            005, 013, 014, 018, 025, 092, 032, 034, 036, 038,
            061, 055, 058, 059, 063, 006, 066, 068, 069, 099,
            072, 076, 082, 085, 087, 089, 090, 091, 094, 100,
            102, 104, 115, 117, 118, 120, 121, 126, 129, 130,
            135, 138, 143, 156, 086, 149, 153, 157, 161, 164,

            015, 019, 057, 070, 148
        };

        internal static readonly int[] G1CaterpieMoves = { 33, 81 };
        internal static readonly int[] G1WeedleMoves = { 40, 81 };
        internal static readonly int[] G1MetapodMoves = G1CaterpieMoves.Concat(new[] { 106 }).ToArray();
        internal static readonly int[] G1KakunaMoves = G1WeedleMoves.Concat(new[] { 106 }).ToArray();
        internal static readonly int[] G1Exeggcute_IncompatibleMoves = { 78, 77, 79 };

        internal static readonly int[] WildPokeBalls1 = {4};

        internal static readonly HashSet<int> FutureEvolutionsGen1 = new HashSet<int>
        {
            169,182,186,196,197,199,208,212,230,233,242,462,463,464,465,466,467,470,471,474,700
        };

        internal static readonly HashSet<int> FutureEvolutionsGen1_Gen2LevelUp = new HashSet<int>
        {
            // Crobat Espeon Umbreon Blissey
            169,196,197,242
        };
        internal static readonly HashSet<int> SpecialMinMoveSlots = new HashSet<int>
        {
            25, 26, 29, 30, 31, 32, 33, 34, 36, 38, 40, 59, 91, 103, 114, 121,
        };
        internal static readonly HashSet<int> Types_Gen1 = new HashSet<int>
        {
            0, 1, 2, 3, 4, 5, 7, 8, 20, 21, 22, 23, 24, 25, 26
        };
        internal static readonly HashSet<int> Species_NotAvailable_CatchRate = new HashSet<int>
        {
            12, 18, 31, 34, 36, 38, 45, 53, 59, 62, 65, 68, 71, 78, 91, 103, 121
        };
        internal static readonly int[] Stadium_CatchRate =
        {
            167, // Normal Box
            168, // Gorgeous Box
        };
        internal static readonly HashSet<int> Stadium_GiftSpecies = new HashSet<int>
        {
            001, // Bulbasaur
            004, // Charmander
            007, // Squirtle
            054, // Psyduck (Amnesia)
            106, // Hitmonlee
            107, // Hitmonchan
            133, // Eevee
            138, // Omanyte
            140, // Kabuto
        };
        internal static readonly HashSet<int> Trade_Evolution1 = new HashSet<int>
        {
            064,
            067,
            075,
            093
        };
    }
}

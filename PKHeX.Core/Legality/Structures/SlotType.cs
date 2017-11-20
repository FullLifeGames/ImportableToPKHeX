﻿using System;

namespace PKHeX.Core
{
    /// <summary>
    /// Wild Encounter data <see cref="EncounterSlot"/> Type
    /// </summary>
    /// <remarks>
    /// Different from <see cref="EncounterType"/>, this corresponds to the method that the <see cref="IEncounterable"/> may be encountered.</remarks>
    [Flags]
    public enum SlotType
    {
        /// <summary>
        /// Default (un-assigned) encounter slot type.
        /// </summary>
        Any = 0,
        /// <summary> 
        /// Slot is encountered via Grass.
        /// </summary>
        Grass        = 1 << 00,
        /// <summary>
        /// Slot is encountered via Surfing.
        /// </summary>
        Surf         = 1 << 01,
        /// <summary>
        /// Slot is encountered via Old Rod (Fishing).
        /// </summary>
        Old_Rod      = 1 << 02,
        /// <summary>
        /// Slot is encountered via Good Rod (Fishing).
        /// </summary>
        Good_Rod     = 1 << 03,
        /// <summary>
        /// Slot is encountered via Super Rod (Fishing).
        /// </summary>
        Super_Rod    = 1 << 04,
        /// <summary>
        /// Slot is encountered via Rock Smash.
        /// </summary>
        Rock_Smash   = 1 << 05,
        /// <summary>
        /// Slot is encountered via a Horde.
        /// </summary>
        Horde        = 1 << 06,
        /// <summary> 
        /// Slot is encountered via the Friend Safari.
        /// </summary>
        FriendSafari = 1 << 07,
        /// <summary>
        /// Slot is encountered through special means. Used to signify special slots for Gen2, sometimes for later follow-up).
        /// </summary>
        Special      = 1 << 08,
        /// <summary>
        /// Slot is encountered via SOS signal.
        /// </summary>
        SOS          = 1 << 09,
        /// <summary>
        /// Slot is encountered in a Swarm.
        /// </summary>
        Swarm        = 1 << 10,
        /// <summary>
        /// Slot is encountered via Headbutt.
        /// </summary>
        Headbutt     = 1 << 11,
        /// <summary>
        /// Slot is encountered via the Poké Radar.
        /// </summary>
        Pokeradar    = 1 << 12,
        /// <summary>
        /// Slot is encountered via a Honey Tree.
        /// </summary>
        HoneyTree    = 1 << 13,
        /// <summary>
        /// Slot is encountered via a Hidden Grotto.
        /// </summary>
        HiddenGrotto = 1 << 14,
        /// <summary>
        /// Slot is encountered via the Bug Catching Contest.
        /// </summary>
        BugContest   = 1 << 15,
        /// <summary>
        /// Slot is encountered in the Safari Zone.
        /// </summary>
        Safari       = 1 << 16, // always used as a modifier to another slot type

        Rough_Terrain  = 1 << 17,
        Yellow_Flowers = 1 << 18,
        Purple_Flowers = 1 << 19,
        Red_Flowers    = 1 << 20,

        // Combined
        Headbutt_Special = Headbutt | Special,

        Grass_Safari = Grass | Safari,
        Surf_Safari = Surf | Safari,
        Old_Rod_Safari = Old_Rod | Safari,
        Good_Rod_Safari = Good_Rod | Safari,
        Super_Rod_Safari = Super_Rod | Safari,
        Rock_Smash_Safari = Rock_Smash | Safari,
        Pokeradar_Safari = Pokeradar | Safari,
    }

    public static partial class Extensions
    {
        internal static bool IsFishingRodType(this SlotType t)
        {
            return t.HasFlag(SlotType.Old_Rod) || t.HasFlag(SlotType.Good_Rod) || t.HasFlag(SlotType.Super_Rod);
        }
    }
}

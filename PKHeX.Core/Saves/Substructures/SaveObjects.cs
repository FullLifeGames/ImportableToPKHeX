﻿namespace PKHeX.Core
{
    public struct DaycareSlot
    {
        public uint Experience;
        public PKM PKM;
        public bool Occupied;
    }
    public struct Daycare
    {
        public DaycareSlot[] Slots;
        public bool EggAvailable;
        public ulong Seed;
    }

    /// <summary>
    /// Structure containing Mystery Gift Block Data
    /// </summary>
    public struct MysteryGiftAlbum
    {
        public MysteryGift[] Gifts;
        public bool[] Flags;
        public uint Seed;
    }
}

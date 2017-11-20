﻿using System;

namespace PKHeX.Core
{
    /// <summary> Generation 3 <see cref="PKM"/> format, exclusively for Pokémon Colosseum. </summary>
    public class CK3 : PKM, IRibbonSetEvent3, IRibbonSetCommon3, IRibbonSetUnique3, IRibbonSetOnly3, IShadowPKM
    {
        public static readonly byte[] ExtraBytes =
        {
            0x11, 0x12, 0x13,
            0x61, 0x62, 0x63, 0x64,
            0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xDA, 0xDB,
            0xE4, 0xE5, 0xE6, 0xE7, 0xCE,
            // 0xFC onwards unused?
        };
        public sealed override int SIZE_PARTY => PKX.SIZE_3CSTORED;
        public override int SIZE_STORED => PKX.SIZE_3CSTORED;
        public override int Format => 3;
        public override PersonalInfo PersonalInfo => PersonalTable.RS[Species];

        public CK3(byte[] decryptedData = null, string ident = null)
        {
            Data = (byte[])(decryptedData ?? new byte[SIZE_PARTY]).Clone();
            PKMConverter.CheckEncrypted(ref Data);
            Identifier = ident;
            if (Data.Length != SIZE_PARTY)
                Array.Resize(ref Data, SIZE_PARTY);
        }
        public override PKM Clone() => new CK3(Data);

        private string GetString(int Offset, int Count) => StringConverter.GetBEString3(Data, Offset, Count);
        private byte[] SetString(string value, int maxLength) => StringConverter.SetBEString3(value, maxLength);

        // Trash Bytes
        public override byte[] Nickname_Trash { get => GetData(0x2E, 20); set { if (value?.Length == 20) value.CopyTo(Data, 0x2E); } }
        public override byte[] OT_Trash { get => GetData(0x18, 20); set { if (value?.Length == 20) value.CopyTo(Data, 0x18); } }

        // Future Attributes
        public override uint EncryptionConstant { get => PID; set { } }
        public override int Nature { get => (int)(PID % 25); set { } }
        public override int AltForm { get => Species == 201 ? PKX.GetUnownForm(PID) : 0; set { } }

        public override bool IsNicknamed { get => PKX.IsNicknamedAnyLanguage(Species, Nickname, Format); set { } }
        public override int Gender { get => PKX.GetGenderFromPID(Species, PID); set { } }
        public override int Characteristic => -1;
        public override int CurrentFriendship { get => OT_Friendship; set => OT_Friendship = value; }
        public override int Ability { get { int[] abils = PersonalTable.RS.GetAbilities(Species, 0); return abils[abils[1] == 0 ? 0 : AbilityNumber >> 1]; } set { } }
        public override int CurrentHandler { get => 0; set { } }
        public override int Egg_Location { get => 0; set { } }

        // Silly Attributes
        public override ushort Sanity { get => 0; set { } } // valid flag set in pkm structure.
        public override ushort Checksum { get => SaveUtil.CRC16_CCITT(Data); set { } } // totally false, just a way to get a 'random' ident for the pkm.
        public override bool ChecksumValid => Valid;

        public override int Species { get => SpeciesConverter.GetG4Species(BigEndian.ToUInt16(Data, 0x00)); set => BigEndian.GetBytes((ushort)SpeciesConverter.GetG3Species(value)).CopyTo(Data, 0x00); }
        // 02-04 unused
        public override uint PID { get => BigEndian.ToUInt32(Data, 0x04); set => BigEndian.GetBytes(value).CopyTo(Data, 0x04); }
        public override int Version { get => SaveUtil.GetG3VersionID(Data[0x08]); set => Data[0x08] = (byte)SaveUtil.GetCXDVersionID(value); }
        public int CurrentRegion { get => Data[0x09]; set => Data[0x09] = (byte)value; }
        public int OriginalRegion { get => Data[0x0A]; set => Data[0x0A] = (byte)value; }
        public override int Language { get => PKX.GetMainLangIDfromGC(Data[0x0B]); set => Data[0x0B] = PKX.GetGCLangIDfromMain((byte)value); }
        public override int Met_Location { get => BigEndian.ToUInt16(Data, 0x0C); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x0C); }
        public override int Met_Level { get => Data[0x0E]; set => Data[0x0E] = (byte)value; }
        public override int Ball { get => Data[0x0F]; set => Data[0x0F] = (byte)value; }
        public override int OT_Gender { get => Data[0x10]; set => Data[0x10] = (byte)value; }
        public override int SID { get => BigEndian.ToUInt16(Data, 0x14); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x14); }
        public override int TID { get => BigEndian.ToUInt16(Data, 0x16); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x16); }
        public override string OT_Name { get => GetString(0x18, 20); set => SetString(value, 10).CopyTo(Data, 0x18); } // +2 terminator
        public override string Nickname { get => GetString(0x2E, 20); set { SetString(value, 10).CopyTo(Data, 0x2E); Nickname2 = value; } } // +2 terminator
        private string Nickname2 { get => GetString(0x44, 20); set => SetString(value, 10).CopyTo(Data, 0x44); } // +2 terminator
        public override uint EXP { get => BigEndian.ToUInt32(Data, 0x5C); set => BigEndian.GetBytes(value).CopyTo(Data, 0x5C); }
        public override int Stat_Level { get => Data[0x60]; set => Data[0x60] = (byte)value; }

        // 0x64-0x77 are battle/status related
        // Not that the program cares

        // Moves
        public override int Move1 { get => BigEndian.ToUInt16(Data, 0x78); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x78); }
        public override int Move1_PP { get => Data[0x7A]; set => Data[0x7A] = (byte)value; }
        public override int Move1_PPUps { get => Data[0x7B]; set => Data[0x7B] = (byte)value; }
        public override int Move2 { get => BigEndian.ToUInt16(Data, 0x7C); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x7C); }
        public override int Move2_PP { get => Data[0x7E]; set => Data[0x7E] = (byte)value; }
        public override int Move2_PPUps { get => Data[0x7F]; set => Data[0x7F] = (byte)value; }
        public override int Move3 { get => BigEndian.ToUInt16(Data, 0x80); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x80); }
        public override int Move3_PP { get => Data[0x82]; set => Data[0x82] = (byte)value; }
        public override int Move3_PPUps { get => Data[0x83]; set => Data[0x83] = (byte)value; }
        public override int Move4 { get => BigEndian.ToUInt16(Data, 0x84); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x84); }
        public override int Move4_PP { get => Data[0x86]; set => Data[0x86] = (byte)value; }
        public override int Move4_PPUps { get => Data[0x87]; set => Data[0x87] = (byte)value; }

        public override int SpriteItem => ItemConverter.GetG4Item((ushort)HeldItem);
        public override int HeldItem { get => BigEndian.ToUInt16(Data, 0x88); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x88); }

        // More party stats
        public override int Stat_HPCurrent { get => BigEndian.ToUInt16(Data, 0x8A); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x8A); }
        public override int Stat_HPMax { get => BigEndian.ToUInt16(Data, 0x8C); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x8C); }
        public override int Stat_ATK { get => BigEndian.ToUInt16(Data, 0x8E); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x8E); }
        public override int Stat_DEF { get => BigEndian.ToUInt16(Data, 0x90); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x90); }
        public override int Stat_SPA { get => BigEndian.ToUInt16(Data, 0x92); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x92); }
        public override int Stat_SPD { get => BigEndian.ToUInt16(Data, 0x94); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x94); }
        public override int Stat_SPE { get => BigEndian.ToUInt16(Data, 0x96); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x96); }
        
        // EVs
        public override int EV_HP {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0x98));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0x98); }
        public override int EV_ATK {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0x9A));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0x9A); }
        public override int EV_DEF {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0x9C));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0x9C); }
        public override int EV_SPA {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0x9E));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0x9E); }
        public override int EV_SPD {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0xA0));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0xA0); }
        public override int EV_SPE {
            get => Math.Min(byte.MaxValue, BigEndian.ToUInt16(Data, 0xA2));
            set => BigEndian.GetBytes((ushort)(value & 0xFF)).CopyTo(Data, 0xA2); }

        // IVs
        public override int IV_HP {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xA4));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xA4); }
        public override int IV_ATK {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xA6));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xA6); }
        public override int IV_DEF {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xA8));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xA8); }
        public override int IV_SPA {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xAA));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xAA); }
        public override int IV_SPD {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xAC));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xAC); }
        public override int IV_SPE {
            get => Math.Min((ushort)31, BigEndian.ToUInt16(Data, 0xAE));
            set => BigEndian.GetBytes((ushort)(value & 0x1F)).CopyTo(Data, 0xAE); }
        
        public override int OT_Friendship { get => Data[0xB0]; set => Data[0xB0] = (byte)value; }

        // Contest
        public override int CNT_Cool { get => Data[0xB2]; set => Data[0xB2] = (byte)value; }
        public override int CNT_Beauty { get => Data[0xB3]; set => Data[0xB3] = (byte)value; }
        public override int CNT_Cute { get => Data[0xB4]; set => Data[0xB4] = (byte)value; }
        public override int CNT_Smart { get => Data[0xB5]; set => Data[0xB5] = (byte)value; }
        public override int CNT_Tough { get => Data[0xB6]; set => Data[0xB6] = (byte)value; }
        public int RibbonCountG3Cool { get => Data[0xB7]; set => Data[0xB7] = (byte)value; }
        public int RibbonCountG3Beauty { get => Data[0xB8]; set => Data[0xB8] = (byte)value; }
        public int RibbonCountG3Cute { get => Data[0xB9]; set => Data[0xB9] = (byte)value; }
        public int RibbonCountG3Smart { get => Data[0xBA]; set => Data[0xBA] = (byte)value; }
        public int RibbonCountG3Tough { get => Data[0xBB]; set => Data[0xBB] = (byte)value; }
        public override int CNT_Sheen { get => Data[0xBC]; set => Data[0xBC] = (byte)value; }
        
        // Ribbons
        public bool RibbonChampionG3Hoenn { get => Data[0xBD] == 1; set => Data[0xBD] = (byte)(value ? 1 : 0); }
        public bool RibbonWinning { get => Data[0xBE] == 1; set => Data[0xBE] = (byte)(value ? 1 : 0); }
        public bool RibbonVictory { get => Data[0xBF] == 1; set => Data[0xBF] = (byte)(value ? 1 : 0); }
        public bool RibbonArtist { get => Data[0xC0] == 1; set => Data[0xC0] = (byte)(value ? 1 : 0); }
        public bool RibbonEffort { get => Data[0xC1] == 1; set => Data[0xC1] = (byte)(value ? 1 : 0); }
        public bool RibbonChampionBattle { get => Data[0xC2] == 1; set => Data[0xC2] = (byte)(value ? 1 : 0); }
        public bool RibbonChampionRegional { get => Data[0xC3] == 1; set => Data[0xC3] = (byte)(value ? 1 : 0); }
        public bool RibbonChampionNational { get => Data[0xC4] == 1; set => Data[0xC4] = (byte)(value ? 1 : 0); }
        public bool RibbonCountry { get => Data[0xC5] == 1; set => Data[0xC5] = (byte)(value ? 1 : 0); }
        public bool RibbonNational { get => Data[0xC6] == 1; set => Data[0xC6] = (byte)(value ? 1 : 0); }
        public bool RibbonEarth { get => Data[0xC7] == 1; set => Data[0xC7] = (byte)(value ? 1 : 0); }
        public bool RibbonWorld { get => Data[0xC8] == 1; set => Data[0xC8] = (byte)(value ? 1 : 0); }
        public bool Unused1 { get => ((Data[0xC9] >> 0) & 1) == 1; set => Data[0xC9] = (byte)(Data[0xC9] & ~1 | (value ? 1 : 0)); }
        public bool Unused2 { get => ((Data[0xC9] >> 1) & 1) == 1; set => Data[0xC9] = (byte)(Data[0xC9] & ~2 | (value ? 2 : 0)); }
        public bool Unused3 { get => ((Data[0xC9] >> 2) & 1) == 1; set => Data[0xC9] = (byte)(Data[0xC9] & ~4 | (value ? 4 : 0)); }
        public bool Unused4 { get => ((Data[0xC9] >> 3) & 1) == 1; set => Data[0xC9] = (byte)(Data[0xC9] & ~8 | (value ? 8 : 0)); }

        public override int PKRS_Strain { get => Data[0xCA] & 0xF; set => Data[0xCA] = (byte)(value & 0xF); }
        public override bool IsEgg { get => Data[0xCB] == 1; set => Data[0xCB] = (byte)(value ? 1 : 0); }
        public override int AbilityNumber { get => 1 << Data[0xCC]; set => Data[0xCC] = (byte)((value >> 1) & 1); }
        public override bool Valid { get => Data[0xCD] == 0; set { if (value) Data[0xCD] = 0; } }
        // 0xCE unknown
        public override int MarkValue { get => SwapBits(Data[0xCF], 1, 2); protected set => Data[0xCF] = (byte)SwapBits(value, 1, 2); }
        public override int PKRS_Days { get => Math.Max((sbyte)Data[0xD0], (sbyte)0); set => Data[0xD0] = (byte)(value == 0 ? 0xFF : value & 0xF); }
        public int ShadowID { get => BigEndian.ToUInt16(Data, 0xD8); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0xD8); }
        public int Purification { get => BigEndian.ToInt32(Data, 0xDC); set => BigEndian.GetBytes(value).CopyTo(Data, 0xDC); }
        public uint EXP_Shadow { get => BigEndian.ToUInt32(Data, 0xC0); set => BigEndian.GetBytes(value).CopyTo(Data, 0xC0); }
        public override bool FatefulEncounter { get => Data[0x11C] == 1; set => Data[0x11C] = (byte)(value ? 1 : 0); }
        public new int EncounterType { get => Data[0xFB]; set => Data[0xFB] = (byte)value; }

        // Generated Attributes
        public override int PSV => (int)((PID >> 16 ^ PID & 0xFFFF) >> 3);
        public override int TSV => (TID ^ SID) >> 3;
        public override bool Japanese => Language == (int)LanguageID.Japanese;

        protected override byte[] Encrypt()
        {
            return (byte[])Data.Clone();
        }

        // Maximums
        public override int MaxMoveID => Legal.MaxMoveID_3;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_3;
        public override int MaxAbilityID => Legal.MaxAbilityID_3;
        public override int MaxItemID => Legal.MaxItemID_3;
        public override int MaxBallID => Legal.MaxBallID_3;
        public override int MaxGameID => 5;
        public override int MaxIV => 31;
        public override int MaxEV => 252;
        public override int OTLength => 7;
        public override int NickLength => 10;
    }
}

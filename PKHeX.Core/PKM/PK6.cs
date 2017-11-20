﻿using System;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary> Generation 6 <see cref="PKM"/> format. </summary>
    public class PK6 : PKM, IRibbonSetEvent3, IRibbonSetEvent4, IRibbonSetCommon3, IRibbonSetCommon4, IRibbonSetCommon6
    {
        public static readonly byte[] ExtraBytes =
        {
            0x36, 0x37, // Unused Ribbons
            0x58, 0x59, 0x73, 0x90, 0x91, 0x9E, 0x9F, 0xA0, 0xA1, 0xA7, 0xAA, 0xAB, 0xAC, 0xAD, 0xC8, 0xC9, 0xD7, 0xE4, 0xE5, 0xE6, 0xE7
        };
        public sealed override int SIZE_PARTY => PKX.SIZE_6PARTY;
        public override int SIZE_STORED => PKX.SIZE_6STORED;
        public override int Format => 6;
        public override PersonalInfo PersonalInfo => PersonalTable.AO.GetFormeEntry(Species, AltForm);

        public PK6(byte[] decryptedData = null, string ident = null)
        {
            Data = (byte[])(decryptedData ?? new byte[SIZE_PARTY]).Clone();
            PKMConverter.CheckEncrypted(ref Data);
            Identifier = ident;
            if (Data.Length != SIZE_PARTY)
                Array.Resize(ref Data, SIZE_PARTY);
        }
        public override PKM Clone() => new PK6(Data);

        private string GetString(int Offset, int Count) => StringConverter.GetString6(Data, Offset, Count);
        private byte[] SetString(string value, int maxLength) => StringConverter.SetString6(value, maxLength);

        // Trash Bytes
        public override byte[] Nickname_Trash { get => GetData(0x40, 24); set { if (value?.Length == 24) value.CopyTo(Data, 0x40); } }
        public override byte[] HT_Trash { get => GetData(0x78, 24); set { if (value?.Length == 24) value.CopyTo(Data, 0x78); } }
        public override byte[] OT_Trash { get => GetData(0xB0, 24); set { if (value?.Length == 24) value.CopyTo(Data, 0xB0); } }

        // Structure
        #region Block A
        public override uint EncryptionConstant
        {
            get => BitConverter.ToUInt32(Data, 0x00);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x00);
        }
        public override ushort Sanity
        {
            get => BitConverter.ToUInt16(Data, 0x04);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x04);
        }
        public override ushort Checksum
        {
            get => BitConverter.ToUInt16(Data, 0x06);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x06);
        }
        public override int Species
        {
            get => BitConverter.ToUInt16(Data, 0x08);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x08);
        }
        public override int HeldItem
        {
            get => BitConverter.ToUInt16(Data, 0x0A);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0A);
        }
        public override int TID
        {
            get => BitConverter.ToUInt16(Data, 0x0C);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0C);
        }
        public override int SID
        {
            get => BitConverter.ToUInt16(Data, 0x0E);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x0E);
        }
        public override uint EXP
        {
            get => BitConverter.ToUInt32(Data, 0x10);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x10);
        }
        public override int Ability { get => Data[0x14]; set => Data[0x14] = (byte)value; }
        public override int AbilityNumber { get => Data[0x15]; set => Data[0x15] = (byte)value; }
        public int TrainingBagHits { get => Data[0x16]; set => Data[0x16] = (byte)value; }
        public int TrainingBag { get => Data[0x17]; set => Data[0x17] = (byte)value; }
        public override uint PID
        {
            get => BitConverter.ToUInt32(Data, 0x18);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x18);
        }
        public override int Nature { get => Data[0x1C]; set => Data[0x1C] = (byte)value; }
        public override bool FatefulEncounter { get => (Data[0x1D] & 1) == 1; set => Data[0x1D] = (byte)(Data[0x1D] & ~0x01 | (value ? 1 : 0)); }
        public override int Gender { get => (Data[0x1D] >> 1) & 0x3; set => Data[0x1D] = (byte)(Data[0x1D] & ~0x06 | (value << 1)); }
        public override int AltForm { get => Data[0x1D] >> 3; set => Data[0x1D] = (byte)(Data[0x1D] & 0x07 | (value << 3)); }
        public override int EV_HP { get => Data[0x1E]; set => Data[0x1E] = (byte)value; }
        public override int EV_ATK { get => Data[0x1F]; set => Data[0x1F] = (byte)value; }
        public override int EV_DEF { get => Data[0x20]; set => Data[0x20] = (byte)value; }
        public override int EV_SPE { get => Data[0x21]; set => Data[0x21] = (byte)value; }
        public override int EV_SPA { get => Data[0x22]; set => Data[0x22] = (byte)value; }
        public override int EV_SPD { get => Data[0x23]; set => Data[0x23] = (byte)value; }
        public override int CNT_Cool { get => Data[0x24]; set => Data[0x24] = (byte)value; }
        public override int CNT_Beauty { get => Data[0x25]; set => Data[0x25] = (byte)value; }
        public override int CNT_Cute { get => Data[0x26]; set => Data[0x26] = (byte)value; }
        public override int CNT_Smart { get => Data[0x27]; set => Data[0x27] = (byte)value; }
        public override int CNT_Tough { get => Data[0x28]; set => Data[0x28] = (byte)value; }
        public override int CNT_Sheen { get => Data[0x29]; set => Data[0x29] = (byte)value; }
        public override int MarkValue { get => Data[0x2A]; protected set => Data[0x2A] = (byte)value; }
        private byte PKRS { get => Data[0x2B]; set => Data[0x2B] = value; }
        public override int PKRS_Days { get => PKRS & 0xF; set => PKRS = (byte)(PKRS & ~0xF | value); }
        public override int PKRS_Strain { get => PKRS >> 4; set => PKRS = (byte)(PKRS & 0xF | value << 4); }
        private byte ST1 { get => Data[0x2C]; set => Data[0x2C] = value; }
        public bool Unused0 { get => (ST1 & (1 << 0)) == 1 << 0; set => ST1 = (byte)(ST1 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool Unused1 { get => (ST1 & (1 << 1)) == 1 << 1; set => ST1 = (byte)(ST1 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool SuperTrain1_SPA { get => (ST1 & (1 << 2)) == 1 << 2; set => ST1 = (byte)(ST1 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool SuperTrain1_HP  { get => (ST1 & (1 << 3)) == 1 << 3; set => ST1 = (byte)(ST1 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool SuperTrain1_ATK { get => (ST1 & (1 << 4)) == 1 << 4; set => ST1 = (byte)(ST1 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool SuperTrain1_SPD { get => (ST1 & (1 << 5)) == 1 << 5; set => ST1 = (byte)(ST1 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool SuperTrain1_SPE { get => (ST1 & (1 << 6)) == 1 << 6; set => ST1 = (byte)(ST1 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool SuperTrain1_DEF { get => (ST1 & (1 << 7)) == 1 << 7; set => ST1 = (byte)(ST1 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        private byte ST2 { get => Data[0x2D]; set => Data[0x2D] = value; }
        public bool SuperTrain2_SPA { get => (ST2 & (1 << 0)) == 1 << 0; set => ST2 = (byte)(ST2 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool SuperTrain2_HP  { get => (ST2 & (1 << 1)) == 1 << 1; set => ST2 = (byte)(ST2 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool SuperTrain2_ATK { get => (ST2 & (1 << 2)) == 1 << 2; set => ST2 = (byte)(ST2 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool SuperTrain2_SPD { get => (ST2 & (1 << 3)) == 1 << 3; set => ST2 = (byte)(ST2 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool SuperTrain2_SPE { get => (ST2 & (1 << 4)) == 1 << 4; set => ST2 = (byte)(ST2 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool SuperTrain2_DEF { get => (ST2 & (1 << 5)) == 1 << 5; set => ST2 = (byte)(ST2 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool SuperTrain3_SPA { get => (ST2 & (1 << 6)) == 1 << 6; set => ST2 = (byte)(ST2 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool SuperTrain3_HP { get => (ST2 & (1 << 7)) == 1 << 7; set => ST2 = (byte)(ST2 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        private byte ST3 { get => Data[0x2E]; set => Data[0x2E] = value; }
        public bool SuperTrain3_ATK { get => (ST3 & (1 << 0)) == 1 << 0; set => ST3 = (byte)(ST3 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool SuperTrain3_SPD { get => (ST3 & (1 << 1)) == 1 << 1; set => ST3 = (byte)(ST3 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool SuperTrain3_SPE { get => (ST3 & (1 << 2)) == 1 << 2; set => ST3 = (byte)(ST3 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool SuperTrain3_DEF { get => (ST3 & (1 << 3)) == 1 << 3; set => ST3 = (byte)(ST3 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool SuperTrain4_1 { get => (ST3 & (1 << 4)) == 1 << 4; set => ST3 = (byte)(ST3 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool SuperTrain5_1 { get => (ST3 & (1 << 5)) == 1 << 5; set => ST3 = (byte)(ST3 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool SuperTrain5_2 { get => (ST3 & (1 << 6)) == 1 << 6; set => ST3 = (byte)(ST3 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool SuperTrain5_3 { get => (ST3 & (1 << 7)) == 1 << 7; set => ST3 = (byte)(ST3 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        private byte ST4 { get => Data[0x2F]; set => Data[0x2F] = value; }
        public bool SuperTrain5_4 { get => (ST4 & (1 << 0)) == 1 << 0; set => ST4 = (byte)(ST4 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool SuperTrain6_1 { get => (ST4 & (1 << 1)) == 1 << 1; set => ST4 = (byte)(ST4 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool SuperTrain6_2 { get => (ST4 & (1 << 2)) == 1 << 2; set => ST4 = (byte)(ST4 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool SuperTrain6_3 { get => (ST4 & (1 << 3)) == 1 << 3; set => ST4 = (byte)(ST4 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool SuperTrain7_1 { get => (ST4 & (1 << 4)) == 1 << 4; set => ST4 = (byte)(ST4 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool SuperTrain7_2 { get => (ST4 & (1 << 5)) == 1 << 5; set => ST4 = (byte)(ST4 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool SuperTrain7_3 { get => (ST4 & (1 << 6)) == 1 << 6; set => ST4 = (byte)(ST4 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool SuperTrain8_1 { get => (ST4 & (1 << 7)) == 1 << 7; set => ST4 = (byte)(ST4 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        private byte RIB0 { get => Data[0x30]; set => Data[0x30] = value; } // Ribbons are read as uints, but let's keep them per byte.
        private byte RIB1 { get => Data[0x31]; set => Data[0x31] = value; }
        private byte RIB2 { get => Data[0x32]; set => Data[0x32] = value; }
        private byte RIB3 { get => Data[0x33]; set => Data[0x33] = value; }
        private byte RIB4 { get => Data[0x34]; set => Data[0x34] = value; }
        private byte RIB5 { get => Data[0x35]; set => Data[0x35] = value; }
        private byte RIB6 { get => Data[0x36]; set => Data[0x36] = value; } // Unused
        private byte RIB7 { get => Data[0x37]; set => Data[0x37] = value; } // Unused
        public bool RibbonChampionKalos         { get => (RIB0 & (1 << 0)) == 1 << 0; set => RIB0 = (byte)(RIB0 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonChampionG3Hoenn       { get => (RIB0 & (1 << 1)) == 1 << 1; set => RIB0 = (byte)(RIB0 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonChampionSinnoh        { get => (RIB0 & (1 << 2)) == 1 << 2; set => RIB0 = (byte)(RIB0 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonBestFriends           { get => (RIB0 & (1 << 3)) == 1 << 3; set => RIB0 = (byte)(RIB0 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonTraining              { get => (RIB0 & (1 << 4)) == 1 << 4; set => RIB0 = (byte)(RIB0 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonBattlerSkillful       { get => (RIB0 & (1 << 5)) == 1 << 5; set => RIB0 = (byte)(RIB0 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonBattlerExpert         { get => (RIB0 & (1 << 6)) == 1 << 6; set => RIB0 = (byte)(RIB0 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonEffort                { get => (RIB0 & (1 << 7)) == 1 << 7; set => RIB0 = (byte)(RIB0 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonAlert                 { get => (RIB1 & (1 << 0)) == 1 << 0; set => RIB1 = (byte)(RIB1 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonShock                 { get => (RIB1 & (1 << 1)) == 1 << 1; set => RIB1 = (byte)(RIB1 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonDowncast              { get => (RIB1 & (1 << 2)) == 1 << 2; set => RIB1 = (byte)(RIB1 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonCareless              { get => (RIB1 & (1 << 3)) == 1 << 3; set => RIB1 = (byte)(RIB1 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonRelax                 { get => (RIB1 & (1 << 4)) == 1 << 4; set => RIB1 = (byte)(RIB1 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonSnooze                { get => (RIB1 & (1 << 5)) == 1 << 5; set => RIB1 = (byte)(RIB1 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonSmile                 { get => (RIB1 & (1 << 6)) == 1 << 6; set => RIB1 = (byte)(RIB1 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonGorgeous              { get => (RIB1 & (1 << 7)) == 1 << 7; set => RIB1 = (byte)(RIB1 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonRoyal                 { get => (RIB2 & (1 << 0)) == 1 << 0; set => RIB2 = (byte)(RIB2 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonGorgeousRoyal         { get => (RIB2 & (1 << 1)) == 1 << 1; set => RIB2 = (byte)(RIB2 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonArtist                { get => (RIB2 & (1 << 2)) == 1 << 2; set => RIB2 = (byte)(RIB2 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonFootprint             { get => (RIB2 & (1 << 3)) == 1 << 3; set => RIB2 = (byte)(RIB2 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonRecord                { get => (RIB2 & (1 << 4)) == 1 << 4; set => RIB2 = (byte)(RIB2 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonLegend                { get => (RIB2 & (1 << 5)) == 1 << 5; set => RIB2 = (byte)(RIB2 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonCountry               { get => (RIB2 & (1 << 6)) == 1 << 6; set => RIB2 = (byte)(RIB2 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonNational              { get => (RIB2 & (1 << 7)) == 1 << 7; set => RIB2 = (byte)(RIB2 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonEarth                 { get => (RIB3 & (1 << 0)) == 1 << 0; set => RIB3 = (byte)(RIB3 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonWorld                 { get => (RIB3 & (1 << 1)) == 1 << 1; set => RIB3 = (byte)(RIB3 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonClassic               { get => (RIB3 & (1 << 2)) == 1 << 2; set => RIB3 = (byte)(RIB3 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonPremier               { get => (RIB3 & (1 << 3)) == 1 << 3; set => RIB3 = (byte)(RIB3 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonEvent                 { get => (RIB3 & (1 << 4)) == 1 << 4; set => RIB3 = (byte)(RIB3 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonBirthday              { get => (RIB3 & (1 << 5)) == 1 << 5; set => RIB3 = (byte)(RIB3 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RibbonSpecial               { get => (RIB3 & (1 << 6)) == 1 << 6; set => RIB3 = (byte)(RIB3 & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool RibbonSouvenir              { get => (RIB3 & (1 << 7)) == 1 << 7; set => RIB3 = (byte)(RIB3 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonWishing               { get => (RIB4 & (1 << 0)) == 1 << 0; set => RIB4 = (byte)(RIB4 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonChampionBattle      { get => (RIB4 & (1 << 1)) == 1 << 1; set => RIB4 = (byte)(RIB4 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonChampionRegional    { get => (RIB4 & (1 << 2)) == 1 << 2; set => RIB4 = (byte)(RIB4 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonChampionNational    { get => (RIB4 & (1 << 3)) == 1 << 3; set => RIB4 = (byte)(RIB4 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonChampionWorld       { get => (RIB4 & (1 << 4)) == 1 << 4; set => RIB4 = (byte)(RIB4 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RIB4_5 { get => (RIB4 & (1 << 5)) == 1 << 5; set => RIB4 = (byte)(RIB4 & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Unused
        public bool RIB4_6 { get => (RIB4 & (1 << 6)) == 1 << 6; set => RIB4 = (byte)(RIB4 & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Unused
        public bool RibbonChampionG6Hoenn       { get => (RIB4 & (1 << 7)) == 1 << 7; set => RIB4 = (byte)(RIB4 & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public bool RibbonContestStar           { get => (RIB5 & (1 << 0)) == 1 << 0; set => RIB5 = (byte)(RIB5 & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool RibbonMasterCoolness        { get => (RIB5 & (1 << 1)) == 1 << 1; set => RIB5 = (byte)(RIB5 & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool RibbonMasterBeauty          { get => (RIB5 & (1 << 2)) == 1 << 2; set => RIB5 = (byte)(RIB5 & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool RibbonMasterCuteness        { get => (RIB5 & (1 << 3)) == 1 << 3; set => RIB5 = (byte)(RIB5 & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool RibbonMasterCleverness      { get => (RIB5 & (1 << 4)) == 1 << 4; set => RIB5 = (byte)(RIB5 & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool RibbonMasterToughness       { get => (RIB5 & (1 << 5)) == 1 << 5; set => RIB5 = (byte)(RIB5 & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool RIB5_6 { get => (RIB5 & (1 << 6)) == 1 << 6; set => RIB5 = (byte)(RIB5 & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Unused
        public bool RIB5_7 { get => (RIB5 & (1 << 7)) == 1 << 7; set => RIB5 = (byte)(RIB5 & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Unused
        public int RibbonCountMemoryContest { get => Data[0x38]; set => Data[0x38] = (byte)value; }
        public int RibbonCountMemoryBattle { get => Data[0x39]; set => Data[0x39] = (byte)value; }
        private ushort DistByte { get => BitConverter.ToUInt16(Data, 0x3A); set => BitConverter.GetBytes(value).CopyTo(Data, 0x3A); }
        public bool DistSuperTrain1 { get => (DistByte & (1 << 0)) == 1 << 0; set => DistByte = (byte)(DistByte & ~(1 << 0) | (value ? 1 << 0 : 0)); }
        public bool DistSuperTrain2 { get => (DistByte & (1 << 1)) == 1 << 1; set => DistByte = (byte)(DistByte & ~(1 << 1) | (value ? 1 << 1 : 0)); }
        public bool DistSuperTrain3 { get => (DistByte & (1 << 2)) == 1 << 2; set => DistByte = (byte)(DistByte & ~(1 << 2) | (value ? 1 << 2 : 0)); }
        public bool DistSuperTrain4 { get => (DistByte & (1 << 3)) == 1 << 3; set => DistByte = (byte)(DistByte & ~(1 << 3) | (value ? 1 << 3 : 0)); }
        public bool DistSuperTrain5 { get => (DistByte & (1 << 4)) == 1 << 4; set => DistByte = (byte)(DistByte & ~(1 << 4) | (value ? 1 << 4 : 0)); }
        public bool DistSuperTrain6 { get => (DistByte & (1 << 5)) == 1 << 5; set => DistByte = (byte)(DistByte & ~(1 << 5) | (value ? 1 << 5 : 0)); }
        public bool Dist7 { get => (DistByte & (1 << 6)) == 1 << 6; set => DistByte = (byte)(DistByte & ~(1 << 6) | (value ? 1 << 6 : 0)); }
        public bool Dist8 { get => (DistByte & (1 << 7)) == 1 << 7; set => DistByte = (byte)(DistByte & ~(1 << 7) | (value ? 1 << 7 : 0)); }
        public uint FormDuration { get => BitConverter.ToUInt32(Data, 0x3C); set => BitConverter.GetBytes(value).CopyTo(Data, 0x3C); }
        #endregion
        #region Block B
        public override string Nickname { get => GetString(0x40, 24); set => SetString(value, 12).CopyTo(Data, 0x40); }
        public override int Move1
        {
            get => BitConverter.ToUInt16(Data, 0x5A);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x5A);
        }
        public override int Move2
        {
            get => BitConverter.ToUInt16(Data, 0x5C);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x5C);
        }
        public override int Move3
        {
            get => BitConverter.ToUInt16(Data, 0x5E);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x5E);
        }
        public override int Move4
        {
            get => BitConverter.ToUInt16(Data, 0x60);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x60);
        }
        public override int Move1_PP { get => Data[0x62]; set => Data[0x62] = (byte)value; }
        public override int Move2_PP { get => Data[0x63]; set => Data[0x63] = (byte)value; }
        public override int Move3_PP { get => Data[0x64]; set => Data[0x64] = (byte)value; }
        public override int Move4_PP { get => Data[0x65]; set => Data[0x65] = (byte)value; }
        public override int Move1_PPUps { get => Data[0x66]; set => Data[0x66] = (byte)value; }
        public override int Move2_PPUps { get => Data[0x67]; set => Data[0x67] = (byte)value; }
        public override int Move3_PPUps { get => Data[0x68]; set => Data[0x68] = (byte)value; }
        public override int Move4_PPUps { get => Data[0x69]; set => Data[0x69] = (byte)value; }
        public override int RelearnMove1
        {
            get => BitConverter.ToUInt16(Data, 0x6A);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x6A);
        }
        public override int RelearnMove2
        {
            get => BitConverter.ToUInt16(Data, 0x6C);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x6C);
        }
        public override int RelearnMove3
        {
            get => BitConverter.ToUInt16(Data, 0x6E);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x6E);
        }
        public override int RelearnMove4
        {
            get => BitConverter.ToUInt16(Data, 0x70);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x70);
        }
        public override bool SecretSuperTrainingUnlocked { get => (Data[0x72] & 1) == 1; set => Data[0x72] = (byte)((Data[0x72] & ~1) | (value ? 1 : 0)); }
        public override bool SecretSuperTrainingComplete { get => (Data[0x72] & 2) == 2; set => Data[0x72] = (byte)((Data[0x72] & ~2) | (value ? 2 : 0)); }
        public byte _0x73 { get => Data[0x73]; set => Data[0x73] = value; }
        private uint IV32 { get => BitConverter.ToUInt32(Data, 0x74); set => BitConverter.GetBytes(value).CopyTo(Data, 0x74); }
        public override int IV_HP { get => (int)(IV32 >> 00) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 00)) | (uint)((value > 31 ? 31 : value) << 00)); }
        public override int IV_ATK { get => (int)(IV32 >> 05) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 05)) | (uint)((value > 31 ? 31 : value) << 05)); }
        public override int IV_DEF { get => (int)(IV32 >> 10) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 10)) | (uint)((value > 31 ? 31 : value) << 10)); }
        public override int IV_SPE { get => (int)(IV32 >> 15) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 15)) | (uint)((value > 31 ? 31 : value) << 15)); }
        public override int IV_SPA { get => (int)(IV32 >> 20) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 20)) | (uint)((value > 31 ? 31 : value) << 20)); }
        public override int IV_SPD { get => (int)(IV32 >> 25) & 0x1F; set => IV32 = (uint)((IV32 & ~(0x1F << 25)) | (uint)((value > 31 ? 31 : value) << 25)); }
        public override bool IsEgg { get => ((IV32 >> 30) & 1) == 1; set => IV32 = (uint)((IV32 & ~0x40000000) | (uint)(value ? 0x40000000 : 0)); }
        public override bool IsNicknamed { get => ((IV32 >> 31) & 1) == 1; set => IV32 = (IV32 & 0x7FFFFFFF) | (value ? 0x80000000 : 0); }
        #endregion
        #region Block C
        public override string HT_Name { get => GetString(0x78, 24); set => SetString(value, 12).CopyTo(Data, 0x78); }
        public override int HT_Gender { get => Data[0x92]; set => Data[0x92] = (byte)value; }
        public override int CurrentHandler { get => Data[0x93]; set => Data[0x93] = (byte)value; }
        public override int Geo1_Region { get => Data[0x94]; set => Data[0x94] = (byte)value; }
        public override int Geo1_Country { get => Data[0x95]; set => Data[0x95] = (byte)value; }
        public override int Geo2_Region { get => Data[0x96]; set => Data[0x96] = (byte)value; }
        public override int Geo2_Country { get => Data[0x97]; set => Data[0x97] = (byte)value; }
        public override int Geo3_Region { get => Data[0x98]; set => Data[0x98] = (byte)value; }
        public override int Geo3_Country { get => Data[0x99]; set => Data[0x99] = (byte)value; }
        public override int Geo4_Region { get => Data[0x9A]; set => Data[0x9A] = (byte)value; }
        public override int Geo4_Country { get => Data[0x9B]; set => Data[0x9B] = (byte)value; }
        public override int Geo5_Region { get => Data[0x9C]; set => Data[0x9C] = (byte)value; }
        public override int Geo5_Country { get => Data[0x9D]; set => Data[0x9D] = (byte)value; }
        public byte _0x9E { get => Data[0x9E]; set => Data[0x9E] = value; }
        public byte _0x9F { get => Data[0x9F]; set => Data[0x9F] = value; }
        public byte _0xA0 { get => Data[0xA0]; set => Data[0xA0] = value; }
        public byte _0xA1 { get => Data[0xA1]; set => Data[0xA1] = value; }
        public override int HT_Friendship { get => Data[0xA2]; set => Data[0xA2] = (byte)value; }
        public override int HT_Affection { get => Data[0xA3]; set => Data[0xA3] = (byte)value; }
        public override int HT_Intensity { get => Data[0xA4]; set => Data[0xA4] = (byte)value; }
        public override int HT_Memory { get => Data[0xA5]; set => Data[0xA5] = (byte)value; }
        public override int HT_Feeling { get => Data[0xA6]; set => Data[0xA6] = (byte)value; }
        public byte _0xA7 { get => Data[0xA7]; set => Data[0xA7] = value; }
        public override int HT_TextVar { get => BitConverter.ToUInt16(Data, 0xA8); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xA8); }
        public byte _0xAA { get => Data[0xAA]; set => Data[0xAA] = value; }
        public byte _0xAB { get => Data[0xAB]; set => Data[0xAB] = value; }
        public byte _0xAC { get => Data[0xAC]; set => Data[0xAC] = value; }
        public byte _0xAD { get => Data[0xAD]; set => Data[0xAD] = value; }
        public override byte Fullness { get => Data[0xAE]; set => Data[0xAE] = value; }
        public override byte Enjoyment { get => Data[0xAF]; set => Data[0xAF] = value; }
        #endregion
        #region Block D
        public override string OT_Name { get => GetString(0xB0, 24); set => SetString(value, 12).CopyTo(Data, 0xB0); }
        public override int OT_Friendship { get => Data[0xCA]; set => Data[0xCA] = (byte)value; }
        public override int OT_Affection { get => Data[0xCB]; set => Data[0xCB] = (byte)value; }
        public override int OT_Intensity { get => Data[0xCC]; set => Data[0xCC] = (byte)value; }
        public override int OT_Memory { get => Data[0xCD]; set => Data[0xCD] = (byte)value; }
        public override int OT_TextVar { get => BitConverter.ToUInt16(Data, 0xCE); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xCE); }
        public override int OT_Feeling { get => Data[0xD0]; set => Data[0xD0] = (byte)value; }
        public override int Egg_Year { get => Data[0xD1]; set => Data[0xD1] = (byte)value; }
        public override int Egg_Month { get => Data[0xD2]; set => Data[0xD2] = (byte)value; }
        public override int Egg_Day { get => Data[0xD3]; set => Data[0xD3] = (byte)value; }
        public override int Met_Year { get => Data[0xD4]; set => Data[0xD4] = (byte)value; }
        public override int Met_Month { get => Data[0xD5]; set => Data[0xD5] = (byte)value; }
        public override int Met_Day { get => Data[0xD6]; set => Data[0xD6] = (byte)value; }
        public byte _0xD7 { get => Data[0xD7]; set => Data[0xD7] = value; }
        public override int Egg_Location { get => BitConverter.ToUInt16(Data, 0xD8); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xD8); }
        public override int Met_Location { get => BitConverter.ToUInt16(Data, 0xDA); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xDA); }
        public override int Ball { get => Data[0xDC]; set => Data[0xDC] = (byte)value; }
        public override int Met_Level { get => Data[0xDD] & ~0x80; set => Data[0xDD] = (byte)((Data[0xDD] & 0x80) | value); }
        public override int OT_Gender { get => Data[0xDD] >> 7; set => Data[0xDD] = (byte)((Data[0xDD] & ~0x80) | (value << 7)); }
        public override int EncounterType { get => Data[0xDE]; set => Data[0xDE] = (byte)value; }
        public override int Version { get => Data[0xDF]; set => Data[0xDF] = (byte)value; }
        public override int Country { get => Data[0xE0]; set => Data[0xE0] = (byte)value; }
        public override int Region { get => Data[0xE1]; set => Data[0xE1] = (byte)value; }
        public override int ConsoleRegion { get => Data[0xE2]; set => Data[0xE2] = (byte)value; }
        public override int Language { get => Data[0xE3]; set => Data[0xE3] = (byte)value; }
        #endregion
        #region Battle Stats
        public override int Stat_Level { get => Data[0xEC]; set => Data[0xEC] = (byte)value; }
        public override int Stat_HPCurrent { get => BitConverter.ToUInt16(Data, 0xF0); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xF0); }
        public override int Stat_HPMax { get => BitConverter.ToUInt16(Data, 0xF2); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xF2); }
        public override int Stat_ATK { get => BitConverter.ToUInt16(Data, 0xF4); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xF4); }
        public override int Stat_DEF { get => BitConverter.ToUInt16(Data, 0xF6); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xF6); }
        public override int Stat_SPE { get => BitConverter.ToUInt16(Data, 0xF8); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xF8); }
        public override int Stat_SPA { get => BitConverter.ToUInt16(Data, 0xFA); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xFA); }
        public override int Stat_SPD { get => BitConverter.ToUInt16(Data, 0xFC); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0xFC); }
        #endregion

        // Simple Generated Attributes
        public override int CurrentFriendship
        {
            get => CurrentHandler == 0 ? OT_Friendship : HT_Friendship;
            set { if (CurrentHandler == 0) OT_Friendship = value; else HT_Friendship = value; }
        }
        public int OppositeFriendship
        {
            get => CurrentHandler == 1 ? OT_Friendship : HT_Friendship;
            set { if (CurrentHandler == 1) OT_Friendship = value; else HT_Friendship = value; }
        }
        public override int SuperTrainingMedalCount(int maxCount = 30)
        {
            uint value = BitConverter.ToUInt32(Data, 0x2C);
            int TrainCount = 0;
            value >>= 2;
            for (int i = 0; i < maxCount; i++)
            {
                if ((value & 1) != 0)
                    TrainCount++;
                value >>= 1;
            }

            return TrainCount;
        }

        public override int PSV => (int)((PID >> 16 ^ PID & 0xFFFF) >> 4);
        public override int TSV => (TID ^ SID) >> 4;
        public bool IsUntradedEvent6 => Geo1_Country == 0 && Geo1_Region == 0 && Met_Location / 10000 == 4 && Gen6;
        
        // Complex Generated Attributes

        public override int Characteristic
        {
            get
            {
                // Characteristic with EC%6
                int pm6 = (int)(EncryptionConstant % 6); // EC MOD 6
                int maxIV = IVs.Max();
                int pm6stat = 0;

                for (int i = 0; i < 6; i++)
                    if (IVs[pm6stat = pm6++ % 6] == maxIV)
                        break;
                return pm6stat*5 + maxIV%5;
            }
        }

        // Methods
        protected override byte[] Encrypt()
        {
            RefreshChecksum();
            return PKX.EncryptArray(Data);
        }

        // General User-error Fixes
        public void FixRelearn()
        {
            while (true)
            {
                if (RelearnMove4 != 0 && RelearnMove3 == 0)
                {
                    RelearnMove3 = RelearnMove4;
                    RelearnMove4 = 0;
                }
                if (RelearnMove3 != 0 && RelearnMove2 == 0)
                {
                    RelearnMove2 = RelearnMove3;
                    RelearnMove3 = 0;
                    continue;
                }
                if (RelearnMove2 != 0 && RelearnMove1 == 0)
                {
                    RelearnMove1 = RelearnMove2;
                    RelearnMove2 = 0;
                    continue;
                }
                break;
            }
        }
        public void FixMemories()
        {
            if (IsEgg) // No memories if is egg.
            {
                Geo1_Country = Geo2_Country = Geo3_Country = Geo4_Country = Geo5_Country =
                Geo1_Region = Geo2_Region = Geo3_Region = Geo4_Region = Geo5_Region =
                HT_Friendship = HT_Affection = HT_TextVar = HT_Memory = HT_Intensity = HT_Feeling =
                /* OT_Friendship */ OT_Affection = OT_TextVar = OT_Memory = OT_Intensity = OT_Feeling = 0;

                // Clear Handler
                HT_Name = "".PadRight(11, '\0');
                return;
            }
            
            if (IsUntraded)
                HT_Friendship = HT_Affection = HT_TextVar = HT_Memory = HT_Intensity = HT_Feeling = 0;
            if (!Gen6)
                OT_Affection = OT_TextVar = OT_Memory = OT_Intensity = OT_Feeling = 0;

            Geo1_Region = Geo1_Country > 0 ? Geo1_Region : 0;
            Geo2_Region = Geo2_Country > 0 ? Geo2_Region : 0;
            Geo3_Region = Geo3_Country > 0 ? Geo3_Region : 0;
            Geo4_Region = Geo4_Country > 0 ? Geo4_Region : 0;
            Geo5_Region = Geo5_Country > 0 ? Geo5_Region : 0;

            while (true)
            {
                if (Geo5_Country != 0 && Geo4_Country == 0)
                {
                    Geo4_Country = Geo5_Country;
                    Geo4_Region = Geo5_Region;
                    Geo5_Country = Geo5_Region = 0;
                }
                if (Geo4_Country != 0 && Geo3_Country == 0)
                {
                    Geo3_Country = Geo4_Country;
                    Geo3_Region = Geo4_Region;
                    Geo4_Country = Geo4_Region = 0;
                    continue;
                }
                if (Geo3_Country != 0 && Geo2_Country == 0)
                {
                    Geo2_Country = Geo3_Country;
                    Geo2_Region = Geo3_Region;
                    Geo3_Country = Geo3_Region = 0;
                    continue;
                }
                if (Geo2_Country != 0 && Geo1_Country == 0)
                {
                    Geo1_Country = Geo2_Country;
                    Geo1_Region = Geo2_Region;
                    Geo2_Country = Geo2_Region = 0;
                    continue;
                }
                if (Geo1_Country == 0 && !IsUntraded && !IsUntradedEvent6)
                {
                    if ((Country | Region) == 0)
                        break;
                    // Traded Non-Eggs/Events need to have a current location.
                    Geo1_Country = Country;
                    Geo1_Region = Region;
                    continue;
                }
                break;
            }
        }

        // Synthetic Trading Logic
        public void Trade(string SAV_Trainer, int SAV_TID, int SAV_SID, int SAV_COUNTRY, int SAV_REGION, int SAV_GENDER, bool Bank, int Day = 1, int Month = 1, int Year = 2015)
        {
            // Eggs do not have any modifications done if they are traded
            if (IsEgg && !(SAV_Trainer == OT_Name && SAV_TID == TID && SAV_SID == SID && SAV_GENDER == OT_Gender))
                UpdateEgg(Day, Month, Year);
            // Process to the HT if the OT of the Pokémon does not match the SAV's OT info.
            else if (!TradeOT(SAV_Trainer, SAV_TID, SAV_SID, SAV_COUNTRY, SAV_REGION, SAV_GENDER))
                TradeHT(SAV_Trainer, SAV_COUNTRY, SAV_REGION, SAV_GENDER, Bank);
        }
        private bool TradeOT(string SAV_Trainer, int SAV_TID, int SAV_SID, int SAV_COUNTRY, int SAV_REGION, int SAV_GENDER)
        {
            // Check to see if the OT matches the SAV's OT info.
            if (!(SAV_Trainer == OT_Name && SAV_TID == TID && SAV_SID == SID && SAV_GENDER == OT_Gender))
                return false;

            CurrentHandler = 0;
            if (!IsUntraded && (SAV_COUNTRY != Geo1_Country || SAV_REGION != Geo1_Region))
                TradeGeoLocation(SAV_COUNTRY, SAV_REGION);

            return true;
        }
        private void TradeHT(string SAV_Trainer, int SAV_COUNTRY, int SAV_REGION, int SAV_GENDER, bool Bank)
        {
            if (SAV_Trainer != HT_Name || SAV_GENDER != HT_Gender || (Geo1_Country == 0 && Geo1_Region == 0 && !IsUntradedEvent6))
                TradeGeoLocation(SAV_COUNTRY, SAV_REGION);

            CurrentHandler = 1;
            if (HT_Name != SAV_Trainer)
            {
                HT_Friendship = PersonalInfo.BaseFriendship;
                HT_Affection = 0;
            }
            HT_Name = SAV_Trainer;
            HT_Gender = SAV_GENDER;

            // Make a memory if no memory already exists. Pretty terrible way of doing this but I'd rather not overwrite existing memories.
            if (HT_Memory == 0)
                TradeMemory(Bank);
        }
        // Misc Updates
        private void UpdateEgg(int Day, int Month, int Year)
        {
            Met_Location = 30002;
            Met_Day = Day;
            Met_Month = Month;
            Met_Year = Year - 2000;
        }
        private void TradeGeoLocation(int GeoCountry, int GeoRegion)
        {
            // Allow the method to abort if the values are invalid
            if (GeoCountry < 0 || GeoRegion < 0)
                return;

            // Trickle down
            Geo5_Country = Geo4_Country;
            Geo5_Region = Geo4_Region;

            Geo4_Country = Geo3_Country;
            Geo4_Region = Geo3_Region;

            Geo3_Country = Geo2_Country;
            Geo3_Region = Geo2_Region;

            Geo2_Country = Geo1_Country;
            Geo2_Region = Geo1_Region;

            Geo1_Country = GeoCountry;
            Geo1_Region = GeoRegion;
        }
        public void TradeMemory(bool Bank)
        {
            HT_Memory = 4; // Link trade to [VAR: General Location]
            HT_TextVar = Bank ? 0 : 9; // Somewhere (Bank) : Pokécenter (Trade)
            HT_Intensity = 1;
            HT_Feeling = Util.Rand.Next(0, Bank ? 9 : 19); // 0-9 Bank, 0-19 Trade
        }

        // Legality Properties
        public override bool WasLink => Met_Location == 30011;
        public override bool WasEvent => Met_Location > 40000 && Met_Location < 50000 || FatefulEncounter;
        public override bool WasEventEgg => GenNumber < 5 ? base.WasEventEgg : ((Egg_Location > 40000 && Egg_Location < 50000) || (FatefulEncounter && Egg_Location == 30002)) && Met_Level == 1;

        // Maximums
        public override int MaxMoveID => Legal.MaxMoveID_6_AO;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_6;
        public override int MaxAbilityID => Legal.MaxAbilityID_6_AO;
        public override int MaxItemID => Legal.MaxItemID_6_AO;
        public override int MaxBallID => Legal.MaxBallID_6;
        public override int MaxGameID => Legal.MaxGameID_6; // OR
        public override int MaxIV => 31;
        public override int MaxEV => 252;
        public override int OTLength => 12;
        public override int NickLength => 12;

        public PK7 ConvertToPK7()
        {
            PK7 pk7 = new PK7(Data)
            {
                Markings = Markings, // Clears old Super Training Bag & Hits Remaining
                Data = { [0x2A] = 0 }, // Clears old Marking Value
            };
            
            switch (AbilityNumber)
            {
                case 1: case 2: case 4: // Valid Ability Numbers
                    int index = AbilityNumber >> 1;
                    if (PersonalInfo.Abilities[index] == Ability) // correct pair
                        pk7.Ability = pk7.PersonalInfo.Abilities[index];
                    break;
            }

            pk7.TradeMemory(Bank: true); // oh no, memories on gen7 pkm
            pk7.Geo1_Country = PKMConverter.Country;
            pk7.Geo1_Region = PKMConverter.Region;

            // Bank-accurate data zeroing
            for (var i = 0x94; i < 0x9E; i++) pk7.Data[i] = 0; /* Geolocations. */
            for (var i = 0xAA; i < 0xB0; i++) pk7.Data[i] = 0; /* Unused/Amie Fullness & Enjoyment. */
            for (var i = 0xE4; i < 0xE8; i++) pk7.Data[i] = 0; /* Unused. */
            pk7.Data[0x72] &= 0xFC; /* Clear lower two bits of Super training flags. */
            pk7.Data[0xDE] = 0; /* Gen IV encounter type. */

            // Fix Checksum
            pk7.RefreshChecksum();

            return pk7; // Done!
        }
    }
}

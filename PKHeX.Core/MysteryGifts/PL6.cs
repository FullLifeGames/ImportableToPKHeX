﻿using System;
using System.Linq;
using System.Text;

namespace PKHeX.Core
{
    /// <summary>
    /// Pokemon Link Data Storage
    /// </summary>
    /// <remarks>
    /// This Template object is very similar to the <see cref="PCD"/> structure in that it stores more data than just the gift.
    /// This template object is only present in Generation 6 save files.
    /// </remarks>
    public class PL6
    {
        public const int Size = 0xA47;
        public const string Filter = "Pokémon Link Data|*.pl6|All Files (*.*)|*.*";

        public readonly byte[] Data;
        public PL6(byte[] data = null)
        {
            Data = (byte[])(data?.Clone() ?? new byte[Size]);
        }
        /// <summary>
        /// Pokémon Link Flag 
        /// </summary>
        public byte PL_Flag {
            get => Data[0x00]; set => Data[0x00] = value;
        }
        public bool PL_enabled { get => PL_Flag != 0; set => PL_Flag = (byte)(value ? 1 << 7 : 0); }

        /// <summary>
        /// Name of data source
        /// </summary>
        public string Origin_app {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, 0x01, 0x6E));
            set => Encoding.Unicode.GetBytes(value.PadRight(54 + 1, '\0')).CopyTo(Data, 0x01);
        }
        
        //Pokemon transfer flags?
        public uint PKM1_flags {
            get => BitConverter.ToUInt32(Data, 0x99);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x99); }
        public uint PKM2_flags {
            get => BitConverter.ToUInt32(Data, 0x141);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x141); }
        public uint PKM3_flags {
            get => BitConverter.ToUInt32(Data, 0x1E9);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x1E9); }
        public uint PKM4_flags {
            get => BitConverter.ToUInt32(Data, 0x291);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x291); }
        public uint PKM5_flags {
            get => BitConverter.ToUInt32(Data, 0x339);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x339); }
        public uint PKM6_flags {
            get => BitConverter.ToUInt32(Data, 0x3E1);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x3E1); }

        public uint[] Flags
        {
            get => new[] { PKM1_flags, PKM2_flags, PKM3_flags, PKM4_flags, PKM5_flags, PKM6_flags }; set
            {
                if (value.Length > 0) PKM1_flags = value[0];
                if (value.Length > 1) PKM2_flags = value[1];
                if (value.Length > 2) PKM3_flags = value[2];
                if (value.Length > 3) PKM4_flags = value[3];
                if (value.Length > 4) PKM5_flags = value[4];
                if (value.Length > 5) PKM6_flags = value[5];
            }
        }
        
        //Pokémon
        
        public PL6_PKM poke1 {
            get => new PL6_PKM(Data.Skip(0x9D).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x9D); }
        public PL6_PKM poke2 {
            get => new PL6_PKM(Data.Skip(0x145).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x145); }
        public PL6_PKM poke3 {
            get => new PL6_PKM(Data.Skip(0x1ED).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x1ED); }
        public PL6_PKM poke4 {
            get => new PL6_PKM(Data.Skip(0x295).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x295); }
        public PL6_PKM poke5 {
            get => new PL6_PKM(Data.Skip(0x33D).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x33D); }
        public PL6_PKM poke6 {
            get => new PL6_PKM(Data.Skip(0x3E5).Take(PL6_PKM.Size).ToArray());
            set => value.Data.CopyTo(Data, 0x3E5); }

        public PL6_PKM[] Pokes
        {
            get => new[] { poke1, poke2, poke3, poke4, poke5, poke6 };
            set
            {
                if (value.Length > 0) poke1 = value[0];
                if (value.Length > 1) poke2 = value[1];
                if (value.Length > 2) poke3 = value[2];
                if (value.Length > 3) poke4 = value[3];
                if (value.Length > 4) poke5 = value[4];
                if (value.Length > 5) poke6 = value[5];
            }
        }
        
        // Item Properties
        public int Item_1 {
            get => BitConverter.ToUInt16(Data, 0x489);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x489); }
        public int Quantity_1 {
            get => BitConverter.ToUInt16(Data, 0x48B);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x48B); }
        public int Item_2 {
            get => BitConverter.ToUInt16(Data, 0x48D);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x48D); }
        public int Quantity_2 {
            get => BitConverter.ToUInt16(Data, 0x48F);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x48F); }
        public int Item_3 {
            get => BitConverter.ToUInt16(Data, 0x491);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x491); }
        public int Quantity_3 {
            get => BitConverter.ToUInt16(Data, 0x493);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x493); }
        public int Item_4 {
            get => BitConverter.ToUInt16(Data, 0x495);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x495); }
        public int Quantity_4 {
            get => BitConverter.ToUInt16(Data, 0x497);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x497); }
        public int Item_5 {
            get => BitConverter.ToUInt16(Data, 0x499);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x499); }
        public int Quantity_5 {
            get => BitConverter.ToUInt16(Data, 0x49B);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x49B); }
        public int Item_6 {
            get => BitConverter.ToUInt16(Data, 0x49D);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x49D); }
        public int Quantity_6 {
            get => BitConverter.ToUInt16(Data, 0x49F);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x49F); }
        
        public int[] Items
        {
            get => new[] { Item_1, Item_2, Item_3, Item_4, Item_5, Item_6 };
            set
            {
                if (value.Length > 0) Item_1 = value[0];
                if (value.Length > 1) Item_2 = value[1];
                if (value.Length > 2) Item_3 = value[2];
                if (value.Length > 3) Item_4 = value[3];
                if (value.Length > 4) Item_5 = value[4];
                if (value.Length > 5) Item_6 = value[5];
            }
        }
        
        public int[] Quantities
        {
            get => new[] { Quantity_1, Quantity_2, Quantity_3, Quantity_4, Quantity_5, Quantity_6 };
            set
            {
                if (value.Length > 0) Quantity_1 = value[0];
                if (value.Length > 1) Quantity_2 = value[1];
                if (value.Length > 2) Quantity_3 = value[2];
                if (value.Length > 3) Quantity_4 = value[3];
                if (value.Length > 4) Quantity_5 = value[4];
                if (value.Length > 5) Quantity_6 = value[5];
            }
        }
        
        
        //Battle Points
        public int BattlePoints {
            get => BitConverter.ToUInt16(Data, 0x4A1);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x4A1); }
        //PokéMiles
        public int Pokemiles {
            get => BitConverter.ToUInt16(Data, 0x4A3);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x4A3); }
    }

    /// <summary>
    /// Pokemon Link Gift Template
    /// </summary>
    /// <remarks>
    /// This Template object is very similar to the <see cref="WC6"/> structure and similar objects, in that the structure offsets are ordered the same.
    /// This template object is only present in Generation 6 save files.
    /// </remarks>
    public class PL6_PKM : IEncounterable, IRibbonSetEvent3, IRibbonSetEvent4
    {
        internal const int Size = 0xA0;

        public readonly byte[] Data;
        public PL6_PKM(byte[] data = null)
        {
            Data = (byte[])(data?.Clone() ?? new byte[Size]);
        }

        public int TID {
            get => BitConverter.ToUInt16(Data, 0x00);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x00); }
        public int SID {
            get => BitConverter.ToUInt16(Data, 0x02);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x02); }
        public int OriginGame {
            get => Data[0x04];
            set => Data[0x04] = (byte)value; }
        public uint EncryptionConstant {
            get => BitConverter.ToUInt32(Data, 0x08);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x08); }
        public int Pokéball {
            get => Data[0xE];
            set => Data[0xE] = (byte)value; }
        public int HeldItem {
            get => BitConverter.ToUInt16(Data, 0x10);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x10); }
        public int Move1 {
            get => BitConverter.ToUInt16(Data, 0x12);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x12); }
        public int Move2 {
            get => BitConverter.ToUInt16(Data, 0x14);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x14); }
        public int Move3 {
            get => BitConverter.ToUInt16(Data, 0x16);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x16); }
        public int Move4 {
            get => BitConverter.ToUInt16(Data, 0x18);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x18); }
        public int Species {
            get => BitConverter.ToUInt16(Data, 0x1A);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x1A); }
        public int Form {
            get => Data[0x1C];
            set => Data[0x1C] = (byte)value; }
        public int Language {
            get => Data[0x1D];
            set => Data[0x1D] = (byte)value; }
        public string Nickname {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, 0x1E, 0x1A));
            set => Encoding.Unicode.GetBytes(value.PadRight(12 + 1, '\0')).CopyTo(Data, 0x1E); }
        public int Nature {
            get => Data[0x38];
            set => Data[0x38] = (byte)value; }
        public int Gender {
            get => Data[0x39];
            set => Data[0x39] = (byte)value; }
        public int AbilityType {
            get => Data[0x3A];
            set => Data[0x3A] = (byte)value; }
        public int PIDType {
            get => Data[0x3B];
            set => Data[0x3B] = (byte)value; }
        public int EggLocation {
            get => BitConverter.ToUInt16(Data, 0x3C);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x3C); }
        public int MetLocation  {
            get => BitConverter.ToUInt16(Data, 0x3E);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x3F); }
        public int MetLevel  {
            get => Data[0x40];
            set => Data[0x40] = (byte)value; }
        public int LevelMin => MetLevel;
        public int LevelMax => MetLevel;

        public int CNT_Cool { get => Data[0x41]; set => Data[0x41] = (byte)value; }
        public int CNT_Beauty { get => Data[0x42]; set => Data[0x42] = (byte)value; }
        public int CNT_Cute { get => Data[0x43]; set => Data[0x43] = (byte)value; }
        public int CNT_Smart { get => Data[0x44]; set => Data[0x44] = (byte)value; }
        public int CNT_Tough { get => Data[0x45]; set => Data[0x45] = (byte)value; }
        public int CNT_Sheen { get => Data[0x46]; set => Data[0x46] = (byte)value; }

        public int IV_HP { get => Data[0x47]; set => Data[0x47] = (byte)value; }
        public int IV_ATK { get => Data[0x48]; set => Data[0x48] = (byte)value; }
        public int IV_DEF { get => Data[0x49]; set => Data[0x49] = (byte)value; }
        public int IV_SPE { get => Data[0x4A]; set => Data[0x4A] = (byte)value; }
        public int IV_SPA { get => Data[0x4B]; set => Data[0x4B] = (byte)value; }
        public int IV_SPD { get => Data[0x4C]; set => Data[0x4C] = (byte)value; }

        public int OTGender { get => Data[0x4D]; set => Data[0x4D] = (byte)value; }
        public string OT {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, 0x4E, 0x1A));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, 0x4E); }
        public int Level { get => Data[0x68]; set => Data[0x68] = (byte)value; }
        public bool IsEgg { get => Data[0x69] == 1; set => Data[0x69] = (byte)(value ? 1 : 0); }
        public uint PID {
            get => BitConverter.ToUInt32(Data, 0x6C);
            set => BitConverter.GetBytes(value).CopyTo(Data, 0x6C); }
        public int RelearnMove1 {
            get => BitConverter.ToUInt16(Data, 0x70);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x70); }
        public int RelearnMove2 {
            get => BitConverter.ToUInt16(Data, 0x72);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x72); }
        public int RelearnMove3 {
            get => BitConverter.ToUInt16(Data, 0x74);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x74); }
        public int RelearnMove4 {
            get => BitConverter.ToUInt16(Data, 0x76);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x76); }
        public int OT_Intensity { get => Data[0x78]; set => Data[0x78] = (byte)value; }
        public int OT_Memory { get => Data[0x79]; set => Data[0x79] = (byte)value; }
        public int OT_TextVar { get => BitConverter.ToUInt16(Data, 0x7A); set => BitConverter.GetBytes((ushort)value).CopyTo(Data, 0x7A); }
        public int OT_Feeling { get => Data[0x7C]; set => Data[0x7C] = (byte)value; }

        private byte RIB0 { get => Data[0x0C]; set => Data[0x0C] = value; }
        public bool RibbonChampionBattle { get => (RIB0 & (1 << 0)) == 1 << 0; set => RIB0 = (byte)(RIB0 & ~(1 << 0) | (value ? 1 << 0 : 0)); } // Battle Champ Ribbon
        public bool RibbonChampionRegional { get => (RIB0 & (1 << 1)) == 1 << 1; set => RIB0 = (byte)(RIB0 & ~(1 << 1) | (value ? 1 << 1 : 0)); } // Regional Champ Ribbon
        public bool RibbonChampionNational { get => (RIB0 & (1 << 2)) == 1 << 2; set => RIB0 = (byte)(RIB0 & ~(1 << 2) | (value ? 1 << 2 : 0)); } // National Champ Ribbon
        public bool RibbonCountry { get => (RIB0 & (1 << 3)) == 1 << 3; set => RIB0 = (byte)(RIB0 & ~(1 << 3) | (value ? 1 << 3 : 0)); } // Country Ribbon
        public bool RibbonNational { get => (RIB0 & (1 << 4)) == 1 << 4; set => RIB0 = (byte)(RIB0 & ~(1 << 4) | (value ? 1 << 4 : 0)); } // National Ribbon
        public bool RibbonEarth { get => (RIB0 & (1 << 5)) == 1 << 5; set => RIB0 = (byte)(RIB0 & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Earth Ribbon
        public bool RibbonWorld { get => (RIB0 & (1 << 6)) == 1 << 6; set => RIB0 = (byte)(RIB0 & ~(1 << 6) | (value ? 1 << 6 : 0)); } // World Ribbon
        public bool RibbonEvent { get => (RIB0 & (1 << 7)) == 1 << 7; set => RIB0 = (byte)(RIB0 & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Event Ribbon
        private byte RIB1 { get => Data[0x0D]; set => Data[0x0D] = value; }
        public bool RibbonChampionWorld { get => (RIB1 & (1 << 0)) == 1 << 0; set => RIB1 = (byte)(RIB1 & ~(1 << 0) | (value ? 1 << 0 : 0)); } // World Champ Ribbon
        public bool RibbonBirthday { get => (RIB1 & (1 << 1)) == 1 << 1; set => RIB1 = (byte)(RIB1 & ~(1 << 1) | (value ? 1 << 1 : 0)); } // Birthday Ribbon
        public bool RibbonSpecial { get => (RIB1 & (1 << 2)) == 1 << 2; set => RIB1 = (byte)(RIB1 & ~(1 << 2) | (value ? 1 << 2 : 0)); } // Special Ribbon
        public bool RibbonSouvenir { get => (RIB1 & (1 << 3)) == 1 << 3; set => RIB1 = (byte)(RIB1 & ~(1 << 3) | (value ? 1 << 3 : 0)); } // Souvenir Ribbon
        public bool RibbonWishing { get => (RIB1 & (1 << 4)) == 1 << 4; set => RIB1 = (byte)(RIB1 & ~(1 << 4) | (value ? 1 << 4 : 0)); } // Wishing Ribbon
        public bool RibbonClassic { get => (RIB1 & (1 << 5)) == 1 << 5; set => RIB1 = (byte)(RIB1 & ~(1 << 5) | (value ? 1 << 5 : 0)); } // Classic Ribbon
        public bool RibbonPremier { get => (RIB1 & (1 << 6)) == 1 << 6; set => RIB1 = (byte)(RIB1 & ~(1 << 6) | (value ? 1 << 6 : 0)); } // Premier Ribbon
        public bool RIB1_7 { get => (RIB1 & (1 << 7)) == 1 << 7; set => RIB1 = (byte)(RIB1 & ~(1 << 7) | (value ? 1 << 7 : 0)); } // Empty

        public int[] Moves
        {
            get => new[] { Move1, Move2, Move3, Move4 };
            set
            {
                if (value.Length > 0) Move1 = value[0];
                if (value.Length > 1) Move2 = value[1];
                if (value.Length > 2) Move3 = value[2];
                if (value.Length > 3) Move4 = value[3];
            }
        }
        public int[] RelearnMoves
        {
            get => new[] { RelearnMove1, RelearnMove2, RelearnMove3, RelearnMove4 };
            set
            {
                if (value.Length > 0) RelearnMove1 = value[0];
                if (value.Length > 1) RelearnMove2 = value[1];
                if (value.Length > 2) RelearnMove3 = value[2];
                if (value.Length > 3) RelearnMove4 = value[3];
            }
        }
        public bool EggEncounter => IsEgg;

        public string Name => "Pokémon Link";
    }
}
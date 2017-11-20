﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKHeX.Core
{
    /// <summary>
    /// Generation 4 <see cref="SaveFile"/> object for Pokémon Battle Revolution saves.
    /// </summary>
    public sealed class SAV4BR : SaveFile
    {
        public override string BAKName => $"{FileName} [{Version} #{SaveCount:0000}].bak";
        public override string Filter => "PbrSaveData|*";
        public override string Extension => "";

        private const int SAVE_COUNT = 4;
        public SAV4BR(byte[] data = null)
        {
            Data = data == null ? new byte[SaveUtil.SIZE_G4BR] : (byte[])data.Clone();
            BAK = (byte[])Data.Clone();
            Exportable = !Data.SequenceEqual(new byte[Data.Length]);

            if (SaveUtil.GetIsG4BRSAV(Data) != GameVersion.BATREV)
                return;

            Data = DecryptPBRSaveData(data);

            // Detect active save
            SaveCount = Math.Max(BigEndian.ToUInt32(Data, 0x1C004C), BigEndian.ToUInt32(Data, 0x4C));
            if (BigEndian.ToUInt32(Data, 0x1C004C) > BigEndian.ToUInt32(Data, 0x4C))
            {
                byte[] tempData = new byte[0x1C0000];
                Array.Copy(Data, 0, tempData, 0, 0x1C0000);
                Array.Copy(Data, 0x1C0000, Data, 0, 0x1C0000);
                tempData.CopyTo(Data, 0x1C0000);
            }

            SaveSlots = new List<int>();
            SaveNames = new string[SAVE_COUNT];
            for (int i = 0; i < SAVE_COUNT; i++)
            {
                if (BitConverter.ToUInt16(Data, 0x390 + 0x6FF00*i) != 0)
                {
                    SaveSlots.Add(i);
                    SaveNames[i] = Encoding.BigEndianUnicode.GetString(Data, 0x390 + 0x6FF00*i, 0x10);
                }
            }

            CurrentSlot = SaveSlots.First();

            Personal = PersonalTable.DP;
            HeldItems = Legal.HeldItems_DP;

            if (!Exportable)
                ClearBoxes();
        }

        private readonly uint SaveCount;

        protected override byte[] Write(bool DSV)
        {
            SetChecksums();
            return EncryptPBRSaveData(Data);
        }

        // Configuration
        public override SaveFile Clone() { return new SAV4BR(Write(DSV: false)); }

        public readonly List<int> SaveSlots;
        public readonly string[] SaveNames;
        public int CurrentSlot;
        protected override int Box { // 4 save slots, data reading depends on current slot
            get => 0x978 + 0x6FF00 * CurrentSlot;
            set { }
        }

        public override int SIZE_STORED => PKX.SIZE_4STORED;
        protected override int SIZE_PARTY => PKX.SIZE_4PARTY - 0x10; // PBR has a party
        public override PKM BlankPKM => new BK4();
        public override Type PKMType => typeof(BK4);

        public override int MaxMoveID => 467;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_4;
        public override int MaxAbilityID => 123;
        public override int MaxItemID => 536;
        public override int MaxBallID => 0x18;
        public override int MaxGameID => 15;

        public override int MaxEV => 255;
        public override int Generation => 4;
        protected override int GiftCountMax => 1;
        public override int OTLength => 7;
        public override int NickLength => 10;
        public override int MaxMoney => 999999;

        public override int BoxCount => 18;
        public override bool HasParty => false;

        // Checksums
        protected override void SetChecksums()
        {
            SetChecksum(Data, 0, 0x100, 8);
            SetChecksum(Data, 0, 0x1C0000, 0x1BFF80);
            SetChecksum(Data, 0x1C0000, 0x100, 0x1C0008);
            SetChecksum(Data, 0x1C0000, 0x1C0000, 0x1BFF80 + 0x1C0000);
        }
        public override bool ChecksumsValid => IsChecksumsValid(Data);
        public override string ChecksumInfo => $"Checksums valid: {ChecksumsValid}.";

        public static bool IsChecksumsValid(byte[] sav)
        {
            return VerifyChecksum(sav, 0x000000, 0x1C0000, 0x1BFF80)
                && VerifyChecksum(sav, 0x000000, 0x000100, 0x000008)
                && VerifyChecksum(sav, 0x1C0000, 0x1C0000, 0x1BFF80 + 0x1C0000)
                && VerifyChecksum(sav, 0x1C0000, 0x000100, 0x1C0008);
        }

        // Trainer Info
        public override GameVersion Version { get => GameVersion.BATREV; protected set { } }

        // Storage
        public override int GetPartyOffset(int slot) // TODO
        {
            return -1;
        }
        public override int GetBoxOffset(int box)
        {
            return Box + SIZE_STORED * box * 30;
        }

        // Save file does not have Box Name / Wallpaper info
        public override string GetBoxName(int box) { return $"BOX {box + 1}"; }
        public override void SetBoxName(int box, string value) { }

        public override PKM GetPKM(byte[] data)
        {
            byte[] pkm = data.Take(SIZE_STORED).ToArray();
            PKM bk = new BK4(pkm);
            return bk;
        }
        public override byte[] DecryptPKM(byte[] data)
        {
            return data;
        }

        protected override void SetDex(PKM pkm) { }

        public static byte[] DecryptPBRSaveData(byte[] input)
        {
            byte[] output = new byte[input.Length];
            for (int base_ofs = 0; base_ofs < SaveUtil.SIZE_G4BR; base_ofs += 0x1C0000)
            {
                Array.Copy(input, base_ofs, output, base_ofs, 8);

                ushort[] keys = new ushort[4];
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = BigEndian.ToUInt16(input, base_ofs + i * 2);

                for (int ofs = base_ofs + 8; ofs < base_ofs + 0x1C0000; ofs += 8)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        ushort val = BigEndian.ToUInt16(input, ofs + i*2);
                        val -= keys[i];
                        BigEndian.GetBytes(val).CopyTo(output, ofs + i*2);
                    }
                    keys = SaveUtil.AdvanceGCKeys(keys);
                }
            }
            return output;
        }

        private static byte[] EncryptPBRSaveData(byte[] input)
        {
            byte[] output = new byte[input.Length];
            for (int base_ofs = 0; base_ofs < SaveUtil.SIZE_G4BR; base_ofs += 0x1C0000)
            {
                Array.Copy(input, base_ofs, output, base_ofs, 8);

                ushort[] keys = new ushort[4];
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = BigEndian.ToUInt16(input, base_ofs + i * 2);

                for (int ofs = base_ofs + 8; ofs < base_ofs + 0x1C0000; ofs += 8)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        ushort val = BigEndian.ToUInt16(input, ofs + i * 2);
                        val += keys[i];
                        BigEndian.GetBytes(val).CopyTo(output, ofs + i * 2);
                    }
                    keys = SaveUtil.AdvanceGCKeys(keys);
                }
            }
            return output;
        }

        public static bool VerifyChecksum(byte[] input, int offset, int len, int checksum_offset)
        {
            uint[] storedChecksums = new uint[16];
            for (int i = 0; i < storedChecksums.Length; i++)
            {
                storedChecksums[i] = BigEndian.ToUInt32(input, checksum_offset + i*4);
                BitConverter.GetBytes((uint)0).CopyTo(input, checksum_offset + i*4);
            }

            uint[] checksums = new uint[16];

            for (int i = 0; i < len; i += 2)
            {
                ushort val = BigEndian.ToUInt16(input, offset + i);
                for (int j = 0; j < 16; j++)
                {
                    checksums[j] += (uint)((val >> j) & 1);
                }
            }

            for (int i = 0; i < storedChecksums.Length; i++)
            {
                BigEndian.GetBytes(storedChecksums[i]).CopyTo(input, checksum_offset + i*4);
            }

            return checksums.SequenceEqual(storedChecksums);
        }

        private static void SetChecksum(byte[] input, int offset, int len, int checksum_offset)
        {
            uint[] storedChecksums = new uint[16];
            for (int i = 0; i < storedChecksums.Length; i++)
            {
                storedChecksums[i] = BigEndian.ToUInt32(input, checksum_offset + i * 4);
                BitConverter.GetBytes((uint)0).CopyTo(input, checksum_offset + i * 4);
            }

            uint[] checksums = new uint[16];

            for (int i = 0; i < len; i += 2)
            {
                ushort val = BigEndian.ToUInt16(input, offset + i);
                for (int j = 0; j < 16; j++)
                {
                    checksums[j] += (uint)((val >> j) & 1);
                }
            }

            for (int i = 0; i < checksums.Length; i++)
            {
                BigEndian.GetBytes(checksums[i]).CopyTo(input, checksum_offset + i * 4);
            }
        }

        public override string GetString(int Offset, int Count) => StringConverter.GetBEString4(Data, Offset, Count);
        public override byte[] SetString(string value, int maxLength, int PadToSize = 0, ushort PadWith = 0)
        {
            if (PadToSize == 0)
                PadToSize = maxLength + 1;
            return StringConverter.SetBEString4(value, maxLength, PadToSize, PadWith);
        }
    }
}

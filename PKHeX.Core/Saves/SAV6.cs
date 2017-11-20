﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PKHeX.Core
{
    /// <summary>
    /// Generation 6 <see cref="SaveFile"/> object.
    /// </summary>
    public sealed class SAV6 : SaveFile
    {
        // Save Data Attributes
        public override string BAKName => $"{FileName} [{OT} ({Version}) - {LastSavedTime}].bak";
        public override string Filter => "Main SAV|*.*";
        public override string Extension => "";
        public SAV6(byte[] data = null)
        {
            Data = data == null ? new byte[SaveUtil.SIZE_G6ORAS] : (byte[])data.Clone();
            BAK = (byte[])Data.Clone();
            Exportable = !Data.All(z => z == 0);

            // Load Info
            GetBlockInfo();
            GetSAVOffsets();

            HeldItems = ORAS ? Legal.HeldItem_AO : Legal.HeldItem_XY;
            Personal = ORAS ? PersonalTable.AO : PersonalTable.XY;
            if (!Exportable)
                ClearBoxes();
        }

        // Configuration
        public override SaveFile Clone() { return new SAV6(Data); }
        
        public override int SIZE_STORED => PKX.SIZE_6STORED;
        protected override int SIZE_PARTY => PKX.SIZE_6PARTY;
        public override PKM BlankPKM => new PK6();
        public override Type PKMType => typeof(PK6);

        public override int BoxCount => 31;
        public override int MaxEV => 252;
        public override int Generation => 6;
        protected override int GiftCountMax => 24;
        protected override int GiftFlagMax => 0x100 * 8;
        protected override int EventFlagMax => 8 * 0x180;
        protected override int EventConstMax => (EventFlag - EventConst) / 2;
        public override int OTLength => 12;
        public override int NickLength => 12;

        public override int MaxMoveID => XY ? Legal.MaxMoveID_6_XY : Legal.MaxMoveID_6_AO;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_6;
        public override int MaxItemID => XY ? Legal.MaxItemID_6_XY : Legal.MaxItemID_6_AO;
        public override int MaxAbilityID => XY ? Legal.MaxAbilityID_6_XY : Legal.MaxAbilityID_6_AO;
        public override int MaxBallID => Legal.MaxBallID_6;
        public override int MaxGameID => Legal.MaxGameID_6; // OR

        // Feature Overrides
        public override bool HasGeolocation => true;

        // Blocks & Offsets
        private int BlockInfoOffset;
        private BlockInfo[] Blocks;
        private void GetBlockInfo()
        {
            BlockInfoOffset = Data.Length - 0x200 + 0x10;
            if (BitConverter.ToUInt32(Data, BlockInfoOffset) != SaveUtil.BEEF)
                BlockInfoOffset -= 0x200; // No savegames have more than 0x3D blocks, maybe in the future?
            int count = (Data.Length - BlockInfoOffset - 0x8) / 8;
            BlockInfoOffset += 4;

            Blocks = new BlockInfo[count];
            int CurrentPosition = 0;
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i] = new BlockInfo
                {
                    Offset = CurrentPosition,
                    Length = BitConverter.ToInt32(Data, BlockInfoOffset + 0 + 8 * i),
                    ID = BitConverter.ToUInt16(Data, BlockInfoOffset + 4 + 8 * i),
                    Checksum = BitConverter.ToUInt16(Data, BlockInfoOffset + 6 + 8 * i)
                };

                // Expand out to nearest 0x200
                CurrentPosition += Blocks[i].Length % 0x200 == 0 ? Blocks[i].Length : 0x200 - Blocks[i].Length % 0x200 + Blocks[i].Length;

                if ((Blocks[i].ID != 0) || i == 0) continue;
                count = i;
                break;
            }
            // Fix Final Array Lengths
            Array.Resize(ref Blocks, count);
        }
        protected override void SetChecksums()
        {
            // Check for invalid block lengths
            if (Blocks.Length < 3) // arbitrary...
            {
                Debug.WriteLine("Not enough blocks ({0}), aborting SetChecksums", Blocks.Length);
                return;
            }
            // Apply checksums
            for (int i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i].Length + Blocks[i].Offset > Data.Length)
                { Debug.WriteLine("Block {0} has invalid offset/length value.", i); return; }
                byte[] array = new byte[Blocks[i].Length];
                Array.Copy(Data, Blocks[i].Offset, array, 0, array.Length);
                BitConverter.GetBytes(SaveUtil.CRC16_CCITT(array)).CopyTo(Data, BlockInfoOffset + 6 + i * 8);
            }
        }
        public override bool ChecksumsValid
        {
            get
            {
                for (int i = 0; i < Blocks.Length; i++)
                {
                    if (Blocks[i].Length + Blocks[i].Offset > Data.Length)
                        return false;
                    byte[] array = new byte[Blocks[i].Length];
                    Array.Copy(Data, Blocks[i].Offset, array, 0, array.Length);
                    if (SaveUtil.CRC16_CCITT(array) != BitConverter.ToUInt16(Data, BlockInfoOffset + 6 + i * 8))
                        return false;
                }
                return true;
            }
        }
        public override string ChecksumInfo
        {
            get
            {
                var list = new List<string>();
                for (int i = 0; i < Blocks.Length; i++)
                {
                    if (Blocks[i].Length + Blocks[i].Offset > Data.Length)
                        return $"Block {i} Invalid Offset/Length.";
                    byte[] array = new byte[Blocks[i].Length];
                    Array.Copy(Data, Blocks[i].Offset, array, 0, array.Length);
                    if (SaveUtil.CRC16_CCITT(array) == BitConverter.ToUInt16(Data, BlockInfoOffset + 6 + i * 8))
                        continue;

                    list.Add($"Invalid: {i:X2} @ Region {Blocks[i].Offset:X5}");
                }
                // Return Outputs
                list.Add($"SAV: {Blocks.Length - list.Count}/{Blocks.Length}");
                return string.Join(Environment.NewLine, list);
            }   
        }
        public override ulong? Secure1
        {
            get => BitConverter.ToUInt64(Data, BlockInfoOffset - 0x14);
            set => BitConverter.GetBytes(value ?? 0).CopyTo(Data, BlockInfoOffset - 0x14);
        }
        public override ulong? Secure2
        {
            get => BitConverter.ToUInt64(Data, BlockInfoOffset - 0xC);
            set => BitConverter.GetBytes(value ?? 0).CopyTo(Data, BlockInfoOffset - 0xC);
        }
        
        private void GetSAVOffsets()
        {
            if (ORASDEMO)
            {
                /* 00: */ Bag = 0x00000;
                /* 01: */ ItemInfo = 0x00C00; // Select Bound Items
                /* 02: */ AdventureInfo = 0x00E00;
                /* 03: */ Trainer1 = 0x01000;
                /* 04: */ // = 0x01200; [00004] // ???
                /* 05: */ PlayTime = 0x01400;
                /* 06: */ // = 0x01600; [00024] // FFFFFFFF
                /* 07: */ // = 0x01800; [02100] // Overworld Data
                /* 08: */ Trainer2 = 0x03A00;
                /* 09: */ TrainerCard = 0x03C00;
                /* 10: */ Party = 0x03E00;
                /* 11: */ EventConst = 0x04600; EventFlag = EventConst + 0x2FC;
                /* 12: */ // = 0x04C00; [00004] // 87B1A23F const
                /* 13: */ // = 0x04E00; [00048] // Repel Info, (Swarm?) and other overworld info
                /* 14: */ SUBE = 0x05000;
                /* 15: */ PSSStats = 0x05400;

                OFS_PouchHeldItem = Bag + 0;
                OFS_PouchKeyItem = Bag + 0x640;
                OFS_PouchTMHM = Bag + 0x7C0;
                OFS_PouchMedicine = Bag + 0x970;
                OFS_PouchBerry = Bag + 0xA70;
            }
            else if (XY)
            {
                Puff = 0x00000;
                Bag = 0x00400;
                ItemInfo = 0x1000;
                AdventureInfo = 0x01200;
                Trainer1 = 0x1400;
                PlayTime = 0x1800;
                Accessories = 0x1A00;
                Trainer2 = 0x4200;
                PCLayout = 0x4400;
                BattleBox = 0x04A00;
                PSS = 0x05000;
                TrainerCard = 0x14000;
                Party = 0x14200;
                EventConst = 0x14A00;
                PSSStats = 0x1E400;
                PokeDex = 0x15000;
                Fused = 0x16000;
                OPower = 0x16A00;
                GTS = 0x17800;
                HoF = 0x19400;
                MaisonStats = 0x1B1C0;
                Daycare = 0x1B200;
                BerryField = 0x1B800;
                WondercardFlags = 0x1BC00;
                SUBE = 0x1D890;
                SuperTrain = 0x1F200;
                LinkInfo = 0x1FE00;
                Box = 0x22600;
                JPEG = 0x57200;

                PCBackgrounds = PCLayout + 0x41E;
                PCFlags = PCLayout + 0x43D;
                LastViewedBox = PCLayout + 0x43F;
                EventFlag = EventConst + 0x2FC;
                PokeDexLanguageFlags = PokeDex + 0x3C8;
                Spinda = PokeDex + 0x648;
                WondercardData = WondercardFlags + 0x100;

                OFS_PouchHeldItem = Bag + 0;
                OFS_PouchKeyItem = Bag + 0x640;
                OFS_PouchTMHM = Bag + 0x7C0;
                OFS_PouchMedicine = Bag + 0x968;
                OFS_PouchBerry = Bag + 0xA68;
            }
            else if (ORAS)
            {
                Puff = 0x00000;
                Bag = 0x00400;
                ItemInfo = 0x1000;
                AdventureInfo = 0x01200;
                Trainer1 = 0x01400;
                PlayTime = 0x1800;
                Accessories = 0x1A00;
                Trainer2 = 0x04200;
                PCLayout = 0x04400;
                BattleBox = 0x04A00;
                PSS = 0x05000;
                TrainerCard = 0x14000;
                Party = 0x14200;
                EventConst = 0x14A00;
                PokeDex = 0x15000;
                Fused = 0x16A00;
                OPower = 0x17400;
                GTS = 0x18200;
                HoF = 0x19E00;
                MaisonStats = 0x1BBC0;
                Daycare = 0x1BC00;
                BerryField = 0x1C400;
                WondercardFlags = 0x1CC00;
                SUBE = 0x1D890;
                PSSStats = 0x1F400;
                SuperTrain = 0x20200;
                LinkInfo = 0x20E00;
                Contest = 0x23600;
                SecretBase = 0x23A00;
                EonTicket = 0x319B8;
                Box = 0x33000;
                JPEG = 0x67C00;

                PCBackgrounds = PCLayout + 0x41E;
                PCFlags = PCLayout + 0x43D;
                LastViewedBox = PCLayout + 0x43F;
                EventFlag = EventConst + 0x2FC;
                PokeDexLanguageFlags = PokeDex + 0x400;
                Spinda = PokeDex + 0x680;
                EncounterCount = PokeDex + 0x686;
                WondercardData = WondercardFlags + 0x100;
                Daycare2 = Daycare + 0x1F0;

                OFS_PouchHeldItem = Bag + 0;
                OFS_PouchKeyItem = Bag + 0x640;
                OFS_PouchTMHM = Bag + 0x7C0;
                OFS_PouchMedicine = Bag + 0x970;
                OFS_PouchBerry = Bag + 0xA70;
            }
            else // Empty input
            {
                Party = 0x0;
                Box = Party + SIZE_PARTY * 6 + 0x1000;
            }
        }

        // Private Only
        private int Bag { get; set; } = int.MinValue;
        private int AdventureInfo { get; set; } = int.MinValue;
        private int Trainer2 { get; set; } = int.MinValue;
        private int LastViewedBox { get; set; } = int.MinValue;
        private int WondercardFlags { get; set; } = int.MinValue;
        private int PlayTime { get; set; } = int.MinValue;
        private int JPEG { get; set; } = int.MinValue;
        private int ItemInfo { get; set; } = int.MinValue;
        private int Daycare2 { get; set; } = int.MinValue;
        private int LinkInfo { get; set; } = int.MinValue;

        // Accessible as SAV6
        public int TrainerCard { get; private set; } = 0x14000;
        public int PCFlags { get; private set; } = int.MinValue;
        public int PSSStats { get; private set; } = int.MinValue;
        public int MaisonStats { get; private set; } = int.MinValue;
        public int EonTicket { get; private set; } = int.MinValue;
        public int PCBackgrounds { get; private set; } = int.MinValue;
        public int Contest { get; private set; } = int.MinValue;
        public int Accessories { get; private set; } = int.MinValue;
        public int PokeDexLanguageFlags { get; private set; } = int.MinValue;
        public int Spinda { get; private set; } = int.MinValue;
        public int EncounterCount { get; private set; } = int.MinValue;

        public override GameVersion Version
        {
            get
            {
                switch (Game)
                {
                    case 24: return GameVersion.X;
                    case 25: return GameVersion.Y;
                    case 26: return GameVersion.AS;
                    case 27: return GameVersion.OR;
                }
                return GameVersion.Unknown;
            }
        }
        
        // Player Information
        public override ushort TID
        {
            get => BitConverter.ToUInt16(Data, TrainerCard + 0);
            set => BitConverter.GetBytes(value).CopyTo(Data, TrainerCard + 0);
        }
        public override ushort SID
        {
            get => BitConverter.ToUInt16(Data, TrainerCard + 2);
            set => BitConverter.GetBytes(value).CopyTo(Data, TrainerCard + 2);
        }
        public override int Game
        {
            get => Data[TrainerCard + 4];
            set => Data[TrainerCard + 4] = (byte)value;
        }
        public override int Gender
        {
            get => Data[TrainerCard + 5];
            set => Data[TrainerCard + 5] = (byte)value;
        }
        public override int MultiplayerSpriteID
        {
            get => Data[TrainerCard + 7];
            set => Data[TrainerCard + 7] = (byte)value;
        }
        public override int GameSyncIDSize => 16; // 64 bits
        public override string GameSyncID
        {
            get
            {
                var data = Data.Skip(TrainerCard + 8).Take(GameSyncIDSize / 2).Reverse().ToArray();
                return BitConverter.ToString(data).Replace("-", "");
            }
            set
            {
                if (value == null)
                    return;
                if (value.Length > GameSyncIDSize)
                    return;
                Enumerable.Range(0, value.Length)
                     .Where(x => x % 2 == 0)
                     .Reverse()
                     .Select(x => Convert.ToByte(value.Substring(x, 2), 16))
                     .ToArray().CopyTo(Data, TrainerCard + 8);
            }
        }
        public override int SubRegion
        {
            get => Data[TrainerCard + 0x26];
            set => Data[TrainerCard + 0x26] = (byte)value;
        }
        public override int Country
        {
            get => Data[TrainerCard + 0x27];
            set => Data[TrainerCard + 0x27] = (byte)value;
        }
        public override int ConsoleRegion
        {
            get => Data[TrainerCard + 0x2C];
            set => Data[TrainerCard + 0x2C] = (byte)value;
        }
        public override int Language
        {
            get => Data[TrainerCard + 0x2D];
            set => Data[TrainerCard + 0x2D] = (byte)value;
        }
        public override string OT
        {
            get => GetString(TrainerCard + 0x48, 0x1A);
            set => SetString(value, OTLength).CopyTo(Data, TrainerCard + 0x48);
        }
        public string OT_Nick
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x62, 0x1A));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x62);
        }
        public string Saying1
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x7C + 0x22 * 0, 0x22));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x7C + 0x22 * 0);
        }
        public string Saying2
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x7C + 0x22 * 1, 0x22));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x7C + 0x22 * 1);
        }
        public string Saying3
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x7C + 0x22 * 2, 0x22));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x7C + 0x22 * 2);
        }
        public string Saying4
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x7C + 0x22 * 3, 0x22));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x7C + 0x22 * 3);
        }
        public string Saying5
        {
            get => Util.TrimFromZero(Encoding.Unicode.GetString(Data, TrainerCard + 0x7C + 0x22 * 4, 0x22));
            set => Encoding.Unicode.GetBytes(value.PadRight(value.Length + 1, '\0')).CopyTo(Data, TrainerCard + 0x7C + 0x22 * 4);
        }
        public bool IsMegaEvolutionUnlocked
        {
            get => (Data[TrainerCard + 0x14A] & 0x01) != 0;
            set => Data[TrainerCard + 0x14A] = (byte)((Data[TrainerCard + 0x14A] & 0xFE) | (value ? 1 : 0)); // in battle
        }

        public int M
        {
            get => BitConverter.ToUInt16(Data, Trainer1 + 0x02);
            set
            {
                var val = BitConverter.GetBytes((ushort)value);
                val.CopyTo(Data, Trainer1 + 0x02);
                val.CopyTo(Data, Trainer1 + 0x02 + 0xF4);
            }
        }
        public float X
        {
            get => BitConverter.ToSingle(Data, Trainer1 + 0x10) / 18;
            set
            {
                var val = BitConverter.GetBytes(value * 18);
                val.CopyTo(Data, Trainer1 + 0x10);
                val.CopyTo(Data, Trainer1 + 0x10 + 0xF4);
            }
        }
        public float Z
        {
            get => BitConverter.ToSingle(Data, Trainer1 + 0x14);
            set
            {
                var val = BitConverter.GetBytes(value * 18);
                val.CopyTo(Data, Trainer1 + 0x14);
                val.CopyTo(Data, Trainer1 + 0x14 + 0xF4);
            }
        }
        public float Y
        {
            get => BitConverter.ToSingle(Data, Trainer1 + 0x18) / 18;
            set
            {
                var val = BitConverter.GetBytes(value * 18);
                val.CopyTo(Data, Trainer1 + 0x18);
                val.CopyTo(Data, Trainer1 + 0x18 + 0xF4);
            }
        }
        public int Style
        {
            get => Data[Trainer1 + 0x14D];
            set => Data[Trainer1 + 0x14D] = (byte)value;
        }
        public override uint Money
        {
            get => BitConverter.ToUInt32(Data, Trainer2 + 0x8);
            set => BitConverter.GetBytes(value).CopyTo(Data, Trainer2 + 0x8);
        }
        public int Badges
        {
            get => Data[Trainer2 + 0xC];
            set => Data[Trainer2 + 0xC] = (byte)value;
        }
        public int BP
        {
            get
            {
                int offset = Trainer2 + 0x3C;
                if (ORAS) offset -= 0xC; // 0x30
                return BitConverter.ToUInt16(Data, offset);
            }
            set
            {
                int offset = Trainer2 + 0x3C;
                if (ORAS) offset -= 0xC; // 0x30
                BitConverter.GetBytes((ushort)value).CopyTo(Data, offset);
            }
        }
        public int Vivillon
        {
            get
            {
                int offset = Trainer2 + 0x50;
                if (ORAS) offset -= 0xC; // 0x44
                return Data[offset];
            }
            set
            {
                int offset = Trainer2 + 0x50;
                if (ORAS) offset -= 0xC; // 0x44
                Data[offset] = (byte)value;
            }
        }

        public override int PlayedHours
        {
            get => BitConverter.ToUInt16(Data, PlayTime);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, PlayTime);
        }
        public override int PlayedMinutes
        {
            get => Data[PlayTime + 2];
            set => Data[PlayTime + 2] = (byte)value;
        }
        public override int PlayedSeconds
        {
            get => Data[PlayTime + 3];
            set => Data[PlayTime + 3] = (byte)value;
        }
        private uint LastSaved { get => BitConverter.ToUInt32(Data, PlayTime + 0x4); set => BitConverter.GetBytes(value).CopyTo(Data, PlayTime + 0x4); }
        private int LastSavedYear { get => (int)(LastSaved & 0xFFF); set => LastSaved = LastSaved & 0xFFFFF000 | (uint)value; }
        private int LastSavedMonth { get => (int)(LastSaved >> 12 & 0xF); set => LastSaved = LastSaved & 0xFFFF0FFF | ((uint)value & 0xF) << 12; }
        private int LastSavedDay { get => (int)(LastSaved >> 16 & 0x1F); set => LastSaved = LastSaved & 0xFFE0FFFF | ((uint)value & 0x1F) << 16; }
        private int LastSavedHour { get => (int)(LastSaved >> 21 & 0x1F); set => LastSaved = LastSaved & 0xFC1FFFFF | ((uint)value & 0x1F) << 21; }
        private int LastSavedMinute { get => (int)(LastSaved >> 26 & 0x3F); set => LastSaved = LastSaved & 0x03FFFFFF | ((uint)value & 0x3F) << 26; }
        private string LastSavedTime => $"{LastSavedYear:0000}{LastSavedMonth:00}{LastSavedDay:00}{LastSavedHour:00}{LastSavedMinute:00}";
        public DateTime? LastSavedDate
        {
            get => !Util.IsDateValid(LastSavedYear, LastSavedMonth, LastSavedDay)
                    ? (DateTime?)null
                    : new DateTime(LastSavedYear, LastSavedMonth, LastSavedDay, LastSavedHour, LastSavedMinute, 0);
            set
            {
                // Only update the properties if a value is provided.
                if (value.HasValue)
                {
                    var dt = value.Value;
                    LastSavedYear = dt.Year;
                    LastSavedMonth = dt.Month;
                    LastSavedDay = dt.Day;
                    LastSavedHour = dt.Hour;
                    LastSavedMinute = dt.Minute;
                }
                else // Clear the date.
                {
                    // If code tries to access MetDate again, null will be returned.
                    LastSavedYear = 0;
                    LastSavedMonth = 0;
                    LastSavedDay = 0;
                    LastSavedHour = 0;
                    LastSavedMinute = 0;
                }
            }
        }

        public int ResumeYear { get => BitConverter.ToInt32(Data, AdventureInfo + 0x4); set => BitConverter.GetBytes(value).CopyTo(Data, AdventureInfo + 0x4); }
        public int ResumeMonth { get => Data[AdventureInfo + 0x8]; set => Data[AdventureInfo + 0x8] = (byte)value; }
        public int ResumeDay { get => Data[AdventureInfo + 0x9]; set => Data[AdventureInfo + 0x9] = (byte)value; }
        public int ResumeHour { get => Data[AdventureInfo + 0xB]; set => Data[AdventureInfo + 0xB] = (byte)value; }
        public int ResumeMinute { get => Data[AdventureInfo + 0xC]; set => Data[AdventureInfo + 0xC] = (byte)value; }
        public int ResumeSeconds { get => Data[AdventureInfo + 0xD]; set => Data[AdventureInfo + 0xD] = (byte)value; }
        public override int SecondsToStart { get => BitConverter.ToInt32(Data, AdventureInfo + 0x18); set => BitConverter.GetBytes(value).CopyTo(Data, AdventureInfo + 0x18); }
        public override int SecondsToFame { get => BitConverter.ToInt32(Data, AdventureInfo + 0x20); set => BitConverter.GetBytes(value).CopyTo(Data, AdventureInfo + 0x20); }

        public uint GetPSSStat(int index) { return BitConverter.ToUInt32(Data, PSSStats + 4*index); }
        public void SetPSSStat(int index, uint value) { BitConverter.GetBytes(value).CopyTo(Data, PSSStats + 4*index); }
        public ushort GetMaisonStat(int index) { return BitConverter.ToUInt16(Data, MaisonStats + 2 * index); }
        public void SetMaisonStat(int index, ushort value) { BitConverter.GetBytes(value).CopyTo(Data, MaisonStats + 2*index); }
        public uint GetEncounterCount(int index) { return BitConverter.ToUInt16(Data, EncounterCount + 2*index); }
        public void SetEncounterCount(int index, ushort value) { BitConverter.GetBytes(value).CopyTo(Data, EncounterCount + 2*index); }
        
        // Daycare
        public override int DaycareSeedSize => 16;
        public override bool HasTwoDaycares => ORAS;
        public override int GetDaycareSlotOffset(int loc, int slot)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs < 0)
                return -1;
            return ofs + 8 + slot*(SIZE_STORED + 8);
        }
        public override uint? GetDaycareEXP(int loc, int slot)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                return BitConverter.ToUInt32(Data, ofs + (SIZE_STORED + 8)*slot + 4);
            return null;
        }
        public override bool? IsDaycareOccupied(int loc, int slot)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                return Data[ofs + (SIZE_STORED + 8) * slot] == 1;
            return null;
        }
        public override string GetDaycareRNGSeed(int loc)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs <= 0)
                return null;
            
            var data = Data.Skip(ofs + 0x1E8).Take(DaycareSeedSize/2).Reverse().ToArray();
            return BitConverter.ToString(data).Replace("-", "");
        }
        public override bool? IsDaycareHasEgg(int loc)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                return Data[ofs + 0x1E0] == 1;
            return null;
        }
        public override void SetDaycareEXP(int loc, int slot, uint EXP)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                BitConverter.GetBytes(EXP).CopyTo(Data, ofs + (SIZE_STORED + 8)*slot + 4);
        }
        public override void SetDaycareOccupied(int loc, int slot, bool occupied)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                Data[ofs + (SIZE_STORED + 8)*slot] = (byte) (occupied ? 1 : 0);
        }
        public override void SetDaycareRNGSeed(int loc, string seed)
        {
            if (loc != 0)
                return;
            if (Daycare < 0)
                return;
            if (seed == null)
                return;
            if (seed.Length > DaycareSeedSize)
                return;

            Enumerable.Range(0, seed.Length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(seed.Substring(x, 2), 16))
                 .Reverse().ToArray().CopyTo(Data, Daycare + 0x1E8);
        }
        public override void SetDaycareHasEgg(int loc, bool hasEgg)
        {
            int ofs = loc == 0 ? Daycare : Daycare2;
            if (ofs > -1)
                Data[ofs + 0x1E0] = (byte)(hasEgg ? 1 : 0);
        }

        public byte[] Puffs { get => GetData(Puff, 100); set => value.CopyTo(Data, Puff); }
        public int PuffCount { get => BitConverter.ToInt32(Data, Puff + 100); set => BitConverter.GetBytes(value).CopyTo(Data, Puff + 100); }

        public int[] SelectItems
        {
            // UP,RIGHT,DOWN,LEFT
            get
            {
                int[] list = new int[4];
                for (int i = 0; i < list.Length; i++)
                    list[i] = BitConverter.ToUInt16(Data, ItemInfo + 10 + 2 * i);
                return list;
            }
            set
            {
                if (value == null || value.Length > 4)
                    return;
                for (int i = 0; i < value.Length; i++)
                    BitConverter.GetBytes((ushort)value[i]).CopyTo(Data, ItemInfo + 10 + 2 * i);
            }
        }
        public int[] RecentItems
        {
            // Items recently interacted with (Give, Use)
            get
            {
                int[] list = new int[12];
                for (int i = 0; i < list.Length; i++)
                    list[i] = BitConverter.ToUInt16(Data, ItemInfo + 20 + 2 * i);
                return list;
            }
            set
            {
                if (value == null || value.Length > 12)
                    return;
                for (int i = 0; i < value.Length; i++)
                    BitConverter.GetBytes((ushort)value[i]).CopyTo(Data, ItemInfo + 20 + 2 * i);
            }
        }

        public override string JPEGTitle => JPEG < 0 ? null : Util.TrimFromZero(Encoding.Unicode.GetString(Data, JPEG, 0x1A));
        public override byte[] JPEGData => JPEG < 0 || Data[JPEG + 0x54] != 0xFF ? null : GetData(JPEG + 0x54, 0xE004);

        // Inventory
        public override InventoryPouch[] Inventory
        {
            get
            {
                ushort[] legalItems = ORAS ? Legal.Pouch_Items_AO : Legal.Pouch_Items_XY;
                ushort[] legalKey = ORAS ? Legal.Pouch_Key_AO : Legal.Pouch_Key_XY;
                ushort[] legalTMHM = ORAS ? Legal.Pouch_TMHM_AO : Legal.Pouch_TMHM_XY;
                ushort[] legalMedicine = ORAS ? Legal.Pouch_Medicine_AO : Legal.Pouch_Medicine_XY;
                InventoryPouch[] pouch =
                {
                    new InventoryPouch(InventoryType.Items, legalItems, 999, OFS_PouchHeldItem),
                    new InventoryPouch(InventoryType.KeyItems, legalKey, 1, OFS_PouchKeyItem),
                    new InventoryPouch(InventoryType.TMHMs, legalTMHM, 1, OFS_PouchTMHM),
                    new InventoryPouch(InventoryType.Medicine, legalMedicine, 999, OFS_PouchMedicine),
                    new InventoryPouch(InventoryType.Berries, Legal.Pouch_Berry_XY, 999, OFS_PouchBerry),
                };
                foreach (var p in pouch)
                    p.GetPouch(ref Data);
                return pouch;
            }
            set
            {
                foreach (var p in value)
                    p.SetPouch(ref Data);
            }
        }

        // Storage
        public override int CurrentBox { get => Data[LastViewedBox]; set => Data[LastViewedBox] = (byte)value; }
        public override int GetPartyOffset(int slot)
        {
            return Party + SIZE_PARTY * slot;
        }
        public override int GetBoxOffset(int box)
        {
            return Box + SIZE_STORED*box*30;
        }
        protected override int GetBoxWallpaperOffset(int box)
        {
            int ofs = PCBackgrounds > 0 && PCBackgrounds < Data.Length ? PCBackgrounds : -1;
            if (ofs > -1)
                return ofs + box;
            return ofs;
        }
        public override string GetBoxName(int box)
        {
            if (PCLayout < 0)
                return "B" + (box + 1);
            return Util.TrimFromZero(Encoding.Unicode.GetString(Data, PCLayout + 0x22*box, 0x22));
        }
        public override void SetBoxName(int box, string val)
        {
            Encoding.Unicode.GetBytes(val.PadRight(0x11, '\0')).CopyTo(Data, PCLayout + 0x22*box);
            Edited = true;
        }
        public override PKM GetPKM(byte[] data)
        {
            return new PK6(data);
        }
        protected override void SetPKM(PKM pkm)
        {
            PK6 pk6 = pkm as PK6;
            // Apply to this Save File
            int CT = pk6.CurrentHandler;
            DateTime Date = DateTime.Now;
            pk6.Trade(OT, TID, SID, Country, SubRegion, Gender, false, Date.Day, Date.Month, Date.Year);
            if (CT != pk6.CurrentHandler) // Logic updated Friendship
            {
                // Copy over the Friendship Value only under certain circumstances
                if (pk6.Moves.Contains(216)) // Return
                    pk6.CurrentFriendship = pk6.OppositeFriendship;
                else if (pk6.Moves.Contains(218)) // Frustration
                    pkm.CurrentFriendship = pk6.OppositeFriendship;
            }
            pkm.RefreshChecksum();
        }
        protected override void SetDex(PKM pkm)
        {
            if (PokeDex < 0)
                return;
            if (pkm.Species == 0)
                return;
            if (pkm.Species > MaxSpeciesID)
                return;
            if (Version == GameVersion.Unknown)
                return;

            const int brSize = 0x60;
            int bit = pkm.Species - 1;
            int lang = pkm.Language - 1; if (lang > 5) lang--; // 0-6 language vals
            int origin = pkm.Version;
            int gender = pkm.Gender % 2; // genderless -> male
            int shiny = pkm.IsShiny ? 1 : 0;
            int shiftoff = brSize*(1 + gender + 2*shiny); // after the Owned region
            int bd = bit >> 3; // div8
            int bm = bit & 7; // mod8
            byte mask = (byte)(1 << bm);
            int ofs = PokeDex + 0x8 + bd;
            
            // Owned quality flag
            if (origin < 0x18 && bit < 649 && !ORAS) // Species: 1-649 for X/Y, and not for ORAS; Set the Foreign Owned Flag
                Data[ofs + 0x644] |= mask;
            else if (origin >= 0x18 || ORAS) // Set Native Owned Flag (should always happen)
                Data[ofs + brSize * 0] |= mask;

            // Set the [Species/Gender/Shiny] Seen Flag
            Data[ofs + shiftoff] |= mask;

            // Set the Display flag if none are set
            bool Displayed = false;
            Displayed |= (Data[ofs + brSize * 5] & mask) != 0;
            Displayed |= (Data[ofs + brSize * 6] & mask) != 0;
            Displayed |= (Data[ofs + brSize * 7] & mask) != 0;
            Displayed |= (Data[ofs + brSize * 8] & mask) != 0;
            if (!Displayed) // offset is already biased by brSize, reuse shiftoff but for the display flags.
                Data[ofs + brSize * 4 + shiftoff] |= mask;

            // Set the Language
            if (lang < 0) lang = 1;
            Data[PokeDexLanguageFlags + (bit * 7 + lang) / 8] |= (byte)(1 << ((bit * 7 + lang) % 8));

            // Set DexNav count (only if not encountered previously)
            if (ORAS && GetEncounterCount(pkm.Species - 1) == 0)
                SetEncounterCount(pkm.Species - 1, 1);

            // Set Form flags
            int fc = Personal[pkm.Species].FormeCount;
            int f = ORAS ? SaveUtil.GetDexFormIndexORAS(pkm.Species, fc) : SaveUtil.GetDexFormIndexXY(pkm.Species, fc);
            if (f < 0) return;

            int FormLen = ORAS ? 0x26 : 0x18;
            int FormDex = PokeDex + 0x8 + brSize*9;
            bit = f + pkm.AltForm;

            // Set Form Seen Flag
            Data[FormDex + FormLen*shiny + bit/8] |= (byte)(1 << (bit%8));

            // Set Displayed Flag if necessary, check all flags
            for (int i = 0; i < fc; i++)
            {
                bit = f + i;
                if ((Data[FormDex + FormLen*2 + bit/8] & (byte) (1 << (bit%8))) != 0) // Nonshiny
                    return; // already set
                if ((Data[FormDex + FormLen*3 + bit/8] & (byte) (1 << (bit%8))) != 0) // Shiny
                    return; // already set
            }
            bit = f + pkm.AltForm;
            Data[FormDex + FormLen * (2 + shiny) + bit / 8] |= (byte)(1 << (bit % 8));
        }
        protected override void SetPartyValues(PKM pkm, bool isParty)
        {
            uint duration = 0;
            if (isParty && pkm.AltForm != 0)
                switch (pkm.Species)
                {
                    case 676:
                        duration = 5;
                        break;
                    case 720: // Hoopa
                        duration = 3;
                        break;
                }

            ((PK6)pkm).FormDuration = duration;
        }

        public override bool GetCaught(int species)
        {
            int bit = species - 1;
            int bd = bit >> 3; // div8
            int bm = bit & 7; // mod8
            int ofs = PokeDex // Raw Offset
                      + 0x08; // Magic + Flags

            if ((1 << bm & Data[ofs + bd]) != 0)
                return true; // Owned Native

            if (ORAS || bit >= 649) // no Foreign flag
                return false;
            return (1 << bm & Data[ofs + bd + 0x644]) != 0;
        }

        public override bool GetSeen(int species)
        {
            const int brSize = 0x60;

            int bit = species - 1;
            int bd = bit >> 3; // div8
            int bm = bit & 7; // mod8
            byte mask = (byte)(1 << bm);
            int ofs = PokeDex // Raw Offset
                      + 0x08; // Magic + Flags

            for (int i = 1; i <= 4; i++) // check all 4 seen flags (gender/shiny)
                if ((Data[ofs + bd + i * brSize] & mask) != 0)
                    return true;
            return false;
        }
        public override byte[] DecryptPKM(byte[] data)
        {
            return PKX.DecryptArray(data);
        }
        public override int PartyCount
        {
            get => Data[Party + 6 * SIZE_PARTY];
            protected set => Data[Party + 6 * SIZE_PARTY] = (byte)value;
        }
        public override bool BattleBoxLocked
        {
            get => Data[BattleBox + 6 * SIZE_STORED] != 0;
            set => Data[BattleBox + 6 * SIZE_STORED] = (byte)(value ? 1 : 0);
        }
        public override int BoxesUnlocked { get => Data[PCFlags + 1] - 1; set => Data[PCFlags + 1] = (byte)(value + 1); }
        public override byte[] BoxFlags
        {
            get => new[] { Data[PCFlags], Data[PCFlags + 2] };
            set
            {
                if (value.Length != 2) return;
                Data[PCFlags] = value[0];
                Data[PCFlags + 2] = value[1];
            }
        }

        // Mystery Gift
        protected override bool[] MysteryGiftReceivedFlags
        {
            get
            {
                if (WondercardData < 0 || WondercardFlags < 0)
                    return null;

                bool[] r = new bool[(WondercardData-WondercardFlags)*8];
                for (int i = 0; i < r.Length; i++)
                    r[i] = (Data[WondercardFlags + (i>>3)] >> (i&7) & 0x1) == 1;
                return r;
            }
            set
            {
                if (WondercardData < 0 || WondercardFlags < 0)
                    return;
                if ((WondercardData - WondercardFlags)*8 != value?.Length)
                    return;

                byte[] data = new byte[value.Length/8];
                for (int i = 0; i < value.Length; i++)
                    if (value[i])
                        data[i>>3] |= (byte)(1 << (i&7));

                data.CopyTo(Data, WondercardFlags);
                Edited = true;
            }
        }
        protected override MysteryGift[] MysteryGiftCards
        {
            get
            {
                if (WondercardData < 0)
                    return null;
                MysteryGift[] cards = new MysteryGift[GiftCountMax];
                for (int i = 0; i < cards.Length; i++)
                    cards[i] = GetWC6(i);

                return cards;
            }
            set
            {
                if (value == null)
                    return;
                if (value.Length > GiftCountMax)
                    Array.Resize(ref value, GiftCountMax);
                
                for (int i = 0; i < value.Length; i++)
                    SetWC6(value[i], i);
                for (int i = value.Length; i < GiftCountMax; i++)
                    SetWC6(new WC6(), i);
            }
        }

        public byte[] LinkBlock
        {
            get
            {
                if (LinkInfo < 0)
                    return null;
                return GetData(LinkInfo, 0xC48);
            }
            set
            {
                if (LinkInfo < 0)
                    return;
                if (value.Length != 0xC48)
                    return;
                value.CopyTo(Data, LinkInfo);
            }
        }

        private MysteryGift GetWC6(int index)
        {
            if (WondercardData < 0)
                return null;
            if (index < 0 || index > GiftCountMax)
                return null;

            return new WC6(GetData(WondercardData + index * WC6.Size, WC6.Size));
        }
        private void SetWC6(MysteryGift wc6, int index)
        {
            if (WondercardData < 0)
                return;
            if (index < 0 || index > GiftCountMax)
                return;

            wc6.Data.CopyTo(Data, WondercardData + index * WC6.Size);

            Edited = true;
        }

        // Gym History
        private ushort[][] GymTeams
        {
            get
            {
                if (SUBE < 0 || ORASDEMO)
                    return null; // no gym data

                const int teamsize = 2 * 6; // 2byte/species, 6species/team
                const int size = teamsize * 8; // 8 gyms
                int ofs = SUBE - size - 4;

                var data = GetData(ofs, size);
                ushort[][] teams = new ushort[8][];
                for (int i = 0; i < teams.Length; i++)
                    Buffer.BlockCopy(data, teamsize * i, teams[i] = new ushort[6], 0, teamsize);
                return teams;
            }
            set
            {
                if (SUBE < 0 || ORASDEMO)
                    return; // no gym data

                const int teamsize = 2 * 6; // 2byte/species, 6species/team
                const int size = teamsize * 8; // 8 gyms
                int ofs = SUBE - size - 4;

                byte[] data = new byte[size];
                for (int i = 0; i < value.Length; i++)
                    Buffer.BlockCopy(value[i], 0, data, teamsize * i, teamsize);
                SetData(data, ofs);
            }
        }

        // Writeback Validity
        public override string MiscSaveChecks()
        {
            string r = "";
            byte[] FFFF = Enumerable.Repeat((byte)0xFF, 0x200).ToArray();
            for (int i = 0; i < Data.Length / 0x200; i++)
            {
                if (!FFFF.SequenceEqual(Data.Skip(i * 0x200).Take(0x200))) continue;
                r = $"0x200 chunk @ 0x{i*0x200:X5} is FF'd."
                    + Environment.NewLine + "Cyber will screw up (as of August 31st 2014)." + Environment.NewLine + Environment.NewLine;

                // Check to see if it is in the Pokedex
                if (i * 0x200 > PokeDex && i * 0x200 < PokeDex + 0x900)
                {
                    r += "Problem lies in the Pokedex. ";
                    if (i * 0x200 == PokeDex + 0x400)
                        r += "Remove a language flag for a species < 585, ie Petilil";
                }
                break;
            }
            return r;
        }
        public override string MiscSaveInfo()
        {
            return string.Join(Environment.NewLine,
                Blocks.Select(b => $"{b.ID:00}: {b.Offset:X5}-{b.Offset + b.Length:X5}, {b.Length:X5}"));
        }

        public override string GetString(int Offset, int Count) => StringConverter.GetString6(Data, Offset, Count);
        public override byte[] SetString(string value, int maxLength, int PadToSize = 0, ushort PadWith = 0)
        {
            if (PadToSize == 0)
                PadToSize = maxLength + 1;
            return StringConverter.SetString6(value, maxLength, PadToSize, PadWith);
        }
    }
}

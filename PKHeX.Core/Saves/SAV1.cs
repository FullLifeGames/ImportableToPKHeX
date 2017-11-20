﻿using System;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary>
    /// Generation 1 <see cref="SaveFile"/> object.
    /// </summary>
    public sealed class SAV1 : SaveFile
    {
        public override string BAKName => $"{FileName} [{OT} ({Version}) - {PlayTimeString}].bak";
        public override string Filter => "SAV File|*.sav|All Files|*.*";
        public override string Extension => ".sav";
        public override string[] PKMExtensions => PKM.Extensions.Where(f =>
        {
            int gen = f.Last() - 0x30;
            return 1 <= gen && gen <= 2;
        }).ToArray();

        public SAV1(byte[] data = null, GameVersion versionOverride = GameVersion.Any)
        {
            Data = data == null ? new byte[SaveUtil.SIZE_G1RAW] : (byte[])data.Clone();
            BAK = (byte[])Data.Clone();
            Exportable = !Data.All(z => z == 0);

            if (data == null)
                Version = GameVersion.RBY;
            else if (versionOverride != GameVersion.Any)
                Version = versionOverride;
            else Version = SaveUtil.GetIsG1SAV(Data);
            if (Version == GameVersion.Invalid)
                return;
            if (Starter != 0)
                Version = Yellow ? GameVersion.YW : GameVersion.RB;

            Box = Data.Length;
            Array.Resize(ref Data, Data.Length + SIZE_RESERVED);
            Party = GetPartyOffset(0);

            Japanese = SaveUtil.GetIsG1SAVJ(Data);
            Personal = PersonalTable.Y;

            // Stash boxes after the save file's end.
            byte[] TempBox = new byte[SIZE_STOREDBOX];
            for (int i = 0; i < BoxCount; i++)
            {
                if (i < BoxCount / 2)
                    Array.Copy(Data, 0x4000 + i * TempBox.Length, TempBox, 0, TempBox.Length);
                else
                    Array.Copy(Data, 0x6000 + (i - BoxCount / 2) * TempBox.Length, TempBox, 0, TempBox.Length);
                PokemonList1 PL1 = new PokemonList1(TempBox, Japanese ? PokemonList1.CapacityType.StoredJP : PokemonList1.CapacityType.Stored, Japanese);
                for (int j = 0; j < PL1.Pokemon.Length; j++)
                {
                    if (j < PL1.Count)
                    {
                        byte[] pkDat = new PokemonList1(PL1[j]).GetBytes();
                        pkDat.CopyTo(Data, Data.Length - SIZE_RESERVED + i * SIZE_BOX + j * SIZE_STORED);
                    }
                    else
                    {
                        byte[] pkDat = new byte[PokemonList1.GetDataLength(PokemonList1.CapacityType.Single, Japanese)];
                        pkDat.CopyTo(Data, Data.Length - SIZE_RESERVED + i * SIZE_BOX + j * SIZE_STORED);
                    }
                }
            }

            Array.Copy(Data, Japanese ? 0x302D : 0x30C0, TempBox, 0, TempBox.Length);
            PokemonList1 curBoxPL = new PokemonList1(TempBox, Japanese ? PokemonList1.CapacityType.StoredJP : PokemonList1.CapacityType.Stored, Japanese);
            for (int i = 0; i < curBoxPL.Pokemon.Length; i++)
            {
                if (i < curBoxPL.Count)
                {
                    byte[] pkDat = new PokemonList1(curBoxPL[i]).GetBytes();
                    pkDat.CopyTo(Data, Data.Length - SIZE_RESERVED + CurrentBox * SIZE_BOX + i * SIZE_STORED);
                }
                else
                {
                    byte[] pkDat = new byte[PokemonList1.GetDataLength(PokemonList1.CapacityType.Single, Japanese)];
                    pkDat.CopyTo(Data, Data.Length - SIZE_RESERVED + CurrentBox * SIZE_BOX + i * SIZE_STORED);
                }
            }

            byte[] TempParty = new byte[PokemonList1.GetDataLength(PokemonList1.CapacityType.Party, Japanese)];
            Array.Copy(Data, Japanese ? 0x2ED5 : 0x2F2C, TempParty, 0, TempParty.Length);
            PokemonList1 partyList = new PokemonList1(TempParty, PokemonList1.CapacityType.Party, Japanese);
            for (int i = 0; i < partyList.Pokemon.Length; i++)
            {
                if (i < partyList.Count)
                {
                    byte[] pkDat = new PokemonList1(partyList[i]).GetBytes();
                    pkDat.CopyTo(Data, GetPartyOffset(i));
                }
                else
                {
                    byte[] pkDat = new byte[PokemonList1.GetDataLength(PokemonList1.CapacityType.Single, Japanese)];
                    pkDat.CopyTo(Data, GetPartyOffset(i));
                }
            }

            byte[] rawDC = new byte[0x38];
            Array.Copy(Data, Japanese ? 0x2CA7 : 0x2CF4, rawDC, 0, rawDC.Length);
            byte[] TempDaycare = new byte[PokemonList1.GetDataLength(PokemonList1.CapacityType.Single, Japanese)];
            TempDaycare[0] = rawDC[0];
            Array.Copy(rawDC, 1, TempDaycare, 2 + 1 + PKX.SIZE_1PARTY + StringLength, StringLength);
            Array.Copy(rawDC, 1 + StringLength, TempDaycare, 2 + 1 + PKX.SIZE_1PARTY, StringLength);
            Array.Copy(rawDC, 1 + 2 * StringLength, TempDaycare, 2 + 1, PKX.SIZE_1STORED);
            PokemonList1 daycareList = new PokemonList1(TempDaycare, PokemonList1.CapacityType.Single, Japanese);
            daycareList.GetBytes().CopyTo(Data, GetPartyOffset(7));
            Daycare = GetPartyOffset(7);

            EventFlag = Japanese ? 0x29E9 : 0x29F3;
            ObjectSpawnFlags = Japanese ? 0x2848 : 0x2852; // 2 bytes after Coin

            // Enable Pokedex editing
            PokeDex = 0;

            if (!Exportable)
                ClearBoxes();
        }

        // Event Flags
        protected override int EventFlagMax => EventFlag > 0 ? 0xA00 : int.MinValue; // 320 * 8
        protected override int EventConstMax => 0;
        private readonly int ObjectSpawnFlags;

        private const int SIZE_RESERVED = 0x8000; // unpacked box data

        protected override byte[] Write(bool DSV)
        {
            for (int i = 0; i < BoxCount; i++)
            {
                PokemonList1 boxPL = new PokemonList1(Japanese ? PokemonList1.CapacityType.StoredJP : PokemonList1.CapacityType.Stored, Japanese);
                int slot = 0;
                for (int j = 0; j < boxPL.Pokemon.Length; j++)
                {
                    PK1 boxPK = (PK1) GetPKM(GetData(GetBoxOffset(i) + j*SIZE_STORED, SIZE_STORED));
                    if (boxPK.Species > 0)
                        boxPL[slot++] = boxPK;
                }
                if (i < BoxCount / 2)
                    boxPL.GetBytes().CopyTo(Data, 0x4000 + i * SIZE_STOREDBOX);
                else
                    boxPL.GetBytes().CopyTo(Data, 0x6000 + (i - BoxCount / 2) * SIZE_STOREDBOX);
                if (i == CurrentBox)
                    boxPL.GetBytes().CopyTo(Data, Japanese ? 0x302D : 0x30C0);
            }

            PokemonList1 partyPL = new PokemonList1(PokemonList1.CapacityType.Party, Japanese);
            int pSlot = 0;
            for (int i = 0; i < 6; i++)
            {
                PK1 partyPK = (PK1)GetPKM(GetData(GetPartyOffset(i), SIZE_STORED));
                if (partyPK.Species > 0)
                    partyPL[pSlot++] = partyPK;
            }
            partyPL.GetBytes().CopyTo(Data, Japanese ? 0x2ED5 : 0x2F2C);

            // Daycare is read-only, but in case it ever becomes editable, copy it back in.
            byte[] rawDC = GetData(GetDaycareSlotOffset(loc: 0, slot: 0), SIZE_STORED);
            byte[] dc = new byte[1 + 2*StringLength + PKX.SIZE_1STORED];
            dc[0] = rawDC[0];
            Array.Copy(rawDC, 2 + 1 + PKX.SIZE_1PARTY + StringLength, dc, 1, StringLength);
            Array.Copy(rawDC, 2 + 1 + PKX.SIZE_1PARTY, dc, 1 + StringLength, StringLength);
            Array.Copy(rawDC, 2 + 1, dc, 1 + 2*StringLength, PKX.SIZE_1STORED);
            dc.CopyTo(Data, Japanese ? 0x2CA7 : 0x2CF4);

            SetChecksums();
            byte[] outData = new byte[Data.Length - SIZE_RESERVED];
            Array.Copy(Data, outData, outData.Length);
            return outData;
        }


        // Configuration
        public override SaveFile Clone() { return new SAV1(Write(DSV: false)); }

        public override int SIZE_STORED => Japanese ? PKX.SIZE_1JLIST : PKX.SIZE_1ULIST;
        protected override int SIZE_PARTY => Japanese ? PKX.SIZE_1JLIST : PKX.SIZE_1ULIST;
        private int SIZE_BOX => BoxSlotCount*SIZE_STORED;
        private int SIZE_STOREDBOX => PokemonList1.GetDataLength(Japanese ? PokemonList1.CapacityType.StoredJP : PokemonList1.CapacityType.Stored, Japanese);

        public override PKM BlankPKM => new PK1(null, null, Japanese);
        public override Type PKMType => typeof(PK1);

        public override int MaxMoveID => Legal.MaxMoveID_1;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_1;
        public override int MaxAbilityID => Legal.MaxAbilityID_1;
        public override int MaxItemID => Legal.MaxItemID_1;
        public override int MaxBallID => 0; // unused
        public override int MaxGameID => 99; // unused
        public override int MaxMoney => 999999;
        public override int MaxCoins => 9999;

        public override int BoxCount => Japanese ? 8 : 12;
        public override int MaxEV => 65535;
        public override int MaxIV => 15;
        public override int Generation => 1;
        protected override int GiftCountMax => 0;
        public override int OTLength => Japanese ? 5 : 7;
        public override int NickLength => Japanese ? 5 : 10;
        public override int BoxSlotCount => Japanese ? 30 : 20;

        public override bool HasParty => true;
        private int StringLength => Japanese ? PK1.STRLEN_J : PK1.STRLEN_U;

        // Checksums
        protected override void SetChecksums()
        {
            int CHECKSUM_OFS = Japanese ? 0x3594 : 0x3523;
            Data[CHECKSUM_OFS] = 0;
            uint chksum = 0;
            for (int i = 0x2598; i < CHECKSUM_OFS; i++)
            {
                chksum += Data[i];
            }

            chksum = ~chksum;
            chksum &= 0xFF;

            Data[CHECKSUM_OFS] = (byte)chksum;
        }
        public override bool ChecksumsValid
        {
            get
            {
                int CHECKSUM_OFS = Japanese ? 0x3594 : 0x3523;
                byte temp = Data[CHECKSUM_OFS]; // cache current chk
                SetChecksums(); // chksum is recalculated (after being set to 0 to perform check)
                byte chk = Data[CHECKSUM_OFS]; // correct checksum
                Data[CHECKSUM_OFS] = temp; // restore old chk
                return temp == chk;
            }
        }
        public override string ChecksumInfo => ChecksumsValid ? "Checksum valid." : "Checksum invalid";

        // Trainer Info
        public override GameVersion Version { get; protected set; }

        public override string OT
        {
            get => GetString(0x2598, OTLength);
            set => SetString(value, OTLength).CopyTo(Data, 0x2598);
        }
        public override int Gender
        {
            get => 0;
            set { }
        }
        public override ushort TID
        {
            get => BigEndian.ToUInt16(Data, Japanese ? 0x25FB : 0x2605);
            set => BigEndian.GetBytes(value).CopyTo(Data, Japanese ? 0x25FB : 0x2605);
        }
        public override ushort SID
        {
            get => 0;
            set { }
        }

        public bool Yellow => Starter == 0x54; // Pikachu
        public int Starter => Data[Japanese ? 0x29B9 : 0x29C3];
        public byte PikaFriendship
        {
            get => Data[Japanese ? 0x2712 : 0x271C];
            set => Data[Japanese ? 0x2712 : 0x271C] = value;
        }
        public override int PlayedHours
        {
            get => BitConverter.ToUInt16(Data, Japanese ? 0x2CA0 : 0x2CED);
            set => BitConverter.GetBytes((ushort)value).CopyTo(Data, Japanese ? 0x2CA0 : 0x2CED);
        }
        public override int PlayedMinutes
        {
            get => Data[Japanese ? 0x2CA2 : 0x2CEF];
            set => Data[Japanese ? 0x2CA2 : 0x2CEF] = (byte)value;
        }
        public override int PlayedSeconds
        {
            get => Data[Japanese ? 0x2CA3 : 0x2CF0];
            set => Data[Japanese ? 0x2CA3 : 0x2CF0] = (byte)value;
        }

        public int Badges
        {
            get => Data[Japanese ? 0x25F8 : 0x2602];
            set { if (value < 0) return; Data[Japanese ? 0x25F8 : 0x2602] = (byte)value; }
        }
        private byte Options
        {
            get => Data[Japanese ? 0x25F7 : 0x2601];
            set => Data[Japanese ? 0x25F7 : 0x2601] = value;
        }
        public bool BattleEffects
        {
            get => (Options & 0x80) == 0;
            set => Options = (byte)((Options & 0x7F) | (value ? 0 : 0x80));
        }
        public bool BattleStyleSwitch
        {
            get => (Options & 0x40) == 0;
            set => Options = (byte)((Options & 0xBF) | (value ? 0 : 0x40));
        }
        public int Sound
        {
            get => (Options & 0x30) >> 4;
            set
            {
                var new_sound = value;
                if (new_sound > 3)
                    new_sound = 3;
                if (new_sound < 0)
                    new_sound = 0;
                Options = (byte)((Options & 0xCF) | (new_sound << 4));
            }
        }
        public int TextSpeed
        {
            get => Options & 0x7;
            set
            {
                var new_speed = value;
                if (new_speed > 7)
                    new_speed = 7;
                if (new_speed < 0)
                    new_speed = 0;
                Options = (byte)((Options & 0xF8) | new_speed);
            }
        }
        public override uint Money
        {
            get => (uint)BigEndian.BCDToInt32(Data, Japanese ? 0x25EE : 0x25F3, 3);
            set
            {
                value = (uint)Math.Min(value, MaxMoney);
                BigEndian.Int32ToBCD((int)value, 3).CopyTo(Data, Japanese ? 0x25EE : 0x25F3);
            }
        }
        public uint Coin
        {
            get => (uint)BigEndian.BCDToInt32(Data, Japanese ? 0x2846 : 0x2850, 2);
            set
            {
                value = (ushort)Math.Min(value, MaxCoins);
                BigEndian.Int32ToBCD((int)value, 2).CopyTo(Data, Japanese ? 0x2846 : 0x2850);
            }
        }

        private readonly ushort[] LegalItems = Legal.Pouch_Items_RBY;
        public override InventoryPouch[] Inventory
        {
            get
            {
                ushort[] legalItems = LegalItems;
                InventoryPouch[] pouch =
                {
                    new InventoryPouch(InventoryType.Items, legalItems, 99, Japanese ? 0x25C4 : 0x25C9, 20),
                    new InventoryPouch(InventoryType.PCItems, legalItems, 99, Japanese ? 0x27DC : 0x27E6, 50)
                };
                foreach (var p in pouch)
                {
                    p.GetPouchG1(ref Data);
                }
                return pouch;
            }
            set
            {
                foreach (var p in value)
                {
                    int ofs = 0;
                    for (int i = 0; i < p.Count; i++)
                    {
                        while (p.Items[ofs].Count == 0)
                            ofs++;
                        p.Items[i] = p.Items[ofs++];
                    }
                    while (ofs < p.Items.Length)
                        p.Items[ofs++] = new InventoryItem { Count = 0, Index = 0 };
                    p.SetPouchG1(ref Data);
                }
            }
        }
        public override int GetDaycareSlotOffset(int loc, int slot)
        {
            return Daycare;
        }
        public override uint? GetDaycareEXP(int loc, int slot)
        {
            return null;
        }
        public override bool? IsDaycareOccupied(int loc, int slot)
        {
            return null;
        }
        public override void SetDaycareEXP(int loc, int slot, uint EXP)
        {

        }
        public override void SetDaycareOccupied(int loc, int slot, bool occupied)
        {

        }

        // Storage
        public override int PartyCount
        {
            get => Data[Japanese ? 0x2ED5 : 0x2F2C];
            protected set => Data[Japanese ? 0x2ED5 : 0x2F2C] = (byte)value;
        }
        public override int GetBoxOffset(int box)
        {
            return Data.Length - SIZE_RESERVED + box * SIZE_BOX;
        }
        public override int GetPartyOffset(int slot)
        {
            return Data.Length - SIZE_RESERVED + BoxCount * SIZE_BOX + slot * SIZE_STORED;
        }
        public override int CurrentBox
        {
            get => Data[Japanese ? 0x2842 : 0x284C] & 0x7F;
            set => Data[Japanese ? 0x2842 : 0x284C] = (byte)((Data[Japanese ? 0x2842 : 0x284C] & 0x80) | (value & 0x7F));
        }
        public override string GetBoxName(int box)
        {
            return $"BOX {box + 1}";
        }
        public override void SetBoxName(int box, string value)
        {
            // Don't allow for custom box names
        }

        public override PKM GetPKM(byte[] data)
        {
            if (data.Length == SIZE_STORED)
                return new PokemonList1(data, PokemonList1.CapacityType.Single, Japanese)[0];
            return new PK1(data);
        }
        public override byte[] DecryptPKM(byte[] data)
        {
            return data;
        }

        // Pokédex
        private int PokedexSeenOffset => Japanese ? 0x25B1 : 0x25B6;
        private int PokedexCaughtOffset => Japanese ? 0x259E : 0x25A3;
        protected override void SetDex(PKM pkm)
        {
            int species = pkm.Species;
            if (!CanSetDex(species))
                return;

            SetCaught(pkm.Species, true);
            SetSeen(pkm.Species, true);
        }
        private bool CanSetDex(int species)
        {
            if (species <= 0)
                return false;
            if (species > MaxSpeciesID)
                return false;
            if (Version == GameVersion.Unknown)
                return false;
            return true;
        }
        public override void SetSeen(int species, bool seen)
        {
            int bit = species - 1;
            int ofs = bit >> 3;
            SetFlag(PokedexSeenOffset + ofs, bit & 7, seen);
        }
        public override void SetCaught(int species, bool caught)
        {
            int bit = species - 1;
            int ofs = bit >> 3;
            SetFlag(PokedexCaughtOffset + ofs, bit & 7, caught);
        }
        public override bool GetSeen(int species)
        {
            int bit = species - 1;
            int ofs = bit >> 3;
            return GetFlag(PokedexSeenOffset + ofs, bit & 7);
        }
        public override bool GetCaught(int species)
        {
            int bit = species - 1;
            int ofs = bit >> 3;
            return GetFlag(PokedexCaughtOffset + ofs, bit & 7);
        }

        private const int SpawnFlagCount = 0xF0;
        public bool[] EventSpawnFlags
        {
            get
            {
                // RB uses 0xE4 (0xE8) flags, Yellow uses 0xF0 flags. Just grab 0xF0
                bool[] data = new bool[SpawnFlagCount];
                for (int i = 0; i < data.Length; i++)
                    data[i] = GetFlag(ObjectSpawnFlags + i >> 3, i & 7);
                return data;
            }
            set
            {
                if (value?.Length != SpawnFlagCount)
                    return;
                for (int i = 0; i < value.Length; i++)
                    SetFlag(ObjectSpawnFlags + i >> 3, i & 7, value[i]);
            }
        }

        public override string GetString(int Offset, int Count) => StringConverter.GetString1(Data, Offset, Count, Japanese);
        public override byte[] SetString(string value, int maxLength, int PadToSize = 0, ushort PadWith = 0)
        {
            if (PadToSize == 0)
                PadToSize = maxLength + 1;
            return StringConverter.SetString1(value, maxLength, Japanese, PadToSize, PadWith);
        }
    }
}

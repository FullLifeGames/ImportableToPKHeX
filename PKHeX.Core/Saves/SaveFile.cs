﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary>
    /// Base Class for Save Files
    /// </summary>
    public abstract class SaveFile
    {
        public static bool SetUpdateDex { protected get; set; } = true;
        public static bool SetUpdatePKM { protected get; set; } = true;

        // General Object Properties
        public byte[] Data;
        public bool Edited;
        public string FileName, FilePath;
        public abstract string BAKName { get; }
        public byte[] BAK { get; protected set; }
        public bool Exportable { get; protected set; }
        public abstract SaveFile Clone();
        public abstract string Filter { get; }
        public byte[] Footer { protected get; set; } = new byte[0]; // .dsv
        public byte[] Header { protected get; set; } = new byte[0]; // .gci
        public bool Japanese { get; protected set; }
        protected string PlayTimeString => $"{PlayedHours}ː{PlayedMinutes:00}ː{PlayedSeconds:00}"; // not :
        public virtual bool IndeterminateGame => false;
        public virtual bool IndeterminateSubVersion => false;
        public abstract string Extension { get; }
        public virtual string[] PKMExtensions => PKM.Extensions.Where(f => 
        {
            int gen = f.Last() - 0x30;
            return 3 <= gen && gen <= Generation;
        }).ToArray();

        // General PKM Properties
        public abstract Type PKMType { get; }
        public abstract PKM GetPKM(byte[] data);
        public abstract PKM BlankPKM { get; }
        public abstract byte[] DecryptPKM(byte[] data);
        public abstract int SIZE_STORED { get; }
        protected abstract int SIZE_PARTY { get; }
        public abstract int MaxEV { get; }
        public virtual int MaxIV => 31;
        public ushort[] HeldItems { get; protected set; }

        // General SAV Properties
        public virtual byte[] Write(bool DSV, bool GCI)
        {
            return Write(DSV);
        }
        protected virtual byte[] Write(bool DSV)
        {
            SetChecksums();
            if (Footer.Length > 0 && DSV)
                return Data.Concat(Footer).ToArray();
            if (Header.Length > 0)
                return Header.Concat(Data).ToArray();
            return Data;
        }
        public virtual string MiscSaveChecks() => string.Empty;
        public virtual string MiscSaveInfo() => string.Empty;
        public virtual GameVersion Version { get; protected set; }
        public abstract bool ChecksumsValid { get; }
        public abstract string ChecksumInfo { get; }
        public abstract int Generation { get; }
        public PersonalTable Personal { get; set; }

        public bool USUM => Data.Length == SaveUtil.SIZE_G7USUM;
        public bool SM => Data.Length == SaveUtil.SIZE_G7SM;
        public bool ORASDEMO => Data.Length == SaveUtil.SIZE_G6ORASDEMO;
        public bool ORAS => Data.Length == SaveUtil.SIZE_G6ORAS;
        public bool XY => Data.Length == SaveUtil.SIZE_G6XY;
        public bool B2W2 => Version == GameVersion.B2W2;
        public bool BW => Version == GameVersion.BW;
        public bool HGSS => Version == GameVersion.HGSS;
        public bool Pt => Version == GameVersion.Pt;
        public bool DP => Version == GameVersion.DP;
        public bool E => Version == GameVersion.E;
        public bool FRLG => Version == GameVersion.FRLG;
        public bool RS => Version == GameVersion.RS;
        public bool GSC => Version == GameVersion.GS || Version == GameVersion.C;
        public bool RBY => Version == GameVersion.RBY;
        public bool GameCube => new[] { GameVersion.COLO, GameVersion.XD, GameVersion.RSBOX }.Contains(Version);

        public abstract int MaxMoveID { get; }
        public abstract int MaxSpeciesID { get; }
        public abstract int MaxAbilityID { get; }
        public abstract int MaxItemID { get; }
        public abstract int MaxBallID { get; }
        public abstract int MaxGameID { get; }

        // Flags
        public bool HasWondercards => WondercardData > -1;
        public bool HasSuperTrain => SuperTrain > -1;
        public bool HasBerryField => BerryField > -1;
        public bool HasHoF => HoF > -1;
        public bool HasSecretBase => SecretBase > -1;
        public bool HasPuff => Puff > -1;
        public bool HasPSS => PSS > -1;
        public bool HasOPower => OPower > -1;
        public bool HasJPEG => JPEGData != null;
        public bool HasBox => Box > -1;
        public virtual bool HasParty => Party > -1;
        public bool HasBattleBox => BattleBox > -1;
        public bool HasFused => Fused > -1;
        public bool HasGTS => GTS > -1;
        public bool HasDaycare => Daycare > -1;
        public virtual bool HasPokeDex => PokeDex > -1;
        public virtual bool HasBoxWallpapers => GetBoxWallpaperOffset(0) > -1;
        public virtual bool HasSUBE => SUBE > -1 && !ORAS;
        public virtual bool HasGeolocation => false;
        public bool HasPokeBlock => ORAS && !ORASDEMO;
        public bool HasEvents => EventFlags != null;
        public bool HasLink => ORAS && !ORASDEMO || XY;

        // Counts
        protected virtual int GiftCountMax { get; } = int.MinValue;
        protected virtual int GiftFlagMax { get; } = 0x800;
        protected virtual int EventFlagMax { get; } = int.MinValue;
        protected virtual int EventConstMax { get; } = int.MinValue;
        public virtual int DaycareSeedSize { get; } = 0;
        public abstract int OTLength { get; }
        public abstract int NickLength { get; }
        public virtual int MaxMoney => 9999999;
        public virtual int MaxCoins => 9999;
        public virtual int MaxShadowID => 0;

        // Offsets
        protected virtual int Box { get; set; } = int.MinValue;
        protected int Party { get; set; } = int.MinValue;
        protected int Trainer1 { get; set; } = int.MinValue;
        protected int Daycare { get; set; } = int.MinValue;
        protected int WondercardData { get; set; } = int.MinValue;
        protected int PCLayout { get; set; } = int.MinValue;
        protected int EventFlag { get; set; } = int.MinValue;
        protected int EventConst { get; set; } = int.MinValue;

        public int GTS { get; protected set; } = int.MinValue;
        public int BattleBox { get; protected set; } = int.MinValue;
        public int Fused { get; protected set; } = int.MinValue;
        public int SUBE { get; protected set; } = int.MinValue;
        public int PokeDex { get; protected set; } = int.MinValue;
        public int SuperTrain { get; protected set; } = int.MinValue;
        public int SecretBase { get; protected set; } = int.MinValue;
        public int Puff { get; protected set; } = int.MinValue;
        public int PSS { get; protected set; } = int.MinValue;
        public int BerryField { get; protected set; } = int.MinValue;
        public int OPower { get; protected set; } = int.MinValue;
        public int HoF { get; protected set; } = int.MinValue;

        // SAV Properties
        public IList<PKM> BoxData
        {
            get
            {
                PKM[] data = new PKM[BoxCount*BoxSlotCount];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = GetStoredSlot(GetBoxOffset(i/BoxSlotCount) + SIZE_STORED*(i%BoxSlotCount));
                    data[i].Identifier = $"{GetBoxName(i/BoxSlotCount)}:{i%BoxSlotCount + 1:00}";
                    data[i].Box = i/BoxSlotCount + 1;
                    data[i].Slot = i%BoxSlotCount + 1;
                    data[i].Locked = IsSlotLocked(data[i].Box, data[i].Slot);
                }
                return data;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                if (value.Count != BoxCount*BoxSlotCount)
                    throw new ArgumentException($"Expected {BoxCount*BoxSlotCount}, got {value.Count}");
                if (value.Any(pk => PKMType != pk.GetType()))
                    throw new ArgumentException($"Not {PKMType} array.");

                for (int i = 0; i < value.Count; i++)
                    SetStoredSlot(value[i], GetBoxOffset(i/BoxSlotCount) + SIZE_STORED*(i%BoxSlotCount));
            }
        }
        public IList<PKM> PartyData
        {
            get
            {
                PKM[] data = new PKM[PartyCount];
                for (int i = 0; i < data.Length; i++)
                    data[i] = GetPartySlot(GetPartyOffset(i));
                return data;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                if (value.Count == 0 || value.Count > 6)
                    throw new ArgumentException($"Expected 1-6, got {value.Count}");
                if (value.Any(pk => PKMType != pk.GetType()))
                    throw new ArgumentException($"Not {PKMType} array.");
                if (value[0].Species == 0)
                    Debug.WriteLine($"Empty first slot, received {value.Count}.");

                PKM[] newParty = new PKM[6];
                value.Where(pk => pk.Species != 0).CopyTo(newParty);

                for (int i = PartyCount; i < newParty.Length; i++)
                    newParty[i] = BlankPKM;
                for (int i = 0; i < newParty.Length; i++)
                    SetPartySlot(newParty[i], GetPartyOffset(i));
            }
        }
        public IList<PKM> BattleBoxData
        {
            get
            {
                if (!HasBattleBox)
                    return new PKM[0];

                PKM[] data = new PKM[6];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = GetStoredSlot(BattleBox + SIZE_STORED * i);
                    data[i].Locked = BattleBoxLocked;
                    if (data[i].Species == 0)
                        return data.Take(i).ToArray();
                }
                return data;
            }
        }

        /// <summary> All Event Flag values for the savegame </summary>
        public bool[] EventFlags
        {
            get
            {
                if (EventFlagMax < 0)
                    return null;

                bool[] Flags = new bool[EventFlagMax];
                for (int i = 0; i < Flags.Length; i++)
                    Flags[i] = GetEventFlag(i);
                return Flags;
            }
            set
            {
                if (EventFlagMax < 0)
                    return;
                if (value.Length != EventFlagMax)
                    return;
                for (int i = 0; i < value.Length; i++)
                    SetEventFlag(i, value[i]);
            }
        }
        /// <summary> All Event Constant values for the savegame </summary>
        public ushort[] EventConsts
        {
            get
            {
                if (EventConstMax < 0)
                    return null;

                ushort[] Constants = new ushort[EventConstMax];
                for (int i = 0; i < Constants.Length; i++)
                    Constants[i] = BitConverter.ToUInt16(Data, EventConst + i * 2);
                return Constants;
            }
            set
            {
                if (EventConstMax < 0)
                    return;
                if (value.Length != EventConstMax)
                    return;

                for (int i = 0; i < value.Length; i++)
                    BitConverter.GetBytes(value[i]).CopyTo(Data, EventConst + i * 2);
            }
        }
        /// <summary>
        /// Gets the <see cref="bool"/> status of a desired Event Flag
        /// </summary>
        /// <param name="flagNumber">Event Flag to check</param>
        /// <returns>Flag is Set (true) or not Set (false)</returns>
        public bool GetEventFlag(int flagNumber)
        {
            if (flagNumber > EventFlagMax)
                throw new ArgumentException($"Event Flag to get ({flagNumber}) is greater than max ({EventFlagMax}).");
            return GetFlag(EventFlag + (flagNumber >> 3), flagNumber & 7);
        }

        /// <summary>
        /// Sets the <see cref="bool"/> status of a desired Event Flag
        /// </summary>
        /// <param name="flagNumber">Event Flag to check</param>
        /// <param name="value">Event Flag status to set</param>
        /// <remarks>Flag is Set (true) or not Set (false)</remarks>
        public void SetEventFlag(int flagNumber, bool value)
        {
            if (flagNumber > EventFlagMax)
                throw new ArgumentException($"Event Flag to set ({flagNumber}) is greater than max ({EventFlagMax}).");
            SetFlag(EventFlag + (flagNumber >> 3), flagNumber & 7, value);
        }
        /// <summary>
        /// Gets the <see cref="bool"/> status of the Flag at the specified offset and index.
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="bitIndex">Bit index to read</param>
        /// <returns>Flag is Set (true) or not Set (false)</returns>
        public bool GetFlag(int offset, int bitIndex)
        {
            bitIndex &= 7; // ensure bit access is 0-7
            return (Data[offset] >> bitIndex & 1) != 0;
        }
        /// <summary>
        /// Sets the <see cref="bool"/> status of the Flag at the specified offset and index.
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="bitIndex">Bit index to read</param>
        /// <param name="value">Flag status to set</param>
        /// <remarks>Flag is Set (true) or not Set (false)</remarks>
        public void SetFlag(int offset, int bitIndex, bool value)
        {
            bitIndex &= 7; // ensure bit access is 0-7
            Data[offset] &= (byte)~(1 << bitIndex);
            Data[offset] |= (byte)((value ? 1 : 0) << bitIndex);
        }

        // Inventory
        public virtual InventoryPouch[] Inventory { get; set; }
        protected int OFS_PouchHeldItem { get; set; } = int.MinValue;
        protected int OFS_PouchKeyItem { get; set; } = int.MinValue;
        protected int OFS_PouchMedicine { get; set; } = int.MinValue;
        protected int OFS_PouchTMHM { get; set; } = int.MinValue;
        protected int OFS_PouchBerry { get; set; } = int.MinValue;
        protected int OFS_PouchBalls { get; set; } = int.MinValue;
        protected int OFS_BattleItems { get; set; } = int.MinValue;
        protected int OFS_MailItems { get; set; } = int.MinValue;
        protected int OFS_PCItem { get; set; } = int.MinValue;
        protected int OFS_PouchZCrystals { get; set; } = int.MinValue;

        // Mystery Gift
        protected virtual bool[] MysteryGiftReceivedFlags { get => null; set { } }
        protected virtual MysteryGift[] MysteryGiftCards { get => null; set { } }
        public virtual MysteryGiftAlbum GiftAlbum
        {
            get => new MysteryGiftAlbum
            {
                Flags = MysteryGiftReceivedFlags,
                Gifts = MysteryGiftCards
            };
            set
            {
                MysteryGiftReceivedFlags = value.Flags;
                MysteryGiftCards = value.Gifts;
            }
        }

        public virtual bool BattleBoxLocked { get => false; set { } }
        public virtual string JPEGTitle => null;
        public virtual byte[] JPEGData => null;
        public virtual int Country { get => -1; set { } }
        public virtual int ConsoleRegion { get => -1; set { } }
        public virtual int SubRegion { get => -1; set { } }

        // Trainer Info
        public virtual int Gender { get; set; }
        public virtual int Language { get => -1; set { } }
        public virtual int Game { get => -1; set { } }
        public virtual ushort TID { get; set; }
        public virtual ushort SID { get; set; }
        public int TrainerID7 => (int)((uint)(TID | (SID << 16)) % 1000000);
        public virtual string OT { get; set; } = "PKHeX";
        public virtual int PlayedHours { get; set; }
        public virtual int PlayedMinutes { get; set; }
        public virtual int PlayedSeconds { get; set; }
        public virtual int SecondsToStart { get; set; }
        public virtual int SecondsToFame { get; set; }
        public virtual uint Money { get; set; }
        public abstract int BoxCount { get; }
        public int SlotCount => BoxCount * BoxSlotCount;
        public virtual int PartyCount { get; protected set; }
        public virtual int MultiplayerSpriteID { get => 0; set { } }
        public bool IsPartyAllEggs(params int[] except)
        {
            if (!HasParty)
                return false;

            var party = PartyData;
            return party.Count == party.Where((t, i) => t.IsEgg || except.Contains(i)).Count();
        }

        // Varied Methods
        protected abstract void SetChecksums();
        public abstract int GetBoxOffset(int box);
        public abstract int GetPartyOffset(int slot);
        public abstract string GetBoxName(int box);
        public abstract void SetBoxName(int box, string val);
        public virtual int GameSyncIDSize { get; } = 8;
        public virtual string GameSyncID { get => null; set { } }
        public virtual ulong? Secure1 { get => null; set { } }
        public virtual ulong? Secure2 { get => null; set { } }

        // Daycare
        public int DaycareIndex = 0;
        public virtual bool HasTwoDaycares => false;
        public virtual int GetDaycareSlotOffset(int loc, int slot) => -1;
        public virtual uint? GetDaycareEXP(int loc, int slot) => null;
        public virtual string GetDaycareRNGSeed(int loc) => null;
        public virtual bool? IsDaycareHasEgg(int loc) => null;
        public virtual bool? IsDaycareOccupied(int loc, int slot) => null;

        public virtual void SetDaycareEXP(int loc, int slot, uint EXP) { }
        public virtual void SetDaycareRNGSeed(int loc, string seed) { }
        public virtual void SetDaycareHasEgg(int loc, bool hasEgg) { }
        public virtual void SetDaycareOccupied(int loc, int slot, bool occupied) { }

        // Storage
        public virtual int BoxSlotCount => 30;
        public virtual int BoxesUnlocked { get => -1; set { } }
        public virtual byte[] BoxFlags { get => null; set { } }
        public virtual int CurrentBox { get => 0; set { } }
        protected int[] LockedSlots = new int[0];
        protected int[] TeamSlots = new int[0];
        public bool MoveBox(int box, int insertBeforeBox)
        {
            if (box == insertBeforeBox) // no movement required
                return true;
            if (box >= BoxCount || insertBeforeBox >= BoxCount) // invalid box positions
                return false;

            int pos1 = BoxSlotCount*box;
            int pos2 = BoxSlotCount*insertBeforeBox;
            int min = Math.Min(pos1, pos2);
            int max = Math.Max(pos1, pos2);
            if (LockedSlots.Any(slot => min <= slot && slot < max)) // slots locked within operation range
                return false;

            int len = BoxSlotCount*SIZE_STORED;
            byte[] boxdata = GetData(GetBoxOffset(0), len*BoxCount); // get all boxes
            string[] boxNames = new int[BoxCount].Select((x, i) => GetBoxName(i)).ToArray();
            int[] boxWallpapers = new int[BoxCount].Select((x, i) => GetBoxWallpaper(i)).ToArray();

            min /= BoxSlotCount;
            max /= BoxSlotCount;
            
            // move all boxes within range to final spot
            for (int i = min, ctr = min; i < max; i++)
            {
                int b = insertBeforeBox; // if box is the moved box, move to insertion point, else move to unused box.
                if (i != box)
                {
                    if (insertBeforeBox == ctr)
                        ++ctr;
                    b = ctr++;
                }
                Buffer.BlockCopy(boxdata, len*i, Data, GetBoxOffset(b), len);
                SetBoxName(b, boxNames[i]);
                SetBoxWallpaper(b, boxWallpapers[i]);
            }
            return true;
        }
        public bool SwapBox(int box1, int box2)
        {
            if (box1 == box2) // no movement required
                return true;
            if (box1 >= BoxCount || box2 >= BoxCount) // invalid box positions
                return false;

            if (!IsBoxAbleToMove(box1) || !IsBoxAbleToMove(box2))
                return false;

            // Data
            int b1o = GetBoxOffset(box1);
            int b2o = GetBoxOffset(box2);
            int len = BoxSlotCount*SIZE_STORED;
            byte[] b1 = new byte[len];
            Buffer.BlockCopy(Data, b1o, b1, 0, len);
            Buffer.BlockCopy(Data, b2o, Data, b1o, len);
            Buffer.BlockCopy(b1, 0, Data, b2o, len);

            // Name
            string b1n = GetBoxName(box1);
            SetBoxName(box1, GetBoxName(box2));
            SetBoxName(box2, b1n);

            // Wallpaper
            int b1w = GetBoxWallpaper(box1);
            SetBoxWallpaper(box1, GetBoxWallpaper(box2));
            SetBoxWallpaper(box2, b1w);
            return true;
        }
        private bool IsBoxAbleToMove(int box)
        {
            int min = BoxSlotCount * box;
            int max = BoxSlotCount * box + BoxSlotCount;
            if (LockedSlots.Any(slot => min <= slot && slot < max)) // locked slot within box
                return false;
            if (TeamSlots.Any(slot => min <= slot && slot < max)) // team slot within box
                return false;
            return true;
        }

        protected virtual int GetBoxWallpaperOffset(int box) { return -1; }
        public virtual int GetBoxWallpaper(int box)
        {
            int offset = GetBoxWallpaperOffset(box);
            if (offset < 0 || box > BoxCount)
                return box;
            return Data[offset];
        }
        public virtual void SetBoxWallpaper(int box, int val)
        {
            int offset = GetBoxWallpaperOffset(box);
            if (offset < 0 || box > BoxCount)
                return;
            Data[offset] = (byte)val;
        }

        public virtual PKM GetPartySlot(int offset)
        {
            return GetPKM(DecryptPKM(GetData(offset, SIZE_PARTY)));
        }
        public virtual PKM GetStoredSlot(int offset)
        {
            return GetPKM(DecryptPKM(GetData(offset, SIZE_STORED)));
        }
        public void SetPartySlot(PKM pkm, int offset, bool? trade = null, bool? dex = null)
        {
            if (pkm == null) return;
            if (pkm.GetType() != PKMType)
                throw new ArgumentException($"PKM Format needs to be {PKMType} when setting to this Save File.");
            if (trade ?? SetUpdatePKM)
                SetPKM(pkm);
            if (dex ?? SetUpdateDex)
                SetDex(pkm);
            SetPartyValues(pkm, isParty: true);

            int i = GetPartyIndex(offset);
            if (i <= -1)
                throw new ArgumentException("Invalid Party offset provided; unable to resolve party slot index.");
            
            // update party count
            if (pkm.Species != 0)
            {
                if (PartyCount <= i)
                    PartyCount = i + 1;
            }
            else if (PartyCount > i)
                PartyCount = i;

            SetData(pkm.EncryptedPartyData, offset);
            Edited = true;
        }
        private int GetPartyIndex(int offset)
        {
            for (int i = 0; i < 6; i++)
                if (GetPartyOffset(i) == offset)
                    return i;
            return -1;
        }
        public virtual void SetStoredSlot(PKM pkm, int offset, bool? trade = null, bool? dex = null)
        {
            if (pkm == null) return;
            if (pkm.GetType() != PKMType)
                throw new ArgumentException($"PKM Format needs to be {PKMType} when setting to this Save File.");
            if (trade ?? SetUpdatePKM)
                SetPKM(pkm);
            if (dex ?? SetUpdateDex)
                SetDex(pkm);
            SetPartyValues(pkm, isParty: false);
            SetData(pkm.EncryptedBoxData, offset);
            Edited = true;
        }
        public void DeletePartySlot(int slot)
        {
            if (PartyCount <= slot) // beyond party range (or empty data already present)
                return;
            // Move all party slots down one
            for (int i = slot + 1; i < 6; i++) // Slide slots down
            {
                int slotTo = GetPartyOffset(i - 1);
                int slotFrom = GetPartyOffset(i);
                SetData(GetData(slotFrom, SIZE_PARTY), slotTo);
            }
            SetStoredSlot(BlankPKM, GetPartyOffset(5), false, false);
            PartyCount -= 1;
        }
        public virtual bool IsSlotLocked(int box, int slot) => false;
        public bool IsAnySlotLockedInBox(int BoxStart, int BoxEnd)
        {
            return LockedSlots.Any(slot => BoxStart*BoxSlotCount <= slot && slot < (BoxEnd + 1)*BoxSlotCount);
        }
        public virtual bool IsSlotInBattleTeam(int box, int slot) => false;

        public void SortBoxes(int BoxStart = 0, int BoxEnd = -1)
        {
            var BD = BoxData;
            int start = BoxSlotCount * BoxStart;
            var Section = BD.Skip(start);
            if (BoxEnd > BoxStart)
                Section = Section.Take(BoxSlotCount * (BoxEnd - BoxStart));

            var Sorted = PKX.SortPKMs(Section);

            Sorted.CopyTo(BD, start);
            BoxData = BD;
        }
        public void ClearBoxes(int BoxStart = 0, int BoxEnd = -1)
        {
            if (BoxEnd < 0)
                BoxEnd = BoxCount;

            var blank = BlankPKM.EncryptedBoxData;
            if (this is SAV3RSBox)
                Array.Resize(ref blank, blank.Length + 4); // 00000 TID/SID at end

            for (int i = BoxStart; i < BoxEnd; i++)
            {
                int offset = GetBoxOffset(i);
                for (int p = 0; p < BoxSlotCount; p++)
                    SetData(blank, offset + SIZE_STORED * p);
            }
        }

        public byte[] PCBinary => BoxData.SelectMany(pk => pk.EncryptedBoxData).ToArray();
        public byte[] GetBoxBinary(int box) => BoxData.Skip(box*BoxSlotCount).Take(BoxSlotCount).SelectMany(pk => pk.EncryptedBoxData).ToArray();
        public bool SetPCBinary(byte[] data)
        {
            if (LockedSlots.Any())
                return false;
            if (data.Length != PCBinary.Length)
                return false;

            var BD = BoxData;
            var pkdata = PKX.GetPKMDataFromConcatenatedBinary(data, BlankPKM.EncryptedBoxData.Length);
            pkdata.Select(z => GetPKM(DecryptPKM(z))).CopyTo(BD);
            BoxData = BD;
            return true;
        }
        public bool SetBoxBinary(byte[] data, int box)
        {
            int start = box * BoxSlotCount;
            int end = start + BoxSlotCount;
            if (LockedSlots.Any(slot => start <= slot && slot < end))
                return false;
            if (data.Length != GetBoxBinary(box).Length)
                return false;

            var BD = BoxData;
            var pkdata = PKX.GetPKMDataFromConcatenatedBinary(data, BlankPKM.EncryptedBoxData.Length);
            pkdata.Select(z => GetPKM(DecryptPKM(z))).CopyTo(BD, start);
            BoxData = BD;
            return true;
        }

        protected virtual void SetPartyValues(PKM pkm, bool isParty) { }
        protected virtual void SetPKM(PKM pkm) { }
        protected virtual void SetDex(PKM pkm) { }
        public virtual bool GetSeen(int species) => false;
        public virtual void SetSeen(int species, bool seen) { }
        public virtual bool GetCaught(int species) => false;
        public virtual void SetCaught(int species, bool caught) { }
        public int SeenCount => HasPokeDex ? Enumerable.Range(1, MaxSpeciesID).Count(GetSeen) : 0;
        public int CaughtCount => HasPokeDex ? Enumerable.Range(1, MaxSpeciesID).Count(GetCaught) : 0;
        public decimal PercentSeen => (decimal) SeenCount / MaxSpeciesID;
        public decimal PercentCaught => (decimal)CaughtCount / MaxSpeciesID;

        public byte[] GetData(int Offset, int Length)
        {
            if (Offset + Length > Data.Length)
                return null;

            byte[] data = new byte[Length];
            Buffer.BlockCopy(Data, Offset, data, 0, Length);
            return data;
        }
        public void SetData(byte[] input, int Offset)
        {
            input.CopyTo(Data, Offset);
            Edited = true;
        }

        public abstract string GetString(int Offset, int Length);
        public abstract byte[] SetString(string value, int maxLength, int PadToSize = 0, ushort PadWith = 0);

        public virtual string EBerryName => string.Empty;
        public virtual bool IsEBerryIsEnigma => true;
    }
}

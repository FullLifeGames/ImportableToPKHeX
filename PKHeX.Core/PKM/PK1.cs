﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary> Generation 1 <see cref="PKM"/> format. </summary>
    public class PK1 : PKM
    {
        // Internal use only
        protected internal byte[] otname;
        protected internal byte[] nick;
        public override PersonalInfo PersonalInfo => PersonalTable.Y[Species];

        public byte[] OT_Name_Raw => (byte[])otname.Clone();
        public byte[] Nickname_Raw => (byte[])nick.Clone();
        public override bool Valid => Species <= 151 && (Data[0] == 0 || Species != 0);

        public sealed override int SIZE_PARTY => PKX.SIZE_1PARTY;
        public override int SIZE_STORED => PKX.SIZE_1STORED;
        internal const int STRLEN_J = 6;
        internal const int STRLEN_U = 11;
        private int StringLength => Japanese ? STRLEN_J : STRLEN_U;

        private string GetString(int Offset, int Count) => StringConverter.GetString1(Data, Offset, Count, Japanese);
        private byte[] SetString(string value, int maxLength) => StringConverter.SetString1(value, maxLength, Japanese);

        // Trash Bytes
        public override byte[] Nickname_Trash { get => nick; set { if (value?.Length == nick.Length) nick = value; } }
        public override byte[] OT_Trash { get => otname; set { if (value?.Length == otname.Length) otname = value; } }

        public override int Format => 1;

        public override bool Japanese => otname.Length == STRLEN_J;
        public override bool Korean => false;

        public override string FileName => $"{Species:000} - {Nickname} - {SaveUtil.CRC16_CCITT(Encrypt()):X4}.{Extension}";

        public PK1(byte[] decryptedData = null, string ident = null, bool jp = false)
        {
            Data = (byte[])(decryptedData ?? new byte[SIZE_PARTY]).Clone();
            Identifier = ident;
            if (Data.Length != SIZE_PARTY)
                Array.Resize(ref Data, SIZE_PARTY);
            int strLen = jp ? STRLEN_J : STRLEN_U;
            otname = Enumerable.Repeat((byte) 0x50, strLen).ToArray();
            nick = Enumerable.Repeat((byte) 0x50, strLen).ToArray();
        }

        public override PKM Clone()
        {
            PK1 new_pk1 = new PK1(Data, Identifier, Japanese);
            Array.Copy(otname, 0, new_pk1.otname, 0, otname.Length);
            Array.Copy(nick, 0, new_pk1.nick, 0, nick.Length);
            return new_pk1;
        }
        public override string Nickname
        {
            get => StringConverter.GetString1(nick, 0, nick.Length, Japanese);
            set
            {
                if (!IsNicknamed)
                    return;

                byte[] strdata = SetString(value, StringLength);
                if (nick.Any(b => b == 0) && nick[StringLength - 1] == 0x50 && Array.FindIndex(nick, b => b == 0) == strdata.Length - 1) // Handle JP Mew event with grace
                {
                    int firstInd = Array.FindIndex(nick, b => b == 0);
                    for (int i = firstInd; i < StringLength - 1; i++)
                        if (nick[i] != 0)
                            break;
                    strdata = strdata.Take(strdata.Length - 1).ToArray();
                }
                strdata.CopyTo(nick, 0);
            }
        }

        public override string OT_Name
        {
            get => StringConverter.GetString1(otname, 0, otname.Length, Japanese);
            set
            {
                byte[] strdata = SetString(value, StringLength);
                if (otname.Any(b => b == 0) && otname[StringLength - 1] == 0x50 && Array.FindIndex(otname, b => b == 0) == strdata.Length - 1) // Handle JP Mew event with grace
                {
                    int firstInd = Array.FindIndex(otname, b => b == 0);
                    for (int i = firstInd; i < StringLength - 1; i++)
                        if (otname[i] != 0)
                            break;
                    strdata = strdata.Take(strdata.Length - 1).ToArray();
                }
                strdata.CopyTo(otname, 0);
            }
        }

        protected override byte[] Encrypt() => new PokemonList1(this).GetBytes();
        public override byte[] EncryptedPartyData => Encrypt();
        public override byte[] EncryptedBoxData => Encrypt();
        public override byte[] DecryptedBoxData => Encrypt();
        public override byte[] DecryptedPartyData => Encrypt();

        private bool? _isnicknamed;
        public override bool IsNicknamed
        {
            get => (bool)(_isnicknamed ?? (_isnicknamed = !nick.SequenceEqual(GetNonNickname())));
            set
            {
                _isnicknamed = value;
                if (_isnicknamed == false)
                    SetNotNicknamed();
            }
        }
        public void SetNotNicknamed() => nick = GetNonNickname().ToArray();
        private IEnumerable<byte> GetNonNickname()
        {
            var name = PKX.GetSpeciesNameGeneration(Species, GuessedLanguage(), Format);
            var bytes = SetString(name, StringLength);
            return bytes.Concat(Enumerable.Repeat((byte)0x50, nick.Length - bytes.Length))
                .Select(b => (byte)(b == 0xF2 ? 0xE8 : b)); // Decimal point<->period fix
        }
        public bool IsNicknamedBank
        {
            get
            {
                var spName = PKX.GetSpeciesNameGeneration(Species, GuessedLanguage(), Format);
                return Nickname != spName;
            }
        }
        public override int Language
        {
            get
            {
                if (Japanese)
                    return (int)LanguageID.Japanese;
                if (StringConverter.IsG12German(otname))
                    return (int)LanguageID.German;
                int lang = PKX.GetSpeciesNameLanguage(Species, Nickname, Format);
                if (lang > 0)
                    return lang;
                return 0;
            }
            set { }
        }
        private int GuessedLanguage(int fallback = (int)LanguageID.English)
        {
            int lang = Language;
            if (lang > 0)
                return lang;
            if (fallback == (int)LanguageID.French || fallback == (int)LanguageID.German) // only other permitted besides English
                return fallback;
            return (int)LanguageID.English;
        }


        #region Stored Attributes
        public override int Species
        {
            get => SpeciesConverter.GetG1Species(Data[0]);
            set
            {
                Data[0] = (byte)SpeciesConverter.SetG1Species(value);

                // Before updating catch rate, check if non-standard
                if (TradebackStatus != TradebackType.WasTradeback && !CatchRateIsItem && !(value == 25 && Catch_Rate == 0xA3)) // Light Ball Pikachu
                {
                    int baseSpecies = Legal.GetBaseSpecies(this);
                    int Rate = Catch_Rate;
                    if (Enumerable.Range(baseSpecies, value).All(z => Rate != PersonalTable.RB[z].CatchRate))
                        Catch_Rate = PersonalTable.RB[value].CatchRate;
                }
                Type_A = PersonalInfo.Types[0];
                Type_B = PersonalInfo.Types[1];
            }
        }

        public override int Stat_HPCurrent { get => BigEndian.ToUInt16(Data, 0x1); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x1); }
        public int Status_Condition { get => Data[4]; set => Data[4] = (byte)value; }
        public int Type_A { get => Data[5]; set => Data[5] = (byte)value; }
        public int Type_B { get => Data[6]; set => Data[6] = (byte)value; }
        public int Catch_Rate { get => Data[7]; set => Data[7] = (byte)value; }
        public override int Move1 { get => Data[8]; set => Data[8] = (byte)value; }
        public override int Move2 { get => Data[9]; set => Data[9] = (byte)value; }
        public override int Move3 { get => Data[10]; set => Data[10] = (byte)value; }
        public override int Move4 { get => Data[11]; set => Data[11] = (byte)value; }
        public override int TID { get => BigEndian.ToUInt16(Data, 0xC); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0xC); }
        public override uint EXP
        {
            get => (BigEndian.ToUInt32(Data, 0xE) >> 8) & 0x00FFFFFF;
            set => Array.Copy(BigEndian.GetBytes((value << 8) & 0xFFFFFF00), 0, Data, 0xE, 3);
        }
        public override int EV_HP { get => BigEndian.ToUInt16(Data, 0x11); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x11); }
        public override int EV_ATK { get => BigEndian.ToUInt16(Data, 0x13); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x13); }
        public override int EV_DEF { get => BigEndian.ToUInt16(Data, 0x15); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x15); }
        public override int EV_SPE { get => BigEndian.ToUInt16(Data, 0x17); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x17); }
        public int EV_SPC { get => BigEndian.ToUInt16(Data, 0x19); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x19); }
        public override int EV_SPA { get => EV_SPC; set => EV_SPC = value; }
        public override int EV_SPD { get => EV_SPC; set { } }
        public ushort DV16 { get => BigEndian.ToUInt16(Data, 0x1B); set => BigEndian.GetBytes(value).CopyTo(Data, 0x1B); }
        public override int IV_HP { get => ((IV_ATK & 1) << 3) | ((IV_DEF & 1) << 2) | ((IV_SPE & 1) << 1) | ((IV_SPC & 1) << 0); set { } }
        public override int IV_ATK { get => (DV16 >> 12) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 12)) | (ushort)((value > 0xF ? 0xF : value) << 12)); }
        public override int IV_DEF { get => (DV16 >> 8) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 8)) | (ushort)((value > 0xF ? 0xF : value) << 8)); }
        public override int IV_SPE { get => (DV16 >> 4) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 4)) | (ushort)((value > 0xF ? 0xF : value) << 4)); }
        public int IV_SPC { get => (DV16 >> 0) & 0xF; set => DV16 = (ushort)((DV16 & ~(0xF << 0)) | (ushort)((value > 0xF ? 0xF : value) << 0)); }
        public override int IV_SPA { get => IV_SPC; set => IV_SPC = value; }
        public override int IV_SPD { get => IV_SPC; set { } }
        public override int Move1_PP { get => Data[0x1D] & 0x3F; set => Data[0x1D] = (byte)((Data[0x1D] & 0xC0) | Math.Min(63, value)); }
        public override int Move2_PP { get => Data[0x1E] & 0x3F; set => Data[0x1E] = (byte)((Data[0x1E] & 0xC0) | Math.Min(63, value)); }
        public override int Move3_PP { get => Data[0x1F] & 0x3F; set => Data[0x1F] = (byte)((Data[0x1F] & 0xC0) | Math.Min(63, value)); }
        public override int Move4_PP { get => Data[0x20] & 0x3F; set => Data[0x20] = (byte)((Data[0x20] & 0xC0) | Math.Min(63, value)); }
        public override int Move1_PPUps { get => (Data[0x1D] & 0xC0) >> 6; set => Data[0x1D] = (byte)((Data[0x1D] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move2_PPUps { get => (Data[0x1E] & 0xC0) >> 6; set => Data[0x1E] = (byte)((Data[0x1E] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move3_PPUps { get => (Data[0x1F] & 0xC0) >> 6; set => Data[0x1F] = (byte)((Data[0x1F] & 0x3F) | ((value & 0x3) << 6)); }
        public override int Move4_PPUps { get => (Data[0x20] & 0xC0) >> 6; set => Data[0x20] = (byte)((Data[0x20] & 0x3F) | ((value & 0x3) << 6)); }
        #endregion

        #region Party Attributes
        public override int Stat_Level
        {
            get => Data[0x21];
            set { Data[0x21] = (byte)value; Data[0x3] = (byte)value; }
        }
        public override int Stat_HPMax { get => BigEndian.ToUInt16(Data, 0x22); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x22); }
        public override int Stat_ATK { get => BigEndian.ToUInt16(Data, 0x24); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x24); }
        public override int Stat_DEF { get => BigEndian.ToUInt16(Data, 0x26); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x26); }
        public override int Stat_SPE { get => BigEndian.ToUInt16(Data, 0x28); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x28); }
        public int Stat_SPC { get => BigEndian.ToUInt16(Data, 0x2A); set => BigEndian.GetBytes((ushort)value).CopyTo(Data, 0x2A); }
        // Leave SPA and SPD as alias for SPC
        public override int Stat_SPA { get => Stat_SPC; set => Stat_SPC = value; }
        public override int Stat_SPD { get => Stat_SPC; set { } }
        #endregion

        public override int GetMovePP(int move, int ppup) => Math.Min(61, base.GetMovePP(move, ppup));
        public override ushort[] GetStats(PersonalInfo p)
        {
            ushort[] Stats = new ushort[6];
            for (int i = 0; i < Stats.Length; i++)
            {
                ushort L = (ushort)Stat_Level;
                ushort B = (ushort)p.Stats[i];
                ushort I = (ushort)IVs[i];
                ushort E = // Fixed formula via http://www.smogon.com/ingame/guides/rby_gsc_stats
                    (ushort)Math.Floor(Math.Min(255, Math.Floor(Math.Sqrt(Math.Max(0, EVs[i] - 1)) + 1)) / 4.0);
                Stats[i] = (ushort)Math.Floor((2 * (B + I) + E) * L / 100.0 + 5);
            }
            Stats[0] += (ushort)(5 + Stat_Level); // HP

            return Stats;
        }

        #region Future, Unused Attributes
        public override bool IsGenderValid() => true; // not a separate property, derived via IVs
        public override uint EncryptionConstant { get => 0; set { } }
        public override uint PID { get => 0; set { } }
        public override int Met_Level { get => 0; set { } }
        public override int Nature { get => 0; set { } }
        public override int AltForm { get => 0; set { } }
        public override bool IsEgg { get => false; set { } }
        public override int HeldItem { get => 0; set { } }
        public override bool CanHoldItem(ushort[] ValidArray) => false;
        public override bool IsShiny => IV_DEF == 10 && IV_SPE == 10 && IV_SPC == 10 && (IV_ATK & 2) == 2;
        public override ushort Sanity { get => 0; set { } }
        public override bool ChecksumValid => true;
        public override ushort Checksum { get => 0; set { } }
        public override bool FatefulEncounter { get => false; set { } }
        public override int TSV => 0x0000;
        public override int PSV => 0xFFFF;
        public override int Characteristic => -1;
        public override int MarkValue { get => 0; protected set { } }
        public override int CurrentFriendship { get => 0; set { } }
        public override int Ability { get => 0; set { } }
        public override int CurrentHandler { get => 0; set { } }
        public override int Met_Location { get => 0; set { } }
        public override int Egg_Location { get => 0; set { } }
        public override int OT_Friendship { get => 0; set { } }
        public override int OT_Gender { get => 0; set { } }
        public override int Ball { get => 0; set { } }
        public override int Version { get => (int)GameVersion.RBY; set { } }
        public override int SID { get => 0; set { } }
        public override int PKRS_Strain { get => 0; set { } }
        public override int PKRS_Days { get => 0; set { } }
        public override int CNT_Cool { get => 0; set { } }
        public override int CNT_Beauty { get => 0; set { } }
        public override int CNT_Cute { get => 0; set { } }
        public override int CNT_Smart { get => 0; set { } }
        public override int CNT_Tough { get => 0; set { } }
        public override int CNT_Sheen { get => 0; set { } }
        #endregion
        public bool CatchRateIsItem = false;

        public override int Gender
        {
            get
            {
                int gv = PersonalInfo.Gender;
                if (gv == 255)
                    return 2;
                if (gv == 254)
                    return 1;
                if (gv == 0)
                    return 0;
                switch (gv)
                {
                    case 31:
                        return IV_ATK >= 2 ? 0 : 1;
                    case 63:
                        return IV_ATK >= 5 ? 0 : 1;
                    case 127:
                        return IV_ATK >= 8 ? 0 : 1;
                    case 191:
                        return IV_ATK >= 12 ? 0 : 1;
                }
                Debug.WriteLine($"Unknown Gender value: {gv}");
                return 0;
            }
            set { }
        }

        // Maximums
        public override int MaxMoveID => Legal.MaxMoveID_1;
        public override int MaxSpeciesID => Legal.MaxSpeciesID_1;
        public override int MaxAbilityID => Legal.MaxAbilityID_1;
        public override int MaxItemID => Legal.MaxItemID_1;
        public override int MaxBallID => -1;
        public override int MaxGameID => -1;
        public override int MaxIV => 15;
        public override int MaxEV => ushort.MaxValue;
        public override int OTLength => Japanese ? 5 : 7;
        public override int NickLength => Japanese ? 5 : 10;

        public PK2 ConvertToPK2()
        {
            PK2 pk2 = new PK2(null, Identifier, Japanese) {Species = Species};
            Array.Copy(Data, 0x7, pk2.Data, 0x1, 0x1A);
            // https://github.com/pret/pokecrystal/blob/master/engine/link.asm#L1132
            if (!Legal.HeldItems_GSC.Contains((ushort)pk2.HeldItem)) 
                switch (pk2.HeldItem)
                {
                    case 0x19:
                        pk2.HeldItem = 0x92; // Leftovers
                        break;
                    case 0x2D:
                        pk2.HeldItem = 0x53; // Bitter Berry
                        break;
                    case 0x32:
                        pk2.HeldItem = 0xAE; // Leftovers
                        break;
                    case 0x5A:
                    case 0x64:
                    case 0x78:
                    case 0x87:
                    case 0xBE:
                    case 0xC3:
                    case 0xDC:
                    case 0xFA:
                    case 0xFF:
                        pk2.HeldItem = 0xAD; // Berry
                        break;
                }
            pk2.CurrentFriendship = pk2.PersonalInfo.BaseFriendship;
            // Pokerus = 0
            // Caught Data = 0
            pk2.Stat_Level = PKX.GetLevel(Species, EXP);
            Array.Copy(otname, 0, pk2.otname, 0, otname.Length);
            Array.Copy(nick, 0, pk2.nick, 0, nick.Length);

            return pk2;
        }

        public PK7 ConvertToPK7()
        {
            var pk7 = new PK7
            {
                EncryptionConstant = Util.Rand32(),
                Species = Species,
                TID = TID,
                CurrentLevel = CurrentLevel,
                EXP = EXP,
                Met_Level = CurrentLevel,
                Nature = (int) (EXP%25),
                PID = Util.Rand32(),
                Ball = 4,
                MetDate = DateTime.Now,
                Version = (int)GameVersion.RD, // Default to red, for now?
                Move1 = Move1,
                Move2 = Move2,
                Move3 = Move3,
                Move4 = Move4,
                Move1_PPUps = Move1_PPUps,
                Move2_PPUps = Move2_PPUps,
                Move3_PPUps = Move3_PPUps,
                Move4_PPUps = Move4_PPUps,
                Move1_PP = Move1_PP,
                Move2_PP = Move2_PP,
                Move3_PP = Move3_PP,
                Move4_PP = Move4_PP,
                Met_Location = 30013, // "Kanto region", hardcoded.
                Gender = Gender,
                OT_Name = StringConverter.GetG1ConvertedString(otname, Japanese),
                IsNicknamed = false,

                Country = PKMConverter.Country,
                Region = PKMConverter.Region,
                ConsoleRegion = PKMConverter.ConsoleRegion,
                CurrentHandler = 1,
                HT_Name = PKMConverter.OT_Name,
                HT_Gender = PKMConverter.OT_Gender,
                Geo1_Country = PKMConverter.Country,
                Geo1_Region = PKMConverter.Region
            };
            pk7.Language = GuessedLanguage(PKMConverter.Language);
            pk7.Nickname = PKX.GetSpeciesNameGeneration(pk7.Species, pk7.Language, pk7.Format);
            if (otname[0] == 0x5D) // Ingame Trade
            {
                var s = StringConverter.GetG1Char(0x5D, Japanese);
                pk7.OT_Name = s.Substring(0, 1) + s.Substring(1).ToLower();
            }
            pk7.OT_Friendship = pk7.HT_Friendship = PersonalTable.SM[Species].BaseFriendship;

            // IVs
            var new_ivs = new int[6];
            int flawless = Species == 151 ? 5 : 3;
            for (var i = 0; i < new_ivs.Length; i++) new_ivs[i] = (int)(Util.Rand32() & 31);
            for (var i = 0; i < flawless; i++) new_ivs[i] = 31;
            Util.Shuffle(new_ivs);
            pk7.IVs = new_ivs;

            // Really? :(
            if (IsShiny)
                pk7.SetShinyPID();

            int abil = 2; // Hidden
            if (Legal.TransferSpeciesDefaultAbility_1.Contains(Species))
                abil = 0; // Reset
            pk7.RefreshAbility(abil); // 0/1/2 (not 1/2/4)

            if (Species == 151) // Mew gets special treatment.
                pk7.FatefulEncounter = true;
            else if (IsNicknamedBank)
            {
                pk7.IsNicknamed = true;
                pk7.Nickname = StringConverter.GetG1ConvertedString(nick, Japanese);
            }
            
            pk7.TradeMemory(Bank:true); // oh no, memories on gen7 pkm

            if (pk7.Species == 150) // Pay Day Mewtwo
            {
                var moves = pk7.Moves;
                var index = Array.IndexOf(moves, 6);
                if (index != -1)
                {
                    moves[index] = 0;
                    pk7.Moves = moves;
                    pk7.FixMoves();
                }
            }
            
            pk7.RefreshChecksum();
            return pk7;
        }
    }

    public class PokemonList1
    {
        private const int CAPACITY_DAYCARE = 1;
        private const int CAPACITY_PARTY = 6;
        private const int CAPACITY_STORED = 20;
        private const int CAPACITY_STORED_JP = 30;

        private readonly bool Japanese;

        private int StringLength => Japanese ? PK1.STRLEN_J : PK1.STRLEN_U;

        public enum CapacityType
        {
            Daycare = CAPACITY_DAYCARE,
            Party = CAPACITY_PARTY,
            Stored = CAPACITY_STORED,
            StoredJP = CAPACITY_STORED_JP,
            Single
        }

        private static int GetEntrySize(CapacityType c) => c == CapacityType.Single || c == CapacityType.Party
            ? PKX.SIZE_1PARTY
            : PKX.SIZE_1STORED;
        private static byte GetCapacity(CapacityType c) => c == CapacityType.Single ? (byte)1 : (byte)c;

        private static byte[] GetEmptyList(CapacityType c, bool is_JP = false)
        {
            int cap = GetCapacity(c);
            return new[] { (byte)0 }.Concat(Enumerable.Repeat((byte)0xFF, cap + 1)).Concat(Enumerable.Repeat((byte)0, GetEntrySize(c) * cap)).Concat(Enumerable.Repeat((byte)0x50, (is_JP ? PK1.STRLEN_J : PK1.STRLEN_U) * 2 * cap)).ToArray();
        }

        public PokemonList1(byte[] d, CapacityType c = CapacityType.Single, bool jp = false)
        {
            Japanese = jp;
            Data = d ?? GetEmptyList(c, Japanese);
            Capacity = GetCapacity(c);
            Entry_Size = GetEntrySize(c);

            if (Data.Length != DataSize)
                Array.Resize(ref Data, DataSize);

            Pokemon = new PK1[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                int base_ofs = 2 + Capacity;
                byte[] dat = new byte[Entry_Size]; 
                byte[] otname = new byte[StringLength];
                byte[] nick = new byte[StringLength];
                Buffer.BlockCopy(Data, base_ofs + Entry_Size * i, dat, 0, Entry_Size);
                Buffer.BlockCopy(Data, base_ofs + Capacity * Entry_Size + StringLength * i, otname, 0, StringLength);
                Buffer.BlockCopy(Data, base_ofs + Capacity * Entry_Size + StringLength * (i + Capacity), nick, 0, StringLength);

                Pokemon[i] = new PK1(dat, null, jp) {otname = otname, nick = nick};
            }
        }

        public PokemonList1(CapacityType c = CapacityType.Single, bool jp = false)
            : this(null, c, jp) => Count = 1;

        public PokemonList1(PK1 pk)
            : this(CapacityType.Single, pk.Japanese)
        {
            this[0] = pk;
            Count = 1;
        }

        private readonly byte[] Data;
        private readonly byte Capacity;
        private readonly int Entry_Size;

        public byte Count
        {
            get => Data[0];
            set => Data[0] = value > Capacity ? Capacity : value;
        }

        public readonly PK1[] Pokemon;

        public PK1 this[int i]
        {
            get
            {
                if (i > Capacity || i < 0) throw new ArgumentOutOfRangeException($"Invalid PokemonList Access: {i}");
                return Pokemon[i];
            }
            set
            {
                if (value == null) return;
                Pokemon[i] = (PK1)value.Clone();
            }
        }

        private void Update()
        {
            int count = Array.FindIndex(Pokemon, pk => pk.Species == 0);
            Count = count < 0 ? Capacity : (byte)count;
            for (int i = 0; i < Count; i++)
            {
                int base_ofs = 2 + Capacity;
                Data[1 + i] = (byte)SpeciesConverter.SetG1Species(Pokemon[i].Species);
                Array.Copy(Pokemon[i].Data, 0, Data, base_ofs + Entry_Size * i, Entry_Size);
                Array.Copy(Pokemon[i].OT_Name_Raw, 0, Data, base_ofs + Capacity * Entry_Size + StringLength * i, StringLength);
                Array.Copy(Pokemon[i].Nickname_Raw, 0, Data, base_ofs + Capacity * Entry_Size + StringLength * (i + Capacity), StringLength);
            }
            Data[1 + Count] = byte.MaxValue;
        }

        public byte[] GetBytes()
        {
            Update();
            return Data;
        }

        private int DataSize => Capacity * (Entry_Size + 1 + 2 * StringLength) + 2;
        public static int GetDataLength(CapacityType c, bool jp = false) => GetCapacity(c) * (GetEntrySize(c) + 1 + 2 * (jp ? PK1.STRLEN_J : PK1.STRLEN_U)) + 2;
    }
}

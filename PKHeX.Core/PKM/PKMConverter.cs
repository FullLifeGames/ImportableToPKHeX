﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PKHeX.Core
{
    /// <summary>
    /// Logic for converting a <see cref="PKM"/> from one generation specific format to another.
    /// </summary>
    public static class PKMConverter
    {
        public static int Country { get; private set; } = 49;
        public static int Region { get; private set; } = 7;
        public static int ConsoleRegion { get; private set; } = 1;
        public static string OT_Name { get; private set; } = "PKHeX";
        public static int OT_Gender { get; private set; } // Male
        public static int Language { get; private set; } = 1; // en

        public static void UpdateConfig(int SUBREGION, int COUNTRY, int _3DSREGION, string TRAINERNAME, int TRAINERGENDER, int LANGUAGE)
        {
            Region = SUBREGION;
            Country = COUNTRY;
            ConsoleRegion = _3DSREGION;
            OT_Name = TRAINERNAME;
            OT_Gender = TRAINERGENDER;
            Language = LANGUAGE;
        }

        /// <summary>
        /// Gets the generation of the Pokemon data.
        /// </summary>
        /// <param name="data">Raw data representing a Pokemon.</param>
        /// <returns>An integer indicating the generation of the PKM file, or -1 if the data is invalid.</returns>
        public static int GetPKMDataFormat(byte[] data)
        {
            if (!PKX.IsPKM(data.Length))
                return -1;

            switch (data.Length)
            {
                case PKX.SIZE_1JLIST:
                case PKX.SIZE_1ULIST:
                    return 1;
                case PKX.SIZE_2ULIST:
                case PKX.SIZE_2JLIST:
                    return 2;
                case PKX.SIZE_3PARTY:
                case PKX.SIZE_3STORED:
                case PKX.SIZE_3CSTORED:
                case PKX.SIZE_3XSTORED:
                    return 3;
                case PKX.SIZE_4PARTY:
                case PKX.SIZE_4STORED:
                case PKX.SIZE_5PARTY:
                    if ((BitConverter.ToUInt16(data, 0x4) == 0) && (BitConverter.ToUInt16(data, 0x80) >= 0x3333 || data[0x5F] >= 0x10) && BitConverter.ToUInt16(data, 0x46) == 0) // PK5
                        return 5;
                    return 4;
                case PKX.SIZE_6STORED:
                    return 6;
                case PKX.SIZE_6PARTY: // collision with PGT, same size.
                    if (BitConverter.ToUInt16(data, 0x4) != 0) // Bad Sanity?
                        return -1;
                    if (BitConverter.ToUInt32(data, 0x06) == PKX.GetCHK(data))
                        return 6;
                    if (BitConverter.ToUInt16(data, 0x58) != 0) // Encrypted?
                    {
                        for (int i = data.Length - 0x10; i < data.Length; i++) // 0x10 of 00's at the end != PK6
                            if (data[i] != 0)
                                return 6;
                        return -1;
                    }
                    return 6;
            }
            return -1;
        }

        /// <summary>
        /// Creates an instance of <see cref="PKM"/> from the given data.
        /// </summary>
        /// <param name="data">Raw data of the Pokemon file.</param>
        /// <param name="ident">Optional identifier for the Pokemon.  Usually the full path of the source file.</param>
        /// <param name="prefer">Optional identifier for the preferred generation.  Usually the generation of the destination save file.</param>
        /// <returns>An instance of <see cref="PKM"/> created from the given <paramref name="data"/>, or null if <paramref name="data"/> is invalid.</returns>
        public static PKM GetPKMfromBytes(byte[] data, string ident = null, int prefer = 7)
        {
            CheckEncrypted(ref data);
            switch (GetPKMDataFormat(data))
            {
                case 1:
                    var PL1 = new PokemonList1(data, PokemonList1.CapacityType.Single, data.Length == PKX.SIZE_1JLIST);
                    if (ident != null)
                        PL1[0].Identifier = ident;
                    return PL1[0];
                case 2:
                    var PL2 = new PokemonList2(data, PokemonList2.CapacityType.Single, data.Length == PKX.SIZE_2JLIST);
                    if (ident != null)
                        PL2[0].Identifier = ident;
                    return PL2[0];
                case 3:
                    switch (data.Length) { 
                        case PKX.SIZE_3CSTORED: return new CK3(data, ident);
                        case PKX.SIZE_3XSTORED: return new XK3(data, ident);
                        default: return new PK3(data, ident);
                    }
                case 4:
                    var pk = new PK4(data, ident);
                    if (!pk.Valid || pk.Sanity != 0)
                    {
                        var bk = new BK4(data, ident);
                        if (bk.Valid)
                            return bk;
                    }
                    return pk;
                case 5:
                    return new PK5(data, ident);
                case 6:
                    var pkx = new PK6(data, ident);
                    return CheckPKMFormat7(pkx, prefer);
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Checks if the input PK6 file is really a PK7, if so, updates the object.
        /// </summary>
        /// <param name="pk">PKM to check</param>
        /// <param name="prefer">Prefer a certain generation over another</param>
        /// <returns>Updated PKM if actually PK7</returns>
        private static PKM CheckPKMFormat7(PK6 pk, int prefer) => IsPK6FormatReallyPK7(pk, prefer) ? new PK7(pk.Data, pk.Identifier) : (PKM)pk;
        /// <summary>
        /// Checks if the input PK6 file is really a PK7.
        /// </summary>
        /// <param name="pk">PK6 to check</param>
        /// <param name="preferredFormat">Prefer a certain generation over another</param>
        /// <returns>Boolean is a PK7</returns>
        private static bool IsPK6FormatReallyPK7(PK6 pk, int preferredFormat)
        {
            if (pk.Version > Legal.MaxGameID_6)
                return true;
            if (pk.Enjoyment != 0 || pk.Fullness != 0)
                return false;

            // Check Ranges
            if (pk.Species > Legal.MaxSpeciesID_6)
                return true;
            if (pk.Moves.Any(move => move > Legal.MaxMoveID_6_AO))
                return true;
            if (pk.RelearnMoves.Any(move => move > Legal.MaxMoveID_6_AO))
                return true;
            if (pk.Ability > Legal.MaxAbilityID_6_AO)
                return true;
            if (pk.HeldItem > Legal.MaxItemID_6_AO)
                return true;

            int et = pk.EncounterType;
            if (et != 0)
            {
                if (pk.CurrentLevel < 100) // can't be hyper trained
                    return false;

                if (pk.GenNumber != 4) // can't have encounter type
                    return true;
                if (et > 24) // invalid encountertype
                    return true;
            }

            int mb = BitConverter.ToUInt16(pk.Data, 0x16);
            if (mb > 0xAAA)
                return false;
            for (int i = 0; i < 6; i++)
                if ((mb >> (i << 1) & 3) == 3) // markings are 10 or 01 (or 00), never 11
                    return false;

            return preferredFormat > 6;
        }

        /// <summary>
        /// Checks if the input <see cref="PKM"/> file is capable of being converted to the desired format.
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsConvertibleToFormat(PKM pk, int format)
        {
            if (pk.Format >= 3 && pk.Format > format)
                return false; // pk3->upward can't go backwards
            if (pk.Format <= 2 && format > 2 && format < 7)
                return false; // pk1/2->upward has to be 7 or greater
            return true;
        }

        /// <summary>
        /// Converts a PKM from one Generation 3 format to another. If it matches the destination format, the conversion will automatically return.
        /// </summary>
        /// <param name="pk">PKM to convert</param>
        /// <param name="PKMType">Format/Type to convert to</param>
        /// <param name="comment">Comments regarding the transfer's success/failure</param>        
        /// <returns>Converted PKM</returns>
        public static PKM ConvertToType(PKM pk, Type PKMType, out string comment)
        {
            if (pk == null)
            {
                comment = $"Bad {nameof(pk)} input. Aborting.";
                return null;
            }

            Type fromType = pk.GetType();
            if (fromType == PKMType)
            {
                comment = "No need to convert, current format matches requested format.";
                return pk;
            }

            if (IsNotTransferrable(pk, out comment))
                return null;

            Debug.WriteLine($"Trying to convert {fromType.Name} to {PKMType.Name}.");

            int fromFormat = int.Parse(fromType.Name.Last().ToString());
            int toFormat = int.Parse(PKMType.Name.Last().ToString());
            if (fromFormat > toFormat && fromFormat != 2)
            {
                comment = $"Cannot convert a {fromType.Name} to a {PKMType.Name}.";
                return null;
            }

            PKM pkm = pk.Clone();
            if (pkm.IsEgg)
                ForceHatchPKM(pkm);

            switch (fromType.Name)
            {
                case nameof(PK1):
                    if (toFormat == 7) // VC->Bank
                        pkm = ((PK1)pk).ConvertToPK7();
                    else if (toFormat == 2) // GB<->GB
                        pkm = ((PK1)pk).ConvertToPK2();
                    break;
                case nameof(PK2):
                    if (toFormat == 7) // VC->Bank
                        pkm = ((PK2)pk).ConvertToPK7();
                    else if (toFormat == 1) // GB<->GB
                    {
                        if (pk.Species > 151)
                        {
                            comment = $"Cannot convert a {PKX.GetSpeciesName(pkm.Species, pkm.Japanese ? 1 : 2)} to {PKMType.Name}";
                            return null;
                        }
                        pkm = ((PK2)pk).ConvertToPK1();
                        pkm.ClearInvalidMoves();
                    }
                    break;
                case nameof(CK3):
                case nameof(XK3):
                    // interconverting C/XD needs to visit main series format
                    // ends up stripping purification/shadow etc stats
                    pkm = pkm.ConvertToPK3();
                    goto case nameof(PK3); // fall through
                case nameof(PK3):
                    if (toFormat == 3) // Gen3 Inter-trading
                    {
                        pkm = InterConvertPK3(pkm, PKMType);
                        break;
                    }
                    if (fromType.Name != nameof(PK3))
                        pkm = pkm.ConvertToPK3();

                    pkm = ((PK3) pkm).ConvertToPK4();
                    if (toFormat == 4)
                        break;
                    goto case nameof(PK4);
                case nameof(BK4):
                    pkm = ((BK4) pkm).ConvertToPK4();
                    if (toFormat == 4)
                        break;
                    goto case nameof(PK4);
                case nameof(PK4):
                    if (PKMType == typeof(BK4))
                    {
                        pkm = ((PK4) pkm).ConvertToBK4();
                        break;
                    }
                    pkm = ((PK4) pkm).ConvertToPK5();
                    if (toFormat == 5)
                        break;
                    goto case nameof(PK5);
                case nameof(PK5):
                    pkm = ((PK5) pkm).ConvertToPK6();
                    if (toFormat == 6)
                        break;
                    goto case nameof(PK6);
                case nameof(PK6):
                    if (pkm.Species == 25 && pkm.AltForm != 0) // cosplay pikachu
                    {
                        comment = "Cannot transfer Cosplay Pikachu forward.";
                        return null;
                    }
                    pkm = ((PK6) pkm).ConvertToPK7();
                    if (toFormat == 7)
                        break;
                    goto case nameof(PK7);
                case nameof(PK7):
                    break;
            }

            comment = pkm == null
                ? $"Cannot convert a {fromType.Name} to a {PKMType.Name}."
                : $"Converted from {fromType.Name} to {PKMType.Name}.";

            return pkm;
        }

        /// <summary>
        /// Checks to see if a PKM is transferrable relative to in-game restrictions and <see cref="PKM.AltForm"/>.
        /// </summary>
        /// <param name="pk">PKM to convert</param>
        /// <param name="comment">Comment indicating why the <see cref="PKM"/> is not transferrable.</param>
        /// <returns>Indication if Not Transferrable</returns>
        private static bool IsNotTransferrable(PKM pk, out string comment)
        {
            switch (pk.Species)
            {
                default:
                    comment = null;
                    return false;
                case 025 when pk.AltForm != 0 && pk.Gen6: // Cosplay Pikachu
                    comment = "Cannot transfer Cosplay Pikachu forward.";
                    return true;
                case 172 when pk.AltForm != 0 && pk.Gen4: // Spiky Eared Pichu
                    comment = "Cannot transfer Spiky-Eared Pichu forward.";
                    return true;
            }
        }

        /// <summary>
        /// Converts a PKM from one Generation 3 format to another. If it matches the destination format, the conversion will automatically return.
        /// </summary>
        /// <param name="pk">PKM to convert</param>
        /// <param name="desiredFormatType">Format/Type to convert to</param>
        /// <remarks><see cref="PK3"/>, <see cref="CK3"/>, and <see cref="XK3"/> are supported.</remarks>
        /// <returns>Converted PKM</returns>
        private static PKM InterConvertPK3(PKM pk, Type desiredFormatType)
        {
            // if already converted it instantly returns
            switch (desiredFormatType.Name)
            {
                case nameof(CK3):
                    return pk.ConvertToCK3();
                case nameof(XK3):
                    return pk.ConvertToXK3();
                case nameof(PK3):
                    return pk.ConvertToPK3();
                default: throw new FormatException();
            }
        }

        /// <summary>
        /// Force hatches a PKM by applying the current species name and a valid Met Location from the origin game.
        /// </summary>
        /// <param name="pkm">PKM to apply hatch details to</param>
        /// <remarks>
        /// <see cref="PKM.IsEgg"/> is not checked; can be abused to re-hatch already hatched <see cref="PKM"/> inputs.
        /// <see cref="PKM.MetDate"/> is not modified; must be updated manually if desired.
        /// </remarks>
        private static void ForceHatchPKM(PKM pkm)
        {
            pkm.IsEgg = false;
            pkm.Nickname = PKX.GetSpeciesNameGeneration(pkm.Species, pkm.Language, pkm.Format);
            var loc = EncounterSuggestion.GetSuggestedEggMetLocation(pkm);
            if (loc >= 0)
                pkm.Met_Location = loc;
        }

        /// <summary>
        /// Checks if a PKM is encrypted; if encrypted, decrypts the PKM.
        /// </summary>
        /// <remarks>The input PKM object is decrypted; no new object is returned.</remarks>
        /// <param name="pkm">PKM to check encryption for (and decrypt if appropriate).</param>
        public static void CheckEncrypted(ref byte[] pkm)
        {
            int format = GetPKMDataFormat(pkm);
            switch (format)
            {
                case 1:
                case 2: // no encryption
                    return;
                case 3:
                    if (pkm.Length == PKX.SIZE_3CSTORED || pkm.Length == PKX.SIZE_3XSTORED)
                        return; // no encryption for C/XD
                    ushort chk = 0;
                    for (int i = 0x20; i < PKX.SIZE_3STORED; i += 2)
                        chk += BitConverter.ToUInt16(pkm, i);
                    if (chk != BitConverter.ToUInt16(pkm, 0x1C))
                        pkm = PKX.DecryptArray3(pkm);
                    return;
                case 4:
                case 5:
                    if (BitConverter.ToUInt16(pkm, 4) != 0) // BK4
                        return;
                    if (BitConverter.ToUInt32(pkm, 0x64) != 0)
                        pkm = PKX.DecryptArray45(pkm);
                    return;
                case 6:
                case 7:
                    if (BitConverter.ToUInt16(pkm, 0xC8) != 0 && BitConverter.ToUInt16(pkm, 0x58) != 0)
                        pkm = PKX.DecryptArray(pkm);
                    return;
                default:
                    return; // bad!
            }
        }

        /// <summary>
        /// Checks if the input <see cref="PKM"/> is compatible with the target <see cref="PKM"/>.
        /// </summary>
        /// <param name="pk">Input to check -> update/sanitize</param>
        /// <param name="target">Target type PKM with misc properties accessible for checking.</param>
        /// <param name="c">Comment output</param>
        /// <param name="pkm">Output compatible PKM</param>
        /// <returns>Indication if the input is (now) compatible with the target.</returns>
        public static bool TryMakePKMCompatible(PKM pk, PKM target, out string c, out PKM pkm)
        {
            if (!IsConvertibleToFormat(pk, target.Format))
            {
                pkm = null;
                c = $"Can't load {pk.GetType().Name}s to Gen{target.Format} saves.";
                return false;
            }
            if (target.Format < 3 && pk.Japanese != target.Japanese)
            {
                pkm = null;
                var strs = new[] { "International", "Japanese" };
                var val = target.Japanese ? 0 : 1;
                c = $"Cannot load {strs[val]} {pk.GetType().Name}s to {strs[val ^ 1]} saves.";
                return false;
            }
            pkm = ConvertToType(pk, target.GetType(), out c);
            Debug.WriteLine(c);
            return pkm != null;
        }

        /// <summary>
        /// Gets a Blank <see cref="PKM"/> object of the specified type.
        /// </summary>
        /// <param name="t">Type of <see cref="PKM"/> instance desired.</param>
        /// <returns>New instance of a blank <see cref="PKM"/> object.</returns>
        public static PKM GetBlank(Type t) => (PKM)Activator.CreateInstance(t, Enumerable.Repeat(null as PKM, t.GetTypeInfo().DeclaredConstructors.First().GetParameters().Length).ToArray());

        public static void TransferProperties(PKM source, PKM dest)
        {
            source.TransferPropertiesWithReflection(source, dest);
        }
    }
}

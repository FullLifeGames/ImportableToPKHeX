﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PKHeX.Core
{
    /// <summary>
    /// Logic for <see cref="SaveFile"/> data loading and manipulation.
    /// </summary>
    public static class SaveUtil
    {
        public const int BEEF = 0x42454546;

        public const int SIZE_G7USUM = 0x6CC00;
        public const int SIZE_G7SM = 0x6BE00;
        public const int SIZE_G6XY = 0x65600;
        public const int SIZE_G6ORAS = 0x76000;
        public const int SIZE_G6ORASDEMO = 0x5A00;
        public const int SIZE_G5RAW = 0x80000;
        public const int SIZE_G5BW = 0x24000;
        public const int SIZE_G5B2W2 = 0x26000;
        public const int SIZE_G4BR = 0x380000;
        public const int SIZE_G4RAW = 0x80000;
        public const int SIZE_G3BOX = 0x76000;
        public const int SIZE_G3BOXGCI = 0x76040; // +64 if has GCI data
        public const int SIZE_G3COLO = 0x60000;
        public const int SIZE_G3COLOGCI = 0x60040; // +64 if has GCI data
        public const int SIZE_G3XD = 0x56000;
        public const int SIZE_G3XDGCI = 0x56040; // +64 if has GCI data
        public const int SIZE_G3RAW = 0x20000;
        public const int SIZE_G3RAWHALF = 0x10000;
        public const int SIZE_G2RAW_U = 0x8000;
        public const int SIZE_G2VC = 0x8010;
        public const int SIZE_G2BAT_U = 0x802C;
        public const int SIZE_G2EMU_U = 0x8030;
        public const int SIZE_G2RAW_J = 0x10000;
        public const int SIZE_G2BAT_J = 0x1002C;
        public const int SIZE_G2EMU_J = 0x10030;
        public const int SIZE_G1RAW = 0x8000;
        public const int SIZE_G1BAT = 0x802C;
        private static readonly HashSet<int> SIZES = new HashSet<int>
        {
            SIZE_G7SM, SIZE_G7USUM,
            SIZE_G6XY, SIZE_G6ORAS, SIZE_G6ORASDEMO,
            SIZE_G5RAW, SIZE_G5BW, SIZE_G5B2W2,
            SIZE_G4BR, SIZE_G4RAW,
            SIZE_G3BOX, SIZE_G3BOXGCI, SIZE_G3COLO, SIZE_G3COLOGCI, SIZE_G3XD, SIZE_G3XDGCI, SIZE_G3RAW, SIZE_G3RAWHALF,
            SIZE_G2RAW_U, SIZE_G2VC, SIZE_G2BAT_U, SIZE_G2EMU_U, SIZE_G2RAW_J, SIZE_G2BAT_J, SIZE_G2EMU_J,
            SIZE_G1RAW, SIZE_G1BAT
        };

        private static readonly byte[] FOOTER_DSV = Encoding.ASCII.GetBytes("|-DESMUME SAVE-|");
        internal static readonly string[] HEADER_COLO =   { "GC6J","GC6E","GC6P" }; // NTSC-J, NTSC-U, PAL
        internal static readonly string[] HEADER_XD =     { "GXXJ","GXXE","GXXP" }; // NTSC-J, NTSC-U, PAL
        internal static readonly string[] HEADER_RSBOX =  { "GPXJ","GPXE","GPXP" }; // NTSC-J, NTSC-U, PAL

        /// <summary>Determines the generation of the given save data.</summary>
        /// <param name="data">Save data of which to determine the generation</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        private static GameVersion GetSAVGeneration(byte[] data)
        {
            if (GetIsG1SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen1;
            if (GetIsG2SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen2;
            if (GetIsG3SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen3;
            if (GetIsG4SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen4;
            if (GetIsG5SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen5;
            if (GetIsG6SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen6;
            if (GetIsG7SAV(data) != GameVersion.Invalid)
                return GameVersion.Gen7;

            if (GetIsG3COLOSAV(data) != GameVersion.Invalid)
                return GameVersion.COLO;
            if (GetIsG3XDSAV(data) != GameVersion.Invalid)
                return GameVersion.XD;
            if (GetIsG3BOXSAV(data) != GameVersion.Invalid)
                return GameVersion.RSBOX;
            if (GetIsG4BRSAV(data) != GameVersion.Invalid)
                return GameVersion.BATREV;

            return GameVersion.Invalid;
        }
        /// <summary>
        /// Determines if a Gen2 Pokémon List is Invalid
        /// </summary>
        /// <param name="data">Save data</param>
        /// <param name="offset">Offset the list starts at</param>
        /// <param name="listCount">Max count of Pokémon in the list</param>
        /// <returns>True if a valid list, False otherwise</returns>
        private static bool IsG12ListValid(byte[] data, int offset, int listCount)
        {
            byte num_entries = data[offset];
            return num_entries <= listCount && data[offset + 1 + num_entries] == 0xFF;
        }
        /// <summary>Determines the type of 1st gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG1SAV(byte[] data)
        {
            if (data.Length != SIZE_G1RAW && data.Length != SIZE_G1BAT)
                return GameVersion.Invalid;

            // Check if it's not an american save or a japanese save
            if (!(GetIsG1SAVU(data) || GetIsG1SAVJ(data)))
                return GameVersion.Invalid;
            // I can't actually detect which game version, because it's not stored anywhere.
            // If you can think of anything to do here, please implement :)
            return GameVersion.RBY;
        }
        /// <summary>Determines if 1st gen save is non-japanese</summary>
        /// <param name="data">Save data of which to determine the region</param>
        /// <returns>True if a valid non-japanese save, False otherwise.</returns>
        private static bool GetIsG1SAVU(byte[] data)
        {
            return IsG12ListValid(data, 0x2F2C, 20) && IsG12ListValid(data, 0x30C0, 20);
        }
        /// <summary>Determines if 1st gen save is japanese</summary>
        /// <param name="data">Save data of which to determine the region</param>
        /// <returns>True if a valid japanese save, False otherwise.</returns>
        internal static bool GetIsG1SAVJ(byte[] data)
        {
            return IsG12ListValid(data, 0x2ED5, 30) && IsG12ListValid(data, 0x302D, 30);
        }
        /// <summary>Determines the type of 2nd gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG2SAV(byte[] data)
        {
            if (!new[] {SIZE_G2RAW_J, SIZE_G2RAW_U, SIZE_G2BAT_J, SIZE_G2BAT_U, SIZE_G2EMU_U, SIZE_G2EMU_J, SIZE_G2VC}.Contains(data.Length))
                return GameVersion.Invalid;

            // Check if it's not an american save or a japanese save
            GameVersion result;
            if ((result = GetIsG2SAVU(data)) != GameVersion.Invalid)
                return result;
            if ((result = GetIsG2SAVJ(data)) != GameVersion.Invalid)
                return result;
            if ((result = GetIsG2SAVK(data)) != GameVersion.Invalid)
                return result;
            return GameVersion.Invalid;
        }
        /// <summary>Determines if 2nd gen save is non-japanese</summary>
        /// <param name="data">Save data of which to determine the region</param>
        /// <returns>True if a valid international save, False otherwise.</returns>
        private static GameVersion GetIsG2SAVU(byte[] data)
        {
            if (IsG12ListValid(data, 0x288A, 20) && IsG12ListValid(data, 0x2D6C, 20))
                return GameVersion.GS;
            if (IsG12ListValid(data, 0x2865, 20) && IsG12ListValid(data, 0x2D10, 20))
                return GameVersion.C;
            return GameVersion.Invalid;
        }
        /// <summary>Determines if 2nd gen save is japanese</summary>
        /// <param name="data">Save data of which to determine the region</param>
        /// <returns>True if a valid Japanese save, False otherwise.</returns>
        internal static GameVersion GetIsG2SAVJ(byte[] data)
        {
            if (!IsG12ListValid(data, 0x2D10, 30))
                return GameVersion.Invalid;
            if (IsG12ListValid(data, 0x283E, 30))
                return GameVersion.GS;
            if (IsG12ListValid(data, 0x281A, 30))
                return GameVersion.C;
            return GameVersion.Invalid;
        }
        /// <summary>Determines if 2nd gen save is Korean</summary>
        /// <param name="data">Save data of which to determine the region</param>
        /// <returns>True if a valid Korean save, False otherwise.</returns>
        internal static GameVersion GetIsG2SAVK(byte[] data)
        {
            if (IsG12ListValid(data, 0x2DAE, 20) && IsG12ListValid(data, 0x28CC, 20))
                return GameVersion.GS;
            return GameVersion.Invalid;
        }
        /// <summary>Determines the type of 3rd gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG3SAV(byte[] data)
        {
            if (data.Length != SIZE_G3RAW && data.Length != SIZE_G3RAWHALF)
                return GameVersion.Invalid;

            // check the save file(s)
            int count = data.Length/SIZE_G3RAWHALF;
            for (int s = 0; s < count; s++)
            {
                int ofs = 0xE000*s;
                int[] BlockOrder = new int[14];
                for (int i = 0; i < 14; i++)
                    BlockOrder[i] = BitConverter.ToInt16(data, i * 0x1000 + 0xFF4 + ofs);

                if (BlockOrder.Any(i => i > 0xD || i < 0))
                    continue;

                // Detect RS/E/FRLG
                // Section 0 stores Game Code @ 0x00AC; 0 for RS, 1 for FRLG, else for Emerald
                int Block0 = Array.IndexOf(BlockOrder, 0);

                // Sometimes not all blocks are present (start of game), yielding multiple block0's.
                // Real 0th block comes before block1.
                if (BlockOrder[0] == 1 && Block0 != BlockOrder.Length - 1)
                    continue;
                if (BlockOrder.Count(v => v == 0) == BlockOrder.Length)
                    continue;
                uint GameCode = BitConverter.ToUInt32(data, Block0 * 0x1000 + 0xAC + ofs);
                switch (GameCode)
                {
                    case 0: return GameVersion.RS;
                    case 1: return GameVersion.FRLG;
                    case uint.MaxValue: return GameVersion.Unknown;  // what a hack
                    default: return BitConverter.ToUInt32(data, Block0 * 0x1000 + 0x1F4 + ofs) == 0 
                            ? GameVersion.RS 
                            : GameVersion.E;
                }
            }
            return GameVersion.Invalid;
        }
        /// <summary>Determines the type of 3rd gen Box RS</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG3BOXSAV(byte[] data)
        {
            if (data.Length != SIZE_G3BOX && data.Length != SIZE_G3BOXGCI)
                return GameVersion.Invalid;

            byte[] sav = data;

            // Verify first checksum
            ushort chk = 0; // initial value
            var ofs = data.Length - SIZE_G3BOX + 0x2000;
            for (int i = 0x4; i < 0x1FFC; i += 2)
                chk += BigEndian.ToUInt16(sav, ofs + i);

            ushort chkA = chk;
            ushort chkB = (ushort)(0xF004 - chkA);

            ushort CHK_A = BigEndian.ToUInt16(sav, ofs + 0);
            ushort CHK_B = BigEndian.ToUInt16(sav, ofs + 2);

            return CHK_A == chkA && CHK_B == chkB ? GameVersion.RSBOX : GameVersion.Invalid;
        }
        /// <summary>Determines the type of 3rd gen Colosseum</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG3COLOSAV(byte[] data)
        {
            if (data.Length != SIZE_G3COLO && data.Length != SIZE_G3COLOGCI)
                return GameVersion.Invalid;

            // Check the intro bytes for each save slot
            int offset = data.Length - SIZE_G3COLO;
            for (int i = 0; i < 3; i++)
            {
                var ofs = 0x6000 + offset + 0x1E000 * i;
                if (BitConverter.ToUInt32(data, ofs) != 0x00000101)
                    return GameVersion.Invalid;
            }
            return GameVersion.COLO;
        }
        /// <summary>Determines the type of 3rd gen XD</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG3XDSAV(byte[] data)
        {
            if (data.Length != SIZE_G3XD && data.Length != SIZE_G3XDGCI)
                return GameVersion.Invalid;

            // Check the intro bytes for each save slot
            int offset = data.Length - SIZE_G3XD;
            for (int i = 0; i < 2; i++)
            {
                var ofs = 0x6000 + offset + 0x28000 * i;
                if ((BitConverter.ToUInt32(data, ofs) & 0xFFFE_FFFF) != 0x00000101)
                    return GameVersion.Invalid;
            }
            return GameVersion.XD;
        }
        /// <summary>Determines the type of 4th gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG4SAV(byte[] data)
        {
            if (data.Length != SIZE_G4RAW)
                return GameVersion.Invalid;
            
            // General Block Checksum
            if (BitConverter.ToUInt16(data, 0xC0FE) == CRC16_CCITT(data, 0, 0xC0EC))
                return GameVersion.DP;
            if (BitConverter.ToUInt16(data, 0xCF2A) == CRC16_CCITT(data, 0, 0xCF18))
                return GameVersion.Pt;
            if (BitConverter.ToUInt16(data, 0xF626) == CRC16_CCITT(data, 0, 0xF618))
                return GameVersion.HGSS;

            // General Block Checksum is invalid, check for block identifiers
            if (data.Skip(0xC0F4).Take(10).SequenceEqual(new byte[] { 0x00, 0xC1, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.DP;
            if (data.Skip(0xCF20).Take(10).SequenceEqual(new byte[] { 0x2C, 0xCF, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.Pt;
            if (data.Skip(0xF61C).Take(10).SequenceEqual(new byte[] { 0x28, 0xF6, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.HGSS;

            // Check the other save
            if (data.Skip(0xC0F4 + 0x40000).Take(10).SequenceEqual(new byte[] { 0x00, 0xC1, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.DP;
            if (data.Skip(0xCF20 + 0x40000).Take(10).SequenceEqual(new byte[] { 0x2C, 0xCF, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.Pt;
            if (data.Skip(0xF61C + 0x40000).Take(10).SequenceEqual(new byte[] { 0x28, 0xF6, 0x00, 0x00, 0x23, 0x06, 0x06, 0x20, 0x00, 0x00 }))
                return GameVersion.HGSS;

            return GameVersion.Invalid;
        }
        /// <summary>Determines the type of 4th gen Battle Revolution</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG4BRSAV(byte[] data)
        {
            if (data.Length != SIZE_G4BR)
                return GameVersion.Invalid;

            byte[] sav = SAV4BR.DecryptPBRSaveData(data);
            return SAV4BR.IsChecksumsValid(sav) ? GameVersion.BATREV : GameVersion.Invalid;
        }
        /// <summary>Determines the type of 5th gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        internal static GameVersion GetIsG5SAV(byte[] data)
        {
            if (data.Length != SIZE_G5RAW)
                return GameVersion.Invalid;

            ushort chk1 = BitConverter.ToUInt16(data, SIZE_G5BW - 0x100 + 0x8C + 0xE);
            ushort actual1 = CRC16_CCITT(data, SIZE_G5BW - 0x100, 0x8C);
            if (chk1 == actual1)
                return GameVersion.BW;
            ushort chk2 = BitConverter.ToUInt16(data, SIZE_G5B2W2 - 0x100 + 0x94 + 0xE);
            ushort actual2 = CRC16_CCITT(data, SIZE_G5B2W2 - 0x100, 0x94);
            if (chk2 == actual2)
                return GameVersion.B2W2;
            return GameVersion.Invalid;
        }
        /// <summary>Determines the type of 6th gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        private static GameVersion GetIsG6SAV(byte[] data)
        {
            if (!new []{SIZE_G6XY, SIZE_G6ORAS, SIZE_G6ORASDEMO}.Contains(data.Length))
                return GameVersion.Invalid;

            if (BitConverter.ToUInt32(data, data.Length - 0x1F0) != BEEF)
                return GameVersion.Invalid;

            switch (data.Length)
            {
                case SIZE_G6XY:
                    return GameVersion.XY;
                case SIZE_G6ORASDEMO:
                    return GameVersion.ORASDEMO;
                case SIZE_G6ORAS:
                    return GameVersion.ORAS;
            }
            return GameVersion.Invalid;
        }
        /// <summary>Determines the type of 7th gen save</summary>
        /// <param name="data">Save data of which to determine the type</param>
        /// <returns>Version Identifier or Invalid if type cannot be determined.</returns>
        private static GameVersion GetIsG7SAV(byte[] data)
        {
            if (data.Length != SIZE_G7SM && data.Length != SIZE_G7USUM)
                return GameVersion.Invalid;

            if (BitConverter.ToUInt32(data, data.Length - 0x1F0) != BEEF)
                return GameVersion.Invalid;

            switch (data.Length)
            {
                case SIZE_G7SM:
                    return GameVersion.SM;
                case SIZE_G7USUM:
                    return GameVersion.USUM;
            }
            return GameVersion.Invalid;
        }


        /// <summary>Creates an instance of a SaveFile using the given save data.</summary>
        /// <param name="data">Save data from which to create a SaveFile.</param>
        /// <returns>An appropriate type of save file for the given data, or null if the save data is invalid.</returns>
        public static SaveFile GetVariantSAV(byte[] data)
        {
            // Pre-check for header/footer signatures
            SaveFile sav;
            byte[] header = new byte[0], footer = new byte[0];
            CheckHeaderFooter(ref data, ref header, ref footer);

            switch (GetSAVGeneration(data))
            {
                // Main Games
                case GameVersion.Gen1:      sav = new SAV1(data); break;
                case GameVersion.Gen2:      sav = new SAV2(data); break;
                case GameVersion.Gen3:      sav = new SAV3(data); break;
                case GameVersion.Gen4:      sav = new SAV4(data); break;
                case GameVersion.Gen5:      sav = new SAV5(data); break;
                case GameVersion.Gen6:      sav = new SAV6(data); break;
                case GameVersion.Gen7:      sav = new SAV7(data); break;

                // Side Games
                case GameVersion.COLO:      sav = new SAV3Colosseum(data); break;
                case GameVersion.XD:        sav = new SAV3XD(data); break;
                case GameVersion.RSBOX:     sav = new SAV3RSBox(data); break;
                case GameVersion.BATREV:    sav = new SAV4BR(data); break;
                
                // No pattern matched
                default: return null;
            }
            sav.Header = header;
            sav.Footer = footer;
            return sav;
        }
        public static SaveFile GetVariantSAV(SAV3GCMemoryCard MC)
        {
            // Pre-check for header/footer signatures
            SaveFile sav;
            byte[] header = new byte[0], footer = new byte[0];
            byte[] data = MC.SelectedSaveData;
            CheckHeaderFooter(ref data, ref header, ref footer);

            switch (MC.SelectedGameVersion)
            {
                // Side Games
                case GameVersion.COLO: sav = new SAV3Colosseum(data, MC); break;
                case GameVersion.XD: sav = new SAV3XD(data, MC); break;
                case GameVersion.RSBOX: sav = new SAV3RSBox(data, MC); break;

                // No pattern matched
                default: return null;
            }
            sav.Header = header;
            sav.Footer = footer;
            return sav;
        }

        /// <summary>
        /// Creates an instance of a SaveFile with a blank base.
        /// </summary>
        /// <param name="Game">Version to create the save file for.</param>
        /// <param name="OT">Trainer Name</param>
        /// <returns></returns>
        public static SaveFile GetBlankSAV(GameVersion Game, string OT)
        {
            var SAV = GetBlankSAV(Game);
            if (SAV == null)
                return null;

            SAV.Game = (int)Game;
            SAV.OT = OT;

            // Secondary Properties may not be used but can be filled in as template.
            SAV.TID = 12345;
            SAV.SID = 54321;
            SAV.Language = (int)LanguageID.English; // English
            SAV.Country = 49; // USA
            SAV.SubRegion = 7; // CA
            SAV.ConsoleRegion = 1; // Americas

            return SAV;
        }
        /// <summary>
        /// Creates an instance of a SaveFile with a blank base.
        /// </summary>
        /// <param name="Game">Version to create the save file for.</param>
        /// <returns></returns>
        private static SaveFile GetBlankSAV(GameVersion Game)
        {
            switch (Game)
            {
                case GameVersion.RBY:
                    return new SAV1();

                case GameVersion.GS: case GameVersion.C: case GameVersion.GSC:
                    return new SAV2();

                case GameVersion.R: case GameVersion.S: case GameVersion.E: case GameVersion.FR: case GameVersion.LG:
                    return new SAV3(versionOverride: Game);

                case GameVersion.COLO:
                    return new SAV3Colosseum();
                case GameVersion.XD:
                    return new SAV3XD();
                case GameVersion.RSBOX:
                    return new SAV3RSBox();

                case GameVersion.D: case GameVersion.P: case GameVersion.DP:
                    return new SAV4(new byte[SIZE_G4RAW], GameVersion.DP);
                case GameVersion.Pt:
                    return new SAV4(new byte[SIZE_G4RAW], GameVersion.Pt);
                case GameVersion.HG: case GameVersion.SS: case GameVersion.HGSS:
                    return new SAV4(new byte[SIZE_G4RAW], GameVersion.HGSS);

                case GameVersion.B: case GameVersion.W: case GameVersion.BW:
                    return new SAV5(new byte[SIZE_G5RAW], GameVersion.BW);
                case GameVersion.B2: case GameVersion.W2: case GameVersion.B2W2:
                    return new SAV5(new byte[SIZE_G5RAW], GameVersion.B2W2);

                case GameVersion.X: case GameVersion.Y: case GameVersion.XY:
                    return new SAV6(new byte[SIZE_G6XY]);
                case GameVersion.ORASDEMO:
                    return new SAV6(new byte[SIZE_G6ORASDEMO]);
                case GameVersion.OR: case GameVersion.AS: case GameVersion.ORAS:
                    return new SAV6(new byte[SIZE_G6ORAS]);

                case GameVersion.SN: case GameVersion.MN: case GameVersion.SM:
                    return new SAV7(new byte[SIZE_G7SM]);
                case GameVersion.US: case GameVersion.UM: case GameVersion.USUM:
                    return new SAV7(new byte[SIZE_G7USUM]);

                default:
                    return null;
            }
        }
        /// <summary>
        /// Creates an instance of a SaveFile with a blank base.
        /// </summary>
        /// <param name="generation">Generation of the Save File.</param>
        /// <param name="OT">Trainer Name</param>
        /// <returns>Save File for that generation.</returns>
        public static SaveFile GetBlankSAV(int generation, string OT)
        {
            var ver = GameUtil.GetVersion(generation);
            return GetBlankSAV(ver, OT);
        }

        /// <summary>
        /// Retrieves the full path of the most recent file based on <see cref="FileInfo.LastWriteTime"/>.
        /// </summary>
        /// <param name="folderPath">Folder to look within</param>
        /// <param name="deep">Search all subfolders</param>
        /// <param name="result">If this function returns true, full path of all <see cref="SaveFile"/> that match criteria. If this function returns false, the error message, or null if the directory could not be found</param>
        /// <returns>Boolean indicating whether or not operation was successful.</returns>
        public static bool GetSavesFromFolder(string folderPath, bool deep, out IEnumerable<string> result)
        {
            if (!Directory.Exists(folderPath))
            {
                result = null;
                return false;
            }
            try
            {
                var searchOption = deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.EnumerateFiles(folderPath, "*", searchOption);
                result = files.Where(f => IsSizeValid((int)new FileInfo(f).Length));
                return true;
            }
            catch (ArgumentException)
            {
                result = new[] {"Error encountered when detecting saves in the following folder:" + Environment.NewLine + folderPath,
                    "Advise manually scanning to remove bad filenames from the folder." + Environment.NewLine + "Likely caused via Homebrew creating invalid filenames."};
                return false;
            }
        }

        /// <summary>
        /// Determines whether the save data size is valid for autodetecting saves.
        /// </summary>
        /// <param name="size">Size in bytes of the save data</param>
        /// <returns>A boolean indicating whether or not the save data size is valid.</returns>
        private static bool IsSizeValid(int size) => SIZES.Contains(size);

        // SAV Manipulation
        /// <summary>Calculates the CRC16-CCITT checksum over an input byte array.</summary>
        /// <param name="data">Input byte array</param>
        /// <param name="start">Starting point for checksum</param>
        /// <param name="length"></param>
        /// <returns>Checksum</returns>
        public static ushort CRC16_CCITT(byte[] data, int start, int length)
        {
            byte top = 0xFF;
            byte bot = 0xFF;
            int end = start + length;
            for (int i = start; i < end; i++)
            {
                var x = data[i] ^ top;
                x ^= (x >> 4);
                top = (byte) (bot ^ (x >> 3) ^ (x << 4));
                bot = (byte) (x ^ (x << 5));
            }
            return (ushort)(top << 8 | bot);
        }

        /// <summary>Calculates the CRC16-CCITT checksum over an input byte array.</summary>
        /// <param name="data">Input byte array</param>
        /// <returns>Checksum</returns>
        public static ushort CRC16_CCITT(byte[] data) => CRC16_CCITT(data, 0, data.Length);

        private static readonly ushort[] crc16 =
        {
            0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
            0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
            0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
            0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
            0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
            0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
            0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
            0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
            0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
            0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
            0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
            0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
            0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
            0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
            0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
            0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
            0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
            0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
            0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
            0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
            0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
            0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
            0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
            0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
            0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
            0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
            0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
            0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
        };

        /// <summary>Calculates the 16bit checksum over an input byte array. Used in Gen7 save files.</summary>
        /// <param name="data">Input byte array</param>
        /// <param name="start">Offset to start checksum at</param>
        /// <param name="length">Length of array to checksum</param>
        /// <param name="initial">Initial value for checksum</param>
        /// <returns>Checksum</returns>
        public static ushort CRC16(byte[] data, int start, int length, ushort initial = 0)
        {
            ushort chk = (ushort)~initial;
            for (var i = start; i < start + length; i++)
                chk = (ushort) (crc16[(data[i] ^ chk) & 0xFF] ^ chk >> 8);
            return (ushort)~chk;
        }
        public static byte[] Resign7(byte[] sav7)
        {
            return MemeCrypto.Resign7(sav7);
        }
        /// <summary>Calculates the 32bit checksum over an input byte array. Used in GBA save files.</summary>
        /// <param name="data">Input byte array</param>
        /// <returns>Checksum</returns>
        public static ushort CRC32(byte[] data)
        {
            uint val = 0;
            for (int i = 0; i < data.Length; i += 4)
                val += BitConverter.ToUInt32(data, i);
            return (ushort)(val + (val >> 16));
        }
        private static void CheckHeaderFooter(ref byte[] input, ref byte[] header, ref byte[] footer)
        {
            if (input.Length > SIZE_G4RAW) // DeSmuME Gen4/5 DSV
            {
                if (!FOOTER_DSV.SequenceEqual(input.Skip(input.Length - FOOTER_DSV.Length)))
                    return;
                footer = input.Skip(SIZE_G4RAW).ToArray();
                input = input.Take(SIZE_G4RAW).ToArray();
            }
            else if (input.Length == SIZE_G3BOXGCI)
            {
                if (!IsGameMatchHeader(HEADER_RSBOX, input))
                    return; // not gci
                header = input.Take(SIZE_G3BOXGCI - SIZE_G3BOX).ToArray();
                input = input.Skip(header.Length).ToArray();
            }
            else if (input.Length == SIZE_G3COLOGCI)
            {
                if (!IsGameMatchHeader(HEADER_COLO, input))
                    return; // not gci
                header = input.Take(SIZE_G3COLOGCI - SIZE_G3COLO).ToArray();
                input = input.Skip(header.Length).ToArray();
            }
            else if (input.Length == SIZE_G3XDGCI)
            {
                if (!IsGameMatchHeader(HEADER_XD, input))
                    return; // not gci
                header = input.Take(SIZE_G3XDGCI - SIZE_G3XD).ToArray();
                input = input.Skip(header.Length).ToArray();
            }
            bool IsGameMatchHeader(IEnumerable<string> headers, byte[] data) => headers.Contains(Encoding.ASCII.GetString(data, 0, 4));
        }

        private static readonly ushort[] formtable_SM = // u16 species, u16 formcount
        {
            0x0003, 0x0002, 0x0006, 0x0003, 0x0009, 0x0002, 0x000F, 0x0002,
            0x0012, 0x0002, 0x0013, 0x0002, 0x0014, 0x0003, 0x0019, 0x0007,
            0x001A, 0x0002, 0x001B, 0x0002, 0x001C, 0x0002, 0x0025, 0x0002,
            0x0026, 0x0002, 0x0032, 0x0002, 0x0033, 0x0002, 0x0034, 0x0002,
            0x0035, 0x0002, 0x0041, 0x0002, 0x004A, 0x0002, 0x004B, 0x0002,
            0x004C, 0x0002, 0x0050, 0x0002, 0x0058, 0x0002, 0x0059, 0x0002,
            0x005E, 0x0002, 0x0067, 0x0002, 0x0069, 0x0002, 0x0073, 0x0002,
            0x007F, 0x0002, 0x0082, 0x0002, 0x008E, 0x0002, 0x0096, 0x0003,
            0x00B5, 0x0002, 0x00C9, 0x001C, 0x00D0, 0x0002, 0x00D4, 0x0002,
            0x00D6, 0x0002, 0x00E5, 0x0002, 0x00F8, 0x0002, 0x00FE, 0x0002,
            0x0101, 0x0002, 0x0104, 0x0002, 0x011A, 0x0002, 0x012E, 0x0002,
            0x012F, 0x0002, 0x0132, 0x0002, 0x0134, 0x0002, 0x0136, 0x0002,
            0x013F, 0x0002, 0x0143, 0x0002, 0x014E, 0x0002, 0x015F, 0x0004,
            0x0162, 0x0002, 0x0167, 0x0002, 0x016A, 0x0002, 0x0175, 0x0002,
            0x0178, 0x0002, 0x017C, 0x0002, 0x017D, 0x0002, 0x017E, 0x0002,
            0x017F, 0x0002, 0x0180, 0x0002, 0x0182, 0x0004, 0x019C, 0x0003,
            0x019D, 0x0003, 0x01A5, 0x0002, 0x01A6, 0x0002, 0x01A7, 0x0002,
            0x01AC, 0x0002, 0x01BD, 0x0002, 0x01C0, 0x0002, 0x01CC, 0x0002,
            0x01DB, 0x0002, 0x01DF, 0x0006, 0x01E7, 0x0002, 0x01EC, 0x0002,
            0x01ED, 0x0012, 0x0213, 0x0002, 0x0226, 0x0002, 0x022B, 0x0002,
            0x0249, 0x0004, 0x024A, 0x0004, 0x0281, 0x0002, 0x0282, 0x0002,
            0x0285, 0x0002, 0x0286, 0x0003, 0x0287, 0x0002, 0x0288, 0x0002,
            0x0289, 0x0005, 0x0292, 0x0003, 0x029A, 0x0014, 0x029D, 0x0005,
            0x029E, 0x0006, 0x029F, 0x0005, 0x02A4, 0x000A, 0x02A6, 0x0002,
            0x02A9, 0x0002, 0x02C6, 0x0004, 0x02C7, 0x0004, 0x02CC, 0x0002,
            0x02CE, 0x0005, 0x02CF, 0x0002, 0x02D0, 0x0002, 0x02DF, 0x0002,
            0x02E2, 0x0002, 0x02E5, 0x0004, 0x02E9, 0x0002, 0x02EA, 0x0002,
            0x02F2, 0x0002, 0x02F6, 0x0002, 0x0305, 0x0012, 0x0306, 0x000E,
            0x030A, 0x0004, 0x0310, 0x0002, 0x0321, 0x0002,
        };
        private static readonly ushort[] formtable_USUM = // u16 species, u16 formcount
        {
            0x0003, 0x0002, 0x0006, 0x0003, 0x0009, 0x0002, 0x000F, 0x0002,
            0x0012, 0x0002, 0x0013, 0x0002, 0x0014, 0x0003, 0x0019, 0x0008,
            0x001A, 0x0002, 0x001B, 0x0002, 0x001C, 0x0002, 0x0025, 0x0002,
            0x0026, 0x0002, 0x0032, 0x0002, 0x0033, 0x0002, 0x0034, 0x0002,
            0x0035, 0x0002, 0x0041, 0x0002, 0x004A, 0x0002, 0x004B, 0x0002,
            0x004C, 0x0002, 0x0050, 0x0002, 0x0058, 0x0002, 0x0059, 0x0002,
            0x005E, 0x0002, 0x0067, 0x0002, 0x0069, 0x0003, 0x0073, 0x0002,
            0x007F, 0x0002, 0x0082, 0x0002, 0x008E, 0x0002, 0x0096, 0x0003,
            0x00B5, 0x0002, 0x00C9, 0x001C, 0x00D0, 0x0002, 0x00D4, 0x0002,
            0x00D6, 0x0002, 0x00E5, 0x0002, 0x00F8, 0x0002, 0x00FE, 0x0002,
            0x0101, 0x0002, 0x0104, 0x0002, 0x011A, 0x0002, 0x012E, 0x0002,
            0x012F, 0x0002, 0x0132, 0x0002, 0x0134, 0x0002, 0x0136, 0x0002,
            0x013F, 0x0002, 0x0143, 0x0002, 0x014E, 0x0002, 0x015F, 0x0004,
            0x0162, 0x0002, 0x0167, 0x0002, 0x016A, 0x0002, 0x0175, 0x0002,
            0x0178, 0x0002, 0x017C, 0x0002, 0x017D, 0x0002, 0x017E, 0x0002,
            0x017F, 0x0002, 0x0180, 0x0002, 0x0182, 0x0004, 0x019C, 0x0003,
            0x019D, 0x0003, 0x019E, 0x0003, 0x01A5, 0x0002, 0x01A6, 0x0002,
            0x01A7, 0x0002, 0x01AC, 0x0002, 0x01BD, 0x0002, 0x01C0, 0x0002,
            0x01CC, 0x0002, 0x01DB, 0x0002, 0x01DF, 0x0006, 0x01E7, 0x0002,
            0x01EC, 0x0002, 0x01ED, 0x0012, 0x0213, 0x0002, 0x0226, 0x0002,
            0x022B, 0x0002, 0x0249, 0x0004, 0x024A, 0x0004, 0x0281, 0x0002,
            0x0282, 0x0002, 0x0285, 0x0002, 0x0286, 0x0003, 0x0287, 0x0002,
            0x0288, 0x0002, 0x0289, 0x0005, 0x0292, 0x0003, 0x0298, 0x0014,
            0x0299, 0x0014, 0x029A, 0x0014, 0x029D, 0x0005, 0x029E, 0x0006,
            0x029F, 0x0005, 0x02A4, 0x000A, 0x02A6, 0x0002, 0x02A9, 0x0002,
            0x02C6, 0x0004, 0x02C7, 0x0004, 0x02CC, 0x0002, 0x02CE, 0x0005,
            0x02CF, 0x0002, 0x02D0, 0x0002, 0x02DF, 0x0002, 0x02E2, 0x0002,
            0x02E5, 0x0004, 0x02E7, 0x0002, 0x02E8, 0x0002, 0x02E9, 0x0003,
            0x02EA, 0x0002, 0x02F0, 0x0002, 0x02F2, 0x0002, 0x02F6, 0x0002,
            0x0305, 0x0012, 0x0306, 0x000E, 0x0309, 0x0002, 0x030A, 0x0004,
            0x0310, 0x0002, 0x0320, 0x0004, 0x0321, 0x0002
        };
        private static int GetDexFormBitIndex(int species, int formct, int start, IReadOnlyList<ushort> formtable)
        {
            int formindex = start;
            int f = 0;
            for (int i = 0; i < formtable.Count; i += 2)
            {
                int s = formtable[i];
                f = formtable[i + 1];
                if (s == species)
                    break;

                formindex += f - 1;
            }
            if (f > formct)
                return -1;
            return formindex;
        }
        private static int GetDexFormCount(int species, IReadOnlyList<ushort> formtable)
        {
            for (int i = 0; i < formtable.Count; i += 2)
                if (formtable[i] == species)
                    return formtable[i + 1];
            return 0;
        }
        public static int GetDexFormIndexBW(int species, int formct)
        {
            if (formct < 1 || species < 0)
                return -1; // invalid
            switch (species)
            {
                case 201: return 000; // 28 Unown
                case 386: return 028; // 4 Deoxys
                case 492: return 032; // 2 Shaymin
                case 487: return 034; // 2 Giratina
                case 479: return 036; // 6 Rotom
                case 422: return 042; // 2 Shellos
                case 423: return 044; // 2 Gastrodon
                case 412: return 046; // 3 Burmy
                case 413: return 049; // 3 Wormadam
                case 351: return 052; // 4 Castform
                case 421: return 056; // 2 Cherrim
                case 585: return 058; // 4 Deerling
                case 586: return 062; // 4 Sawsbuck
                case 648: return 066; // 2 Meloetta
                case 555: return 068; // 2 Darmanitan
                case 550: return 070; // 2 Basculin
                default: return -1;
            }
        }
        public static int GetDexFormIndexB2W2(int species, int formct)
        {
            if (formct < 1 || species < 0)
                return -1; // invalid
            switch (species)
            {
                case 646: return 072; // 3 Kyurem
                case 647: return 075; // 2 Keldeo
                case 642: return 077; // 2 Thundurus
                case 641: return 079; // 2 Tornadus
                case 645: return 081; // 2 Landorus
                default: return GetDexFormIndexBW(species, formct);
            }
        }
        public static int GetDexFormIndexXY(int species, int formct)
        {
            if (formct < 1 || species < 0)
                return -1; // invalid
            switch (species)
            {
                case 666: return 083; // 20 Vivillion
                case 669: return 103; // 5 Flabébé
                case 670: return 108; // 6 Floette
                case 671: return 114; // 5 Florges
                case 710: return 119; // 4 Pumpkaboo
                case 711: return 123; // 4 Gourgeist
                case 681: return 127; // 2 Aegislash
                case 716: return 129; // 2 Xerneas
                case 003: return 131; // 2 Venusaur
                case 006: return 133; // 3 Charizard
                case 009: return 136; // 2 Blastoise
                case 065: return 138; // 2 Alakazam
                case 094: return 140; // 2 Gengar
                case 115: return 142; // 2 Kangaskhan
                case 127: return 144; // 2 Pinsir
                case 130: return 146; // 2 Gyarados
                case 142: return 148; // 2 Aerodactyl
                case 150: return 150; // 3 Mewtwo
                case 181: return 153; // 2 Ampharos
                case 212: return 155; // 2 Scizor
                case 214: return 157; // 2 Heracros
                case 229: return 159; // 2 Houndoom
                case 248: return 161; // 2 Tyranitar
                case 257: return 163; // 2 Blaziken
                case 282: return 165; // 2 Gardevoir
                case 303: return 167; // 2 Mawile
                case 306: return 169; // 2 Aggron
                case 308: return 171; // 2 Medicham
                case 310: return 173; // 2 Manetric
                case 354: return 175; // 2 Banette
                case 359: return 177; // 2 Absol
                case 380: return 179; // 2 Latias
                case 381: return 181; // 2 Latios
                case 445: return 183; // 2 Garchomp
                case 448: return 185; // 2 Lucario
                case 460: return 187; // 2 Abomasnow
                default: return GetDexFormIndexB2W2(species, formct);
            }
        }
        public static int GetDexFormIndexORAS(int species, int formct)
        {
            if (formct < 1 || species < 0)
                return -1; // invalid
            switch (species)
            {
                case 025: return 189; // 7 Pikachu
                case 720: return 196; // 2 Hoopa
                case 015: return 198; // 2 Beedrill
                case 018: return 200; // 2 Pidgeot
                case 080: return 202; // 2 Slowbro
                case 208: return 204; // 2 Steelix
                case 254: return 206; // 2 Sceptile
                case 360: return 208; // 2 Swampert
                case 302: return 210; // 2 Sableye
                case 319: return 212; // 2 Sharpedo
                case 323: return 214; // 2 Camerupt
                case 334: return 216; // 2 Altaria
                case 362: return 218; // 2 Glalie
                case 373: return 220; // 2 Salamence
                case 376: return 222; // 2 Metagross
                case 384: return 224; // 2 Rayquaza
                case 428: return 226; // 2 Lopunny
                case 475: return 228; // 2 Gallade
                case 531: return 230; // 2 Audino
                case 719: return 232; // 2 Diancie
                case 382: return 234; // 2 Kyogre
                case 383: return 236; // 2 Groudon
                case 493: return 238; // 18 Arceus
                case 649: return 256; // 5 Genesect
                case 676: return 261; // 10 Furfrou
                default: return GetDexFormIndexXY(species, formct);
            }
        }
        public static int GetDexFormIndexSM(int species, int formct, int start) => GetDexFormBitIndex(species, formct, start, formtable_SM);
        public static int GetDexFormIndexUSUM(int species, int formct, int start) => GetDexFormBitIndex(species, formct, start, formtable_USUM);
        public static int GetDexFormCountSM(int species) => GetDexFormCount(species, formtable_SM);
        public static int GetDexFormCountUSUM(int species) => GetDexFormCount(species, formtable_USUM);

        public static int GetCXDVersionID(int gen3version)
        {
            switch ((GameVersion)gen3version)
            {
                case GameVersion.FR: return 1;
                case GameVersion.LG: return 2;
                case GameVersion.S: return 8;
                case GameVersion.R: return 9;
                case GameVersion.E: return 10;
                case GameVersion.CXD: return 11;
                default: return 0;
            }
        }
        public static int GetG3VersionID(int CXDversion)
        {
            switch (CXDversion)
            {
                case 1: return (int)GameVersion.FR;
                case 2: return (int)GameVersion.LG;
                case 8: return (int)GameVersion.S;
                case 9: return (int)GameVersion.R;
                case 10: return (int)GameVersion.E;
                case 11: return (int)GameVersion.CXD;
                default: return (int)GameVersion.Unknown;
            }
        }

        public static byte[] DecryptGC(byte[] input, int start, int end, ushort[] keys)
        {
            byte[] output = (byte[])input.Clone();
            for (int ofs = start; ofs < end; ofs += 8)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    ushort val = BigEndian.ToUInt16(input, ofs + i * 2);
                    val -= keys[i];
                    BigEndian.GetBytes(val).CopyTo(output, ofs + i * 2);
                }
                keys = AdvanceGCKeys(keys);
            }
            return output;
        }
        public static byte[] EncryptGC(byte[] input, int start, int end, ushort[] keys)
        {
            byte[] output = (byte[])input.Clone();
            for (int ofs = start; ofs < end; ofs += 8)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    ushort val = BigEndian.ToUInt16(input, ofs + i * 2);
                    val += keys[i];
                    BigEndian.GetBytes(val).CopyTo(output, ofs + i * 2);
                }
                keys = AdvanceGCKeys(keys);
            }
            return output;
        }

        public static ushort[] AdvanceGCKeys(ushort[] oldKeys)
        {
            oldKeys[0] += 0x43;
            oldKeys[1] += 0x29;
            oldKeys[2] += 0x17;
            oldKeys[3] += 0x13;

            return new[]
            {
                (ushort)(oldKeys[0] & 0xf         | oldKeys[1] << 4 & 0xf0    | oldKeys[2] << 8 & 0xf00   | oldKeys[3] << 12 & 0xf000),
                (ushort)(oldKeys[0] >> 4 & 0xf    | oldKeys[1] & 0xf0         | oldKeys[2] << 4 & 0xf00   | oldKeys[3] << 8 & 0xf000),
                (ushort)(oldKeys[2] & 0xf00       | (oldKeys[1] & 0xf00) >> 4 | (oldKeys[0] & 0xf00) >> 8 | oldKeys[3] << 4 & 0xf000),
                (ushort)(oldKeys[0] >> 12 & 0xf   | oldKeys[1] >> 8 & 0xf0    | oldKeys[2] >> 4 & 0xf00   | oldKeys[3] & 0xf000),
            };
        }

        /// <summary>
        /// Creates a 16bit TID/SID tuple for a given G7TID.
        /// </summary>
        /// <param name="G7TID">Desired G7TID</param>
        /// <param name="minimizeSID">Optional param to yield minimum SID.</param>
        /// <returns>16bit TID/SID tuple</returns>
        public static Tuple<uint, uint> GetTIDSID(uint G7TID, bool minimizeSID = false)
        {
            // 32 bit number = 4294 967295
            // lowest 6 digits G7TID

            // Bare minimum 32bit value to get ID, yields min SID
            uint val = G7TID;
            if (!minimizeSID) // randomize SID
            {
                uint s7 = 4294;
                if (val > 967295)
                    s7 -= 1;
                s7 = (uint)Util.Rand.Next(0, (int)s7);
                val += s7 * 1000000;
            }
            uint TID = val & 0xFFFF;
            uint SID = val >> 16;

            return new Tuple<uint, uint>(TID, SID);
        }

        /// <summary>
        /// Creates a <see cref="SaveFile"/> via decryption using a stored xorpad.
        /// </summary>
        /// <param name="input">Encrypted byte array of savedata to decrypt.</param>
        /// <param name="XORpads">Array of possible paths to check for xorpad compatibility.</param>
        /// <returns>Returns a <see cref="SaveFile"/> if decryption was successful, else null.</returns>
        public static SaveFile GetSAVfromXORpads(byte[] input, IEnumerable<string> XORpads)
        {
            byte[] savID = new byte[0x10];
            Array.Copy(input, 0x10, savID, 0, 0x10);
            int[] sizes = { SIZE_G6XY, SIZE_G6ORAS, SIZE_G7SM };

            foreach (var file in XORpads)
            {
                // Check if xorpad
                FileInfo fi = new FileInfo(file);

                string filename = fi.Name.ToLower();
                if (!filename.Contains("xorpad") && !filename.Contains("key"))
                    continue;

                var length = fi.Length;
                if (length != 0x10009C && length != 0x100000)
                    continue;
                
                // Fix xorpad alignment
                byte[] xorpad = File.ReadAllBytes(file);
                if (xorpad.Length == 0x10009C) // Trim off Powersaves' header
                {
                    Array.Copy(xorpad, 0x9C, xorpad, 0, 0x100000);
                    Array.Resize(ref xorpad, 0x100000);
                }

                // Check if encrypted 00's match save
                if (!xorpad.Skip(0x10).Take(0x10).SequenceEqual(savID))
                    continue;

                // Set up Decrypted File
                const int mainOffset = 0x5400;
                int maxSize = sizes.Max();
                byte[] decryptedPS = new byte[maxSize];
                Array.Copy(input, mainOffset, decryptedPS, 0, decryptedPS.Length);

                // xor through and decrypt
                for (int z = 0; z < decryptedPS.Length; z++)
                    decryptedPS[z] ^= xorpad[mainOffset + z];

                // Weakly check the validity of the decrypted content
                int i; for (i = 0; i < sizes.Length; i++)
                {
                    if (BitConverter.ToUInt32(decryptedPS, sizes[i] - 0x1F0) != BEEF)
                        continue;
                    Array.Resize(ref decryptedPS, sizes[i]);
                    break;
                }
                if (i == sizes.Length)
                    continue;
                
                // Save file is now decrypted!
                var SAV = GetVariantSAV(decryptedPS);
                if (SAV == null)
                    continue;

                SAV.FileName = file;
                return SAV;
            }
            return null; // no xorpad compatible
        }

        /// <summary>
        /// Checks if the <see cref="PKM"/> is compatible with the input <see cref="SaveFile"/>, and makes any necessary modifications to force compatibility.
        /// </summary>
        /// <remarks>Should only be used when forcing a backwards conversion to sanitize the PKM fields to the target format. 
        /// If the PKM is compatible, some properties may be forced to sanitized values.</remarks>
        /// <param name="SAV">Save File target that the PKM will be injected.</param>
        /// <param name="pk">PKM input that is to be injected into the Save File.</param>
        /// <returns>Indication whether or not the PKM is compatible.</returns>
        public static bool IsPKMCompatibleWithModifications(SaveFile SAV, PKM pk)
        {
            if (pk.Species > SAV.MaxSpeciesID)
                return false;

            if (pk.HeldItem > SAV.MaxItemID)
                pk.HeldItem = 0;
            if (pk.Nickname.Length > SAV.NickLength)
                pk.Nickname = pk.Nickname.Substring(0, SAV.NickLength);
            if (pk.OT_Name.Length > SAV.OTLength)
                pk.OT_Name = pk.OT_Name.Substring(0, SAV.OTLength);
            if (pk.Moves.Any(move => move > SAV.MaxMoveID))
                pk.ClearInvalidMoves();
            if (pk.EVs.Any(ev => ev > SAV.MaxEV))
                pk.EVs = pk.EVs.Select(ev => Math.Min(SAV.MaxEV, ev)).ToArray();
            if (pk.IVs.Any(ev => ev > SAV.MaxEV))
                pk.IVs = pk.IVs.Select(iv => Math.Min(SAV.MaxIV, iv)).ToArray();

            return true;
        }
    }
}

﻿using System.Linq;

namespace PKHeX.Core
{
    /// <summary>
    /// <see cref="PersonalInfo"/> class with values from Generation 2 games.
    /// </summary>
    public class PersonalInfoG2 : PersonalInfo
    {
        protected PersonalInfoG2() { }
        public const int SIZE = 0x20;
        public PersonalInfoG2(byte[] data)
        {
            if (data.Length != SIZE)
                return;

            Data = data;
            TMHM = GetBits(Data.Skip(0x18).Take(0x8).ToArray());
        }
        public override byte[] Write()
        {
            SetBits(TMHM).CopyTo(Data, 0x18);
            return Data;
        }

        public int DEX_ID { get => Data[0x00]; set => Data[0x00] = (byte)value; }
        public override int HP { get => Data[0x01]; set => Data[0x01] = (byte)value; }
        public override int ATK { get => Data[0x02]; set => Data[0x02] = (byte)value; }
        public override int DEF { get => Data[0x03]; set => Data[0x03] = (byte)value; }
        public override int SPE { get => Data[0x04]; set => Data[0x04] = (byte)value; }
        public override int SPA { get => Data[0x05]; set => Data[0x05] = (byte)value; }
        public override int SPD { get => Data[0x06]; set => Data[0x06] = (byte)value; }
        public override int Type1 { get => Data[0x06]; set => Data[0x06] = (byte)value; }
        public override int Type2 { get => Data[0x07]; set => Data[0x07] = (byte)value; }
        public override int CatchRate { get => Data[0x09]; set => Data[0x09] = (byte)value; }
        public override int BaseEXP { get => Data[0x0A]; set => Data[0x0A] = (byte)value; }
        public int Item1 { get => Data[0xB]; set => Data[0xB] = (byte)value; }
        public int Item2 { get => Data[0xC]; set => Data[0xC] = (byte)value; }
        public override int Gender { get => Data[0xD]; set => Data[0xD] = (byte)value; }
        public override int HatchCycles { get => Data[0xF]; set => Data[0xF] = (byte)value; }
        public override int EXPGrowth { get => Data[0x16]; set => Data[0x16] = (byte)value; }
        public override int EggGroup1 { get => Data[0x17] & 0xF; set => Data[0x17] = (byte)(Data[0x17] & 0xF0 | value); }
        public override int EggGroup2 { get => Data[0x17] >> 4; set => Data[0x17] = (byte)(Data[0x17] & 0x0F | value << 4); }

        public override int[] Items
        {
            get => new[] { Item1, Item2 };
            set
            {
                if (value?.Length != 2) return;
                Item1 = value[0];
                Item2 = value[1];
            }
        }

        // EV Yields are just aliases for base stats in Gen I
        public override int EV_HP { get => HP; set { } }
        public override int EV_ATK { get => ATK; set { } }
        public override int EV_DEF { get => DEF; set { } }
        public override int EV_SPE { get => SPE; set { } }
        public override int EV_SPA { get => SPA; set { } }
        public override int EV_SPD { get => SPD; set { } }

        // Future game values, unused
        public override int[] Abilities { get => new[] { 0, 0 }; set { } }
        public override int BaseFriendship { get => 70; set { } }
        public override int EscapeRate { get => 0; set { } }
        public override int Color { get => 0; set { } }
    }
}

﻿namespace PKHeX.Core
{
    public class EncounterSlot : IEncounterable
    {
        public int Species { get; set; }
        public int Form;
        public int LevelMin;
        public int LevelMax;
        public SlotType Type = SlotType.Any;
        public bool AllowDexNav;
        public bool Pressure;
        public bool DexNav;
        public bool WhiteFlute;
        public bool BlackFlute;
        public bool Normal => !(WhiteFlute || BlackFlute || DexNav);
        public EncounterSlot() { }

        public EncounterSlot(EncounterSlot template)
        {
            Species = template.Species;
            AllowDexNav = template.AllowDexNav;
            LevelMax = template.LevelMax;
            LevelMin = template.LevelMin;
            Type = template.Type;
            Pressure = template.Pressure;
        }

        public string Name => "Wild Encounter";
    }
}

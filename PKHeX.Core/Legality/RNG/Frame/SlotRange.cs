﻿using System.Linq;

namespace PKHeX.Core
{
    public static class SlotRange
    {
        private static readonly Range[] H_OldRod = GetRanges(70, 30);
        private static readonly Range[] H_GoodRod = GetRanges(60, 20, 20);
        private static readonly Range[] H_SuperRod = GetRanges(40, 40, 15, 4, 1);
        private static readonly Range[] H_Surf = GetRanges(60, 30, 5, 4, 1);
        private static readonly Range[] H_Regular = GetRanges(20, 20, 10, 10, 10, 10, 5, 5, 4, 4, 1, 1);

        private static readonly Range[] J_SuperRod = GetRanges(40, 40, 15, 4, 1);
        private static readonly Range[] K_SuperRod = GetRanges(40, 30, 15, 10, 5);
        private static readonly Range[] K_BCC = GetRanges(5,5,5,5, 10,10,10,10, 20,20).Reverse().ToArray();
        private static readonly Range[] K_Headbutt = GetRanges(50, 15, 15, 10, 5, 5);

        public static int GetSlot(SlotType type, uint rand, FrameType t)
        {
            switch (t)
            {
                case FrameType.MethodH:
                    return HSlot(type, rand);
                case FrameType.MethodJ:
                    return JSlot(type, rand);
                case FrameType.MethodK:
                    return KSlot(type, rand);
            }
            return -1;
        }

        private static int HSlot(SlotType type, uint rand)
        {
            var ESV = rand % 100;
            switch (type)
            {
                case SlotType.Old_Rod:
                case SlotType.Old_Rod_Safari:
                    return CalcSlot(ESV, H_OldRod);
                case SlotType.Good_Rod:
                case SlotType.Good_Rod_Safari:
                    return CalcSlot(ESV, H_GoodRod);
                case SlotType.Super_Rod:
                case SlotType.Super_Rod_Safari:
                    return CalcSlot(ESV, H_SuperRod);

                case SlotType.Rock_Smash:
                case SlotType.Rock_Smash_Safari:
                case SlotType.Surf:
                case SlotType.Surf_Safari:
                    return CalcSlot(ESV, H_Surf);

                case SlotType.Swarm:
                    return ESV < 50 ? 0 : -1;
                default:
                    return CalcSlot(ESV, H_Regular);
            }
        }
        private static int KSlot(SlotType type, uint rand)
        {
            var ESV = rand % 100;
            switch (type)
            {
                case SlotType.Rock_Smash:
                case SlotType.Surf:
                    return CalcSlot(ESV, H_Surf);
                case SlotType.Super_Rod:
                case SlotType.Good_Rod:
                case SlotType.Old_Rod:
                    return CalcSlot(ESV, K_SuperRod);
                case SlotType.BugContest:
                    return CalcSlot(ESV, K_BCC);
                case SlotType.Grass_Safari:
                case SlotType.Surf_Safari:
                case SlotType.Old_Rod_Safari:
                case SlotType.Good_Rod_Safari:
                case SlotType.Super_Rod_Safari:
                case SlotType.Rock_Smash_Safari:
                    return 0; // (int)(rand % 10); /* Block Slot Priority not implemented */
                case SlotType.Headbutt:
                case SlotType.Headbutt_Special:
                    return CalcSlot(ESV, K_Headbutt);
                default:
                    return CalcSlot(ESV, H_Regular);
            }
        }
        private static int JSlot(SlotType type, uint rand)
        {
            uint ESV = rand / 656;
            switch (type)
            {
                case SlotType.Old_Rod:
                case SlotType.Rock_Smash:
                case SlotType.Surf:
                    return CalcSlot(ESV, H_Surf);
                case SlotType.Good_Rod:
                case SlotType.Super_Rod:
                    return CalcSlot(ESV, J_SuperRod);
                case SlotType.HoneyTree:
                    return 0;
                default:
                    return CalcSlot(ESV, H_Regular);
            }
        }

        private struct Range
        {
            internal Range(uint min, uint max)
            {
                Min = min;
                Max = max;
            }

            internal uint Min { get; }
            internal uint Max { get; }
        }

        private static Range[] GetRanges(params uint[] rates)
        {
            var len = rates.Length;
            var arr = new Range[len];
            uint sum = 0;
            for (int i = 0; i < len; ++i)
                arr[i] = new Range(sum, (sum += rates[i]) - 1);
            return arr;
        }

        private static int CalcSlot(uint esv, Range[] ranges)
        {
            for (int i = 0; i < ranges.Length; ++i)
                if (esv >= ranges[i].Min && esv <= ranges[i].Max)
                    return i;

            return -1;
        }

        public static int GetLevel(EncounterSlot slot, LeadRequired lead, uint lvlrand)
        {
            if (lead == LeadRequired.PressureHustleSpirit)
                return slot.LevelMax;
            if (slot.LevelMin == slot.LevelMax)
                return slot.LevelMin;
            int delta = slot.LevelMax - slot.LevelMin + 1;
            var adjust = (int)(lvlrand % delta);

            return slot.LevelMin + adjust;
        }
        public static bool GetIsEncounterable(EncounterSlot slot, FrameType frameType, int rand, LeadRequired lead)
        {
            if (slot.Type.IsSweetScentType())
                return true;
            return true; // todo
            return GetCanEncounter(slot, frameType, rand, lead);
        }
        private static bool GetCanEncounter(EncounterSlot slot, FrameType frameType, int rand, LeadRequired lead)
        {
            int proc = frameType == FrameType.MethodJ ? rand / 656 : rand % 100;
            if (slot.Type.HasFlag(SlotType.Rock_Smash))
                return proc < 60;
            if (frameType == FrameType.MethodH)
                return true; // fishing encounters are disjointed by the hooked message.

            // fishing
            if (slot.Type.HasFlag(SlotType.Old_Rod))
            {
                if (proc < 25)
                    return true;
                if (proc < 50)
                    return lead == LeadRequired.None;
            }
            else if (slot.Type.HasFlag(SlotType.Good_Rod))
            {
                if (proc < 50)
                    return true;
                if (proc < 75 && lead == LeadRequired.None)
                    return lead == LeadRequired.None;
            }
            else if (slot.Type.HasFlag(SlotType.Super_Rod))
            {
                if (proc < 75)
                    return true;
                return lead == LeadRequired.None; // < 100 always true
            }
            return false; // shouldn't hit here
        }

        /// <summary>
        /// Checks both Static and Magnet Pull ability type selection encounters to see if the encounter can be selected.
        /// </summary>
        /// <param name="slot">Slot Data</param>
        /// <param name="ESV">Rand16 value for the call</param>
        /// <returns>Slot number from the slot data if the slot is selected on this frame, else an invalid slot value.</returns>
        internal static int GetSlotStaticMagnet(EncounterSlot slot, uint ESV)
        {
            if (slot.Permissions.StaticIndex >= 0)
            {
                var index = ESV % slot.Permissions.StaticCount;
                if (index == slot.Permissions.StaticIndex)
                    return slot.SlotNumber;
            }
            if (slot.Permissions.MagnetPullIndex >= 0)
            {
                var index = ESV % slot.Permissions.MagnetPullCount;
                if (index == slot.Permissions.MagnetPullIndex)
                    return slot.SlotNumber;
            }
            return -1;
        }
    }
}

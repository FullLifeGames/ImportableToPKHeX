﻿using System.Collections.Generic;

namespace PKHeX.Core
{
    internal class LearnInfo
    {
        public bool MixedGen12NonTradeback { get; set; }
        public List<int> Gen1Moves { get; } = new List<int>();
        public List<int> Gen2PreevoMoves { get; } = new List<int>();
        public List<int> EggMovesLearned { get; } = new List<int>();
        public List<int> LevelUpEggMoves { get; } = new List<int>();
        public List<int> EventEggMoves { get; } = new List<int>();
        public List<int> IncenseMoves { get; } = new List<int>();
        public MoveParseSource Source { get; set; }

        public readonly bool IsGen2Pkm;
        public LearnInfo(PKM pkm)
        {
            IsGen2Pkm = pkm.Format == 2 || pkm.VC2;
        }
    }
}

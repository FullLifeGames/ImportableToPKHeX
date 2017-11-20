﻿namespace PKHeX.Core
{
    public class PIDIV
    {
        /// <summary> The RNG that generated the PKM from the <see cref="OriginSeed"/> </summary>
        public RNG RNG;

        /// <summary> The RNG seed which immediately generates the PIDIV (starting with PID or IVs, whichever comes first). </summary>
        public uint OriginSeed;

        /// <summary> Indicates that there is no <see cref="OriginSeed"/> to refer to. </summary>
        /// <remarks> Some PIDIVs may be generated without a single seed, but may follow a traceable pattern. </remarks>
        public bool NoSeed;

        /// <summary> Type of PIDIV correlation </summary>
        public PIDType Type;
    }
}

namespace PKHeX.Core
{
    /// <summary>
    /// Rejected Encounter Data containing a reason why the encounter was rejected (not compatible).
    /// </summary>
    public class EncounterRejected : IEncounterable
    {
        public readonly IEncounterable Encounter;
        public readonly CheckResult Check;
        public string Reason => Check.Comment;

        public int Species => Encounter.Species;
        public string Name => Encounter.Name;
        public bool EggEncounter => Encounter.EggEncounter;
        public int LevelMin => Encounter.LevelMin;
        public int LevelMax => Encounter.LevelMax;

        public EncounterRejected(IEncounterable encounter, CheckResult check)
        {
            Encounter = encounter;
            Check = check;
        }
    }
}
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Graphics;



namespace osu.Game.Rulesets.Mods
{
    public class ModNoDrain : Mod, IApplicableToDifficulty
    {
        public override string Name => "No Drain";

        public override string Acronym => "ND";

        public override double ScoreMultiplier => 1;

        public override string Description => "Disables Drain!";

        public override ModType Type => ModType.DifficultyReduction;
        public virtual void ReadFromDifficulty(BeatmapDifficulty difficulty)
        {
        }

        public virtual void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.DrainRate *= 0;
        }
    }
}
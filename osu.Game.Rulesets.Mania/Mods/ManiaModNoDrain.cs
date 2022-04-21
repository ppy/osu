using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Graphics;



namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModNoDrain : Mod, IApplicableToDifficulty
    {
        public override string Name => "No Drain";

        public override string Acronym => "ND";

        public override double ScoreMultiplier => 0.5;

        public override string Description => "No drain for mania!";

        //public override IconUsage? Icon => FontAwesome.Solid.Equals;

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
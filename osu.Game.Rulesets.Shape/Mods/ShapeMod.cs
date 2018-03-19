using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Shape.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Shape.Mods
{
    public class ShapeModNoFail : ModNoFail
    {

    }

    public class ShapeModEasy : ModEasy
    {

    }

    public class ShapeModHidden : ModHidden
    {
        public override string Description => @"Notes fade out";
        public override double ScoreMultiplier => 1.06;
    }

    public class ShapeModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.08;
        public override bool Ranked => true;
    }

    public class ShapeModSuddenDeath : ModSuddenDeath
    {
        public override string Description => "Don't Miss";
        public override bool Ranked => true;
    }

    public class ShapeModDaycore : ModDaycore
    {
        public override double ScoreMultiplier => 0.3;
    }

    public class ShapeModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.16;
    }

    public class ShapeModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.3;
    }

    public class ShapeModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.16;
    }

    public class ShapeModFlashlight : ModFlashlight
    {
        public override string Description => @"I don't even know how to play with this";
        public override double ScoreMultiplier => 1.18;
    }

    public class ShapeRelax : ModRelax
    {
        public override bool Ranked => false;
    }

    public class ShapeModAutoplay : ModAutoplay<ShapeHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<ShapeHitObject> beatmap) => new Score
        {

        };
    }
}

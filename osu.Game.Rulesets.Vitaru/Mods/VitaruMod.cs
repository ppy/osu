using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Vitaru.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osu.Game.Rulesets.Vitaru.Replays;

namespace osu.Game.Rulesets.Vitaru.Mods
{
    public class VitaruModNoFail : ModNoFail
    {

    }

    public class VitaruModEasy : ModEasy
    {

    }

    public class VitaruModHidden : ModHidden
    {
        public override string Description => @"Play with bullets dissapearing once they leave enemies immediate area";
        public override double ScoreMultiplier => 1.32;
    }

    public class VitaruModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class VitaruModSuddenDeath : ModSuddenDeath
    {
        public override string Description => "Don't get hit";
    }

    public class VitaruModPerfect : ModPerfect
    {
        public override string Description => "Leave no survivors";
    }

    public class VitaruModDaycore : ModDaycore
    {
        public override double ScoreMultiplier => 0.4;
    }

    public class VitaruModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.08;
    }

    public class VitaruModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.4;
    }

    public class VitaruModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.08;
    }

    public class VitaruModFlashlight : ModFlashlight
    {
        public override string Description => @"Play with bullets only appearing when they are close";
        public override double ScoreMultiplier => 1.18;
    }

    public class VitaruRelax : ModRelax
    {
        public override string Description => @"Player moves to the cursor instantly";
        public override bool Ranked => false;
    }

    public class VitaruModAutoplay : ModAutoplay<VitaruHitObject>
    {/*
        protected override Score CreateReplayScore(Beatmap<VitaruHitObject> beatmap) => new Score
        {
            User = new User { Username = "reimosu!" },
            Replay = new VitaruAutoGenerator(beatmap).Generate(),
        };
    */}
}

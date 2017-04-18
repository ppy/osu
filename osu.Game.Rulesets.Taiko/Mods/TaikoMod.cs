// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModNoFail : ModNoFail
    {

    }

    public class TaikoModEasy : ModEasy
    {

    }

    public class TaikoModHidden : ModHidden
    {
        public override string Description => @"The notes fade out before you hit them!";
        public override double ScoreMultiplier => 1.06;
    }

    public class TaikoModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.06;
        public override bool Ranked => true;
    }

    public class TaikoModSuddenDeath : ModSuddenDeath
    {

    }

    public class TaikoModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class TaikoModRelax : ModRelax
    {
        public override string Description => @"Relax! You will no longer get dizzyfied by ninja-like spinners, demanding drumrolls or unexpected katu's.";
    }

    public class TaikoModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class TaikoModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class TaikoModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.12;
    }

    public class TaikoModPerfect : ModPerfect
    {

    }

    public class TaikoModAutoplay : ModAutoplay<TaikoHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<TaikoHitObject> beatmap) => new Score
        {
            User = new User { Username = "mekkadosu!" },
            Replay = new TaikoAutoReplay(beatmap)
        };
    }
}

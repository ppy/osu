// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Catch
{
    public class CatchModNoFail : ModNoFail
    {

    }

    public class CatchModEasy : ModEasy
    {

    }

    public class CatchModHidden : ModHidden
    {
        public override string Description => @"Play with no approach circles and fading notes for a slight score advantage.";
        public override double ScoreMultiplier => 1.06;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class CatchModHardRock : ModHardRock
    {
        public override double ScoreMultiplier => 1.12;
        public override bool Ranked => true;
    }

    public class CatchModSuddenDeath : ModSuddenDeath
    {

    }

    public class CatchModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => 1.06;
    }

    public class CatchModRelax : ModRelax
    {
        public override string Description => @"Use the mouse to control the catcher.";
    }

    public class CatchModHalfTime : ModHalfTime
    {
        public override double ScoreMultiplier => 0.5;
    }

    public class CatchModNightcore : ModNightcore
    {
        public override double ScoreMultiplier => 1.06;
    }

    public class CatchModFlashlight : ModFlashlight
    {
        public override double ScoreMultiplier => 1.12;
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class CatchModPerfect : ModPerfect
    {

    }
}

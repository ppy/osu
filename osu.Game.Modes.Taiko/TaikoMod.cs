// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Modes.Taiko
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
        public override Mods[] DisablesMods => new Mods[] { };
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
        public override Mods[] DisablesMods => new Mods[] { };
    }

    public class TaikoModPerfect : ModPerfect
    {

    }
}

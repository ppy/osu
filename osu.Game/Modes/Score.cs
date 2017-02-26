// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


namespace osu.Game.Modes
{
    public class Score
    {
        public double TotalScore { get; set; }
        public double Accuracy { get; set; }
        public double Combo { get; set; }
        public double MaxCombo { get; set; }
        public double Health { get; set; }
    }
}

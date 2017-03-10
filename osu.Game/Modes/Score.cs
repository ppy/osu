// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Database;

namespace osu.Game.Modes
{
    public class Score
    {
        public double TotalScore { get; set; }
        public double Accuracy { get; set; }
        public double Health { get; set; }
        public long Combo { get; set; }
        public long MaxCombo { get; set; }

        public Replay Replay;
        public BeatmapInfo Beatmap;
    }
}

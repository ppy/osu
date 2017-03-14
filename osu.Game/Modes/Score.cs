// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;
using osu.Game.Database;
using osu.Game.Modes.Mods;

namespace osu.Game.Modes
{
    public class Score
    {
        public ScoreRank Rank { get; set; }
        public double TotalScore { get; set; }
        public double Accuracy { get; set; }
        public double Health { get; set; }
        public int MaxCombo { get; set; }
        public int Combo { get; set; }
        public Mod[] Mods { get; set; }

        public User User { get; set; }
        public Replay Replay;
        public BeatmapInfo Beatmap;
    }
}

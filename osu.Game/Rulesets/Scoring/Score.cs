// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Scoring
{
    public class Score
    {
        public ScoreRank Rank { get; set; }

        public double TotalScore { get; set; }

        public double Accuracy { get; set; }

        public double Health { get; set; } = 1;

        public int MaxCombo { get; set; }

        public int Combo { get; set; }

        public RulesetInfo Ruleset { get; set; }

        public Mod[] Mods { get; set; }

        public User User;

        public Replay Replay;

        public BeatmapInfo Beatmap;

        public long OnlineScoreID;

        public DateTimeOffset Date;

        public Dictionary<string, dynamic> Statistics = new Dictionary<string, dynamic>();
    }
}

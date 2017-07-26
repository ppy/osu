// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class Play
    {
        public ScoreRank Rank;
        public BeatmapInfo Beatmap;
        public DateTimeOffset Date;
        public IEnumerable<Mod> Mods;
        public int PerformancePoints;
        public double Accuracy;
    }
}

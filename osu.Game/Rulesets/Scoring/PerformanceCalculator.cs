// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    public abstract class PerformanceCalculator
    {
        public abstract double Calculate(Dictionary<string, string> categoryDifficulty = null);
    }

    public abstract class PerformanceCalculator<TObject> : PerformanceCalculator
        where TObject : HitObject
    {
        private readonly Dictionary<string, string> attributes = new Dictionary<string, string>();
        protected IDictionary<string, string> Attributes => attributes;

        protected readonly Beatmap<TObject> Beatmap;
        protected readonly Score Score;

        public PerformanceCalculator(Ruleset ruleset, Beatmap beatmap, Score score)
        {
            Beatmap = CreateBeatmapConverter().Convert(beatmap);
            Score = score;

            var diffCalc = ruleset.CreateDifficultyCalculator(beatmap);
            diffCalc.Calculate(attributes);
        }

        protected abstract BeatmapConverter<TObject> CreateBeatmapConverter();
    }
}

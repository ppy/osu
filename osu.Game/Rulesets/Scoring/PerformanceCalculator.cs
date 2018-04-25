// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Scoring
{
    public abstract class PerformanceCalculator
    {
        public abstract double Calculate(Dictionary<string, double> categoryDifficulty = null);
    }

    public abstract class PerformanceCalculator<TObject> : PerformanceCalculator
        where TObject : HitObject
    {
        private readonly Dictionary<string, double> attributes = new Dictionary<string, double>();
        protected IDictionary<string, double> Attributes => attributes;

        protected readonly Beatmap<TObject> Beatmap;
        protected readonly Score Score;

        protected PerformanceCalculator(Ruleset ruleset, Beatmap beatmap, Score score)
        {
            Score = score;

            var converter = CreateBeatmapConverter();

            foreach (var mod in score.Mods.OfType<IApplicableToBeatmapConverter<TObject>>())
                mod.ApplyToBeatmapConverter(converter);

            Beatmap = converter.Convert(beatmap);

            var diffCalc = ruleset.CreateDifficultyCalculator(beatmap, score.Mods);
            diffCalc.Calculate(attributes);
        }

        protected abstract BeatmapConverter<TObject> CreateBeatmapConverter();
    }
}

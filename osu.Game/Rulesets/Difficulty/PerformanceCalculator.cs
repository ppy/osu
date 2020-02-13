// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        protected readonly DifficultyAttributes Attributes;

        protected readonly Ruleset Ruleset;
        protected readonly IBeatmap Beatmap;
        protected readonly ScoreInfo Score;

        protected double TimeRate { get; private set; } = 1;

        protected PerformanceCalculator(Ruleset ruleset, WorkingBeatmap beatmap, ScoreInfo score)
        {
            Ruleset = ruleset;
            Score = score;

            Beatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, score.Mods);

            Attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods);

            ApplyMods(score.Mods);
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var track = new TrackVirtual(10000);
            mods.OfType<IApplicableToTrack>().ForEach(m => m.ApplyToTrack(track));
            TimeRate = track.Rate;
        }

        public abstract double Calculate(Dictionary<string, double> categoryDifficulty = null);
    }
}

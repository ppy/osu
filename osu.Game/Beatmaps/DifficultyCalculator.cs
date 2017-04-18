// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    public abstract class DifficultyCalculator
    {
        protected double TimeRate = 1;

        protected abstract double CalculateInternal(Dictionary<string, string> categoryDifficulty);

        private void loadTiming()
        {
            // TODO: Handle mods
            const int audio_rate = 100;
            TimeRate = audio_rate / 100.0;
        }

        public double Calculate(Dictionary<string, string> categoryDifficulty = null)
        {
            loadTiming();
            double difficulty = CalculateInternal(categoryDifficulty);
            return difficulty;
        }
    }

    public abstract class DifficultyCalculator<T> : DifficultyCalculator where T : HitObject
    {
        protected List<T> Objects;

        protected DifficultyCalculator(Beatmap beatmap)
        {
            Objects = CreateBeatmapConverter().Convert(beatmap).HitObjects;
            PreprocessHitObjects();
        }

        protected virtual void PreprocessHitObjects()
        {
        }

        protected abstract BeatmapConverter<T> CreateBeatmapConverter();
    }
}

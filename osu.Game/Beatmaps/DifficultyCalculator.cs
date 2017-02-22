// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes;
using osu.Game.Modes.Objects;
using System;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    public abstract class DifficultyCalculator
    {
        protected abstract PlayMode PlayMode { get; }

        protected double TimeRate = 1;

        protected abstract double ComputeDifficulty(Dictionary<String, String> categoryDifficulty);

        private void loadTiming()
        {
            // TODO: Handle mods
            int audioRate = 100;
            TimeRate = audioRate / 100.0;
        }

        public double GetDifficulty(Dictionary<string, string> categoryDifficulty = null)
        {
            loadTiming();
            double difficulty = ComputeDifficulty(categoryDifficulty);
            return difficulty;
        }
    }

    public abstract class DifficultyCalculator<T> : DifficultyCalculator where T : HitObject
    {
        protected List<T> Objects;

        protected abstract HitObjectConverter<T> Converter { get; }

        public DifficultyCalculator(Beatmap beatmap)
        {
            Objects = Converter.Convert(beatmap);
            PreprocessHitObjects();
        }

        protected virtual void PreprocessHitObjects()
        {
        }
    }
}

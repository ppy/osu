// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Framework.Timing;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Beatmaps
{
    public abstract class DifficultyCalculator
    {
        protected double TimeRate = 1;

        public abstract double Calculate(Dictionary<string, string> categoryDifficulty = null);
    }

    public abstract class DifficultyCalculator<T> : DifficultyCalculator where T : HitObject
    {
        protected readonly Beatmap Beatmap;
        protected readonly Mod[] Mods;

        protected List<T> Objects;

        protected DifficultyCalculator(Beatmap beatmap)
            : this(beatmap, null)
        {
        }

        protected DifficultyCalculator(Beatmap beatmap, Mod[] mods)
        {
            Beatmap = beatmap;
            Mods = mods ?? new Mod[0];

            Objects = CreateBeatmapConverter().Convert(beatmap).HitObjects;

            foreach (var h in Objects)
                h.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);

            ApplyMods(mods);

            PreprocessHitObjects();
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));

            TimeRate = clock.Rate;
        }

        protected virtual void PreprocessHitObjects()
        {
        }

        protected abstract BeatmapConverter<T> CreateBeatmapConverter();
    }
}

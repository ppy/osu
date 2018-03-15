// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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

        public abstract double Calculate(Dictionary<string, double> categoryDifficulty = null);
    }

    public abstract class DifficultyCalculator<T> : DifficultyCalculator where T : HitObject
    {
        protected readonly Beatmap<T> Beatmap;
        protected readonly Mod[] Mods;

        protected DifficultyCalculator(Beatmap beatmap, Mod[] mods = null)
        {
            Mods = mods ?? new Mod[0];

            var converter = CreateBeatmapConverter(beatmap);

            foreach (var mod in Mods.OfType<IApplicableToBeatmapConverter<T>>())
                mod.ApplyToBeatmapConverter(converter);

            Beatmap = converter.Convert(beatmap);

            ApplyMods(Mods);

            PreprocessHitObjects();
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));
            TimeRate = clock.Rate;

            foreach (var mod in Mods.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(Beatmap.BeatmapInfo.BaseDifficulty);

            foreach (var h in Beatmap.HitObjects)
                h.ApplyDefaults(Beatmap.ControlPointInfo, Beatmap.BeatmapInfo.BaseDifficulty);

            foreach (var mod in mods.OfType<IApplicableToHitObject<T>>())
                foreach (var obj in Beatmap.HitObjects)
                    mod.ApplyToHitObject(obj);
        }

        protected virtual void PreprocessHitObjects()
        {
        }

        protected abstract BeatmapConverter<T> CreateBeatmapConverter(Beatmap beatmap);
    }
}

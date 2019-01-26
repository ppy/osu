// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod
    {
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime), typeof(ModHalfTime) };
        public abstract double AppendRate { get; }
    }

    public abstract class ModTimeRamp<T> : ModTimeRamp, IUpdatableByPlayfield, IApplicableToClock, IApplicableToBeatmap<T>
        where T : HitObject
    {
        private double lastObjectEndTime;
        private IAdjustableClock clock;
        private IHasPitchAdjust pitchAdjust;

        public virtual void ApplyToClock(IAdjustableClock clk)
        {
            clock = clk;
            pitchAdjust = clk as IHasPitchAdjust;
        }

        public virtual void ApplyToBeatmap(Beatmap<T> beatmap)
        {
            HitObject lastObject = beatmap.HitObjects[beatmap.HitObjects.Count - 1];
            lastObjectEndTime = (lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0;
        }

        public virtual void Update(Playfield playfield)
        {
            double newRate = 1 + (AppendRate * (clock.CurrentTime / lastObjectEndTime));
            clock.Rate = newRate;
            pitchAdjust.PitchAdjust = newRate;
        }
    }
}
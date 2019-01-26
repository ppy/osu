using System;
using System.Collections.Generic;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModWindUp : Mod
    {
        public override string Name => "Wind Up";
        public override string Acronym => "WU";
        public override ModType Type => ModType.Fun;
        public override FontAwesome Icon => FontAwesome.fa_chevron_circle_up;
        public override string Description => "Crank it up!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModDoubleTime), typeof(ModHalfTime) };
        public abstract double AppendRate { get; }
    }

    public abstract class ModWindUp<T> : ModWindUp, IUpdatableByPlayfield, IApplicableToClock, IApplicableToBeatmap<T>
        where T : HitObject
    {
        private double LastObjectEndTime;
        private IAdjustableClock Clock;
        private IHasPitchAdjust ClockAdjust;
        public override double AppendRate => 0.5;

        public virtual void ApplyToClock(IAdjustableClock clock)
        {
            Clock = clock;
            ClockAdjust = clock as IHasPitchAdjust;
        }

        public virtual void ApplyToBeatmap(Beatmap<T> beatmap)
        {
            HitObject LastObject = beatmap.HitObjects[beatmap.HitObjects.Count - 1];
            LastObjectEndTime = (LastObject as IHasEndTime)?.EndTime ?? LastObject?.StartTime ?? 0;
        }

        public virtual void Update(Playfield playfield)
        {
            double newRate = 1 + (AppendRate * (Clock.CurrentTime / LastObjectEndTime));
            Clock.Rate = newRate;
            ClockAdjust.PitchAdjust = newRate;
        }
    }
}
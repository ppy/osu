// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModShrinking : Mod, IUpdatableByPlayfield, IApplicableToBeatmap, IApplicableToTrack
    {
        public override string Name => "Shrinking Playfield";
        public override string Acronym => "SP";
        public override string Description => "The playfield will shrink over time.";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.CompressArrowsAlt;

        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        public const double FINAL_RATE_PROGRESS = 0.75f;

        private double finalRateTime;
        private double beginRampTime;

        [SettingSource("Initial scale", "The initial scale of the playfield.")]
        public BindableNumber<double> InitialScale { get; } = new BindableDouble(1)
        {
            Default = 1,
            Value = 1,
            MinValue = 0.25,
            MaxValue = 1,
            Precision = 0.01,
        };

        [SettingSource("Final scale", "The playfield scale to ramp towards.")]
        public BindableNumber<double> FinalScale { get; } = new BindableDouble(0.4)
        {
            Default = 0.4,
            Value = 0.4,
            MinValue = 0.2,
            MaxValue = 0.95,
            Precision = 0.01,
        };

        [SettingSource("Axis", "The axes to scale.")]
        public Bindable<Axes> ActiveAxes { get; } = new Bindable<Axes>();

        private ITrack track;

        public void ApplyToTrack(ITrack track)
        {
            this.track = track;
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            double firstObjectStart = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            double lastObjectEnd = beatmap.HitObjects.LastOrDefault()?.GetEndTime() ?? 0;

            beginRampTime = firstObjectStart;
            finalRateTime = firstObjectStart + FINAL_RATE_PROGRESS * (lastObjectEnd - firstObjectStart);
        }

        public void Update(Playfield playfield)
        {
            playfield.Anchor = Anchor.Centre;
            playfield.Origin = Anchor.Centre;

            double amount = (track.CurrentTime - beginRampTime) / Math.Max(1, finalRateTime - beginRampTime);
            float scale = (float)(InitialScale.Value + (FinalScale.Value - InitialScale.Value) * Math.Clamp(amount, 0, 1));

            switch (ActiveAxes.Value)
            {
                case Axes.Both: playfield.Scale = new Vector2(scale, scale); break;
                case Axes.X: playfield.Scale = new Vector2(scale, 1); break;
                case Axes.Y: playfield.Scale = new Vector2(1, scale); break;
            }
        }

        public enum Axes
        {
            Both,
            X,
            Y
        }
    }
}

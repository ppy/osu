// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.ComponentModel;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModCover : ModHidden
    {
        public override string Name => "Cover";
        public override string Acronym => "CO";
        public override IconUsage? Icon => OsuIcon.ModCover;
        public override LocalisableString Description => @"Decrease the playfield's viewing area.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight)};
        public override bool Ranked => false;

        [SettingSource("Coverage", "The proportion of playfield height that notes will be hidden for.")]
        public BindableNumber<float> Coverage { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.01f,
            MinValue = 0.2f,
            MaxValue = 0.8f,
            Default = 0.5f,
        };

        [SettingSource("Direction", "The direction on which the cover is applied")]
        public Bindable<CoverExpandDirection> Direction { get; } = new Bindable<CoverExpandDirection>();

        private const double fade_fraction = 0.16;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
            => ApplyNormalVisibilityState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            if (hitObject is not DrawableCatchHitObject catchDrawable) 
                return;

            if (Direction.Value == CoverExpandDirection.AlongScroll && state != ArmedState.Idle)
                return;

            if (catchDrawable.NestedHitObjects.Any())
            {
                foreach (var nested in catchDrawable.NestedHitObjects)
                {
                    if (nested is DrawableCatchHitObject nestedCatch)
                        applyCoverEffect(nestedCatch);
                }
            }
            else
            {
                applyCoverEffect(catchDrawable);
            }
        }

        private void applyCoverEffect(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double preempt = hitObject.TimePreempt;
            double spawnTime = hitObject.StartTime - preempt;

            double fade_duration = preempt * fade_fraction;

            if (Direction.Value == CoverExpandDirection.AlongScroll)
            {
                double boundaryTime = spawnTime + (preempt * Coverage.Value);
                double fadeStartTime = boundaryTime - fade_duration;

                using (drawable.BeginAbsoluteSequence(double.MinValue))
                    drawable.FadeTo(0);

                using (drawable.BeginAbsoluteSequence(fadeStartTime))
                    drawable.FadeIn(fade_duration);
            }
            else
            {
                double boundaryTime = spawnTime + (preempt * (1 - Coverage.Value));

                using (drawable.BeginAbsoluteSequence(boundaryTime))
                    drawable.FadeOut(fade_duration);
            }
        }
    }

    public enum CoverExpandDirection
    {
        /// <summary>
        /// The cover expands along the scrolling direction.
        /// </summary>
        [Description("Along scroll")]
        AlongScroll,

        /// <summary>
        /// The cover expands against the scrolling direction.
        /// </summary>
        [Description("Against scroll")]
        AgainstScroll
    }
}
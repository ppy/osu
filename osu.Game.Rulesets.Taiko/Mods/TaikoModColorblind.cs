// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModColorBlind : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Name => "ColorBlind";
        public override string Acronym => "CB";

        public override LocalisableString Description => @"What these notes color?";

        public override double ScoreMultiplier => 1.03;

        public override ModType Type => ModType.DifficultyIncrease;

        public override Type[] IncompatibleMods => new[] { typeof(TaikoModHidden) };

        /// <summary>
        /// How far away from the hit target should hitobjects start to lose color.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_start_time = 1f;

        /// <summary>
        /// How long hitobjects take to lose color, in terms of the scrolling length.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_duration = 0.375f;

        private DrawableTaikoRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            ApplyNormalVisibilityState(hitObject, state);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableHit:
                    double preempt = drawableRuleset.TimeRange.Value / drawableRuleset.ControlPointAt(hitObject.HitObject.StartTime).Multiplier;
                    double start = hitObject.HitObject.StartTime - preempt * fade_out_start_time;
                    double duration = preempt * fade_out_duration;

                    using (hitObject.BeginAbsoluteSequence(start))
                    {
                        hitObject.TransformBindableTo(hitObject.AccentColour, Color4.Gray, duration);
                    }

                    break;
            }
        }
    }
}

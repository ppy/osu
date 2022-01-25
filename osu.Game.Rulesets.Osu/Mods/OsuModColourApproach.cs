// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModColourApproach : ModWithVisibilityAdjustment, IHidesApproachCircles
    {
        public override string Name => "Colour Approach";
        public override string Acronym => "CA";
        public override string Description => "Something about colours and such...";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Palette;
        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles) };
        protected override bool IsFirstAdjustableObject(HitObject hitObject) => !(hitObject is Spinner || hitObject is SpinnerTick);

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyColouredState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyColouredState(hitObject, state);

        private void applyColouredState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject drawableOsuObject))
                return;

            OsuHitObject hitObject = drawableOsuObject.HitObject;
            var h = (OsuHitObject)drawable.HitObject;

            //apply coloured effect

            switch (drawable)
            {
                case DrawableSlider _:
                case DrawableSliderHead _:
                case DrawableHitCircle _:
                {
                    using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        drawable.FadeColour(Colour4.Red, h.TimePreempt * 0.33).Then().FadeColour(Colour4.Yellow, h.TimePreempt * 0.33).Then().FadeColour(Colour4.Green, h.TimePreempt * 0.33);
                    break;
                }
            }

            {
                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        // we don't want to see the approach circle
                        using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                            circle.ApproachCircle.Hide();
                        break;
                }
            }
        }
    }
}

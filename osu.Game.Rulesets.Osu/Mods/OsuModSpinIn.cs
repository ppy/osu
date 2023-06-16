// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpinIn : ModWithVisibilityAdjustment, IHidesApproachCircles
    {
        public override string Name => "Spin In";
        public override string Acronym => "SI";
        public override IconUsage? Icon => FontAwesome.Solid.Undo;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Circles spin in. No approach circles.";
        public override double ScoreMultiplier => 1;

        // todo: this mod needs to be incompatible with "hidden" due to forcing the circle to remain opaque,
        // further implementation will be required for supporting that.
        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles), typeof(OsuModObjectScaleTween), typeof(OsuModHidden), typeof(OsuModSlide) };

        private const int rotate_offset = 360;
        private const float rotate_starting_width = 2;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyZoomState(hitObject, state);

        private void applyZoomState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableSpinner)
                return;

            var h = (OsuHitObject)drawable.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    {
                        circle.ApproachCircle.Hide();

                        circle.RotateTo(rotate_offset).Then().RotateTo(0, h.TimePreempt, Easing.InOutSine);
                        circle.ScaleTo(new Vector2(rotate_starting_width, 0)).Then().ScaleTo(1, h.TimePreempt, Easing.InOutSine);

                        // bypass fade in.
                        if (state == ArmedState.Idle)
                            circle.FadeIn();
                    }

                    break;

                case DrawableSlider slider:
                    using (slider.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    {
                        slider.ScaleTo(0).Then().ScaleTo(1, h.TimePreempt, Easing.InOutSine);

                        // bypass fade in.
                        if (state == ArmedState.Idle)
                            slider.FadeIn();
                    }

                    break;
            }
        }
    }
}

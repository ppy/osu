// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpinIn : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Spin In";
        public override string ShortenedName => "SI";
        public override FontAwesome Icon => FontAwesome.fa_rotate_right;
        public override ModType Type => ModType.Fun;
        public override string Description => "Circle spin in. No approach circles.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModHidden) };

        private const int rotate_offset = 360;
        private const float rotate_starting_width = 2.5f;


        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                drawable.ApplyCustomUpdateState += ApplyZoomState;
            }
        }

        protected void ApplyZoomState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject)) return;
            if (state != ArmedState.Idle) return;

            var h = (OsuHitObject)drawable.HitObject;
            var appearTime = h.StartTime - h.TimePreempt;
            var moveDuration = h.TimePreempt;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // Disable Fade
                    circle.Transforms
                          .Where(t => t.TargetMember == "Alpha")
                          .ForEach(t => circle.RemoveTransform(t));

                    using (circle.BeginAbsoluteSequence(appearTime, true))
                    {
                        var origScale = drawable.Scale;
                        var origRotate = circle.Rotation;

                        circle
                            .RotateTo(origRotate+rotate_offset)
                            .RotateTo(origRotate, moveDuration)
                            .ScaleTo(origScale * new Vector2(rotate_starting_width, 0))
                            .ScaleTo(origScale, moveDuration)
                            .FadeTo(1);
                    }

                    circle.ApproachCircle.Hide();

                    break;

                case DrawableSlider slider:
                    // Disable fade
                    slider.Transforms
                          .Where(t => t.TargetMember == "Alpha")
                          .ForEach(t => slider.RemoveTransform(t));

                    using (slider.BeginAbsoluteSequence(appearTime, true))
                    {
                        var origScale = slider.Scale;

                        slider
                            .ScaleTo(0)
                            .ScaleTo(origScale, moveDuration)
                            .FadeTo(1);
                    }

                    break;
            }
        }
    }
}

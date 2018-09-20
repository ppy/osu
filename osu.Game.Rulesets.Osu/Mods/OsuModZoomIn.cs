// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModZoomIn : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Zoom In";
        public override string ShortenedName => "ZI";
        public override FontAwesome Icon => FontAwesome.fa_dot_circle_o;
        public override ModType Type => ModType.Fun;
        public override string Description => "Circles zoom in. No approach circles.";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                drawable.ApplyCustomUpdateState += ApplyBounceState;
            }
        }

        protected void ApplyBounceState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject)) return;
            if (state != ArmedState.Idle) return;

            var h = (OsuHitObject)drawable.HitObject;
            var appearTime = h.StartTime - h.TimePreempt;
            var moveDuration = h.TimePreempt;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    foreach (var t in circle.Transforms.Where(t => t.TargetMember == "Alpha"))
                        circle.RemoveTransform(t);
                    using (circle.BeginAbsoluteSequence(appearTime, true))
                    {
                        var origScale = drawable.Scale;

                        circle
                            .ScaleTo(0)
                            .ScaleTo(origScale, moveDuration, Easing.OutSine)
                            .FadeTo(1);
                    }

                    circle.ApproachCircle.Hide();

                    break;

                case DrawableSlider slider:
                    foreach (var t in slider.Transforms.Where(t => t.TargetMember == "Alpha"))
                        slider.RemoveTransform(t);

                    using (slider.BeginAbsoluteSequence(appearTime, true))
                    {
                        var origScale = slider.Scale;

                        slider
                            .ScaleTo(0)
                            .ScaleTo(origScale, moveDuration, Easing.OutSine)
                            .FadeTo(1);
                    }

                    break;
            }
        }
    }
}

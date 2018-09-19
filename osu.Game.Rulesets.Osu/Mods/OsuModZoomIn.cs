// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using OpenTK;

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
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = (OsuHitObject)drawable.HitObject;

            double appearTime = h.StartTime - h.TimePreempt;
            double moveDuration = h.TimePreempt;

            using (drawable.BeginAbsoluteSequence(appearTime, true))
            {
                var origScale = drawable.Scale;

                drawable
                    .ScaleTo(0.0f)
                    .ScaleTo(origScale, moveDuration, Easing.InOutSine);
            }

            // Hide approach circle
            (drawable as DrawableHitCircle)?.ApproachCircle.Hide();
        }
    }
}

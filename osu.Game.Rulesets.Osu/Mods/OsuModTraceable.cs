// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : OsuModHidden, IReadFromConfig, IApplicableToDrawableHitObjects
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override IconUsage Icon => FontAwesome.Brands.SnapchatGhost;
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModHidden), typeof(OsuModGrow) };

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables.Skip(IncreaseFirstObjectVisibility.Value ? 1 : 0))
            {
                switch (drawable)
                {
                    case DrawableHitCircle _:
                        drawable.ApplyCustomUpdateState += ApplyTraceableState;
                        break;
                    case DrawableSlider slider:
                        slider.ApplyCustomUpdateState += ApplyHiddenState;
                        slider.HeadCircle.ApplyCustomUpdateState += ApplyTraceableState;
                        break;
                    default:
                        drawable.ApplyCustomUpdateState += ApplyHiddenState;
                        break;
                }
            }
        }

        protected void ApplyTraceableState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableHitCircle circle))
                return;

            var h = circle.HitObject;

            // we only want to see the approach circle
            using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
            {
                circle.circle.Hide();   // CirclePiece
                circle.circle.AlwaysPresent = true;
                circle.ring.Hide();
                circle.flash.Hide();
                circle.explode.Hide();
                circle.number.Hide();
                circle.glow.Hide();
                circle.ApproachCircle.Show();
            }
        }
    }
}

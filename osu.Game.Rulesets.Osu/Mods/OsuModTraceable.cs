// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : OsuModHidden
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override IconUsage Icon => FontAwesome.Brands.SnapchatGhost;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModGrow) };

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables.Skip(IncreaseFirstObjectVisibility.Value ? 1 : 0))
                drawable.ApplyCustomUpdateState += ApplyTraceableState;
        }

        protected void ApplyTraceableState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we only want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                    {
                        circle.Circle.Hide(); // CirclePiece
                        circle.Circle.AlwaysPresent = true;
                        circle.Ring.Hide();
                        circle.Flash.Hide();
                        circle.Explode.Hide();
                        circle.Number.Hide();
                        circle.Glow.Hide();
                        circle.ApproachCircle.Show();
                    }

                    break;

                case DrawableSlider slider:
                    ApplyTraceableState(slider.HeadCircle, state);
                    slider.Body.AccentColour = Color4.Transparent;

                    break;

                default:
                    ApplyHiddenState(drawable, state);

                    break;
            }
        }
    }
}

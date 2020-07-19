// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModNinja : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Ninja";
        public override string Acronym => "NJ";
        public override IconUsage? Icon => FontAwesome.Solid.UserNinja;
        public override ModType Type => ModType.Fun;
        public override string Description => "Slice the circles!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModWiggle), typeof(OsuModTransform), typeof(OsuModSpinIn) };

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += applyTransform;
        }

        private void applyTransform(DrawableHitObject drawable, ArmedState state)
        {
            switch (drawable)
            {
                case DrawableSliderHead _:
                case DrawableSliderTail _:
                case DrawableSliderTick _:
                case DrawableSliderRepeat _:
                    return;

                default:
                    var hitObject = (OsuHitObject)drawable.HitObject;

                    var originalPosition = drawable.Position;
                    var appearPosition = new Vector2(originalPosition.X, OsuPlayfield.BASE_SIZE.Y);

                    // the - 1 and + 1 prevents the hit objects to appear in the wrong position.
                    double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
                    double moveDuration = hitObject.TimePreempt + 1;

                    using (drawable.BeginAbsoluteSequence(appearTime, true))
                    {
                        drawable
                            .MoveTo(appearPosition)
                            .MoveTo(originalPosition, moveDuration, Easing.Out);
                    }
                    break;
            }
        }
    }
}

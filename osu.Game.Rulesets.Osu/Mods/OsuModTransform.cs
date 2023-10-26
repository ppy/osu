// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
    internal class OsuModTransform : ModWithVisibilityAdjustment
    {
        public override string Name => "Transform";
        public override string Acronym => "TR";
        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAlt;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Everything rotates. EVERYTHING.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModWiggle), typeof(OsuModMagnetised), typeof(OsuModRepel), typeof(OsuModFreezeFrame) }).ToArray();

        private float theta;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        private void applyTransform(DrawableHitObject drawable, ArmedState state)
        {
            switch (drawable)
            {
                case DrawableSliderHead:
                case DrawableSliderTail:
                case DrawableSliderTick:
                case DrawableSliderRepeat:
                    return;

                default:
                    var hitObject = (OsuHitObject)drawable.HitObject;

                    float appearDistance = (float)(hitObject.TimePreempt - hitObject.TimeFadeIn) / 2;

                    Vector2 originalPosition = drawable.Position;
                    Vector2 appearOffset = new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * appearDistance;

                    // the - 1 and + 1 prevents the hit objects to appear in the wrong position.
                    double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
                    double moveDuration = hitObject.TimePreempt + 1;

                    using (drawable.BeginAbsoluteSequence(appearTime))
                    {
                        drawable
                            .MoveToOffset(appearOffset)
                            .MoveTo(originalPosition, moveDuration, Easing.InOutSine);
                    }

                    theta += (float)hitObject.TimeFadeIn / 1000;
                    break;
            }
        }
    }
}

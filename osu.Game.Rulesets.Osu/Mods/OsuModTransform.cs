// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTransform : ModWithVisibilityAdjustment
    {
        public override string Name => "Transform";
        public override string Acronym => "TR";
        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAlt;
        public override ModType Type => ModType.Fun;
        public override string Description => "Everything rotates. EVERYTHING.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModWiggle), typeof(OsuModMagnetised), typeof(OsuModRepel) };

        private float theta;

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject, state);

        [SettingSource("Movement type", "Alter how circles move into place", 1)]
        public Bindable<MoveType> Move { get; } = new Bindable<MoveType>();

        private void applyTransform(DrawableHitObject drawable, ArmedState state)
        {
            switch (Move.Value)
            {
                case MoveType.Rotate:
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

                    break;

                case MoveType.Emit:
                    var osuObject = (OsuHitObject)drawable.HitObject;
                    var h = (OsuHitObject)drawable.HitObject;
                    Vector2 origin = drawable.Position;

                    if (osuObject is SliderRepeat || osuObject is SliderTailCircle)
                        return;

                    void emit()
                    {
                        drawable.ScaleTo(.8f).Then().ScaleTo(1, h.TimePreempt - h.TimePreempt / 3);
                        drawable.MoveTo(OsuPlayfield.BASE_SIZE / 2).Then().MoveTo(origin, (h.TimePreempt - h.TimePreempt / 2), Easing.Out);
                    }

                    for (int i = 0; i < 1; i++)
                    {
                        using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                            emit();
                    }

                    return;
            }
        }
    }

    public enum MoveType
    {
        Rotate,
        Emit
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Framework.Localisation;
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
        public override LocalisableString Description => "Everything moves. EVERYTHING.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModWiggle), typeof(OsuModMagnetised), typeof(OsuModRepel) };

        [SettingSource("Movement type", "Change where the circles originate from")]
        public Bindable<TransformMovementType> MovementType { get; } = new Bindable<TransformMovementType>();

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject);
        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyTransform(hitObject);

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyFadeInAdjustment(obj);

            static void applyFadeInAdjustment(OsuHitObject osuObject)
            {
                osuObject.TimeFadeIn = osuObject.TimePreempt * .2;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyFadeInAdjustment(nested);
            }
        }

        private void applyTransform(DrawableHitObject drawable)
        {
            switch (drawable)
            {
                case DrawableSliderHead:
                case DrawableSliderTail:
                case DrawableSliderTick:
                case DrawableSliderRepeat:
                    return;

                default:

                    switch (MovementType.Value)
                    {
                        case TransformMovementType.Rotate:
                            applyRotateState(drawable);
                            break;

                        case TransformMovementType.Radiate:
                            applyRadiateState(drawable);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return;
            }
        }

        private float theta;

        private void applyRotateState(DrawableHitObject drawable)
        {
            var hitObject = (OsuHitObject)drawable.HitObject;
            float appearDistance = (float)(hitObject.TimePreempt - hitObject.TimeFadeIn);

            Vector2 originalPosition = drawable.Position;
            Vector2 appearOffset = new Vector2(MathF.Cos(theta), MathF.Sin(theta)) * appearDistance;

            // the - 1 and + 1 prevents the hit objects to appear in the wrong position.
            double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
            double moveDuration = hitObject.TimePreempt + 1;

            using (drawable.BeginAbsoluteSequence(appearTime))
            {
                drawable
                    .MoveToOffset(appearOffset)
                    .MoveTo(originalPosition, moveDuration, Easing.Out);
            }

            theta += (float)hitObject.TimeFadeIn / 1000;
        }

        private void applyRadiateState(DrawableHitObject drawable)
        {
            var hitObject = (OsuHitObject)drawable.HitObject;

            Vector2 originalPosition = drawable.Position;
            Vector2 playfieldCenter = OsuPlayfield.BASE_SIZE / 2;

            // the - 1 and + 1 prevents the hit objects to appear in the wrong position.
            double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
            double moveDuration = hitObject.TimePreempt + 1;

            using (drawable.BeginAbsoluteSequence(appearTime))
            {
                drawable.ScaleTo(.6f).Then().ScaleTo(1, moveDuration, Easing.OutSine);
                drawable.MoveTo(playfieldCenter).Then().MoveTo(originalPosition, moveDuration, Easing.Out);
            }
        }
    }

    public enum TransformMovementType
    {
        Rotate,
        Radiate
    }
}

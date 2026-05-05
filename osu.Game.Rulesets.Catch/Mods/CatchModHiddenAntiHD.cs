// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModHiddenAntiHD : ModHidden, IApplicableToDrawableRuleset<CatchHitObject>
    {
        public override string Name => "Anti-Hidden";
        public override string Acronym => "AH";
        public override LocalisableString Description => "Play with fruits that fade in late.";
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.06 : 1;
        public override Type[] IncompatibleMods => new[] { typeof(CatchModHidden), typeof(CatchModFlashlight) };

        private const double fade_in_duration_multiplier = 0.16;

        private readonly Dictionary<DrawableHitObject, HitObject> nestedHitObjects = new Dictionary<DrawableHitObject, HitObject>();

        [SettingSource("Reveal height", "How far down the playfield fruits should begin fading in.")]
        public BindableDouble RevealHeight { get; } = new BindableDouble(50)
        {
            MinValue = 30,
            MaxValue = 70,
            Precision = 1,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            var drawableCatchRuleset = (DrawableCatchRuleset)drawableRuleset;
            var catchPlayfield = (CatchPlayfield)drawableCatchRuleset.Playfield;

            catchPlayfield.Catcher.CatchFruitOnPlate = true;
        }

        public override void ApplyToDrawableHitObject(DrawableHitObject drawableHitObject)
        {
            trackNestedHitObjects(drawableHitObject);

            drawableHitObject.ApplyCustomUpdateState += (o, state) =>
            {
                if (ReferenceEquals(drawableHitObject, o))
                {
                    trackNestedHitObjects(drawableHitObject);

                    if (nestedHitObjects.TryGetValue(o, out var nestedHitObject) && ReferenceEquals(nestedHitObject, o.HitObject))
                        return;
                }
                else if (!isDirectNestedHitObject(drawableHitObject, o))
                    return;

                ApplyNormalVisibilityState(o, state);
            };
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
            => ApplyNormalVisibilityState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            if (!(hitObject is DrawableCatchHitObject catchDrawable))
                return;

            if (state != ArmedState.Idle)
                return;

            if (catchDrawable.NestedHitObjects.Count != 0)
                return;

            fadeInHitObject(catchDrawable);
        }

        private void fadeInHitObject(DrawableCatchHitObject drawable)
        {
            var hitObject = drawable.HitObject;

            double offset = hitObject.TimePreempt * (1 - RevealHeight.Value / 100);
            double duration = Math.Min(hitObject.TimePreempt * fade_in_duration_multiplier, offset);

            drawable.Hide();

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt))
                drawable.Hide();

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - offset))
                drawable.FadeIn(duration);
        }

        private void trackNestedHitObjects(DrawableHitObject drawable)
        {
            foreach (var nested in drawable.NestedHitObjects)
            {
                nestedHitObjects[nested] = nested.HitObject;
                trackNestedHitObjects(nested);
            }
        }

        private static bool isDirectNestedHitObject(DrawableHitObject parent, DrawableHitObject target)
        {
            foreach (var nested in parent.NestedHitObjects)
            {
                if (ReferenceEquals(nested, target))
                    return true;
            }

            return false;
        }
    }
}

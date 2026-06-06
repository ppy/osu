// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFadein : ModHidden
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override LocalisableString Description => "Reduce reaction time.";
        public override Type[] IncompatibleMods => [typeof(CatchModHidden), typeof(CatchModFlashlight)];

        private const double fade_in_duration_multiplier = 0.5;

        private readonly Dictionary<DrawableHitObject, HitObject> nestedHitObjects = new Dictionary<DrawableHitObject, HitObject>();

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

            double duration = hitObject.TimePreempt * fade_in_duration_multiplier;

            drawable.Hide();

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - duration))
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

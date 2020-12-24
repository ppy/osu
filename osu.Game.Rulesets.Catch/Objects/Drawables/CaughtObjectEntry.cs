// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Performance;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public abstract class CaughtObjectEntry : LifetimeEntry
    {
        public readonly CatchHitObject HitObject;

        public readonly bool HyperDash;

        public readonly Color4 AccentColour;

        public readonly Vector2 DisplaySize;

        public readonly float DisplayRotation;

        public readonly FruitVisualRepresentation VisualRepresentation;

        protected CaughtObjectEntry(IHasCatchObjectState source)
        {
            HitObject = source.HitObject;
            HyperDash = source.HyperDash.Value;
            AccentColour = source.AccentColour.Value;
            DisplaySize = source.DisplaySize;
            DisplayRotation = source.DisplayRotation;

            if (source is IHasFruitState fruitState)
                VisualRepresentation = fruitState.VisualRepresentation.Value;
        }

        /// <summary>
        /// Apply transforms to drawable representation of this caught object.
        /// </summary>
        public abstract void ApplyTransforms(Drawable d);
    }
}

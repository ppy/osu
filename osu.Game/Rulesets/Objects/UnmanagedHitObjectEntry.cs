// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    internal class UnmanagedHitObjectEntry : HitObjectLifetimeEntry
    {
        public readonly DrawableHitObject DrawableHitObject;

        public UnmanagedHitObjectEntry(HitObject hitObject, DrawableHitObject drawableHitObject)
            : base(hitObject)
        {
            DrawableHitObject = drawableHitObject;
            LifetimeStart = DrawableHitObject.LifetimeStart;
            LifetimeEnd = DrawableHitObject.LifetimeEnd;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>Pool for hit of a specific HitType.</summary>
    internal partial class HitPool : DrawablePool<DrawableHit>
    {
        private readonly HitType hitType;

        public HitPool(HitType hitType, int initialSize)
            : base(initialSize)
        {
            this.hitType = hitType;
        }

        protected override DrawableHit CreateNewDrawable() => new DrawableHit(new Hit(hitType));
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Pool for hit explosions of a specific type.
    /// </summary>
    internal partial class KiaiHitExplosionPool : DrawablePool<KiaiHitExplosion>
    {
        private readonly HitType hitType;

        public KiaiHitExplosionPool(HitType hitType)
            : base(15)
        {
            this.hitType = hitType;
        }

        protected override KiaiHitExplosion CreateNewDrawable() => new KiaiHitExplosion(hitType);
    }
}

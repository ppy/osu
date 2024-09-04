// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class MovementMarkerContainer : PooledDrawableWithLifetimeContainer<AimPointEntry, HitMarker>
    {
        private readonly DrawablePool<HitMarkerMovement> pool;

        public MovementMarkerContainer()
        {
            AddInternal(pool = new DrawablePool<HitMarkerMovement>(80));
        }

        protected override HitMarker GetDrawable(AimPointEntry entry) => pool.Get(d => d.Apply(entry));
    }
}

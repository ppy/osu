// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class ClickMarkerContainer : PooledDrawableWithLifetimeContainer<HitMarkerEntry, HitMarker>
    {
        private readonly DrawablePool<HitMarkerLeftClick> leftPool;
        private readonly DrawablePool<HitMarkerRightClick> rightPool;

        public ClickMarkerContainer()
        {
            AddInternal(leftPool = new DrawablePool<HitMarkerLeftClick>(15));
            AddInternal(rightPool = new DrawablePool<HitMarkerRightClick>(15));
        }

        protected override HitMarker GetDrawable(HitMarkerEntry entry)
        {
            if (entry.IsLeftMarker)
                return leftPool.Get(d => d.Apply(entry));

            return rightPool.Get(d => d.Apply(entry));
        }
    }
}

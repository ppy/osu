// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class FrameMarkerContainer : PooledDrawableWithLifetimeContainer<AnalysisFrameEntry, AnalysisMarker>
    {
        private readonly DrawablePool<FrameMarker> pool;

        public FrameMarkerContainer()
        {
            AddInternal(pool = new DrawablePool<FrameMarker>(80));
        }

        protected override AnalysisMarker GetDrawable(AnalysisFrameEntry entry) => pool.Get(d => d.Apply(entry));
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class ClickMarkerContainer : PooledDrawableWithLifetimeContainer<AnalysisFrameEntry, AnalysisMarker>
    {
        private readonly DrawablePool<ClickMarker> clickMarkerPool;

        public ClickMarkerContainer()
        {
            AddInternal(clickMarkerPool = new DrawablePool<ClickMarker>(30));
        }

        protected override AnalysisMarker GetDrawable(AnalysisFrameEntry entry) => clickMarkerPool.Get(d => d.Apply(entry));
    }
}

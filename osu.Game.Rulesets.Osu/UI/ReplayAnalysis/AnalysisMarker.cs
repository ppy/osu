// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public abstract partial class AnalysisMarker : PoolableDrawableWithLifetime<AnalysisFrameEntry>
    {
        [Resolved]
        protected OsuColour Colours { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.Centre;
        }

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            Position = entry.Position;
            Depth = -(float)entry.LifetimeEnd;
        }
    }
}

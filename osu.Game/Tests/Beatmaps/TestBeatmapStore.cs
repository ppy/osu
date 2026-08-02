// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Database;

namespace osu.Game.Tests.Beatmaps
{
    internal partial class TestBeatmapStore : BeatmapStore
    {
        public readonly BindableList<BeatmapSetInfo> BeatmapSets = new BindableList<BeatmapSetInfo>();
        public override IBindableList<BeatmapSetInfo> GetBeatmapSets(CancellationToken? cancellationToken) => BeatmapSets.GetBoundCopy();
    }
}

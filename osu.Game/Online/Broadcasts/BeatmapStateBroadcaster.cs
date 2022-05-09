// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Online.Broadcasts
{
    public class BeatmapStateBroadcaster : GameStateBroadcaster<BeatmapInfo>
    {
        public override string Type => @"Beatmap";
        public override BeatmapInfo Message => beatmap?.Value.BeatmapInfo;
        private IBindable<WorkingBeatmap> beatmap;

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            this.beatmap = beatmap.GetBoundCopy();
            this.beatmap.ValueChanged += _ => Broadcast();
        }
    }
}

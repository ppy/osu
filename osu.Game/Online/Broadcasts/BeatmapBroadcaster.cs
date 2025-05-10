// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;

namespace osu.Game.Online.Broadcasts
{
    public partial class BeatmapBroadcaster : Broadcaster<BeatmapInfo>
    {
        private IBindable<WorkingBeatmap>? beatmap;

        public BeatmapBroadcaster()
            : base(@"beatmap")
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            this.beatmap = beatmap.GetBoundCopy();
            this.beatmap.BindValueChanged(value => Broadcast(value.NewValue.BeatmapInfo), true);
        }
    }
}

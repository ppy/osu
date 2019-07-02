// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online;

namespace osu.Game.Overlays.Direct
{
    public abstract class BeatmapDownloadTrackingComposite : DownloadTrackingComposite<BeatmapSetInfo, BeatmapManager>
    {
        public Bindable<BeatmapSetInfo> BeatmapSet => Model;

        protected BeatmapDownloadTrackingComposite(BeatmapSetInfo set = null)
            : base(set)
        {
        }
    }
}

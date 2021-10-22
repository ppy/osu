// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public abstract class BeatmapDownloadTrackingComposite : DownloadTrackingComposite<BeatmapSetInfo, BeatmapManager>
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();

        protected BeatmapDownloadTrackingComposite(BeatmapSetInfo set = null)
            : base(set)
        {
            BeatmapSet.Value = set;
            BeatmapSet.BindValueChanged(s => Model.Value = s.NewValue);
            Model.BindValueChanged(m => BeatmapSet.Value = m.NewValue as BeatmapSetInfo);
        }
    }
}

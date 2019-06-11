using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online;

namespace osu.Game.Overlays.Direct
{
    public abstract class BeatmapDownloadTrackingComposite : DownloadTrackingComposite<BeatmapSetInfo, BeatmapManager>
    {
        public Bindable<BeatmapSetInfo> BeatmapSet => ModelInfo;

        public BeatmapDownloadTrackingComposite(BeatmapSetInfo set = null)
            : base(set)
        {
        }
    }
}

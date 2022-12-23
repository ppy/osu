using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.LLin.Misc
{
    [Cached]
    public partial class BeatmapHashResolver : Component
    {
        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        public BeatmapInfo? ResolveHash(string hash)
        {
            return beatmapManager.QueryBeatmap(b => b.MD5Hash == hash);
        }
    }
}

using System;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Mvis.Misc
{
    [Obsolete("原Mvis播放器现已移动至LLin(osu.Game.Screens.LLin)")]
    public class BeatmapCover : osu.Game.Screens.LLin.Misc.BeatmapCover
    {
        public BeatmapCover(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }
    }
}

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;

namespace osu.Desktop.VisualTests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        public TestWorkingBeatmap(Beatmap beatmap)
            : base(beatmap.BeatmapInfo, beatmap.BeatmapInfo.BeatmapSet)
        {
            this.beatmap = beatmap;
        }

        private Beatmap beatmap;
        public override Beatmap Beatmap => beatmap;
        public override Texture Background => null;
        public override Track Track => null;
        
        public override void Dispose()
        {
            // This space intentionally left blank
        }
        
        public override void TransferTo(WorkingBeatmap other)
        {
            // This space intentionally left blank
        }
    }
}

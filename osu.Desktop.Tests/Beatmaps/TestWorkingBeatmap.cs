// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Desktop.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        public TestWorkingBeatmap(Beatmap beatmap)
            : base(beatmap.BeatmapInfo)
        {
            this.beatmap = beatmap;
        }

        private readonly Beatmap beatmap;

        protected override Beatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetTrack() => null;
    }
}

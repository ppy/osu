// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        private readonly IBeatmap beatmap;

        /// <summary>
        /// Create an instance which provides the <see cref="IBeatmap"/> when requested.
        /// </summary>
        /// <param name="beatmap">The beatmap</param>
        public TestWorkingBeatmap(IBeatmap beatmap)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;
        }

        protected override IBeatmap GetBeatmap() => beatmap;

        protected override Texture GetBackground() => null;

        protected override VideoSprite GetVideo() => null;

        protected override Track GetTrack() => null;
    }
}

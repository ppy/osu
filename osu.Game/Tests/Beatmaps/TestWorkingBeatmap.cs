// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        private readonly IBeatmap beatmap;
        private readonly Storyboard? storyboard;

        /// <summary>
        /// Create an instance which provides the <see cref="IBeatmap"/> when requested.
        /// </summary>
        /// <param name="beatmap">The beatmap.</param>
        /// <param name="storyboard">An optional storyboard.</param>
        /// <param name="audioManager">The <see cref="AudioManager"/>.</param>
        public TestWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null, AudioManager? audioManager = null)
            : base(beatmap.BeatmapInfo, audioManager)
        {
            this.beatmap = beatmap;
            this.storyboard = storyboard;
        }

        public override bool BeatmapLoaded => true;

        protected override IBeatmap GetBeatmap() => beatmap;

        protected override Storyboard GetStoryboard() => storyboard ?? base.GetStoryboard();

        protected internal override ISkin? GetSkin() => null;

        public override Stream? GetStream(string storagePath) => null;

        public override Texture? GetBackground() => null;

        protected override Track? GetBeatmapTrack() => null;
    }
}

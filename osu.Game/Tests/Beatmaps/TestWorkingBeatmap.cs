﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        private readonly IBeatmap beatmap;
        private readonly Storyboard storyboard;

        /// <summary>
        /// Create an instance which provides the <see cref="IBeatmap"/> when requested.
        /// </summary>
        /// <param name="beatmap">The beatmap.</param>
        /// <param name="storyboard">An optional storyboard.</param>
        public TestWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;
            this.storyboard = storyboard;
        }

        protected override IBeatmap GetBeatmap() => beatmap;

        protected override Storyboard GetStoryboard() => storyboard ?? base.GetStoryboard();

        protected override Texture GetBackground() => null;

        protected override VideoSprite GetVideo() => null;

        protected override Track GetTrack() => null;
    }
}

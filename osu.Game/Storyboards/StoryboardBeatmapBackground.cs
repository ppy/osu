// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class StoryboardBeatmapBackground : IStoryboardElement
    {
        public string Path { get; }
        public bool IsDrawable { get; } = true;
        public double StartTime { get; } = 0;

        public Anchor Anchor = Anchor.Centre;
        public Anchor Origin = Anchor.Centre;

        public Drawable CreateDrawable() => new DrawableStoryboardBeatmapBackground(this);

        public StoryboardBeatmapBackground(string path)
        {
            Path = path;
        }
    }
}

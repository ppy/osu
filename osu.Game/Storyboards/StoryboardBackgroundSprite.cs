// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class StoryboardBackgroundSprite : IStoryboardElementWithDuration
    {
        public string Path { get; }
        public bool IsDrawable => true;
        public double StartTime => 0;
        public double EndTime { get; }

        public StoryboardBackgroundSprite(string path, double endTime)
        {
            Path = path;
            EndTime = endTime;
        }

        public Drawable CreateDrawable() => new DrawableStoryboardBackgroundSprite(this);
    }
}

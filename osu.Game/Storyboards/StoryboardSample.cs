// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class StoryboardSample : IStoryboardElement
    {
        public string Path { get; set; }
        public bool IsDrawable => true;

        public double StartTime { get; }

        public float Volume;

        public StoryboardSample(string path, double time, float volume)
        {
            Path = path;
            StartTime = time;
            Volume = volume;
        }

        public Drawable CreateDrawable() => new DrawableStoryboardSample(this);
    }
}

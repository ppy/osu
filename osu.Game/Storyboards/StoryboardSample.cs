// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards
{
    public class StoryboardSampleInfo : IStoryboardElement, ISampleInfo
    {
        public string Path { get; }
        public bool IsDrawable => true;

        public double StartTime { get; }

        public int Volume { get; }

        public IEnumerable<string> LookupNames => new[]
        {
            // Try first with the full name, then attempt with no path
            Path,
            System.IO.Path.ChangeExtension(Path, null),
        };

        public StoryboardSampleInfo(string path, double time, int volume)
        {
            Path = path;
            StartTime = time;
            Volume = volume;
        }

        public Drawable CreateDrawable() => new DrawableStoryboardSample(this);
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using System;

namespace osu.Game.Storyboards
{
    public class StoryboardSample : IStoryboardElement
    {
        public string Path { get; set; }
        public bool IsDrawable => false;

        public double Time;
        public float Volume;

        public StoryboardSample(string path, double time, float volume)
        {
            Path = path;
            Time = time;
            Volume = volume;
        }

        public Drawable CreateDrawable()
        {
            throw new InvalidOperationException();
        }
    }
}

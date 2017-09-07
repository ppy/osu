// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Storyboards
{
    public class SampleDefinition : ElementDefinition
    {
        public string Path { get; private set; }
        public double Time;
        public float Volume;

        public SampleDefinition(string path, double time, float volume)
        {
            Path = path;
            Time = time;
            Volume = volume;
        }

        public Drawable CreateDrawable()
            => null;
    }
}

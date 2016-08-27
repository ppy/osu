//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Timing
{
    /// <summary>
    /// Takes a clock source and separates time reading on a per-frame level.
    /// The CurrentTime value will only update when ProcessFrame is run.
    /// </summary>
    public class FramedClock : IFrameBasedClock
    {
        public IClock Source { get; }

        public FramedClock(IClock source)
        {
            Source = source;
        }

        public virtual double CurrentTime { get; private set;}

        public virtual double LastFrameTime { get; private set; }

        public double Rate => Source.Rate;

        protected double SourceTime => Source.CurrentTime;

        public double ElapsedFrameTime => CurrentTime - LastFrameTime;

        public bool IsRunning => Source?.IsRunning ?? false;

        public virtual void ProcessFrame()
        {
            LastFrameTime = CurrentTime;
            CurrentTime = SourceTime;
        }
    }
}

// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Storyboards
{
    public class CommandLoop : CommandTimelineGroup
    {
        private double startTime;
        private int loopCount;

        public CommandLoop(double startTime, int loopCount)
        {
            this.startTime = startTime;
            this.loopCount = loopCount;
        }

        public override void ApplyTransforms(Drawable drawable)
        {
            //base.ApplyTransforms(drawable);
        }

        public override string ToString()
            => $"{startTime} x{loopCount}";
    }
}

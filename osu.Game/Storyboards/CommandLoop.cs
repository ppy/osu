// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards
{
    public class CommandLoop : CommandTimelineGroup
    {
        public double LoopStartTime;
        public int LoopCount;

        public CommandLoop(double startTime, int loopCount)
        {
            LoopStartTime = startTime;
            LoopCount = loopCount;
        }

        public override void ApplyTransforms(Drawable drawable, double offset = 0)
            => base.ApplyTransforms(drawable, offset + LoopStartTime);

        protected override void PostProcess(Command command, TransformSequence<Drawable> sequence)
            => sequence.Loop(Duration - command.Duration, LoopCount);

        public override string ToString()
            => $"{LoopStartTime} x{LoopCount}";
    }
}

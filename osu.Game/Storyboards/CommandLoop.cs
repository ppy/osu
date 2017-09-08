// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class CommandLoop : CommandTimelineGroup
    {
        public double LoopStartTime;
        public int LoopCount;

        public override double StartTime => LoopStartTime;
        public override double EndTime => LoopStartTime + CommandsDuration * LoopCount;

        public CommandLoop(double startTime, int loopCount)
        {
            LoopStartTime = startTime;
            LoopCount = loopCount;
        }

        public override IEnumerable<CommandTimeline<T>.TypedCommand> GetCommands<T>(CommandTimelineSelector<T> timelineSelector, double offset = 0)
        {
            for (var loop = 0; loop < LoopCount; loop++)
            {
                var loopOffset = LoopStartTime + loop * CommandsDuration;
                foreach (var command in base.GetCommands(timelineSelector, offset + loopOffset))
                    yield return command;
            }
        }

        public override string ToString()
            => $"{LoopStartTime} x{LoopCount}";
    }
}

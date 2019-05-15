// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class CommandLoop : CommandTimelineGroup
    {
        public double LoopStartTime;
        public int LoopCount;

        public override double StartTime => LoopStartTime + CommandsStartTime;
        public override double EndTime => StartTime + CommandsDuration * LoopCount;

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

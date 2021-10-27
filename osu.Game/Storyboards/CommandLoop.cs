// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class CommandLoop : CommandTimelineGroup
    {
        public double LoopStartTime;

        /// <summary>
        /// The total number of times this loop is played back. Always greater than zero.
        /// </summary>
        public readonly int TotalIterations;

        public override double StartTime => LoopStartTime + CommandsStartTime;
        public override double EndTime => StartTime + CommandsDuration * TotalIterations;

        /// <summary>
        /// Construct a new command loop.
        /// </summary>
        /// <param name="startTime">The start time of the loop.</param>
        /// <param name="repeatCount">The number of times the loop should repeat. Should be greater than zero. Zero means a single playback.</param>
        public CommandLoop(double startTime, int repeatCount)
        {
            if (repeatCount < 0) throw new ArgumentException("Repeat count must be zero or above.", nameof(repeatCount));

            LoopStartTime = startTime;
            TotalIterations = repeatCount + 1;
        }

        public override IEnumerable<CommandTimeline<T>.TypedCommand> GetCommands<T>(CommandTimelineSelector<T> timelineSelector, double offset = 0)
        {
            for (int loop = 0; loop < TotalIterations; loop++)
            {
                double loopOffset = LoopStartTime + loop * CommandsDuration;
                foreach (var command in base.GetCommands(timelineSelector, offset + loopOffset))
                    yield return command;
            }
        }

        public override string ToString()
            => $"{LoopStartTime} x{TotalIterations}";
    }
}

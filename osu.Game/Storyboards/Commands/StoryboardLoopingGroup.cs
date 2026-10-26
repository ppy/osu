// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardLoopingGroup : StoryboardCommandGroup
    {
        /// <summary>
        /// Time this at which the loop starts.
        /// </summary>
        /// <remarks>
        /// Different from `StoryboardCommandGroup.StartTime`, which is the time of the earliest
        /// command of this group.
        /// </remarks>
        public readonly double LoopStartTime;

        /// <summary>
        /// The total number of times this loop is played back. Always greater than zero.
        /// </summary>
        public readonly int TotalIterations;

        /// <summary>
        /// Construct a new command loop.
        /// </summary>
        /// <param name="startTime">The start time of the loop.</param>
        /// <param name="repeatCount">The number of times the loop should repeat. Should be greater than zero. Zero means a single playback.</param>
        public StoryboardLoopingGroup(double startTime, int repeatCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(repeatCount);

            LoopStartTime = startTime;
            TotalIterations = repeatCount + 1;
        }

        protected override void AddCommand<T>(ICollection<StoryboardCommand<T>> list, StoryboardCommand<T> command)
            => base.AddCommand(list, new StoryboardLoopingCommand<T>(command, this));

        public override string ToString() => $"{LoopStartTime} x{TotalIterations}";

        private class StoryboardLoopingCommand<T> : StoryboardCommand<T>, IStoryboardLoopingCommand
        {
            IStoryboardCommand IStoryboardLoopingCommand.OriginalCommand => command;

            private readonly StoryboardCommand<T> command;
            private readonly StoryboardLoopingGroup loopingGroup;

            public StoryboardLoopingCommand(StoryboardCommand<T> command, StoryboardLoopingGroup loopingGroup)
                // In an ideal world, we would multiply the command duration by TotalIterations in command end time.
                // Unfortunately this would clash with how stable handled end times, and results in some storyboards playing outro
                // sequences for minutes or hours.
                : base(command.Easing, loopingGroup.LoopStartTime + command.StartTime, loopingGroup.LoopStartTime + command.EndTime, command.StartValue, command.EndValue)
            {
                this.command = command;
                this.loopingGroup = loopingGroup;
            }

            public override string PropertyName => command.PropertyName;

            public override void ApplyInitialValue<TDrawable>(TDrawable d) => command.ApplyInitialValue(d);

            public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            {
                double loopingGroupDuration = loopingGroup.Duration;

                if (loopingGroupDuration == 0 || loopingGroup.TotalIterations == 0)
                    return command.ApplyTransforms(d);

                return command.ApplyTransforms(d).Loop(loopingGroupDuration - Duration, loopingGroup.TotalIterations);
            }
        }
    }
}

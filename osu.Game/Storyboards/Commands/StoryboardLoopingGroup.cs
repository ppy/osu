// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardLoopingGroup : StoryboardCommandGroup
    {
        private readonly double loopStartTime;

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
            if (repeatCount < 0) throw new ArgumentException("Repeat count must be zero or above.", nameof(repeatCount));

            loopStartTime = startTime;
            TotalIterations = repeatCount + 1;
        }

        protected override void AddCommand<T>(ICollection<StoryboardCommand<T>> list, StoryboardCommand<T> command)
            => base.AddCommand(list, new StoryboardLoopingCommand<T>(command, this));

        public override string ToString() => $"{loopStartTime} x{TotalIterations}";

        private class StoryboardLoopingCommand<T> : StoryboardCommand<T>
        {
            private readonly StoryboardCommand<T> command;
            private readonly StoryboardLoopingGroup loopingGroup;

            public StoryboardLoopingCommand(StoryboardCommand<T> command, StoryboardLoopingGroup loopingGroup)
                // In an ideal world, we would multiply the command duration by TotalIterations in command end time.
                // Unfortunately this would clash with how stable handled end times, and results in some storyboards playing outro
                // sequences for minutes or hours.
                : base(loopingGroup.loopStartTime + command.StartTime, loopingGroup.loopStartTime + command.EndTime, command.StartValue, command.EndValue, command.Easing)
            {
                this.command = command;
                this.loopingGroup = loopingGroup;
            }

            public override string PropertyName => command.PropertyName;

            public override void ApplyInitialValue(Drawable d) => command.ApplyInitialValue(d);

            public override TransformSequence<Drawable> ApplyTransforms(Drawable d)
            {
                if (loopingGroup.TotalIterations == 0)
                    return command.ApplyTransforms(d);

                double loopingGroupDuration = loopingGroup.Duration;
                return command.ApplyTransforms(d).Loop(loopingGroupDuration - Duration, loopingGroup.TotalIterations);
            }
        }
    }
}

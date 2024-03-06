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
        public double LoopStartTime;

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

            LoopStartTime = startTime;
            TotalIterations = repeatCount + 1;
        }

        protected override void AddCommand<T>(ICollection<StoryboardCommand<T>> list, StoryboardCommand<T> command)
        {
            // todo: this is broke!
            double fullLoopDuration = EndTime - StartTime;
            double loopDelay = fullLoopDuration - command.EndTime + command.StartTime;
            base.AddCommand(list, new StoryboardLoopingCommand<T>(command, LoopStartTime, TotalIterations, loopDelay));
        }

        public override string ToString() => $"{LoopStartTime} x{TotalIterations}";

        private class StoryboardLoopingCommand<T> : StoryboardCommand<T>
        {
            private readonly StoryboardCommand<T> command;
            private readonly int loopCount;
            private readonly double loopDelay;

            public StoryboardLoopingCommand(StoryboardCommand<T> command, double loopStartTime, int loopCount, double loopDelay)
                // In an ideal world, we would multiply the command duration by TotalIterations in command end time.
                // Unfortunately this would clash with how stable handled end times, and results in some storyboards playing outro
                // sequences for minutes or hours.
                : base(loopStartTime + command.StartTime, loopStartTime + command.EndTime, command.StartValue, command.EndValue, command.Easing)
            {
                this.command = command;
                this.loopCount = loopCount;
                this.loopDelay = loopDelay;
            }

            public override void SetInitialValue(Drawable d) => command.SetInitialValue(d);

            public override TransformSequence<Drawable> ApplyTransform(Drawable d)
            {
                if (loopCount == 0)
                    return command.ApplyTransform(d);

                return command.ApplyTransform(d).Loop(loopDelay, loopCount);
            }
        }
    }
}

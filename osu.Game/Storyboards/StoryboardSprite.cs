// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Commands;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Storyboards
{
    public class StoryboardSprite : IStoryboardElementWithDuration
    {
        private readonly List<StoryboardLoopingGroup> loopingGroups = new List<StoryboardLoopingGroup>();
        private readonly List<StoryboardTriggerGroup> triggerGroups = new List<StoryboardTriggerGroup>();

        public string Path { get; }
        public bool IsDrawable => HasCommands;

        public Anchor Origin;
        public Vector2 InitialPosition;

        public readonly StoryboardCommandGroup Commands = new StoryboardCommandGroup();

        public double StartTime
        {
            get
            {
                // To get the initial start time, we need to check whether the first alpha command to exist (across all loops) has a StartValue of zero.
                // A StartValue of zero governs, above all else, the first valid display time of a sprite.
                //
                // You can imagine that the first command of each type decides that type's start value, so if the initial alpha is zero,
                // anything before that point can be ignored (the sprite is not visible after all).
                var alphaCommands = new List<(double startTime, bool isZeroStartValue)>();

                var command = Commands.Alpha.FirstOrDefault();
                if (command != null) alphaCommands.Add((command.StartTime, command.StartValue == 0));

                foreach (var loop in loopingGroups)
                {
                    command = loop.Alpha.FirstOrDefault();
                    if (command != null) alphaCommands.Add((command.StartTime, command.StartValue == 0));
                }

                if (alphaCommands.Count > 0)
                {
                    var firstAlpha = alphaCommands.MinBy(t => t.startTime);

                    if (firstAlpha.isZeroStartValue)
                        return firstAlpha.startTime;
                }

                return EarliestTransformTime;
            }
        }

        public double EarliestTransformTime
        {
            get
            {
                // If we got to this point, either no alpha commands were present, or the earliest had a non-zero start value.
                // The sprite's StartTime will be determined by the earliest command, regardless of type.
                double earliestStartTime = Commands.StartTime;
                foreach (var l in loopingGroups)
                    earliestStartTime = Math.Min(earliestStartTime, l.StartTime);
                return earliestStartTime;
            }
        }

        public double EndTime
        {
            get
            {
                double latestEndTime = Commands.EndTime;

                foreach (var l in loopingGroups)
                    latestEndTime = Math.Max(latestEndTime, l.EndTime);

                return latestEndTime;
            }
        }

        public double EndTimeForDisplay
        {
            get
            {
                double latestEndTime = double.MaxValue;

                // Ignore the whole setup if there are loops. In theory they can be handled here too, however the logic will be overly complex.
                if (loopingGroups.Count == 0)
                {
                    // Take the minimum time of all the potential "death" reasons.
                    latestEndTime = calculateOptimisedEndTime(Commands);
                }

                // If the logic above fails to find anything or discarded by the fact that there are loops present, latestEndTime will be double.MaxValue
                // and thus conservativeEndTime will be used.
                double conservativeEndTime = TimelineGroup.EndTime;

                foreach (var l in loops)
                    conservativeEndTime = Math.Max(conservativeEndTime, l.StartTime + l.CommandsDuration * l.TotalIterations);

                return Math.Min(latestEndTime, conservativeEndTime);
            }
        }

        public bool HasCommands => Commands.HasCommands || loopingGroups.Any(l => l.HasCommands);

        public StoryboardSprite(string path, Anchor origin, Vector2 initialPosition)
        {
            Path = path;
            Origin = origin;
            InitialPosition = initialPosition;
        }

        public virtual Drawable CreateDrawable() => new DrawableStoryboardSprite(this);

        public StoryboardLoopingGroup AddLoopingGroup(double loopStartTime, int repeatCount)
        {
            var loop = new StoryboardLoopingGroup(loopStartTime, repeatCount);
            loopingGroups.Add(loop);
            return loop;
        }

        public StoryboardTriggerGroup AddTriggerGroup(string triggerName, double startTime, double endTime, int groupNumber)
        {
            var trigger = new StoryboardTriggerGroup(triggerName, startTime, endTime, groupNumber);
            triggerGroups.Add(trigger);
            return trigger;
        }

        public void ApplyTransforms<TDrawable>(TDrawable drawable)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            HashSet<string> appliedProperties = new HashSet<string>();

            // For performance reasons, we need to apply the commands in chronological order.
            // Not doing so will cause many functions to be interleaved, resulting in O(n^2) complexity.
            IEnumerable<IStoryboardCommand> commands = Commands.AllCommands;
            commands = commands.Concat(loopingGroups.SelectMany(l => l.AllCommands));

            foreach (var command in commands.OrderBy(c => c.StartTime))
            {
                if (appliedProperties.Add(command.PropertyName))
                    command.ApplyInitialValue(drawable);

                using (drawable.BeginAbsoluteSequence(command.StartTime))
                    command.ApplyTransforms(drawable);
            }
        }

        private static double calculateOptimisedEndTime(StoryboardCommandGroup commands)
        {
            // Here we are starting from maximum value and trying to minimise the end time on each step.
            // There are few solid guesses we can make using which sprite's end time can be minimised: alpha = 0, scale = 0, colour.a = 0.
            double[] deathTimes =
            {
                double.MaxValue, // alpha
                double.MaxValue, // colour alpha
                double.MaxValue, // scale
                double.MaxValue, // scale x
                double.MaxValue, // scale y
            };

            // The loops below are following the same pattern.
            // We could be using TimelineGroup.EndValue here, however it's possible to have multiple commands with 0 value in a row
            // so we are saving the earliest of them.
            foreach (var alphaCommand in commands.Alpha)
            {
                if (alphaCommand.EndValue == 0)
                    // commands are ordered by the start time, however end time may vary. Save the earliest.
                    deathTimes[0] = Math.Min(alphaCommand.EndTime, deathTimes[0]);
                else
                    // If value isn't 0 (sprite becomes visible again), revert the saved state.
                    deathTimes[0] = double.MaxValue;
            }

            foreach (var colourCommand in commands.Colour)
                deathTimes[1] = colourCommand.EndValue.A == 0 ? Math.Min(colourCommand.EndTime, deathTimes[1]) : double.MaxValue;

            foreach (var scaleCommand in commands.Scale)
                deathTimes[2] = scaleCommand.EndValue == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[2]) : double.MaxValue;

            foreach (var scaleCommand in commands.VectorScale)
            {
                deathTimes[3] = scaleCommand.EndValue.X == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[3]) : double.MaxValue;
                deathTimes[4] = scaleCommand.EndValue.Y == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[4]) : double.MaxValue;
            }

            return deathTimes.Min();
        }

        public override string ToString() => $"{Path}, {Origin}, {InitialPosition}";
    }
}


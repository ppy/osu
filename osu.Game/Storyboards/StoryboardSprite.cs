// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Storyboards
{
    public class StoryboardSprite : IStoryboardElementWithDuration
    {
        private readonly List<CommandLoop> loops = new List<CommandLoop>();
        private readonly List<CommandTrigger> triggers = new List<CommandTrigger>();

        public string Path { get; }
        public bool IsDrawable => HasCommands;

        public Anchor Origin;
        public Vector2 InitialPosition;

        public readonly CommandTimelineGroup TimelineGroup = new CommandTimelineGroup();

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

                var command = TimelineGroup.Alpha.Commands.FirstOrDefault();
                if (command != null) alphaCommands.Add((command.StartTime, command.StartValue == 0));

                foreach (var loop in loops)
                {
                    command = loop.Alpha.Commands.FirstOrDefault();
                    if (command != null) alphaCommands.Add((command.StartTime + loop.LoopStartTime, command.StartValue == 0));
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
                double earliestStartTime = TimelineGroup.StartTime;
                foreach (var l in loops)
                    earliestStartTime = Math.Min(earliestStartTime, l.StartTime);
                return earliestStartTime;
            }
        }

        public double EndTime
        {
            get
            {
                double latestEndTime = TimelineGroup.EndTime;

                foreach (var l in loops)
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
                if (loops.Count == 0)
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
                    foreach (var alphaCommand in TimelineGroup.Alpha.Commands)
                    {
                        deathTimes[0] = alphaCommand.EndValue == 0
                            ? Math.Min(alphaCommand.EndTime, deathTimes[0]) // commands are ordered by the start time, however end time may vary. Save the earliest.
                            : double.MaxValue; // If value isn't 0 (sprite becomes visible again), revert the saved state.
                    }

                    foreach (var colourCommand in TimelineGroup.Colour.Commands)
                        deathTimes[1] = colourCommand.EndValue.A == 0 ? Math.Min(colourCommand.EndTime, deathTimes[1]) : double.MaxValue;

                    foreach (var scaleCommand in TimelineGroup.Scale.Commands)
                        deathTimes[2] = scaleCommand.EndValue == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[2]) : double.MaxValue;

                    foreach (var scaleCommand in TimelineGroup.VectorScale.Commands)
                    {
                        deathTimes[3] = scaleCommand.EndValue.X == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[3]) : double.MaxValue;
                        deathTimes[4] = scaleCommand.EndValue.Y == 0 ? Math.Min(scaleCommand.EndTime, deathTimes[4]) : double.MaxValue;
                    }

                    // Take the minimum time of all the potential "death" reasons.
                    latestEndTime = deathTimes.Min();
                }

                // If the logic above fails to find anything or discarded by the fact that there are loops present, latestEndTime will be double.MaxValue
                // and thus conservativeEndTime will be used.
                double conservativeEndTime = TimelineGroup.EndTime;

                foreach (var l in loops)
                    conservativeEndTime = Math.Max(conservativeEndTime, l.StartTime + l.CommandsDuration * l.TotalIterations);

                return Math.Min(latestEndTime, conservativeEndTime);
            }
        }

        public bool HasCommands => TimelineGroup.HasCommands || loops.Any(l => l.HasCommands);

        private delegate void DrawablePropertyInitializer<in T>(Drawable drawable, T value);

        private delegate void DrawableTransformer<in T>(Drawable drawable, T value, double duration, Easing easing);

        public StoryboardSprite(string path, Anchor origin, Vector2 initialPosition)
        {
            Path = path;
            Origin = origin;
            InitialPosition = initialPosition;
        }

        public CommandLoop AddLoop(double startTime, int repeatCount)
        {
            var loop = new CommandLoop(startTime, repeatCount);
            loops.Add(loop);
            return loop;
        }

        public CommandTrigger AddTrigger(string triggerName, double startTime, double endTime, int groupNumber)
        {
            var trigger = new CommandTrigger(triggerName, startTime, endTime, groupNumber);
            triggers.Add(trigger);
            return trigger;
        }

        public virtual Drawable CreateDrawable()
            => new DrawableStoryboardSprite(this);

        public void ApplyTransforms(Drawable drawable, IEnumerable<Tuple<CommandTimelineGroup, double>>? triggeredGroups = null)
        {
            // For performance reasons, we need to apply the commands in order by start time. Not doing so will cause many functions to be interleaved, resulting in O(n^2) complexity.
            // To achieve this, commands are "generated" as pairs of (command, initFunc, transformFunc) and batched into a contiguous list
            // The list is then stably-sorted (to preserve command order), and applied to the drawable sequentially.

            List<IGeneratedCommand> generated = new List<IGeneratedCommand>();

            generateCommands(generated, getCommands(g => g.X, triggeredGroups), (d, value) => d.X = value, (d, value, duration, easing) => d.MoveToX(value, duration, easing));
            generateCommands(generated, getCommands(g => g.Y, triggeredGroups), (d, value) => d.Y = value, (d, value, duration, easing) => d.MoveToY(value, duration, easing));
            generateCommands(generated, getCommands(g => g.Scale, triggeredGroups), (d, value) => d.Scale = new Vector2(value), (d, value, duration, easing) => d.ScaleTo(value, duration, easing));
            generateCommands(generated, getCommands(g => g.Rotation, triggeredGroups), (d, value) => d.Rotation = value, (d, value, duration, easing) => d.RotateTo(value, duration, easing));
            generateCommands(generated, getCommands(g => g.Colour, triggeredGroups), (d, value) => d.Colour = value, (d, value, duration, easing) => d.FadeColour(value, duration, easing));
            generateCommands(generated, getCommands(g => g.Alpha, triggeredGroups), (d, value) => d.Alpha = value, (d, value, duration, easing) => d.FadeTo(value, duration, easing));
            generateCommands(generated, getCommands(g => g.BlendingParameters, triggeredGroups), (d, value) => d.Blending = value, (d, value, duration, _) => d.TransformBlendingMode(value, duration),
                false);

            if (drawable is IVectorScalable vectorScalable)
            {
                generateCommands(generated, getCommands(g => g.VectorScale, triggeredGroups), (_, value) => vectorScalable.VectorScale = value,
                    (_, value, duration, easing) => vectorScalable.VectorScaleTo(value, duration, easing));
            }

            if (drawable is IFlippable flippable)
            {
                generateCommands(generated, getCommands(g => g.FlipH, triggeredGroups), (_, value) => flippable.FlipH = value, (_, value, duration, _) => flippable.TransformFlipH(value, duration),
                    false);
                generateCommands(generated, getCommands(g => g.FlipV, triggeredGroups), (_, value) => flippable.FlipV = value, (_, value, duration, _) => flippable.TransformFlipV(value, duration),
                    false);
            }

            foreach (var command in generated.OrderBy(g => g.StartTime))
                command.ApplyTo(drawable);
        }

        private void generateCommands<T>(List<IGeneratedCommand> resultList, IEnumerable<CommandTimeline<T>.TypedCommand> commands,
                                         DrawablePropertyInitializer<T> initializeProperty, DrawableTransformer<T> transform, bool alwaysInitialize = true)
        {
            bool initialized = false;

            foreach (var command in commands)
            {
                DrawablePropertyInitializer<T>? initFunc = null;

                if (!initialized)
                {
                    if (alwaysInitialize || command.StartTime == command.EndTime)
                        initFunc = initializeProperty;
                    initialized = true;
                }

                resultList.Add(new GeneratedCommand<T>(command, initFunc, transform));
            }
        }

        private IEnumerable<CommandTimeline<T>.TypedCommand> getCommands<T>(CommandTimelineSelector<T> timelineSelector, IEnumerable<Tuple<CommandTimelineGroup, double>>? triggeredGroups)
        {
            var commands = TimelineGroup.GetCommands(timelineSelector);
            foreach (var loop in loops)
                commands = commands.Concat(loop.GetCommands(timelineSelector));

            if (triggeredGroups != null)
            {
                foreach (var pair in triggeredGroups)
                    commands = commands.Concat(pair.Item1.GetCommands(timelineSelector, pair.Item2));
            }

            return commands;
        }

        public override string ToString()
            => $"{Path}, {Origin}, {InitialPosition}";

        private interface IGeneratedCommand
        {
            double StartTime { get; }

            void ApplyTo(Drawable drawable);
        }

        private readonly struct GeneratedCommand<T> : IGeneratedCommand
        {
            public double StartTime => command.StartTime;

            private readonly DrawablePropertyInitializer<T>? initializeProperty;
            private readonly DrawableTransformer<T> transform;
            private readonly CommandTimeline<T>.TypedCommand command;

            public GeneratedCommand(CommandTimeline<T>.TypedCommand command, DrawablePropertyInitializer<T>? initializeProperty, DrawableTransformer<T> transform)
            {
                this.command = command;
                this.initializeProperty = initializeProperty;
                this.transform = transform;
            }

            public void ApplyTo(Drawable drawable)
            {
                initializeProperty?.Invoke(drawable, command.StartValue);

                using (drawable.BeginAbsoluteSequence(command.StartTime))
                {
                    transform(drawable, command.StartValue, 0, Easing.None);
                    transform(drawable, command.EndValue, command.Duration, command.Easing);
                }
            }
        }
    }
}

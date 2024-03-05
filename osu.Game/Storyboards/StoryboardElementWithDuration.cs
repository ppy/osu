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
    public abstract class StoryboardElementWithDuration : IStoryboardElementWithDuration
    {
        protected readonly List<CommandLoop> Loops = new List<CommandLoop>();
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

                foreach (var loop in Loops)
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
                foreach (var l in Loops)
                    earliestStartTime = Math.Min(earliestStartTime, l.StartTime);
                return earliestStartTime;
            }
        }

        public double EndTime
        {
            get
            {
                double latestEndTime = TimelineGroup.EndTime;

                foreach (var l in Loops)
                    latestEndTime = Math.Max(latestEndTime, l.EndTime);

                return latestEndTime;
            }
        }

        public double EndTimeForDisplay
        {
            get
            {
                double latestEndTime = TimelineGroup.EndTime;

                foreach (var l in Loops)
                    latestEndTime = Math.Max(latestEndTime, l.StartTime + l.CommandsDuration * l.TotalIterations);

                return latestEndTime;
            }
        }

        public bool HasCommands => TimelineGroup.HasCommands || Loops.Any(l => l.HasCommands);

        protected StoryboardElementWithDuration(string path, Anchor origin, Vector2 initialPosition)
        {
            Path = path;
            Origin = origin;
            InitialPosition = initialPosition;
        }

        public abstract Drawable CreateDrawable();

        public CommandLoop AddLoop(double startTime, int repeatCount)
        {
            var loop = new CommandLoop(startTime, repeatCount);
            Loops.Add(loop);
            return loop;
        }

        public CommandTrigger AddTrigger(string triggerName, double startTime, double endTime, int groupNumber)
        {
            var trigger = new CommandTrigger(triggerName, startTime, endTime, groupNumber);
            triggers.Add(trigger);
            return trigger;
        }

        protected IEnumerable<CommandTimeline<T>.TypedCommand> GetCommands<T>(CommandTimelineSelector<T> timelineSelector, IEnumerable<Tuple<CommandTimelineGroup, double>>? triggeredGroups)
        {
            var commands = TimelineGroup.GetCommands(timelineSelector);
            foreach (var loop in Loops)
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
    }

    public abstract class StoryboardElementWithDuration<U> : StoryboardElementWithDuration
        where U : Drawable, IDrawableStoryboardElement
    {
        private delegate void DrawablePropertyInitializer<in T>(U drawable, T value);

        protected StoryboardElementWithDuration(string path, Anchor origin, Vector2 initialPosition)
            : base(path, origin, initialPosition)
        {
        }

        public override Drawable CreateDrawable() => CreateStoryboardDrawable();

        public abstract U CreateStoryboardDrawable();

        public void ApplyTransforms(U drawable, IEnumerable<Tuple<CommandTimelineGroup, double>>? triggeredGroups = null)
        {
            // For performance reasons, we need to apply the commands in order by start time. Not doing so will cause many functions to be interleaved, resulting in O(n^2) complexity.
            // To achieve this, commands are "generated" as pairs of (command, initFunc, transformFunc) and batched into a contiguous list
            // The list is then stably-sorted (to preserve command order), and applied to the drawable sequentially.

            List<IGeneratedCommand<U>> generated = new List<IGeneratedCommand<U>>();

            generateCommands(generated, GetCommands(g => g.X, triggeredGroups), (d, value) => d.X = value);
            generateCommands(generated, GetCommands(g => g.Y, triggeredGroups), (d, value) => d.Y = value);
            generateCommands(generated, GetCommands(g => g.Scale, triggeredGroups), (d, value) => d.Scale = value);
            generateCommands(generated, GetCommands(g => g.Rotation, triggeredGroups), (d, value) => d.Rotation = value);
            generateCommands(generated, GetCommands(g => g.Colour, triggeredGroups), (d, value) => d.Colour = value);
            generateCommands(generated, GetCommands(g => g.Alpha, triggeredGroups), (d, value) => d.Alpha = value);
            generateCommands(generated, GetCommands(g => g.BlendingParameters, triggeredGroups), (d, value) => d.Blending = value, false);
            generateCommands(generated, GetCommands(g => g.VectorScale, triggeredGroups), (d, value) => d.VectorScale = value);
            generateCommands(generated, GetCommands(g => g.FlipH, triggeredGroups), (d, value) => d.FlipH = value, false);
            generateCommands(generated, GetCommands(g => g.FlipV, triggeredGroups), (d, value) => d.FlipV = value, false);

            foreach (var command in generated.OrderBy(g => g.StartTime))
                command.ApplyTo(drawable);
        }

        private void generateCommands<T>(List<IGeneratedCommand<U>> resultList, IEnumerable<CommandTimeline<T>.TypedCommand> commands,
                                         DrawablePropertyInitializer<T> initializeProperty, bool alwaysInitialize = true)
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

                resultList.Add(new GeneratedCommand<T, U>(command, initFunc));
            }
        }

        private interface IGeneratedCommand<in TDrawable>
            where TDrawable : U
        {
            double StartTime { get; }

            void ApplyTo(TDrawable drawable);
        }

        private readonly struct GeneratedCommand<T, TDrawable> : IGeneratedCommand<TDrawable>
            where TDrawable : U
        {
            public double StartTime => command.StartTime;

            private readonly DrawablePropertyInitializer<T>? initializeProperty;
            private readonly CommandTimeline<T>.TypedCommand command;

            public GeneratedCommand(CommandTimeline<T>.TypedCommand command, DrawablePropertyInitializer<T>? initializeProperty)
            {
                this.command = command;
                this.initializeProperty = initializeProperty;
            }

            public void ApplyTo(TDrawable drawable)
            {
                initializeProperty?.Invoke(drawable, command.StartValue);

                using (drawable.BeginAbsoluteSequence(command.StartTime))
                    transform(drawable);
            }

            private void transform(TDrawable drawable)
            {
                if (command.IsParameterCommand)
                {
                    if (command.LoopCount == 0)
                    {
                        drawable.TransformTo(command.PropertyName, command.StartValue).Delay(command.Duration)
                                .TransformTo(command.PropertyName, command.EndValue);
                    }
                    else
                    {
                        drawable.TransformTo(command.PropertyName, command.StartValue).Delay(command.Duration)
                                .TransformTo(command.PropertyName, command.EndValue)
                                .Loop(command.Delay, command.LoopCount);
                    }
                }
                else
                {
                    if (command.LoopCount == 0)
                    {
                        drawable.TransformTo(command.PropertyName, command.StartValue).Then()
                                .TransformTo(command.PropertyName, command.EndValue, command.Duration, command.Easing);
                    }
                    else
                    {
                        drawable.TransformTo(command.PropertyName, command.StartValue).Then()
                                .TransformTo(command.PropertyName, command.EndValue, command.Duration, command.Easing)
                                .Loop(command.Delay, command.LoopCount);
                    }
                }
            }
        }
    }
}

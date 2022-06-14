// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
                // check for presence affecting commands as an initial pass.
                double earliestStartTime = TimelineGroup.EarliestDisplayedTime ?? double.MaxValue;

                foreach (var l in loops)
                {
                    if (l.EarliestDisplayedTime is double loopEarliestDisplayTime)
                        earliestStartTime = Math.Min(earliestStartTime, l.LoopStartTime + loopEarliestDisplayTime);
                }

                if (earliestStartTime < double.MaxValue)
                    return earliestStartTime;

                // if an alpha-affecting command was not found, use the earliest of any command.
                earliestStartTime = TimelineGroup.StartTime;

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

        public void ApplyTransforms(Drawable drawable, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups = null)
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
            generateCommands(generated, getCommands(g => g.BlendingParameters, triggeredGroups), (d, value) => d.Blending = value, (d, value, duration, easing) => d.TransformBlendingMode(value, duration),
                false);

            if (drawable is IVectorScalable vectorScalable)
            {
                generateCommands(generated, getCommands(g => g.VectorScale, triggeredGroups), (d, value) => vectorScalable.VectorScale = value,
                    (d, value, duration, easing) => vectorScalable.VectorScaleTo(value, duration, easing));
            }

            if (drawable is IFlippable flippable)
            {
                generateCommands(generated, getCommands(g => g.FlipH, triggeredGroups), (d, value) => flippable.FlipH = value, (d, value, duration, easing) => flippable.TransformFlipH(value, duration),
                    false);
                generateCommands(generated, getCommands(g => g.FlipV, triggeredGroups), (d, value) => flippable.FlipV = value, (d, value, duration, easing) => flippable.TransformFlipV(value, duration),
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
                DrawablePropertyInitializer<T> initFunc = null;

                if (!initialized)
                {
                    if (alwaysInitialize || command.StartTime == command.EndTime)
                        initFunc = initializeProperty;
                    initialized = true;
                }

                resultList.Add(new GeneratedCommand<T>(command, initFunc, transform));
            }
        }

        private IEnumerable<CommandTimeline<T>.TypedCommand> getCommands<T>(CommandTimelineSelector<T> timelineSelector, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups)
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

            private readonly DrawablePropertyInitializer<T> initializeProperty;
            private readonly DrawableTransformer<T> transform;
            private readonly CommandTimeline<T>.TypedCommand command;

            public GeneratedCommand([NotNull] CommandTimeline<T>.TypedCommand command, [CanBeNull] DrawablePropertyInitializer<T> initializeProperty, [NotNull] DrawableTransformer<T> transform)
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

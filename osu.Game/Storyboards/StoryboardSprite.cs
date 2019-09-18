// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class StoryboardSprite : IStoryboardElement
    {
        private readonly List<CommandLoop> loops = new List<CommandLoop>();
        private readonly List<CommandTrigger> triggers = new List<CommandTrigger>();

        public string Path { get; set; }
        public bool IsDrawable => HasCommands;

        public Anchor Origin;
        public Vector2 InitialPosition;

        public readonly CommandTimelineGroup TimelineGroup = new CommandTimelineGroup();

        public double StartTime => Math.Min(
            TimelineGroup.HasCommands ? TimelineGroup.CommandsStartTime : double.MaxValue,
            loops.Any(l => l.HasCommands) ? loops.Where(l => l.HasCommands).Min(l => l.StartTime) : double.MaxValue);

        public double EndTime => Math.Max(
            TimelineGroup.HasCommands ? TimelineGroup.CommandsEndTime : double.MinValue,
            loops.Any(l => l.HasCommands) ? loops.Where(l => l.HasCommands).Max(l => l.EndTime) : double.MinValue);

        public bool HasCommands => TimelineGroup.HasCommands || loops.Any(l => l.HasCommands);

        private delegate void DrawablePropertyInitializer<in T>(Drawable drawable, T value);

        private delegate void DrawableTransformer<in T>(Drawable drawable, T value, double duration, Easing easing);

        public StoryboardSprite(string path, Anchor origin, Vector2 initialPosition)
        {
            Path = path;
            Origin = origin;
            InitialPosition = initialPosition;
        }

        public CommandLoop AddLoop(double startTime, int loopCount)
        {
            var loop = new CommandLoop(startTime, loopCount);
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
            applyCommands(drawable, getCommands(g => g.X, triggeredGroups), (d, value) => d.X = value, (d, value, duration, easing) => d.MoveToX(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.Y, triggeredGroups), (d, value) => d.Y = value, (d, value, duration, easing) => d.MoveToY(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.Scale, triggeredGroups), (d, value) => d.Scale = value, (d, value, duration, easing) => d.ScaleTo(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.Rotation, triggeredGroups), (d, value) => d.Rotation = value, (d, value, duration, easing) => d.RotateTo(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.Colour, triggeredGroups), (d, value) => d.Colour = value, (d, value, duration, easing) => d.FadeColour(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.Alpha, triggeredGroups), (d, value) => d.Alpha = value, (d, value, duration, easing) => d.FadeTo(value, duration, easing));
            applyCommands(drawable, getCommands(g => g.BlendingParameters, triggeredGroups), (d, value) => d.Blending = value, (d, value, duration, easing) => d.TransformBlendingMode(value, duration), false);

            if (drawable is IFlippable flippable)
            {
                applyCommands(drawable, getCommands(g => g.FlipH, triggeredGroups), (d, value) => flippable.FlipH = value, (d, value, duration, easing) => flippable.TransformFlipH(value, duration), false);
                applyCommands(drawable, getCommands(g => g.FlipV, triggeredGroups), (d, value) => flippable.FlipV = value, (d, value, duration, easing) => flippable.TransformFlipV(value, duration), false);
            }
        }

        private void applyCommands<T>(Drawable drawable, IEnumerable<CommandTimeline<T>.TypedCommand> commands, DrawablePropertyInitializer<T> initializeProperty, DrawableTransformer<T> transform, bool alwaysInitialize = true)
            where T : struct
        {
            var initialized = false;

            foreach (var command in commands.OrderBy(l => l))
            {
                if (!initialized)
                {
                    if (alwaysInitialize || command.StartTime == command.EndTime)
                        initializeProperty.Invoke(drawable, command.StartValue);
                    initialized = true;
                }

                using (drawable.BeginAbsoluteSequence(command.StartTime))
                {
                    transform(drawable, command.StartValue, 0, Easing.None);
                    transform(drawable, command.EndValue, command.Duration, command.Easing);
                }
            }
        }

        private IEnumerable<CommandTimeline<T>.TypedCommand> getCommands<T>(CommandTimelineSelector<T> timelineSelector, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups)
        {
            var commands = TimelineGroup.GetCommands(timelineSelector);
            foreach (var loop in loops)
                commands = commands.Concat(loop.GetCommands(timelineSelector));
            if (triggeredGroups != null)
                foreach (var pair in triggeredGroups)
                    commands = commands.Concat(pair.Item1.GetCommands(timelineSelector, pair.Item2));
            return commands;
        }

        public override string ToString()
            => $"{Path}, {Origin}, {InitialPosition}";
    }
}

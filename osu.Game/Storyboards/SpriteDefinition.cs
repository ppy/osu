// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class SpriteDefinition : CommandTimelineGroup, IElementDefinition
    {
        public string Path { get; set; }
        public Anchor Origin;
        public Vector2 InitialPosition;

        private readonly List<CommandLoop> loops = new List<CommandLoop>();
        private readonly List<CommandTrigger> triggers = new List<CommandTrigger>();

        private delegate void DrawablePropertyInitializer<in T>(Drawable drawable, T value);
        private delegate void DrawableTransformer<in T>(Drawable drawable, T value, double duration, Easing easing);

        public SpriteDefinition(string path, Anchor origin, Vector2 initialPosition)
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
            => new StoryboardSprite(this);

        public override IEnumerable<CommandTimeline<T>.TypedCommand> GetCommands<T>(CommandTimelineSelector<T> timelineSelector, double offset = 0)
        {
            var result = base.GetCommands(timelineSelector, offset);
            foreach (var loop in loops)
                result = result.Concat(loop.GetCommands(timelineSelector, offset));
            return result;
        }

        public void ApplyTransforms(Drawable drawable, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups = null)
        {
            applyCommands(drawable, triggeredGroups, g => g.X, (d, value) => d.X = value, (d, value, duration, easing) => d.MoveToX(value, duration, easing));
            applyCommands(drawable, triggeredGroups, g => g.Y, (d, value) => d.Y = value, (d, value, duration, easing) => d.MoveToY(value, duration, easing));
            applyCommands(drawable, triggeredGroups, g => g.Scale, (d, value) => d.Scale = value, (d, value, duration, easing) => d.ScaleTo(value, duration, easing));
            applyCommands(drawable, triggeredGroups, g => g.Rotation, (d, value) => d.Rotation = value, (d, value, duration, easing) => d.RotateTo(value, duration, easing));
            applyCommands(drawable, triggeredGroups, g => g.Colour, (d, value) => d.Colour = value, (d, value, duration, easing) => d.FadeColour(value, duration, easing));
            applyCommands(drawable, triggeredGroups, g => g.Alpha, (d, value) => d.Alpha = value, (d, value, duration, easing) => d.FadeTo(value, duration, easing));

            if (getAggregatedCommands(g => g.Additive, triggeredGroups).Any())
                drawable.BlendingMode = BlendingMode.Additive;

            var flippable = drawable as IFlippable;
            if (flippable != null)
            {
                flippable.FlipH = getAggregatedCommands(g => g.FlipH, triggeredGroups).Any();
                flippable.FlipV = getAggregatedCommands(g => g.FlipV, triggeredGroups).Any();
            }
        }

        private void applyCommands<T>(Drawable drawable, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups,
            CommandTimelineSelector<T> timelineSelector, DrawablePropertyInitializer<T> initializeProperty, DrawableTransformer<T> transform)
        {
            var initialized = false;
            foreach (var command in getAggregatedCommands(timelineSelector, triggeredGroups).OrderBy(l => l))
            {
                if (!initialized)
                {
                    initializeProperty(drawable, command.StartValue);
                    initialized = true;
                }
                using (drawable.BeginAbsoluteSequence(command.StartTime))
                {
                    transform(drawable, command.StartValue, 0, Easing.None);
                    transform(drawable, command.EndValue, command.Duration, command.Easing);
                }
            }
        }

        private IEnumerable<CommandTimeline<T>.TypedCommand> getAggregatedCommands<T>(CommandTimelineSelector<T> timelineSelector, IEnumerable<Tuple<CommandTimelineGroup, double>> triggeredGroups)
        {
            var commands = GetCommands(timelineSelector);
            if (triggeredGroups != null)
                foreach (var pair in triggeredGroups)
                    commands = commands.Concat(pair.Item1.GetCommands(timelineSelector, pair.Item2));
            return commands;
        }

        public override string ToString()
            => $"{Path}, {Origin}, {InitialPosition}";
    }
}

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
        private readonly List<StoryboardLoopingGroup> loopGroups = new List<StoryboardLoopingGroup>();
        private readonly List<StoryboardTriggerGroup> triggerGroups = new List<StoryboardTriggerGroup>();

        public string Path { get; }
        public bool IsDrawable => HasCommands;

        public Anchor Origin;
        public Vector2 InitialPosition;

        public readonly StoryboardCommandGroup Group = new StoryboardCommandGroup();

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

                var command = Group.Alpha.FirstOrDefault();
                if (command != null) alphaCommands.Add((command.StartTime, command.StartValue == 0));

                foreach (var loop in loopGroups)
                {
                    command = loop.Alpha.FirstOrDefault();
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
                double earliestStartTime = Group.StartTime;
                foreach (var l in loopGroups)
                    earliestStartTime = Math.Min(earliestStartTime, l.StartTime);
                return earliestStartTime;
            }
        }

        public double EndTime
        {
            get
            {
                double latestEndTime = Group.EndTime;

                foreach (var l in loopGroups)
                    latestEndTime = Math.Max(latestEndTime, l.EndTime);

                return latestEndTime;
            }
        }

        public double EndTimeForDisplay
        {
            get
            {
                double latestEndTime = Group.StartTime;

                foreach (var l in loopGroups)
                    latestEndTime = Math.Max(latestEndTime, l.StartTime + l.Duration * l.TotalIterations);

                return latestEndTime;
            }
        }

        public bool HasCommands => Group.HasCommands || loopGroups.Any(l => l.HasCommands);

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
            loopGroups.Add(loop);
            return loop;
        }

        public StoryboardTriggerGroup AddTriggerGroup(string triggerName, double startTime, double endTime, int groupNumber)
        {
            var trigger = new StoryboardTriggerGroup(triggerName, startTime, endTime, groupNumber);
            triggerGroups.Add(trigger);
            return trigger;
        }

        public override string ToString() => $"{Path}, {Origin}, {InitialPosition}";

        public void ApplyTransforms(Drawable drawable, IEnumerable<Tuple<StoryboardCommandGroup, double>>? triggeredGroups = null)
        {
            // For performance reasons, we need to apply the commands in order by start time. Not doing so will cause many functions to be interleaved, resulting in O(n^2) complexity.

            var commands = Group.GetAllCommands();
            commands = commands.Concat(loopGroups.SelectMany(l => l.GetAllCommands()));

            // todo: triggers are not implemented yet.
            // if (triggeredGroups != null)
            //     commands = commands.Concat(triggeredGroups.SelectMany(tuple => tuple.Item1.GetAllCommands(tuple.Item2)));

            foreach (var command in commands.OrderBy(c => c.StartTime))
            {
                using (drawable.BeginAbsoluteSequence(command.StartTime))
                    command.ApplyTransform(drawable);
            }
        }

        // todo: need to revisit property initialisation. apparently it has to be done per first command of every affected property (transforms are supposed to do that already?).
        // private void generateCommands<T>(List<IGeneratedCommand> resultList, IEnumerable<StoryboardCommandList<T>.TypedCommand> commands,
        //                                  DrawablePropertyInitializer<T> initializeProperty, DrawableTransform<T> transform, bool alwaysInitialize = true)
        // {
        //     bool initialized = false;
        //
        //     foreach (var command in commands)
        //     {
        //         DrawablePropertyInitializer<T>? initFunc = null;
        //
        //         if (!initialized)
        //         {
        //             if (alwaysInitialize || command.StartTime == command.EndTime)
        //                 initFunc = initializeProperty;
        //             initialized = true;
        //         }
        //
        //         resultList.Add(new GeneratedCommand<T>(command, initFunc, transform));
        //     }
        // }
    }
}

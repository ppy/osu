// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Storyboards
{
    public delegate CommandTimeline<T> CommandTimelineSelector<T>(CommandTimelineGroup commandTimelineGroup);

    public class CommandTimelineGroup
    {
        public CommandTimeline<float> X = new CommandTimeline<float>("X");
        public CommandTimeline<float> Y = new CommandTimeline<float>("Y");
        public CommandTimeline<Vector2> Scale = new CommandTimeline<Vector2>("Scale");
        public CommandTimeline<Vector2> VectorScale = new CommandTimeline<Vector2>("VectorScale");
        public CommandTimeline<float> Rotation = new CommandTimeline<float>("Rotation");
        public CommandTimeline<ColourInfo> Colour = new CommandTimeline<ColourInfo>("Colour");
        public CommandTimeline<float> Alpha = new CommandTimeline<float>("Alpha");
        public CommandTimeline<BlendingParameters> BlendingParameters = new CommandTimeline<BlendingParameters>("Blending") { IsParameterTimeline = true };
        public CommandTimeline<bool> FlipH = new CommandTimeline<bool>("FlipH") { IsParameterTimeline = true };
        public CommandTimeline<bool> FlipV = new CommandTimeline<bool>("FlipV") { IsParameterTimeline = true };

        private readonly ICommandTimeline[] timelines;

        public CommandTimelineGroup()
        {
            timelines = new ICommandTimeline[]
            {
                X,
                Y,
                Scale,
                VectorScale,
                Rotation,
                Colour,
                Alpha,
                BlendingParameters,
                FlipH,
                FlipV
            };
        }

        [JsonIgnore]
        public double CommandsStartTime => timelines.Min(static t => t.StartTime);

        [JsonIgnore]
        public double CommandsEndTime => timelines.Max(static t => t.EndTime);

        [JsonIgnore]
        public double CommandsDuration => CommandsEndTime - CommandsStartTime;

        [JsonIgnore]
        public virtual double StartTime => CommandsStartTime;

        [JsonIgnore]
        public virtual double EndTime => CommandsEndTime;

        [JsonIgnore]
        public bool HasCommands => timelines.Any(static t => t.HasCommands);

        public virtual IEnumerable<CommandTimeline<T>.TypedCommand> GetCommands<T>(CommandTimelineSelector<T> timelineSelector, double offset = 0)
        {
            if (offset != 0)
            {
                return timelineSelector(this).Commands.Select(command =>
                    new CommandTimeline<T>.TypedCommand
                    {
                        Easing = command.Easing,
                        StartTime = offset + command.StartTime,
                        EndTime = offset + command.EndTime,
                        StartValue = command.StartValue,
                        EndValue = command.EndValue,
                        PropertyName = command.PropertyName,
                        IsParameterCommand = command.IsParameterCommand
                    });
            }

            return timelineSelector(this).Commands;
        }
    }
}

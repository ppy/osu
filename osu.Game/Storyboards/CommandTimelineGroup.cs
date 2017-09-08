// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Storyboards
{
    public class CommandTimelineGroup
    {
        public CommandTimeline<float> X = new CommandTimeline<float>();
        public CommandTimeline<float> Y = new CommandTimeline<float>();
        public CommandTimeline<Vector2> Scale = new CommandTimeline<Vector2>();
        public CommandTimeline<float> Rotation = new CommandTimeline<float>();
        public CommandTimeline<Color4> Colour = new CommandTimeline<Color4>();
        public CommandTimeline<float> Alpha = new CommandTimeline<float>();
        public CommandTimeline<bool> Additive = new CommandTimeline<bool>();
        public CommandTimeline<bool> FlipH = new CommandTimeline<bool>();
        public CommandTimeline<bool> FlipV = new CommandTimeline<bool>();

        public IEnumerable<CommandTimeline> Timelines
        {
            get
            {
                yield return X;
                yield return Y;
                yield return Scale;
                yield return Rotation;
                yield return Colour;
                yield return Alpha;
                yield return Additive;
                yield return FlipH;
                yield return FlipV;
            }
        }

        public double StartTime => Timelines.Where(t => t.HasCommands).Min(t => t.StartTime);
        public double EndTime => Timelines.Where(t => t.HasCommands).Max(t => t.EndTime);
        public double Duration => EndTime - StartTime;
        public bool HasCommands => Timelines.Any(t => t.HasCommands);

        public virtual void ApplyTransforms(Drawable drawable, double offset = 0)
        {
            if (X.HasCommands) drawable.X = X.StartValue;
            foreach (var command in X.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.MoveToX(command.StartValue)
                                .MoveToX(command.EndValue, command.Duration, command.Easing));

            if (Y.HasCommands) drawable.Y = Y.StartValue;
            foreach (var command in Y.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.MoveToY(command.StartValue)
                                .MoveToY(command.EndValue, command.Duration, command.Easing));

            if (Scale.HasCommands) drawable.Scale = Scale.StartValue;
            foreach (var command in Scale.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.ScaleTo(command.StartValue)
                                .ScaleTo(command.EndValue, command.Duration, command.Easing));

            if (Rotation.HasCommands) drawable.Rotation = Rotation.StartValue;
            foreach (var command in Rotation.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.RotateTo(command.StartValue)
                                .RotateTo(command.EndValue, command.Duration, command.Easing));

            if (Colour.HasCommands) drawable.Colour = Colour.StartValue;
            foreach (var command in Colour.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.FadeColour(command.StartValue)
                                .FadeColour(command.EndValue, command.Duration, command.Easing));

            if (Alpha.HasCommands) drawable.Alpha = Alpha.StartValue;
            foreach (var command in Alpha.Commands)
                using (drawable.BeginAbsoluteSequence(offset + command.StartTime))
                    PostProcess(command,
                        drawable.FadeTo(command.StartValue)
                                .FadeTo(command.EndValue, command.Duration, command.Easing));

            if (Additive.HasCommands)
                drawable.BlendingMode = BlendingMode.Additive;

            var flippable = drawable as IFlippable;
            if (flippable != null)
            {
                flippable.FlipH = FlipH.HasCommands;
                flippable.FlipV = FlipV.HasCommands;
            }
        }

        protected virtual void PostProcess(Command command, TransformSequence<Drawable> sequence)
        {
        }
    }
}

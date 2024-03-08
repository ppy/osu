// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardFlipVCommand : StoryboardCommand<bool>
    {
        public StoryboardFlipVCommand(Easing easing, double startTime, double endTime, bool startValue, bool endValue)
            : base(easing, startTime, endTime, startValue, endValue)
        {
        }

        public override string PropertyName => nameof(IFlippable.FlipV);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.FlipV = StartValue;

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.TransformTo(nameof(IFlippable.FlipV), StartValue).Delay(Duration)
                .TransformTo(nameof(IFlippable.FlipV), EndValue);
    }
}

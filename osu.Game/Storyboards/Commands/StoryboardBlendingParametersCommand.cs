// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardBlendingParametersCommand : StoryboardCommand<BlendingParameters>
    {
        public StoryboardBlendingParametersCommand(Easing easing, double startTime, double endTime, BlendingParameters startValue, BlendingParameters endValue)
            : base(easing, startTime, endTime, startValue, endValue)
        {
        }

        public override string PropertyName => nameof(Drawable.Blending);

        public override void ApplyInitialValue<TDrawable>(TDrawable d)
        {
            if (StartTime == EndTime)
                d.Blending = StartValue;
        }

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.TransformTo(nameof(d.Blending), StartValue).Delay(Duration)
                .TransformTo(nameof(d.Blending), EndValue);
    }
}

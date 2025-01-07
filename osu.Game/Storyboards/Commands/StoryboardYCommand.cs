// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardYCommand : StoryboardCommand<float>
    {
        public StoryboardYCommand(Easing easing, double startTime, double endTime, float startValue, float endValue)
            : base(easing, startTime, endTime, startValue, endValue)
        {
        }

        public override string PropertyName => nameof(Drawable.Y);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.Y = StartValue;

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.MoveToY(StartValue).Then().MoveToY(EndValue, Duration, Easing);
    }
}

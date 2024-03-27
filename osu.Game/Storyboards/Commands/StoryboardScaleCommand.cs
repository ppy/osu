// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardScaleCommand : StoryboardCommand<float>
    {
        public StoryboardScaleCommand(Easing easing, double startTime, double endTime, float startValue, float endValue)
            : base(easing, startTime, endTime, startValue, endValue)
        {
        }

        public override string PropertyName => nameof(Drawable.Scale);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.Scale = new Vector2(StartValue);

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.ScaleTo(StartValue).Then().ScaleTo(EndValue, Duration, Easing);
    }
}

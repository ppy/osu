// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardScaleCommand : StoryboardCommand<float>
    {
        public StoryboardScaleCommand(double startTime, double endTime, float startValue, float endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override string PropertyName => nameof(Drawable.Scale);

        public override void ApplyInitialValue(Drawable d) => d.Scale = new Vector2(StartValue);

        public override TransformSequence<Drawable> ApplyTransforms(Drawable d)
            => d.ScaleTo(StartValue).Then().ScaleTo(EndValue, Duration, Easing);
    }
}

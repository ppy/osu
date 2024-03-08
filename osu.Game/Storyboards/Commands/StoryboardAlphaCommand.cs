// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardAlphaCommand : StoryboardCommand<float>
    {
        public StoryboardAlphaCommand(double startTime, double endTime, float startValue, float endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override string PropertyName => nameof(Drawable.Alpha);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.Alpha = StartValue;

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.FadeTo(StartValue).Then().FadeTo(EndValue, Duration, Easing);
    }
}

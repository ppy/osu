// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK.Graphics;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardColourCommand : StoryboardCommand<Color4>
    {
        public StoryboardColourCommand(Easing easing, double startTime, double endTime, Color4 startValue, Color4 endValue)
            : base(easing, startTime, endTime, startValue, endValue)
        {
        }

        public override string PropertyName => nameof(Drawable.Colour);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.Colour = StartValue;

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.FadeColour(StartValue).Then().FadeColour(EndValue, Duration, Easing);
    }
}

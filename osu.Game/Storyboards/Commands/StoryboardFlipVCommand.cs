// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardFlipVCommand : StoryboardCommand<bool>
    {
        public StoryboardFlipVCommand(double startTime, double endTime, bool startValue, bool endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override string PropertyName => nameof(IDrawableStoryboardElement.FlipV);

        public override void ApplyInitialValue(Drawable d) => ((IDrawableStoryboardElement)d).FlipV = StartValue;

        public override TransformSequence<Drawable> ApplyTransforms(Drawable d)
            => d.TransformTo(nameof(IDrawableStoryboardElement.FlipV), StartValue).Delay(Duration)
                .TransformTo(nameof(IDrawableStoryboardElement.FlipV), EndValue);
    }
}

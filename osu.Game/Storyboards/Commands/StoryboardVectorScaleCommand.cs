// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardVectorScaleCommand : StoryboardCommand<Vector2>
    {
        public StoryboardVectorScaleCommand(double startTime, double endTime, Vector2 startValue, Vector2 endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override string PropertyName => nameof(IVectorScalable.VectorScale);

        public override void ApplyInitialValue<TDrawable>(TDrawable d) => d.VectorScale = StartValue;

        public override TransformSequence<TDrawable> ApplyTransforms<TDrawable>(TDrawable d)
            => d.TransformTo(nameof(d.VectorScale), StartValue).Then()
                .TransformTo(nameof(d.VectorScale), EndValue, Duration, Easing);
    }
}

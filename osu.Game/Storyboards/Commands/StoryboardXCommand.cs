// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardXCommand : StoryboardCommand<float>
    {
        public StoryboardXCommand(double startTime, double endTime, float startValue, float endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override void SetInitialValue(Drawable d) => d.X = StartValue;

        public override TransformSequence<Drawable> ApplyTransform(Drawable d)
            => d.MoveToX(StartValue).Then().MoveToX(EndValue, Duration, Easing);
    }
}

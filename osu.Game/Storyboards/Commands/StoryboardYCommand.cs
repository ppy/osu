// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardYCommand : StoryboardCommand<float>
    {
        public StoryboardYCommand(double startTime, double endTime, float startValue, float endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override void SetInitialValue(Drawable d) => d.Y = StartValue;

        public override TransformSequence<Drawable> ApplyTransform(Drawable d)
            => d.MoveToY(StartValue).Then().MoveToY(EndValue, Duration, Easing);
    }
}

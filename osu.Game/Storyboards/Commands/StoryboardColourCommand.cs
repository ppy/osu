// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osuTK.Graphics;

namespace osu.Game.Storyboards.Commands
{
    public class StoryboardColourCommand : StoryboardCommand<Color4>
    {
        public StoryboardColourCommand(double startTime, double endTime, Color4 startValue, Color4 endValue, Easing easing)
            : base(startTime, endTime, startValue, endValue, easing)
        {
        }

        public override void SetInitialValue(Drawable d) => d.Colour = StartValue;
        public override TransformSequence<Drawable> ApplyTransform(Drawable d) => d.FadeColour(StartValue).Then().FadeColour(EndValue, Duration, Easing);
    }
}

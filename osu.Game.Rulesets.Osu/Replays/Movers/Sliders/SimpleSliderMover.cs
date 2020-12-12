// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Replays.Movers.Sliders
{
    public class SimpleSliderMover : BaseDanceObjectMover<Slider>
    {
        public override Vector2 Update(double time) => Object.StackedPositionAt(T(time));
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderSnaking : TestSceneSlider
    {
        protected override DrawableSlider CreateDrawableSlider(Slider slider)
        {
            var snakingSlider = base.CreateDrawableSlider(slider);
            snakingSlider.OnLoadComplete += d =>
            {
                var body = (d as DrawableSlider)?.Body.Drawable as PlaySliderBody;
                if (body is null) return;

                body.SnakingIn.Value = true;
                body.SnakingOut.Value = true;
            };

            return snakingSlider;
        }
    }
}

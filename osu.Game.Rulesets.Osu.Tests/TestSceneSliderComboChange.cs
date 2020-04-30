// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderComboChange : TestSceneSlider
    {
        private readonly Bindable<int> comboIndex = new Bindable<int>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(() => comboIndex.Value++, 250, true);
        }

        protected override DrawableSlider CreateDrawableSlider(Slider slider)
        {
            slider.ComboIndexBindable.BindTo(comboIndex);
            slider.IndexInCurrentComboBindable.BindTo(comboIndex);

            return base.CreateDrawableSlider(slider);
        }
    }
}

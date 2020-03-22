// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderComboChange : TestSceneSlider
    {
        private readonly Bindable<ComboIndex> comboIndex = new Bindable<ComboIndex>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(() => comboIndex.Value = ComboIndex.Add(comboIndex.Value, 1), 250, true);
        }

        protected override DrawableSlider CreateDrawableSlider(Slider slider)
        {
            slider.ComboIndexBindable.BindTo(comboIndex);
            slider.ComboIndexBindable.BindValueChanged(ci =>
            {
                slider.IndexInCurrentCombo = ci.NewValue.Ordinal;
            }, true);

            return base.CreateDrawableSlider(slider);
        }
    }
}

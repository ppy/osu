// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneHitCircleComboChange : TestSceneHitCircle
    {
        private readonly Bindable<ComboIndex> comboIndex = new Bindable<ComboIndex>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(() => comboIndex.Value = ComboIndex.Add(comboIndex.Value, 1), 250, true);
        }

        protected override TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto)
        {
            circle.ComboIndexBindable.BindTo(comboIndex);
            circle.ComboIndexBindable.BindValueChanged(ci =>
            {
                circle.IndexInCurrentCombo = ci.NewValue.Ordinal;
            }, true);

            return base.CreateDrawableHitCircle(circle, auto);
        }
    }
}

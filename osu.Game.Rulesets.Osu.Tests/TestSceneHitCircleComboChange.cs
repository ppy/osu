// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitCircleComboChange : TestSceneHitCircle
    {
        private readonly Bindable<int> comboIndex = new Bindable<int>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(() => comboIndex.Value++, 250, true);
        }

        protected override TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto, double hitOffset = 0)
        {
            circle.ComboIndexBindable.BindTo(comboIndex);
            circle.IndexInCurrentComboBindable.BindTo(comboIndex);
            return base.CreateDrawableHitCircle(circle, auto, hitOffset);
        }
    }
}

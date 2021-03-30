// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneFruitVisualChange : TestSceneFruitObjects
    {
        private readonly Bindable<int> indexInBeatmap = new Bindable<int>();
        private readonly Bindable<bool> hyperDash = new Bindable<bool>();

        protected override void LoadComplete()
        {
            AddStep("fruit changes visual and hyper", () => SetContents(() => new TestDrawableCatchHitObjectSpecimen(new DrawableFruit(new Fruit
            {
                IndexInBeatmapBindable = { BindTarget = indexInBeatmap },
                HyperDashBindable = { BindTarget = hyperDash },
            }))));

            AddStep("droplet changes hyper", () => SetContents(() => new TestDrawableCatchHitObjectSpecimen(new DrawableDroplet(new Droplet
            {
                HyperDashBindable = { BindTarget = hyperDash },
            }))));

            Scheduler.AddDelayed(() => indexInBeatmap.Value++, 250, true);
            Scheduler.AddDelayed(() => hyperDash.Value = !hyperDash.Value, 1000, true);
        }
    }
}

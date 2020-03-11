// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcherArea : SkinnableTestScene
    {
        private RulesetInfo catchRuleset;

        public TestSceneCatcherArea()
        {
            AddSliderStep<float>("CircleSize", 0, 8, 5, createCatcher);
            AddToggleStep("Hyperdash", t =>
                CreatedDrawables.OfType<CatchInputManager>().Select(i => i.Child)
                                .OfType<TestCatcherArea>().ForEach(c => c.ToggleHyperDash(t)));

            AddRepeatStep("catch fruit", () =>
                this.ChildrenOfType<CatcherArea>().ForEach(area =>
                    area.MovableCatcher.PlaceOnPlate(new DrawableFruit(new TestSceneFruitObjects.TestCatchFruit(FruitVisualRepresentation.Grape)))), 20);
        }

        private void createCatcher(float size)
        {
            SetContents(() => new CatchInputManager(catchRuleset)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TestCatcherArea(new BeatmapDifficulty { CircleSize = size })
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.TopLeft
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            catchRuleset = rulesets.GetRuleset(2);
        }

        private class TestCatcherArea : CatcherArea
        {
            public TestCatcherArea(BeatmapDifficulty beatmapDifficulty)
                : base(beatmapDifficulty)
            {
            }

            public new Catcher MovableCatcher => base.MovableCatcher;

            public void ToggleHyperDash(bool status) => MovableCatcher.SetHyperDashState(status ? 2 : 1);
        }
    }
}

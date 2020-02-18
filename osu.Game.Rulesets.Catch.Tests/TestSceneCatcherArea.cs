// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcherArea : SkinnableTestScene
    {
        private RulesetInfo catchRuleset;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatcherArea),
        };

        public TestSceneCatcherArea()
        {
            AddSliderStep<float>("CircleSize", 0, 8, 5, createCatcher);
            AddToggleStep("Hyperdash", t =>
                CreatedDrawables.OfType<CatchInputManager>().Select(i => i.Child)
                                .OfType<TestCatcherArea>().ForEach(c => c.ToggleHyperDash(t)));
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

            public void ToggleHyperDash(bool status) => MovableCatcher.SetHyperDashState(status ? 2 : 1);
        }
    }
}

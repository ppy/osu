// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatchTouchInput : OsuTestScene
    {
        private CatchTouchInputMapper catchTouchInputMapper = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create input overlay", () =>
            {
                Child = new CatchInputManager(new CatchRuleset().RulesetInfo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        catchTouchInputMapper = new CatchTouchInputMapper
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("show overlay", () => catchTouchInputMapper.Show());
        }
    }
}

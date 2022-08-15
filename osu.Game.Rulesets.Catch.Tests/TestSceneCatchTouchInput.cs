// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        private TouchInputField touchInputField = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create inputfield", () =>
            {
                Child = new CatchInputManager(new CatchRuleset().RulesetInfo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        touchInputField = new TouchInputField
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
                };
            });
        }

        [Test]
        public void TestInputField()
        {
            AddStep("show inputfield", () => touchInputField.Show());
        }
    }
}

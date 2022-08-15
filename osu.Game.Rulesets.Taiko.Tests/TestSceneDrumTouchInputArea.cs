// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneDrumTouchInputArea : OsuTestScene
    {
        private DrumTouchInputArea drumTouchInputArea = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create drum", () =>
            {
                Child = new TaikoInputManager(new TaikoRuleset().RulesetInfo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new InputDrum
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 0.2f,
                        },
                        drumTouchInputArea = new DrumTouchInputArea
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                        },
                    },
                };
            });
        }

        [Test]
        public void TestDrum()
        {
            AddStep("show drum", () => drumTouchInputArea.Show());
        }
    }
}

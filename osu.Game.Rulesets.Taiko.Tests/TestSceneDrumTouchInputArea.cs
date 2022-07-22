// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneDrumTouchInputArea : OsuTestScene
    {
        [Cached]
        private TaikoInputManager taikoInputManager = new TaikoInputManager(new TaikoRuleset().RulesetInfo);

        private DrumTouchInputArea drumTouchInputArea = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create drum", () =>
            {
                Children = new Drawable[]
                {
                    new InputDrum
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 0.5f,
                    },
                    drumTouchInputArea = new DrumTouchInputArea
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Height = 0.5f,
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

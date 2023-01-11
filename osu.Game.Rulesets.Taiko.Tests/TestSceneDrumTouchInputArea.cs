// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.Configuration;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public partial class TestSceneDrumTouchInputArea : OsuTestScene
    {
        private DrumTouchInputArea drumTouchInputArea = null!;

        private void createDrum(TaikoTouchControlScheme _forcedControlScheme)
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
                        ForceControlScheme = _forcedControlScheme
                    }
                }
            };
        }

        [Test]
        public void TestDrum()
        {
            AddStep("create drum (kddk)", () => createDrum(TaikoTouchControlScheme.KDDK));
            AddStep("show drum", () => drumTouchInputArea.Show());
            AddStep("create drum (ddkk)", () => createDrum(TaikoTouchControlScheme.DDKK));
            AddStep("show drum", () => drumTouchInputArea.Show());
            AddStep("create drum (kkdd)", () => createDrum(TaikoTouchControlScheme.KKDD));
            AddStep("show drum", () => drumTouchInputArea.Show());
        }

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}

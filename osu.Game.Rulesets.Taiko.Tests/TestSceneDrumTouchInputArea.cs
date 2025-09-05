// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
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

        private readonly Bindable<TaikoTouchControlScheme> controlScheme = new Bindable<TaikoTouchControlScheme>();
        private readonly Bindable<float> drumSize = new Bindable<float>();

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (TaikoRulesetConfigManager)RulesetConfigs.GetConfigFor(Ruleset.Value.CreateInstance()).AsNonNull();
            config.BindWith(TaikoRulesetSetting.TouchControlScheme, controlScheme);
            config.BindWith(TaikoRulesetSetting.DrumTouchSize, drumSize);
        }

        private void createDrum()
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
                    }
                }
            };
        }

        [Test]
        public void TestDrum()
        {
            AddStep("create drum", createDrum);
            AddStep("show drum", () => drumTouchInputArea.Show());

            AddStep("change scheme (kddk)", () => controlScheme.Value = TaikoTouchControlScheme.KDDK);
            AddStep("change scheme (kkdd)", () => controlScheme.Value = TaikoTouchControlScheme.KKDD);
            AddStep("change scheme (ddkk)", () => controlScheme.Value = TaikoTouchControlScheme.DDKK);

            AddStep("set drum size (small)", () => drumSize.Value = 0.5f);
            AddStep("set drum size (normal)", () => drumSize.Value = 1.0f);
            AddStep("set drum size (large)", () => drumSize.Value = 1.5f);
            AddStep("set drum size (extra large)", () => drumSize.Value = 2.0f);
        }

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}

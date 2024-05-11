// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaPlayer : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("change direction to down", () => changeDirectionTo(ManiaScrollingDirection.Down));
            AddStep("change direction to up", () => changeDirectionTo(ManiaScrollingDirection.Up));
        }

        private void changeDirectionTo(ManiaScrollingDirection direction)
        {
            var rulesetConfig = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(new ManiaRuleset()).AsNonNull();
            rulesetConfig.SetValue(ManiaRulesetSetting.ScrollDirection, direction);
        }
    }
}

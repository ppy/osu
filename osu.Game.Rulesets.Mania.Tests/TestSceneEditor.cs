// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestSceneEditor : EditorTestScene
    {
        private readonly Bindable<ManiaScrollingDirection> direction = new Bindable<ManiaScrollingDirection>();

        public TestSceneEditor()
            : base(new ManiaRuleset())
        {
            AddStep("upwards scroll", () => direction.Value = ManiaScrollingDirection.Up);
            AddStep("downwards scroll", () => direction.Value = ManiaScrollingDirection.Down);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetConfigCache configCache)
        {
            var config = (ManiaRulesetConfigManager)configCache.GetConfigFor(Ruleset.Value.CreateInstance());
            config.BindWith(ManiaRulesetSetting.ScrollDirection, direction);
        }
    }
}

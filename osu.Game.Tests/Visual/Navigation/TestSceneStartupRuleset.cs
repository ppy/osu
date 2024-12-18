// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Development;
using osu.Game.Configuration;

namespace osu.Game.Tests.Visual.Navigation
{
    [TestFixture]
    public partial class TestSceneStartupRuleset : OsuGameTestScene
    {
        protected override TestOsuGame CreateTestGame()
        {
            // Must be done in this function due to the RecycleLocalStorage call just before.
            var config = DebugUtils.IsDebugBuild
                ? new DevelopmentOsuConfigManager(LocalStorage)
                : new OsuConfigManager(LocalStorage);

            config.SetValue(OsuSetting.Ruleset, "mania");
            config.Save();

            return base.CreateTestGame();
        }

        [Test]
        public void TestRulesetConsumed()
        {
            AddUntilStep("ruleset correct", () => Game.Ruleset.Value.ShortName == "mania");
        }
    }
}

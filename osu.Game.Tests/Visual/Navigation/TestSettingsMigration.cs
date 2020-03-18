// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Configuration;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSettingsMigration : OsuGameTestScene
    {
        public override void RecycleLocalStorage()
        {
            base.RecycleLocalStorage();

            using (var config = new OsuConfigManager(LocalStorage))
            {
                config.Set(OsuSetting.Version, "2020.101.0");
                config.Set(OsuSetting.DisplayStarsMaximum, 10.0);
            }
        }

        [Test]
        public void TestDisplayStarsMigration()
        {
            AddAssert("config has migrated value", () => Precision.AlmostEquals(Game.LocalConfig.Get<double>(OsuSetting.DisplayStarsMaximum), 10.1));

            AddStep("set value again", () => Game.LocalConfig.Set<double>(OsuSetting.DisplayStarsMaximum, 10));

            AddStep("force save config", () => Game.LocalConfig.Save());

            AddStep("remove game", () => Remove(Game));

            AddStep("create game again", CreateGame);

            AddUntilStep("Wait for load", () => Game.IsLoaded);

            AddAssert("config did not migrate value", () => Precision.AlmostEquals(Game.LocalConfig.Get<double>(OsuSetting.DisplayStarsMaximum), 10));
        }
    }
}

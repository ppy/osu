// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Configuration;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSettingsMigration : OsuGameTestScene
    {
        public override void RecycleLocalStorage(bool isDisposing)
        {
            base.RecycleLocalStorage(isDisposing);

            if (isDisposing)
                return;

            using (var config = new OsuConfigManager(LocalStorage))
            {
                config.SetValue(OsuSetting.Version, "2020.101.0");
                config.SetValue(OsuSetting.DisplayStarsMaximum, 10.0);
            }
        }

        [Test]
        public void TestDisplayStarsMigration()
        {
            AddAssert("config has migrated value", () => Precision.AlmostEquals(Game.LocalConfig.Get<double>(OsuSetting.DisplayStarsMaximum), 10.1));

            AddStep("set value again", () => Game.LocalConfig.SetValue(OsuSetting.DisplayStarsMaximum, 10.0));

            AddStep("force save config", () => Game.LocalConfig.Save());

            AddStep("remove game", () => Remove(Game, true));

            AddStep("create game again", CreateGame);

            AddUntilStep("Wait for load", () => Game.IsLoaded);

            AddAssert("config did not migrate value", () => Precision.AlmostEquals(Game.LocalConfig.Get<double>(OsuSetting.DisplayStarsMaximum), 10));
        }
    }
}

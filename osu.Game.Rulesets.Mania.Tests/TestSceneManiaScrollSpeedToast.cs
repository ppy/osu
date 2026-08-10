// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public partial class TestSceneManiaScrollSpeedToast : OsuTestScene
    {
        private ManiaRulesetConfigManager config = null!;
        private TestOnScreenDisplay osd = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set up OSD", () =>
            {
                config = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(new ManiaRuleset())!;
                config.LookupKeyBindings = action => action.ToString();

                Child = osd = new TestOnScreenDisplay();
                osd.BeginTracking(this, config);
            });
        }

        [Test]
        public void TestScrollSpeedToastShowsShortcut()
        {
            AddStep("increase scroll speed", () => config.SetValue(ManiaRulesetSetting.ScrollSpeed, config.Get<double>(ManiaRulesetSetting.ScrollSpeed) + 1));
            AddUntilStep("wait for toast", () => osd.ChildrenOfType<TrackedSettingToast>().Any());
            AddAssert("toast shortcut is not empty", () => osd.ChildrenOfType<TrackedSettingToast>().Single().ExtraText != default);
        }

        private partial class TestOnScreenDisplay : OnScreenDisplay
        {
            protected override void DisplayTemporarily(Drawable toDisplay) => toDisplay.FadeIn().ResizeHeightTo(110);
        }
    }
}

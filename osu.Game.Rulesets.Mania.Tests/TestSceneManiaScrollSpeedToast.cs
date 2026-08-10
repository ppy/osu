// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
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
            var lookedUpActions = new List<GlobalAction>();

            AddStep("set lookup spy", () =>
                config.LookupKeyBindings = action =>
                {
                    lookedUpActions.Add(action);
                    return action.ToString();
                });

            AddStep("increase scroll speed", () => config.SetValue(ManiaRulesetSetting.ScrollSpeed, config.Get<double>(ManiaRulesetSetting.ScrollSpeed) + 1));
            AddUntilStep("wait for toast", () => osd.ChildrenOfType<TrackedSettingToast>().Any());
            AddAssert("shortcut uses increase and decrease actions", () =>
                lookedUpActions.Contains(GlobalAction.IncreaseScrollSpeed) &&
                lookedUpActions.Contains(GlobalAction.DecreaseScrollSpeed));
        }

        private partial class TestOnScreenDisplay : OnScreenDisplay
        {
            protected override void DisplayTemporarily(Drawable toDisplay) => toDisplay.FadeIn().ResizeHeightTo(110);
        }
    }
}

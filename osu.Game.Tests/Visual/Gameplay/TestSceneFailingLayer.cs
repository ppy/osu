// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailingLayer : OsuTestScene
    {
        private readonly FailingLayer layer;

        [Resolved]
        private OsuConfigManager config { get; set; }

        public TestSceneFailingLayer()
        {
            Child = layer = new FailingLayer();
        }

        [Test]
        public void TestLayerConfig()
        {
            AddStep("enable layer", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddWaitStep("wait for transition to finish", 5);
            AddAssert("layer is enabled", () => layer.IsPresent);

            AddStep("disable layer", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddWaitStep("wait for transition to finish", 5);
            AddAssert("layer is disabled", () => !layer.IsPresent);
            AddStep("restore layer enabling", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, true));
        }

        [Test]
        public void TestLayerFading()
        {
            AddSliderStep("current health", 0.0, 1.0, 1.0, val => layer.Current.Value = val);
            var box = layer.Child;

            AddStep("set health to 0.10", () => layer.Current.Value = 0.10);
            AddWaitStep("wait for fade to finish", 5);
            AddAssert("layer fade is visible", () => box.IsPresent);
            AddStep("set health to 1", () => layer.Current.Value = 1f);
            AddWaitStep("wait for fade to finish", 10);
            AddAssert("layer fade is invisible", () => !box.IsPresent);
        }
    }
}

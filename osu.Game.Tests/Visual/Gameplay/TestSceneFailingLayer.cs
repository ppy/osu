// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneFailingLayer : OsuTestScene
    {
        private FailingLayer layer;

        private readonly Bindable<bool> showHealth = new Bindable<bool>();

        [Resolved]
        private OsuConfigManager config { get; set; }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create layer", () =>
            {
                Child = layer = new FailingLayer();
                layer.BindHealthProcessor(new DrainingHealthProcessor(1));
                layer.ShowHealth.BindTo(showHealth);
            });

            AddStep("show health", () => showHealth.Value = true);
            AddStep("enable layer", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddUntilStep("layer is visible", () => layer.IsPresent);
        }

        [Test]
        public void TestLayerFading()
        {
            AddSliderStep("current health", 0.0, 1.0, 1.0, val =>
            {
                if (layer != null)
                    layer.Current.Value = val;
            });

            AddStep("set health to 0.10", () => layer.Current.Value = 0.1);
            AddUntilStep("layer fade is visible", () => layer.Child.Alpha > 0.1f);
            AddStep("set health to 1", () => layer.Current.Value = 1f);
            AddUntilStep("layer fade is invisible", () => !layer.Child.IsPresent);
        }

        [Test]
        public void TestLayerDisabledViaConfig()
        {
            AddStep("disable layer", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddStep("set health to 0.10", () => layer.Current.Value = 0.1);
            AddUntilStep("layer is not visible", () => !layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithAccumulatingProcessor()
        {
            AddStep("bind accumulating processor", () => layer.BindHealthProcessor(new AccumulatingHealthProcessor(1)));
            AddStep("set health to 0.10", () => layer.Current.Value = 0.1);
            AddUntilStep("layer is not visible", () => !layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithDrainingProcessor()
        {
            AddStep("bind accumulating processor", () => layer.BindHealthProcessor(new DrainingHealthProcessor(1)));
            AddStep("set health to 0.10", () => layer.Current.Value = 0.1);
            AddWaitStep("wait for potential fade", 10);
            AddAssert("layer is still visible", () => layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithDifferentOptions()
        {
            AddStep("set health to 0.10", () => layer.Current.Value = 0.1);

            AddStep("don't show health", () => showHealth.Value = false);
            AddStep("disable FadePlayfieldWhenHealthLow", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("don't show health", () => showHealth.Value = false);
            AddStep("enable FadePlayfieldWhenHealthLow", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("show health", () => showHealth.Value = true);
            AddStep("disable FadePlayfieldWhenHealthLow", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("show health", () => showHealth.Value = true);
            AddStep("enable FadePlayfieldWhenHealthLow", () => config.Set(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddUntilStep("layer fade is visible", () => layer.IsPresent);
        }
    }
}

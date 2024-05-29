// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneFailingLayer : OsuTestScene
    {
        private FailingLayer layer;

        private readonly Bindable<bool> showHealth = new Bindable<bool>();

        private HealthProcessor healthProcessor;

        [Resolved]
        private OsuConfigManager config { get; set; }

        private void create(HealthProcessor healthProcessor)
        {
            AddStep("create layer", () =>
            {
                Child = new HealthProcessorContainer(this.healthProcessor = healthProcessor)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = layer = new FailingLayer()
                };

                layer.ShowHealth.BindTo(showHealth);
            });

            AddStep("show health", () => showHealth.Value = true);
            AddStep("enable layer", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, true));
        }

        [Test]
        public void TestLayerFading()
        {
            create(new DrainingHealthProcessor(0));

            AddSliderStep("current health", 0.0, 1.0, 1.0, val =>
            {
                if (layer != null)
                    healthProcessor.Health.Value = val;
            });

            AddStep("set health to 0.10", () => healthProcessor.Health.Value = 0.1);
            AddUntilStep("layer fade is visible", () => layer.ChildrenOfType<Container>().First().Alpha > 0.1f);
            AddStep("set health to 1", () => healthProcessor.Health.Value = 1f);
            AddUntilStep("layer fade is invisible", () => !layer.ChildrenOfType<Container>().First().IsPresent);
        }

        [Test]
        public void TestLayerDisabledViaConfig()
        {
            create(new DrainingHealthProcessor(0));
            AddUntilStep("layer is visible", () => layer.IsPresent);
            AddStep("disable layer", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddStep("set health to 0.10", () => healthProcessor.Health.Value = 0.1);
            AddUntilStep("layer is not visible", () => !layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithAccumulatingProcessor()
        {
            create(new AccumulatingHealthProcessor(1));
            AddUntilStep("layer is not visible", () => !layer.IsPresent);
            AddStep("set health to 0.10", () => healthProcessor.Health.Value = 0.1);
            AddUntilStep("layer is not visible", () => !layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithDrainingProcessor()
        {
            create(new DrainingHealthProcessor(0));
            AddStep("set health to 0.10", () => healthProcessor.Health.Value = 0.1);
            AddWaitStep("wait for potential fade", 10);
            AddAssert("layer is still visible", () => layer.IsPresent);
        }

        [Test]
        public void TestLayerVisibilityWithDifferentOptions()
        {
            create(new DrainingHealthProcessor(0));

            AddStep("set health to 0.10", () => healthProcessor.Health.Value = 0.1);

            AddStep("don't show health", () => showHealth.Value = false);
            AddStep("disable FadePlayfieldWhenHealthLow", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("don't show health", () => showHealth.Value = false);
            AddStep("enable FadePlayfieldWhenHealthLow", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("show health", () => showHealth.Value = true);
            AddStep("disable FadePlayfieldWhenHealthLow", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, false));
            AddUntilStep("layer fade is invisible", () => !layer.IsPresent);

            AddStep("show health", () => showHealth.Value = true);
            AddStep("enable FadePlayfieldWhenHealthLow", () => config.SetValue(OsuSetting.FadePlayfieldWhenHealthLow, true));
            AddUntilStep("layer fade is visible", () => layer.IsPresent);
        }

        private partial class HealthProcessorContainer : Container
        {
            [Cached(typeof(HealthProcessor))]
            private readonly HealthProcessor healthProcessor;

            public HealthProcessorContainer(HealthProcessor healthProcessor)
            {
                this.healthProcessor = healthProcessor;
            }
        }
    }
}

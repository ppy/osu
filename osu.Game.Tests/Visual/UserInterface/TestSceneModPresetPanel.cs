// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModPresetPanel : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset selected mods", () => SelectedMods.SetDefault());
        }

        [Test]
        public void TestVariousModPresets()
        {
            AddStep("create content", () => Child = new FillFlowContainer
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(0, 5),
                ChildrenEnumerable = createTestPresets().Select(preset => new ModPresetPanel(preset.ToLiveUnmanaged()))
            });
        }

        [Test]
        public void TestPresetSelectionStateAfterExternalModChanges()
        {
            ModPresetPanel? panel = null;

            AddStep("create panel", () => Child = panel = new ModPresetPanel(createTestPresets().First().ToLiveUnmanaged())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f
            });
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            AddStep("set mods to HR", () => SelectedMods.Value = new[] { new OsuModHardRock() });
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            AddStep("set mods to DT", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            AddStep("set mods to HR+DT", () => SelectedMods.Value = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() });
            AddAssert("panel is active", () => panel.AsNonNull().Active.Value);

            AddStep("set mods to HR+customised DT", () => SelectedMods.Value = new Mod[]
            {
                new OsuModHardRock(),
                new OsuModDoubleTime
                {
                    SpeedChange = { Value = 1.25 }
                }
            });
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            AddStep("set mods to HR+DT", () => SelectedMods.Value = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() });
            AddAssert("panel is active", () => panel.AsNonNull().Active.Value);

            AddStep("customise mod in place", () => SelectedMods.Value.OfType<OsuModDoubleTime>().Single().SpeedChange.Value = 1.33);
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            AddStep("set mods to HD+HR+DT", () => SelectedMods.Value = new Mod[] { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime() });
            AddAssert("panel is not active", () => !panel.AsNonNull().Active.Value);

            // system mods are not included in presets.
            AddStep("set mods to HR+DT+TD", () => SelectedMods.Value = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime(), new OsuModTouchDevice() });
            AddAssert("panel is active", () => panel.AsNonNull().Active.Value);
        }

        [Test]
        public void TestActivatingPresetTogglesIncludedMods()
        {
            ModPresetPanel? panel = null;

            AddStep("create panel", () => Child = panel = new ModPresetPanel(createTestPresets().First().ToLiveUnmanaged())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f
            });

            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() });

            AddStep("deactivate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(Array.Empty<Mod>());

            AddStep("set different mod", () => SelectedMods.Value = new[] { new OsuModHidden() });
            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() });

            AddStep("set customised mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.25 } } });
            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(new Mod[] { new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.5 } } });

            AddStep("set system mod", () => SelectedMods.Value = new[] { new OsuModTouchDevice() });
            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(new Mod[] { new OsuModTouchDevice(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.5 } } });
        }

        [Test]
        public void TestActivatingPresetWithAutoplayWhenSystemModEnabled()
        {
            ModPresetPanel? panel = null;

            AddStep("create panel", () => Child = panel = new ModPresetPanel(new ModPreset
            {
                Name = "Autoplay include",
                Description = "no way",
                Mods = new Mod[]
                {
                    new OsuModAutoplay()
                },
                Ruleset = new OsuRuleset().RulesetInfo
            }.ToLiveUnmanaged())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f
            });

            AddStep("Add touch device to selected mod", () => SelectedMods.Value = new Mod[] { new OsuModTouchDevice() });
            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());

            // touch device should be removed due to incompatible with autoplay.
            assertSelectedModsEquivalentTo(new Mod[] { new OsuModAutoplay() });

            AddStep("deactivate panel", () => panel.AsNonNull().TriggerClick());
            assertSelectedModsEquivalentTo(Array.Empty<Mod>());

            // just for test purpose
            AddStep("Add score v2 to selected mod", () => SelectedMods.Value = new Mod[] { new ModScoreV2() });
            AddStep("activate panel", () => panel.AsNonNull().TriggerClick());

            assertSelectedModsEquivalentTo(new Mod[] { new OsuModAutoplay(), new ModScoreV2() });
        }

        private void assertSelectedModsEquivalentTo(IEnumerable<Mod> mods)
            => AddAssert("selected mods changed correctly", () => new HashSet<Mod>(SelectedMods.Value).SetEquals(mods));

        private static IEnumerable<ModPreset> createTestPresets() => new[]
        {
            new ModPreset
            {
                Name = "First preset",
                Description = "Please ignore",
                Mods = new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                },
                Ruleset = new OsuRuleset().RulesetInfo
            },
            new ModPreset
            {
                Name = "AR0",
                Description = "For good readers",
                Mods = new Mod[]
                {
                    new OsuModDifficultyAdjust
                    {
                        ApproachRate = { Value = 0 }
                    }
                },
                Ruleset = new OsuRuleset().RulesetInfo
            },
            new ModPreset
            {
                Name = "This preset is going to have an extraordinarily long name",
                Description = "This is done so that the capability to truncate overlong texts may be demonstrated",
                Mods = new Mod[]
                {
                    new OsuModFlashlight(),
                    new OsuModSpinIn()
                },
                Ruleset = new OsuRuleset().RulesetInfo
            }
        };
    }
}

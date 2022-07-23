// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModPresetColumn : OsuTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private RulesetStore rulesets = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear contents", Clear);
            AddStep("reset storage", () =>
            {
                Realm.Write(realm =>
                {
                    realm.RemoveAll<ModPreset>();

                    var testPresets = createTestPresets();
                    foreach (var preset in testPresets)
                        preset.Ruleset = realm.Find<RulesetInfo>(preset.Ruleset.ShortName);

                    realm.Add(testPresets);
                });
            });
        }

        [Test]
        public void TestBasicOperation()
        {
            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = new ModPresetColumn
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);

            AddStep("change ruleset to mania", () => Ruleset.Value = rulesets.GetRuleset(3));
            AddUntilStep("1 panel visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 1);

            AddStep("add another mania preset", () => Realm.Write(r => r.Add(new ModPreset
            {
                Name = "and another one",
                Mods = new Mod[]
                {
                    new ManiaModMirror(),
                    new ManiaModNightcore(),
                    new ManiaModHardRock()
                },
                Ruleset = r.Find<RulesetInfo>("mania")
            })));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("add another osu! preset", () => Realm.Write(r => r.Add(new ModPreset
            {
                Name = "hdhr",
                Mods = new Mod[]
                {
                    new OsuModHidden(),
                    new OsuModHardRock()
                },
                Ruleset = r.Find<RulesetInfo>("osu")
            })));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("remove mania preset", () => Realm.Write(r =>
            {
                var toRemove = r.All<ModPreset>().Single(preset => preset.Name == "Different ruleset");
                r.Remove(toRemove);
            }));
            AddUntilStep("1 panel visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 1);

            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddUntilStep("4 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 4);
        }

        [Test]
        public void TestSoftDeleteSupport()
        {
            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddStep("create content", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(30),
                Child = new ModPresetColumn
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);

            AddStep("soft delete preset", () => Realm.Write(r =>
            {
                var toSoftDelete = r.All<ModPreset>().Single(preset => preset.Name == "AR0");
                toSoftDelete.DeletePending = true;
            }));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("soft delete all presets", () => Realm.Write(r =>
            {
                foreach (var preset in r.All<ModPreset>())
                    preset.DeletePending = true;
            }));
            AddUntilStep("no panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 0);

            AddStep("undelete preset", () => Realm.Write(r =>
            {
                foreach (var preset in r.All<ModPreset>())
                    preset.DeletePending = false;
            }));
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);
        }

        private ICollection<ModPreset> createTestPresets() => new[]
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
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
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
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
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
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
            },
            new ModPreset
            {
                Name = "Different ruleset",
                Description = "Just to shake things up",
                Mods = new Mod[]
                {
                    new ManiaModKey4(),
                    new ManiaModFadeIn()
                },
                Ruleset = rulesets.GetRuleset(3).AsNonNull()
            }
        };
    }
}

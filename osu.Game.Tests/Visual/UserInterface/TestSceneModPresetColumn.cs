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

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset storage", () =>
            {
                Realm.Write(realm =>
                {
                    realm.RemoveAll<ModPreset>();
                    realm.Add(createTestPresets());
                });
            });
        }

        [Test]
        public void TestBasicAppearance()
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
        }

        private IEnumerable<ModPreset> createTestPresets() => new[]
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

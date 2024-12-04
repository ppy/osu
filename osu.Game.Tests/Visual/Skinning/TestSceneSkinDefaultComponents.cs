// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania.Skinning.Argon;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Skinning.Triangles;

namespace osu.Game.Tests.Visual.Skinning
{
    /// <summary>
    /// This test ensures all relevant skin components are present at the tested screen.
    /// Whenever a new skin component is added or a change is made to the placement of an existing component (i.e. moved from global to per-ruleset),
    /// this test should be updated accordingly.
    /// </summary>
    public partial class TestSceneSkinDefaultComponents : ScreenTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Test]
        public void TestArgonPlayerScreen()
        {
            AddStep("use argon skin", () => skins.CurrentSkinInfo.Value = ArgonSkin.CreateInfo().ToLiveUnmanaged());
            testPlayerScreen((r, components) =>
            {
                string[] globalComponents =
                {
                    nameof(ArgonAccuracyCounter), nameof(ArgonHealthDisplay), nameof(ArgonKeyCounterDisplay),
                    nameof(ArgonPerformancePointsCounter), nameof(ArgonScoreCounter), nameof(ArgonSongProgress), nameof(ArgonWedgePiece),
                    nameof(ArgonWedgePiece), nameof(BarHitErrorMeter), nameof(BarHitErrorMeter), nameof(BoxElement),
                };

                string[] rulesetComponents;

                switch (r.ShortName)
                {
                    default:
                        rulesetComponents = new[] { nameof(ArgonComboCounter) };
                        break;

                    case @"mania":
                        rulesetComponents = new[] { nameof(ArgonManiaComboCounter) };
                        break;
                }

                Assert.That(components.Select(c => c.GetType().Name).Order(), Is.EquivalentTo(globalComponents.Concat(rulesetComponents).Order()));
                return true;
            });
        }

        [Test]
        public void TestTrianglesPlayerScreen()
        {
            AddStep("use triangles skin", () => skins.CurrentSkinInfo.Value = TrianglesSkin.CreateInfo().ToLiveUnmanaged());
            testPlayerScreen((r, components) =>
            {
                Assert.That(components.Select(c => c.GetType().Name).Order(), Is.EquivalentTo(
                    new[]
                    {
                        nameof(BarHitErrorMeter), nameof(BarHitErrorMeter), nameof(DefaultAccuracyCounter), nameof(DefaultComboCounter), nameof(DefaultHealthDisplay),
                        nameof(DefaultKeyCounterDisplay), nameof(DefaultScoreCounter), nameof(DefaultSongProgress), nameof(TrianglesPerformancePointsCounter)
                    }));

                return true;
            });
        }

        [Test]
        public void TestLegacyPlayerScreen()
        {
            AddStep("use legacy skin", () => skins.CurrentSkinInfo.Value = DefaultLegacySkin.CreateInfo().ToLiveUnmanaged());
            testPlayerScreen((r, components) =>
            {
                string[] globalComponents =
                {
                    nameof(BarHitErrorMeter), nameof(LegacyAccuracyCounter), nameof(LegacyScoreCounter), nameof(LegacySongProgress),
                };

                string[] rulesetComponents = Array.Empty<string>();

                switch (r.ShortName)
                {
                    case @"osu":
                        rulesetComponents = new[] { nameof(LegacyDefaultComboCounter), nameof(LegacyKeyCounterDisplay), nameof(LegacyHealthDisplay) };
                        break;

                    case @"taiko":
                        rulesetComponents = new[] { nameof(LegacyDefaultComboCounter), nameof(LegacyHealthDisplay) };
                        break;

                    case @"fruits":
                        rulesetComponents = new[] { nameof(LegacyKeyCounterDisplay), nameof(LegacyHealthDisplay) };
                        break;

                    case @"mania":
                        rulesetComponents = new[] { nameof(LegacyManiaComboCounter), nameof(LegacyHealthDisplay) };
                        break;
                }

                Assert.That(components.Select(c => c.GetType().Name).Order(), Is.EquivalentTo(globalComponents.Concat(rulesetComponents).Order()));
                return true;
            });
        }

        private void testPlayerScreen(Func<RulesetInfo, IEnumerable<ISerialisableDrawable>, bool> checkComponents)
        {
            foreach (string rulesetName in new[] { @"osu", @"taiko", @"fruits", @"mania" })
            {
                AddStep($"set ruleset to {rulesetName}", () => Ruleset.Value = rulesets.GetRuleset(rulesetName));
                AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value));
                AddStep("load player", () => LoadScreen(new TestPlayer()));
                AddUntilStep("wait for load", () => Stack.CurrentScreen is Player player && player.IsLoaded);
                AddUntilStep("wait for components", () => this.ChildrenOfType<SkinnableContainer>().All(s => s.ComponentsLoaded));
                AddAssert("all components displayed", () =>
                {
                    checkComponents(Ruleset.Value, this.ChildrenOfType<ISerialisableDrawable>());
                    return true;
                });
                AddStep("exit player", () => Stack.Exit());
            }
        }
    }
}

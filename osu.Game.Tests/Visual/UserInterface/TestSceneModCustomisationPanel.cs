// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModCustomisationPanel : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        private ModCustomisationPanel panel = null!;
        private ModCustomisationHeader header = null!;
        private Container content = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(20f),
                Child = panel = new ModCustomisationPanel
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 400f,
                    State = { Value = Visibility.Visible },
                    SelectedMods = { BindTarget = SelectedMods },
                }
            };

            header = panel.Children.OfType<ModCustomisationHeader>().First();
            content = panel.Children.OfType<Container>().First();
        });

        [Test]
        public void TestDisplay()
        {
            AddStep("set DT", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = panel.Expanded = true;
            });
            AddStep("set DA", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModDifficultyAdjust() };
                panel.Enabled.Value = panel.Expanded = true;
            });
            AddStep("set FL+WU+DA+AD", () =>
            {
                SelectedMods.Value = new Mod[] { new OsuModFlashlight(), new ModWindUp(), new OsuModDifficultyAdjust(), new OsuModApproachDifferent() };
                panel.Enabled.Value = panel.Expanded = true;
            });
            AddStep("set empty", () =>
            {
                SelectedMods.Value = Array.Empty<Mod>();
                panel.Enabled.Value = panel.Expanded = false;
            });
        }

        [Test]
        public void TestHoverExpand()
        {
            // Can not expand by hovering when no supported mod
            {
                AddStep("hover header", () => InputManager.MoveMouseTo(header));

                AddAssert("not expanded", () => !panel.Expanded);

                AddStep("hover content", () => InputManager.MoveMouseTo(content));

                AddAssert("neither expanded", () => !panel.Expanded);

                AddStep("left from content", () => InputManager.MoveMouseTo(Vector2.One));
            }

            AddStep("add customisable mod", () =>
            {
                SelectedMods.Value = new[] { new OsuModDoubleTime() };
                panel.Enabled.Value = true;
            });

            // Can expand by hovering when supported mod
            {
                AddStep("hover header", () => InputManager.MoveMouseTo(header));

                AddAssert("expanded", () => panel.Expanded);

                AddStep("hover content", () => InputManager.MoveMouseTo(content));

                AddAssert("still expanded", () => panel.Expanded);
            }

            // Will collapse when mouse left from content
            {
                AddStep("left from content", () => InputManager.MoveMouseTo(Vector2.One));

                AddAssert("not expanded", () => !panel.Expanded);
            }

            // Will collapse when mouse left from header
            {
                AddStep("hover header", () => InputManager.MoveMouseTo(header));

                AddAssert("expanded", () => panel.Expanded);

                AddStep("left from header", () => InputManager.MoveMouseTo(Vector2.One));

                AddAssert("not expanded", () => !panel.Expanded);
            }

            // Not collapse when mouse left if not expanded by hovering
            {
                AddStep("expand not by hovering", () => panel.Expanded = true);

                AddStep("hover content", () => InputManager.MoveMouseTo(content));

                AddStep("moust left", () => InputManager.MoveMouseTo(Vector2.One));

                AddAssert("still expanded", () => panel.Expanded);
            }
        }
    }
}

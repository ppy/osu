// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModSettings : OsuTestScene
    {
        private TestModSelectOverlay modSelect;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(modSelect = new TestModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            var testMod = new TestModCustomisable1();

            AddStep("open", modSelect.Show);
            AddAssert("button disabled", () => !modSelect.CustomiseButton.Enabled.Value);
            AddUntilStep("wait for button load", () => modSelect.ButtonsLoaded);
            AddStep("select mod", () => modSelect.SelectMod(testMod));
            AddAssert("button enabled", () => modSelect.CustomiseButton.Enabled.Value);
            AddStep("open Customisation", () => modSelect.CustomiseButton.Click());
            AddStep("deselect mod", () => modSelect.SelectMod(testMod));
            AddAssert("controls hidden", () => modSelect.ModSettingsContainer.Alpha == 0);
        }

        private class TestModSelectOverlay : ModSelectOverlay
        {
            public new Container ModSettingsContainer => base.ModSettingsContainer;
            public new TriangleButton CustomiseButton => base.CustomiseButton;

            public bool ButtonsLoaded => ModSectionsContainer.Children.All(c => c.ModIconsLoaded);

            public void SelectMod(Mod mod) =>
                ModSectionsContainer.Children.Single(s => s.ModType == mod.Type)
                                    .ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType())).SelectNext(1);

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var section in ModSectionsContainer)
                {
                    if (section.ModType == ModType.Conversion)
                    {
                        section.Mods = new Mod[]
                        {
                            new TestModCustomisable1(),
                            new TestModCustomisable2()
                        };
                    }
                    else
                        section.Mods = Array.Empty<Mod>();
                }
            }
        }

        private class TestModCustomisable1 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 1";

            public override string Acronym => "CM1";
        }

        private class TestModCustomisable2 : TestModCustomisable
        {
            public override string Name => "Customisable Mod 2";

            public override string Acronym => "CM2";
        }

        private abstract class TestModCustomisable : Mod, IApplicableMod
        {
            public override double ScoreMultiplier => 1.0;

            public override ModType Type => ModType.Conversion;

            [SettingSource("Sample float", "Change something for a mod")]
            public BindableFloat SliderBindable { get; } = new BindableFloat
            {
                MinValue = 0,
                MaxValue = 10,
                Default = 5,
                Value = 7
            };

            [SettingSource("Sample bool", "Clicking this changes a setting")]
            public BindableBool TickBindable { get; } = new BindableBool();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModCustomizationSettings : OsuTestScene
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

            var testMod = new TestModCustomizable1();

            AddStep("open", modSelect.Show);
            AddAssert("button disabled", () => !modSelect.CustomizeButton.Enabled.Value);
            AddStep("select mod", () => modSelect.SelectMod(testMod));
            AddAssert("button enabled", () => modSelect.CustomizeButton.Enabled.Value);
            AddStep("open customization", () => modSelect.CustomizeButton.Click());
            AddStep("deselect mod", () => modSelect.SelectMod(testMod));
            AddAssert("controls hidden", () => modSelect.ModSettingsContainer.Alpha == 0);
        }

        private class TestModSelectOverlay : ModSelectOverlay
        {
            public new Container ModSettingsContainer => base.ModSettingsContainer;
            public new TriangleButton CustomizeButton => base.CustomizeButton;

            public void SelectMod(Mod mod) =>
                ModSectionsContainer.Children.Single(s => s.ModType == mod.Type)
                                    .ButtonsContainer.OfType<ModButton>().Single(b => b.Mods.Any(m => m.GetType() == mod.GetType())).SelectNext(1);

            public ModControlSection GetControlSection(Mod mod) =>
                ModSettingsContent.Children.FirstOrDefault(s => s.Mod == mod);

            protected override void LoadComplete()
            {
                base.LoadComplete();

                foreach (var section in ModSectionsContainer)
                {
                    if (section.ModType == ModType.Conversion)
                    {
                        section.Mods = new Mod[]
                        {
                            new TestModCustomizable1(),
                            new TestModCustomizable2()
                        };
                    }
                    else
                        section.Mods = new Mod[] { };
                }
            }
        }

        private class TestModCustomizable1 : TestModCustomizable
        {
            public override string Name => "Customizable Mod 1";

            public override string Acronym => "CM1";
        }

        private class TestModCustomizable2 : TestModCustomizable
        {
            public override string Name => "Customizable Mod 2";

            public override string Acronym => "CM2";
        }

        private abstract class TestModCustomizable : Mod, IModHasSettings
        {
            public override double ScoreMultiplier => 1.0;

            public override ModType Type => ModType.Conversion;

            public readonly BindableFloat SliderBindable = new BindableFloat
            {
                MinValue = 0,
                MaxValue = 10,
            };

            public readonly BindableBool TickBindable = new BindableBool();

            public Drawable[] CreateControls() =>
                new Drawable[]
                {
                    new SettingsSlider<float>
                    {
                        LabelText = "Slider",
                        Bindable = SliderBindable
                    },
                    new SettingsCheckbox
                    {
                        LabelText = "Checkbox",
                        Bindable = TickBindable
                    }
                };
        }
    }
}

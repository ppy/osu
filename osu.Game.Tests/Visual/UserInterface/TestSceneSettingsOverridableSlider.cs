// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneSettingsOverridableSlider : OsuManualInputManagerTestScene
    {
        private OverridableBindable<float> setting, setting2;
        private SettingsOverridableSlider<float> slider1;

        [SetUp]
        public void SetUp() => Schedule(() => Child = new Container
        {
            Masking = true,
            CornerRadius = 10f,
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Scale = new Vector2(1.5f),
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                new FillFlowContainer
                {
                    Width = 250f,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Vertical = 10f },
                    Direction = FillDirection.Vertical,
                    Children = new[]
                    {
                        slider1 = new SettingsOverridableSlider<float>(setting = new OverridableBindable<float>(5, 1, 10, 0.05f))
                        {
                            LabelText = "Customizable setting #1",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        new SettingsOverridableSlider<float>(setting2 = new OverridableBindable<float>(5, 5, 6, 0.01f))
                        {
                            LabelText = "Customizable setting #2",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            }
        });

        [Test]
        public void TestBaseSliders()
        {
            AddSliderStep("change base of setting 1", 1f, 10f, 2.5f, v =>
            {
                if (setting != null)
                    setting.BaseValue.Value = v;
            });

            AddSliderStep("change base of setting 2", 5f, 6f, 5.25f, v =>
            {
                if (setting2 != null)
                    setting2.BaseValue.Value = v;
            });
        }

        [Test]
        public void TestSettingRevertsCustomOnDisabling()
        {
            AddStep("enable setting", clickCheckbox);

            AddStep("change slider value", () =>
            {
                InputManager.MoveMouseTo(slider1.ChildrenOfType<OsuSliderBar<float>>().Single());
                InputManager.Key(Key.Right);
            });

            AddStep("disable setting", clickCheckbox);
            AddStep("re-enable setting", clickCheckbox);

            AddAssert("slider value still at base", () => slider1.Current.Value == setting.BaseValue.Value);
            AddAssert("slider enabled", () => !slider1.Current.Disabled);

            void clickCheckbox()
            {
                InputManager.MoveMouseTo(slider1.ChildrenOfType<OsuCheckbox>().Single().ChildrenOfType<Nub>().Single());
                InputManager.Click(MouseButton.Left);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFormSliderBar : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestTransferValueOnCommit()
        {
            OsuSpriteText text;
            FormSliderBar<float> slider = null!;

            AddStep("create content", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText(),
                        slider = new FormSliderBar<float>
                        {
                            Caption = "Slider",
                            Current = new BindableFloat
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 0.1f,
                                Default = 5f,
                            }
                        },
                    }
                };
                slider.Current.BindValueChanged(_ => text.Text = $"Current value is: {slider.Current.Value}", true);
            });
            AddToggleStep("toggle transfer value on commit", b =>
            {
                if (slider.IsNotNull())
                    slider.TransferValueOnCommit = b;
            });
        }

        [Test]
        public void TestNubDoubleClickRevertToDefault()
        {
            FormSliderBar<float> slider = null!;

            AddStep("create content", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        slider = new FormSliderBar<float>
                        {
                            Caption = "Slider",
                            Current = new BindableFloat
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 0.1f,
                                Default = 5f,
                            }
                        },
                    }
                };
            });
            AddStep("set slider to 1", () => slider.Current.Value = 1);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<Circle>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("slider is default", () => slider.Current.IsDefault);
        }

        [Test]
        public void TestDisabled()
        {
            FormSliderBar<float> slider = null!;

            AddStep("create content", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        slider = new FormSliderBar<float>
                        {
                            Caption = "Slider",
                            Current = new BindableFloat
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 0.1f,
                                Default = 5f,
                            }
                        },
                    }
                };
            });
            AddStep("set slider to 1", () => slider.Current.Value = 1);
            AddStep("disable slider", () => slider.Current.Disabled = true);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<Circle>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("slider is still at 1", () => slider.Current.Value, () => Is.EqualTo(1));

            AddStep("click on textbox part", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("no text selected", () => slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().SelectedText, () => Is.Empty);
            AddStep("attempt to input text", () =>
            {
                InputManager.Key(Key.Number4);
                InputManager.Key(Key.Enter);
            });
            AddAssert("slider is still at 1", () => slider.Current.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestDisabledImmediately()
        {
            FormSliderBar<float> slider = null!;

            AddStep("create content", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        slider = new FormSliderBar<float>
                        {
                            Caption = "Slider",
                            Current = new BindableFloat
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 0.1f,
                                Default = 5f,
                                Disabled = true,
                            },
                            TransferValueOnCommit = true,
                        },
                    }
                };
            });

            AddStep("click on textbox part", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("no text selected", () => slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().SelectedText, () => Is.Empty);
        }
    }
}

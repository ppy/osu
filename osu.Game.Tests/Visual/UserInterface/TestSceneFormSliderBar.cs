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

        [TestCase(false)]
        [TestCase(true)]
        public void TestNubDoubleClickRevertToDefault(bool transferValueOnCommit)
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
                            TransferValueOnCommit = transferValueOnCommit,
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

            AddStep("re-enable slider", () => slider.Current.Disabled = false);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<Circle>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("slider is at 5", () => slider.Current.Value, () => Is.EqualTo(5));
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

        [Test]
        public void TestDisplayAsPercentageFloat()
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
                                MaxValue = 1,
                                Precision = 0.01f,
                                Default = 0.5f,
                                Value = 0.5f,
                            },
                            DisplayAsPercentage = true,
                        },
                    }
                };
                slider.Current.BindValueChanged(_ => text.Text = $"Current value is: {slider.Current.Value}", true);
            });

            AddStep("click on textbox part", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("text selected", () => slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().SelectedText, () => Is.EqualTo("50"));
            AddStep("input 9%", () =>
            {
                slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().Text = "9";
                InputManager.Key(Key.Enter);
            });
            AddAssert("slider is at 0.09", () => slider.Current.Value, () => Is.EqualTo(0.09f));

            AddStep("start dragging nub", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormSliderBar<float>.InnerSliderNub>().Single());
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("drag nub to 50%", () =>
            {
                var innerSlider = slider.ChildrenOfType<FormSliderBar<float>.InnerSlider>().Single();
                InputManager.MoveMouseTo((innerSlider.ScreenSpaceDrawQuad.TopLeft + innerSlider.ScreenSpaceDrawQuad.TopRight) / 2);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("slider is at ~0.5", () => slider.Current.Value, () => Is.EqualTo(0.5).Within(0.01f));
        }

        [Test]
        public void TestDisplayAsPercentageInt()
        {
            OsuSpriteText text;
            FormSliderBar<int> slider = null!;

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
                        slider = new FormSliderBar<int>
                        {
                            Caption = "Slider",
                            Current = new BindableInt
                            {
                                MinValue = 0,
                                MaxValue = 100,
                                Precision = 1,
                                Default = 50,
                                Value = 50,
                            },
                            DisplayAsPercentage = true,
                        },
                    }
                };
                slider.Current.BindValueChanged(_ => text.Text = $"Current value is: {slider.Current.Value}", true);
            });

            AddStep("click on textbox part", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("text selected", () => slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().SelectedText, () => Is.EqualTo("50"));
            AddStep("input 9%", () =>
            {
                slider.ChildrenOfType<FormTextBox.InnerTextBox>().Single().Text = "9";
                InputManager.Key(Key.Enter);
            });
            AddAssert("slider is at 9", () => slider.Current.Value, () => Is.EqualTo(9));

            AddStep("start dragging nub", () =>
            {
                InputManager.MoveMouseTo(slider.ChildrenOfType<FormSliderBar<int>.InnerSliderNub>().Single());
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("drag nub to 50%", () =>
            {
                var innerSlider = slider.ChildrenOfType<FormSliderBar<int>.InnerSlider>().Single();
                InputManager.MoveMouseTo((innerSlider.ScreenSpaceDrawQuad.TopLeft + innerSlider.ScreenSpaceDrawQuad.TopRight) / 2);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("slider is at ~50", () => slider.Current.Value, () => Is.EqualTo(50).Within(1));
        }
    }
}

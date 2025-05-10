// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSafeAreaHandling : OsuGameTestScene
    {
        private SafeAreaDefiningContainer safeAreaContainer;

        private static BindableSafeArea safeArea;

        private readonly Bindable<float> safeAreaPaddingTop = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingBottom = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingLeft = new BindableFloat { MinValue = 0, MaxValue = 200 };
        private readonly Bindable<float> safeAreaPaddingRight = new BindableFloat { MinValue = 0, MaxValue = 200 };

        private readonly Bindable<bool> applySafeAreaConsiderations = new Bindable<bool>(true);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Usually this would be placed between the host and the game, but that's a bit of a pain to do with the test scene hierarchy.

            // Add is required for the container to get a size (and give out correct metrics to the usages in SafeAreaContainer).
            Add(safeAreaContainer = new SafeAreaDefiningContainer(safeArea = new BindableSafeArea())
            {
                RelativeSizeAxes = Axes.Both
            });

            // Cache is required for the test game to see the safe area.
            Dependencies.CacheAs<ISafeArea>(safeAreaContainer);
        }

        public override void SetUpSteps()
        {
            AddStep("Add adjust controls", () =>
            {
                Add(new Container
                {
                    Depth = float.MinValue,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.8f,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Width = 400,
                            Children = new Drawable[]
                            {
                                new SettingsSlider<float>
                                {
                                    Current = safeAreaPaddingTop,
                                    LabelText = "Top"
                                },
                                new SettingsSlider<float>
                                {
                                    Current = safeAreaPaddingBottom,
                                    LabelText = "Bottom"
                                },
                                new SettingsSlider<float>
                                {
                                    Current = safeAreaPaddingLeft,
                                    LabelText = "Left"
                                },
                                new SettingsSlider<float>
                                {
                                    Current = safeAreaPaddingRight,
                                    LabelText = "Right"
                                },
                                new SettingsCheckbox
                                {
                                    LabelText = "Apply",
                                    Current = applySafeAreaConsiderations,
                                },
                            }
                        }
                    }
                });

                safeAreaPaddingTop.BindValueChanged(_ => updateSafeArea());
                safeAreaPaddingBottom.BindValueChanged(_ => updateSafeArea());
                safeAreaPaddingLeft.BindValueChanged(_ => updateSafeArea());
                safeAreaPaddingRight.BindValueChanged(_ => updateSafeArea());
                applySafeAreaConsiderations.BindValueChanged(_ => updateSafeArea());
            });

            base.SetUpSteps();
        }

        private void updateSafeArea()
        {
            safeArea.Value = new MarginPadding
            {
                Top = safeAreaPaddingTop.Value,
                Bottom = safeAreaPaddingBottom.Value,
                Left = safeAreaPaddingLeft.Value,
                Right = safeAreaPaddingRight.Value,
            };

            Game.LocalConfig.SetValue(OsuSetting.SafeAreaConsiderations, applySafeAreaConsiderations.Value);
        }

        [Test]
        public void TestSafeArea()
        {
        }
    }
}

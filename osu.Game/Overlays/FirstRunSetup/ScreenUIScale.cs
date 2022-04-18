// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    public class ScreenUIScale : FirstRunSetupScreen
    {
        [Resolved]
        private OsuConfigManager osuConfig { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(20),
                                Children = new Drawable[]
                                {
                                    new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 24))
                                    {
                                        Text = "The osu! user interface size can be adjusted to your liking.",
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y
                                    },
                                    new SettingsSlider<float, UIScaleSlider>
                                    {
                                        LabelText = GraphicsSettingsStrings.UIScaling,
                                        Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                                        KeyboardStep = 0.01f,
                                    },
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new SampleScreenContainer(new MainMenu()),
                                        new SampleScreenContainer(new PlaySongSelect()),
                                    },
                                    new Drawable[]
                                    {
                                        new SampleScreenContainer(new MainMenu()),
                                        new SampleScreenContainer(new MainMenu()),
                                    }
                                }
                            }
                        },
                    }
                }
            };
        }

        private class SampleScreenContainer : CompositeDrawable
        {
            public override bool HandlePositionalInput => false;
            public override bool HandleNonPositionalInput => false;
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;

            public SampleScreenContainer(Screen screen)
            {
                OsuScreenStack stack;
                RelativeSizeAxes = Axes.Both;

                OsuLogo logo;

                InternalChildren = new Drawable[]
                {
                    new DependencyProvidingContainer
                    {
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(OsuLogo), logo = new OsuLogo
                            {
                                RelativePositionAxes = Axes.Both,
                                Position = new Vector2(0.5f),
                            })
                        },
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new ScalingContainer(ScalingMode.Off)
                            {
                                Masking = true,
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    stack = new OsuScreenStack(),
                                    logo
                                },
                            },
                        }
                    },
                };

                stack.Push(screen);
            }
        }

        private class UIScaleSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }
    }
}

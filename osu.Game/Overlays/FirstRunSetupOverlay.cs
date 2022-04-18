// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    [Cached]
    public class FirstRunSetupOverlay : OsuFocusedOverlayContainer
    {
        protected override bool StartHidden => true;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        private ScreenStack stack = null!;

        public PurpleTriangleButton NextButton = null!;
        public DangerousTriangleButton BackButton = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private int? currentStepIndex;

        private const float scale_when_hidden = 0.9f;

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public FirstRunSetupScreen? CurrentScreen => (FirstRunSetupScreen?)stack.CurrentScreen;

        private readonly FirstRunStep[] steps =
        {
            new FirstRunStep(typeof(ScreenWelcome), "Welcome"),
            new FirstRunStep(typeof(ScreenUIScale), "UI Scale"),
        };

        private Container stackContainer = null!;

        public FirstRunSetupOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.95f);

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 5,
                Colour = Color4.Black.Opacity(0.2f),
            };

            Masking = true;
            CornerRadius = 10;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        Colour = colourProvider.Background5,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Margin = new MarginPadding(10),
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = "First run setup",
                                                Font = OsuFont.Default.With(size: 32),
                                                Colour = colourProvider.Content2,
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                            },
                                            new OsuTextFlowContainer
                                            {
                                                Text = "Setup osu! to suit you",
                                                Colour = colourProvider.Content1,
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                AutoSizeAxes = Axes.Both,
                                            },
                                        }
                                    },
                                }
                            },
                        },
                        new Drawable[]
                        {
                            stackContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding(20),
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding(20),
                                Child = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.Absolute, 10),
                                        new Dimension(),
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            BackButton = new DangerousTriangleButton
                                            {
                                                Width = 200,
                                                Text = "Back",
                                                Action = showLastStep,
                                                Enabled = { Value = false },
                                            },
                                            Empty(),
                                            NextButton = new PurpleTriangleButton
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Width = 1,
                                                Text = "Get started",
                                                Action = showNextStep
                                            }
                                        },
                                    }
                                },
                            }
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // if we are valid for display, only do so after reaching the main menu.
            performer.PerformFromScreen(_ => { Show(); }, new[] { typeof(MainMenu) });
        }

        private void showLastStep()
        {
            Debug.Assert(currentStepIndex > 0);

            stack.CurrentScreen.Exit();
            currentStepIndex--;

            BackButton.Enabled.Value = currentStepIndex != 0;

            updateButtonText();
        }

        private void showNextStep()
        {
            if (currentStepIndex == null)
            {
                stackContainer.Child = stack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                };

                currentStepIndex = 0;
            }
            else
                currentStepIndex++;

            Debug.Assert(currentStepIndex != null);
            BackButton.Enabled.Value = currentStepIndex > 0;

            if (currentStepIndex < steps.Length)
            {
                stack.Push((Screen)Activator.CreateInstance(steps[currentStepIndex.Value].ScreenType));
                updateButtonText();
            }
            else
            {
                currentStepIndex = null;
                Hide();
            }
        }

        private void updateButtonText()
        {
            Debug.Assert(currentStepIndex != null);

            NextButton.Text = currentStepIndex + 1 < steps.Length
                ? $"Next ({steps[currentStepIndex.Value + 1].Description})"
                : "Finish";
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.ScaleTo(scale_when_hidden)
                .ScaleTo(1, 400, Easing.OutElasticHalf);

            this.FadeIn(400, Easing.OutQuint);

            if (currentStepIndex == null)
                showNextStep();
        }

        protected override void PopOut()
        {
            if (currentStepIndex != null)
            {
                notificationOverlay.Post(new SimpleNotification
                {
                    Text = "Click here to resume initial setup at any point",
                    Icon = FontAwesome.Solid.Horse,
                    Activated = () =>
                    {
                        Show();
                        return true;
                    },
                });
            }
            else
            {
                stack?
                    .FadeOut(100)
                    .Expire();
            }

            base.PopOut();

            this.ScaleTo(0.96f, 400, Easing.OutQuint);
            this.FadeOut(200, Easing.OutQuint);
        }

        private class FirstRunStep
        {
            public readonly Type ScreenType;
            public readonly LocalisableString Description;

            public FirstRunStep(Type screenType, LocalisableString description)
            {
                ScreenType = screenType;
                Description = description;
            }
        }
    }
}

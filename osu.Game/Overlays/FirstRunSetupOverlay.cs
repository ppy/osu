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
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.FirstRunSetup;
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
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        private ScreenStack stack = null!;

        public PurpleTriangleButton NextButton = null!;
        public DangerousTriangleButton BackButton = null!;
        private Container mainContent = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private int? currentStepIndex;

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public FirstRunSetupScreen? CurrentScreen => (FirstRunSetupScreen?)stack.CurrentScreen;

        private readonly FirstRunStep[] steps =
        {
            new FirstRunStep(typeof(ScreenWelcome), "Welcome"),
            new FirstRunStep(typeof(ScreenUIScale), "UI Scale"),
        };

        public FirstRunSetupOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                mainContent = new BlockingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.95f),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Masking = true,
                    CornerRadius = 10,
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
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding(20),
                                        Child = stack = new ScreenStack
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
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
                    }
                }
            };
        }

        private class BlockingContainer : Container
        {
            protected override bool OnMouseDown(MouseDownEvent e) => true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // if we are valid for display, only do so after reaching the main menu.
            performer.PerformFromScreen(_ => { Show(); }, new[] { typeof(MainMenu) });
        }

        protected override bool OnClick(ClickEvent e)
        {
            dialogOverlay.Push(new ConfirmDialog("Are you sure you want to exit the setup process?", Hide, () => { }));

            return base.OnClick(e);
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(400, Easing.OutQuint);

            if (currentStepIndex == null)
                showNextStep();
        }

        private void showLastStep()
        {
            Debug.Assert(currentStepIndex > 0);

            stack.CurrentScreen.Exit();
            currentStepIndex--;

            BackButton.Enabled.Value = currentStepIndex != 0;
        }

        private void showNextStep()
        {
            if (currentStepIndex == null)
                currentStepIndex = 0;
            else
                currentStepIndex++;

            Debug.Assert(currentStepIndex != null);
            BackButton.Enabled.Value = currentStepIndex > 0;

            if (currentStepIndex < steps.Length)
            {
                var nextStep = steps[currentStepIndex.Value];
                stack.Push((Screen)Activator.CreateInstance(nextStep.ScreenType));
            }
            else
            {
                Hide();
            }

            NextButton.Text = currentStepIndex + 1 < steps.Length
                ? $"Next ({steps[currentStepIndex.Value + 1].Description})"
                : "Finish";
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
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

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private ScreenStack? stack;

        public PurpleTriangleButton NextButton = null!;
        public DangerousTriangleButton BackButton = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly Bindable<bool> showFirstRunSetup = new Bindable<bool>();

        private int? currentStepIndex;

        private const float scale_when_hidden = 0.9f;

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public FirstRunSetupScreen? CurrentScreen => (FirstRunSetupScreen?)stack?.CurrentScreen;

        private readonly FirstRunStep[] steps =
        {
            new FirstRunStep(typeof(ScreenWelcome), FirstRunSetupOverlayStrings.Welcome),
            new FirstRunStep(typeof(ScreenUIScale), GraphicsSettingsStrings.UIScaling),
        };

        private Container stackContainer = null!;

        private Bindable<OverlayActivation>? overlayActivationMode;

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
                                                Text = FirstRunSetupOverlayStrings.FirstRunSetup,
                                                Font = OsuFont.Default.With(size: 32),
                                                Colour = colourProvider.Content1,
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                            },
                                            new OsuTextFlowContainer
                                            {
                                                Text = FirstRunSetupOverlayStrings.SetupOsuToSuitYou,
                                                Colour = colourProvider.Content2,
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
                                Padding = new MarginPadding(20)
                                {
                                    Top = 0 // provided by the stack container above.
                                },
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
                                                Text = CommonStrings.Back,
                                                Action = showPreviousStep,
                                                Enabled = { Value = false },
                                            },
                                            Empty(),
                                            NextButton = new PurpleTriangleButton
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Width = 1,
                                                Text = FirstRunSetupOverlayStrings.GetStarted,
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

            config.BindWith(OsuSetting.ShowFirstRunSetup, showFirstRunSetup);

            if (showFirstRunSetup.Value) Show();
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!e.Repeat)
            {
                switch (e.Action)
                {
                    case GlobalAction.Select:
                        NextButton.TriggerClick();
                        return true;

                    case GlobalAction.Back:
                        if (BackButton.Enabled.Value)
                        {
                            BackButton.TriggerClick();
                            return true;
                        }

                        // If back button is disabled, we are at the first step.
                        // The base call will handle dismissal of the overlay.
                        break;
                }
            }

            return base.OnPressed(e);
        }

        public override void Show()
        {
            // if we are valid for display, only do so after reaching the main menu.
            performer.PerformFromScreen(screen =>
            {
                MainMenu menu = (MainMenu)screen;

                // Eventually I'd like to replace this with a better method that doesn't access the screen.
                // Either this dialog would be converted to its own screen, or at very least be "hosted" by a screen pushed to the main menu.
                // Alternatively, another method of disabling notifications could be added to `INotificationOverlay`.
                if (menu != null)
                {
                    overlayActivationMode = menu.OverlayActivationMode.GetBoundCopy();
                    overlayActivationMode.Value = OverlayActivation.UserTriggered;
                }

                base.Show();
            }, new[] { typeof(MainMenu) });
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.ScaleTo(scale_when_hidden)
                .ScaleTo(1, 400, Easing.OutElasticHalf);

            this.FadeIn(400, Easing.OutQuint);

            if (currentStepIndex == null)
                showFirstStep();
        }

        protected override void PopOut()
        {
            if (overlayActivationMode != null)
            {
                // If this is non-null we are guaranteed to have come from the main menu.
                overlayActivationMode.Value = OverlayActivation.All;
                overlayActivationMode = null;
            }

            if (currentStepIndex != null)
            {
                notificationOverlay.Post(new SimpleNotification
                {
                    Text = FirstRunSetupOverlayStrings.ClickToResumeFirstRunSetupAtAnyPoint,
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
                stack?.FadeOut(100)
                     .Expire();
            }

            base.PopOut();

            this.ScaleTo(0.96f, 400, Easing.OutQuint);
            this.FadeOut(200, Easing.OutQuint);
        }

        private void showFirstStep()
        {
            Debug.Assert(currentStepIndex == null);

            stackContainer.Child = stack = new ScreenStack
            {
                RelativeSizeAxes = Axes.Both,
            };

            currentStepIndex = -1;
            showNextStep();
        }

        private void showPreviousStep()
        {
            if (currentStepIndex == 0)
                return;

            Debug.Assert(stack != null);

            stack.CurrentScreen.Exit();
            currentStepIndex--;

            updateButtonText();
        }

        private void showNextStep()
        {
            Debug.Assert(currentStepIndex != null);
            Debug.Assert(stack != null);

            currentStepIndex++;

            if (currentStepIndex < steps.Length)
            {
                stack.Push((Screen)Activator.CreateInstance(steps[currentStepIndex.Value].ScreenType));
                updateButtonText();
            }
            else
            {
                showFirstRunSetup.Value = false;
                currentStepIndex = null;
                Hide();
            }
        }

        private void updateButtonText()
        {
            Debug.Assert(currentStepIndex != null);

            BackButton.Enabled.Value = currentStepIndex != 0;

            NextButton.Text = currentStepIndex + 1 < steps.Length
                ? FirstRunSetupOverlayStrings.Next(steps[currentStepIndex.Value + 1].Description)
                : CommonStrings.Finish;
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

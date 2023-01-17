// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game.Overlays
{
    [Cached]
    public partial class FirstRunSetupOverlay : ShearedOverlayContainer
    {
        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private ScreenStack? stack;

        public ShearedButton NextButton = null!;
        public ShearedButton BackButton = null!;

        private readonly Bindable<bool> showFirstRunSetup = new Bindable<bool>();

        private int? currentStepIndex;

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public FirstRunSetupScreen? CurrentScreen => (FirstRunSetupScreen?)stack?.CurrentScreen;

        private readonly List<Type> steps = new List<Type>();

        private Container screenContent = null!;

        private Container content = null!;

        private LoadingSpinner loading = null!;
        private ScheduledDelegate? loadingShowDelegate;

        public FirstRunSetupOverlay()
            : base(OverlayColourScheme.Purple)
        {
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, LegacyImportManager? legacyImportManager)
        {
            steps.Add(typeof(ScreenWelcome));
            steps.Add(typeof(ScreenUIScale));
            steps.Add(typeof(ScreenBeatmaps));
            if (legacyImportManager?.SupportsImportFromStable == true)
                steps.Add(typeof(ScreenImportFromStable));
            steps.Add(typeof(ScreenBehaviour));

            Header.Title = FirstRunSetupOverlayStrings.FirstRunSetupTitle;
            Header.Description = FirstRunSetupOverlayStrings.FirstRunSetupDescription;

            MainAreaContent.AddRange(new Drawable[]
            {
                content = new PopoverContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = 20, },
                    Child = new GridContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(minSize: 640, maxSize: 800),
                            new Dimension(),
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                Empty(),
                                new InputBlockingContainer
                                {
                                    Masking = true,
                                    CornerRadius = 14,
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = ColourProvider.Background6,
                                        },
                                        loading = new LoadingSpinner(),
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Vertical = 20 },
                                            Child = screenContent = new Container { RelativeSizeAxes = Axes.Both, },
                                        },
                                    },
                                },
                                Empty(),
                            },
                        }
                    }
                },
            });

            FooterContent.Add(new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Vertical = PADDING },
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 10),
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new[]
                {
                    new[]
                    {
                        Empty(),
                        BackButton = new ShearedButton(300)
                        {
                            Text = CommonStrings.Back,
                            Action = showPreviousStep,
                            Enabled = { Value = false },
                            DarkerColour = colours.Pink2,
                            LighterColour = colours.Pink1,
                        },
                        NextButton = new ShearedButton(0)
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 1,
                            Text = FirstRunSetupOverlayStrings.GetStarted,
                            DarkerColour = ColourProvider.Colour2,
                            LighterColour = ColourProvider.Colour1,
                            Action = showNextStep
                        },
                        Empty(),
                    },
                }
            });
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
                // Hides the toolbar for us.
                if (screen is MainMenu menu)
                    menu.ReturnToOsuLogo();

                base.Show();
            }, new[] { typeof(MainMenu) });
        }

        protected override void PopIn()
        {
            base.PopIn();

            content.ScaleTo(0.99f)
                   .ScaleTo(1, 400, Easing.OutQuint);

            if (currentStepIndex == null)
                showFirstStep();
        }

        protected override void PopOut()
        {
            base.PopOut();

            content.ScaleTo(0.99f, 400, Easing.OutQuint);

            if (currentStepIndex != null)
            {
                notificationOverlay.Post(new SimpleNotification
                {
                    Text = FirstRunSetupOverlayStrings.ClickToResumeFirstRunSetupAtAnyPoint,
                    Icon = FontAwesome.Solid.Redo,
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
        }

        private void showFirstStep()
        {
            Debug.Assert(currentStepIndex == null);

            screenContent.Child = stack = new ScreenStack
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

            updateButtons();
        }

        private void showNextStep()
        {
            Debug.Assert(currentStepIndex != null);
            Debug.Assert(stack != null);

            currentStepIndex++;

            if (currentStepIndex < steps.Count)
            {
                var nextScreen = (Screen)Activator.CreateInstance(steps[currentStepIndex.Value])!;

                loadingShowDelegate = Scheduler.AddDelayed(() => loading.Show(), 200);
                nextScreen.OnLoadComplete += _ =>
                {
                    loadingShowDelegate?.Cancel();
                    loading.Hide();
                };

                stack.Push(nextScreen);
            }
            else
            {
                showFirstRunSetup.Value = false;
                currentStepIndex = null;
                Hide();
            }

            updateButtons();
        }

        private void updateButtons()
        {
            BackButton.Enabled.Value = currentStepIndex > 0;
            NextButton.Enabled.Value = currentStepIndex != null;

            if (currentStepIndex == null)
                return;

            bool isFirstStep = currentStepIndex == 0;
            bool isLastStep = currentStepIndex == steps.Count - 1;

            if (isFirstStep)
            {
                BackButton.Text = CommonStrings.Back;
                NextButton.Text = FirstRunSetupOverlayStrings.GetStarted;
            }
            else
            {
                BackButton.Text = LocalisableString.Interpolate($@"{CommonStrings.Back} ({steps[currentStepIndex.Value - 1].GetLocalisableDescription()})");

                NextButton.Text = isLastStep
                    ? CommonStrings.Finish
                    : LocalisableString.Interpolate($@"{CommonStrings.Next} ({steps[currentStepIndex.Value + 1].GetLocalisableDescription()})");
            }
        }
    }
}

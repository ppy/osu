// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Overlays
{
    [Cached]
    public class FirstRunSetupOverlay : ShearedOverlayContainer
    {
        protected override OverlayColourScheme ColourScheme => OverlayColourScheme.Purple;

        [Resolved]
        private IPerformFromScreenRunner performer { get; set; } = null!;

        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private ScreenStack? stack;

        public PurpleTriangleButton NextButton = null!;
        public DangerousTriangleButton BackButton = null!;

        private readonly Bindable<bool> showFirstRunSetup = new Bindable<bool>();

        private int? currentStepIndex;

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public FirstRunSetupScreen? CurrentScreen => (FirstRunSetupScreen?)stack?.CurrentScreen;

        private readonly FirstRunStep[] steps =
        {
            new FirstRunStep(typeof(ScreenWelcome), FirstRunSetupOverlayStrings.WelcomeTitle),
            new FirstRunStep(typeof(ScreenUIScale), GraphicsSettingsStrings.UIScaling),
        };

        private Container stackContainer = null!;

        private Bindable<OverlayActivation>? overlayActivationMode;

        private Container content = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Header.Title = FirstRunSetupOverlayStrings.FirstRunSetupTitle;
            Header.Description = FirstRunSetupOverlayStrings.FirstRunSetupDescription;

            MainAreaContent.AddRange(new Drawable[]
            {
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 50 },
                    Child = new InputBlockingContainer
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
                            stackContainer = new Container
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Vertical = 20,
                                    Horizontal = 20,
                                },
                            }
                        },
                    },
                },
            });

            FooterContent.Add(new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.98f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
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
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.ShowFirstRunSetup, showFirstRunSetup);

            // TODO: uncomment when happy with the whole flow.
            // if (showFirstRunSetup.Value) Show();
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

            content.ScaleTo(0.99f)
                   .ScaleTo(1, 400, Easing.OutQuint);

            if (currentStepIndex == null)
                showFirstStep();
        }

        protected override void PopOut()
        {
            base.PopOut();

            content.ScaleTo(0.99f, 400, Easing.OutQuint);

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

            updateButtons();
        }

        private void showNextStep()
        {
            Debug.Assert(currentStepIndex != null);
            Debug.Assert(stack != null);

            currentStepIndex++;

            if (currentStepIndex < steps.Length)
            {
                stack.Push((Screen)Activator.CreateInstance(steps[currentStepIndex.Value].ScreenType));
            }
            else
            {
                // TODO: uncomment when happy with the whole flow.
                // showFirstRunSetup.Value = false;
                currentStepIndex = null;
                Hide();
            }

            updateButtons();
        }

        private void updateButtons()
        {
            BackButton.Enabled.Value = currentStepIndex > 0;
            NextButton.Enabled.Value = currentStepIndex != null;

            if (currentStepIndex != null)
            {
                NextButton.Text = currentStepIndex + 1 < steps.Length
                    ? FirstRunSetupOverlayStrings.Next(steps[currentStepIndex.Value + 1].Description)
                    : CommonStrings.Finish;
            }
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

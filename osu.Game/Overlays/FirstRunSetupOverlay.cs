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
using osu.Game.Screens.Footer;
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

        public ShearedButton? NextButton => DisplayedFooterContent?.NextButton;

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
                    Padding = new MarginPadding { Bottom = 20 },
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.ShowFirstRunSetup, showFirstRunSetup);

            if (showFirstRunSetup.Value) Show();
        }

        [Resolved]
        private ScreenFooter footer { get; set; } = null!;

        public new FirstRunSetupFooterContent? DisplayedFooterContent => base.DisplayedFooterContent as FirstRunSetupFooterContent;

        public override VisibilityContainer CreateFooterContent()
        {
            var footerContent = new FirstRunSetupFooterContent
            {
                ShowNextStep = showNextStep,
            };

            footerContent.OnLoadComplete += _ => updateButtons();
            return footerContent;
        }

        public override bool OnBackButton()
        {
            if (currentStepIndex == 0)
                return false;

            Debug.Assert(stack != null);

            stack.CurrentScreen.Exit();
            currentStepIndex--;

            updateButtons();
            return true;
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!e.Repeat)
            {
                switch (e.Action)
                {
                    case GlobalAction.Select:
                        DisplayedFooterContent?.NextButton.TriggerClick();
                        return true;

                    case GlobalAction.Back:
                        footer.BackButton.TriggerClick();
                        return false;
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

        private void updateButtons() => DisplayedFooterContent?.UpdateButtons(currentStepIndex, steps);

        public partial class FirstRunSetupFooterContent : VisibilityContainer
        {
            public ShearedButton NextButton { get; private set; } = null!;

            public Action? ShowNextStep;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = NextButton = new ShearedButton(0)
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding { Right = 12f },
                    RelativeSizeAxes = Axes.X,
                    Width = 1,
                    Text = FirstRunSetupOverlayStrings.GetStarted,
                    DarkerColour = colourProvider.Colour2,
                    LighterColour = colourProvider.Colour1,
                    Action = () => ShowNextStep?.Invoke(),
                };
            }

            public void UpdateButtons(int? currentStep, IReadOnlyList<Type> steps)
            {
                NextButton.Enabled.Value = currentStep != null;

                if (currentStep == null)
                    return;

                bool isFirstStep = currentStep == 0;
                bool isLastStep = currentStep == steps.Count - 1;

                if (isFirstStep)
                    NextButton.Text = FirstRunSetupOverlayStrings.GetStarted;
                else
                {
                    NextButton.Text = isLastStep
                        ? CommonStrings.Finish
                        : LocalisableString.Interpolate($@"{CommonStrings.Next} ({steps[currentStep.Value + 1].GetLocalisableDescription()})");
                }
            }

            protected override void PopIn()
            {
                this.FadeIn();
            }

            protected override void PopOut()
            {
                this.Delay(400).FadeOut();
            }
        }
    }
}

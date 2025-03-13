// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;

namespace osu.Game.Overlays
{
    public partial class WizardOverlay : ShearedOverlayContainer
    {
        private ScreenStack? stack;

        public ShearedButton? NextButton => DisplayedFooterContent?.NextButton;

        protected int? CurrentStepIndex { get; private set; }

        /// <summary>
        /// The currently displayed screen, if any.
        /// </summary>
        public WizardScreen? CurrentScreen => (WizardScreen?)stack?.CurrentScreen;

        private readonly List<Type> steps = new List<Type>();

        private Container screenContent = null!;

        private Container content = null!;

        private LoadingSpinner loading = null!;
        private ScheduledDelegate? loadingShowDelegate;

        public bool Completed { get; private set; }

        protected WizardOverlay(OverlayColourScheme scheme)
            : base(scheme)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

        [Resolved]
        private ScreenFooter footer { get; set; } = null!;

        public new WizardFooterContent? DisplayedFooterContent => base.DisplayedFooterContent as WizardFooterContent;

        public override VisibilityContainer CreateFooterContent()
        {
            var footerContent = new WizardFooterContent
            {
                ShowNextStep = ShowNextStep,
            };

            footerContent.OnLoadComplete += _ => updateButtons();
            return footerContent;
        }

        public override bool OnBackButton()
        {
            if (CurrentStepIndex == 0)
                return false;

            Debug.Assert(stack != null);

            stack.CurrentScreen.Exit();
            CurrentStepIndex--;

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

        protected override void PopIn()
        {
            base.PopIn();

            content.ScaleTo(0.99f)
                   .ScaleTo(1, 400, Easing.OutQuint);

            if (CurrentStepIndex == null)
                showFirstStep();
        }

        protected override void PopOut()
        {
            base.PopOut();

            content.ScaleTo(0.99f, 400, Easing.OutQuint);

            if (CurrentStepIndex == null)
            {
                stack?.FadeOut(100)
                     .Expire();
            }
        }

        protected void AddStep<T>()
            where T : WizardScreen
        {
            steps.Add(typeof(T));
        }

        private void showFirstStep()
        {
            Debug.Assert(CurrentStepIndex == null);

            screenContent.Child = stack = new ScreenStack
            {
                RelativeSizeAxes = Axes.Both,
            };

            CurrentStepIndex = -1;
            ShowNextStep();
        }

        protected virtual void ShowNextStep()
        {
            Debug.Assert(CurrentStepIndex != null);
            Debug.Assert(stack != null);

            CurrentStepIndex++;

            if (CurrentStepIndex < steps.Count)
            {
                var nextScreen = (Screen)Activator.CreateInstance(steps[CurrentStepIndex.Value])!;

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
                CurrentStepIndex = null;
                Completed = true;
                Hide();
            }

            updateButtons();
        }

        private void updateButtons() => DisplayedFooterContent?.UpdateButtons(CurrentStepIndex, CurrentScreen, steps);

        public partial class WizardFooterContent : VisibilityContainer
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
                    DarkerColour = colourProvider.Colour3,
                    LighterColour = colourProvider.Colour2,
                    Action = () => ShowNextStep?.Invoke(),
                };
            }

            public void UpdateButtons(int? currentStep, WizardScreen? currentScreen, IReadOnlyList<Type> steps)
            {
                NextButton.Enabled.Value = currentStep != null;

                if (currentStep == null)
                    return;

                bool isLastStep = currentStep == steps.Count - 1;

                if (currentScreen?.NextStepText != null)
                    NextButton.Text = currentScreen.NextStepText.Value;
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

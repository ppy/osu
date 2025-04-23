// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Screens.Footer
{
    public partial class ScreenFooter : OverlayContainer
    {
        private const int padding = 60;
        private const float delay_per_button = 30;
        private const double transition_duration = 400;

        public const int HEIGHT = 50;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        private Box background = null!;
        private FillFlowContainer<ScreenFooterButton> buttonsFlow = null!;
        private Container footerContentContainer = null!;
        private Container<ScreenFooterButton> hiddenButtonsContainer = null!;
        private LogoTrackingContainer logoTrackingContainer = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Resolved]
        private OsuGame? game { get; set; }

        public ScreenBackButton BackButton { get; private set; } = null!;

        public Action<bool>? RequestLogoInFront { get; set; }

        public Action? OnBack;

        public ScreenFooter(BackReceptor? receptor = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            if (receptor == null)
                Add(receptor = new BackReceptor());

            receptor.OnBackPressed = () => BackButton.TriggerClick();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 12f + ScreenBackButton.BUTTON_WIDTH + padding },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            buttonsFlow = new FillFlowContainer<ScreenFooterButton>
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Y = 10f,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(7, 0),
                                AutoSizeAxes = Axes.Both,
                            },
                            footerContentContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Y = -15f,
                            },
                        },
                    }
                },
                BackButton = new ScreenBackButton
                {
                    Margin = new MarginPadding { Bottom = OsuGame.SCREEN_EDGE_MARGIN, Left = OsuGame.SCREEN_EDGE_MARGIN },
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = onBackPressed,
                },
                hiddenButtonsContainer = new Container<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = OsuGame.SCREEN_EDGE_MARGIN + ScreenBackButton.BUTTON_WIDTH + padding },
                    Y = 10f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    AutoSizeAxes = Axes.Both,
                },
                (logoTrackingContainer = new LogoTrackingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                }).WithChild(logoTrackingContainer.LogoFacade.With(f =>
                {
                    f.Anchor = Anchor.BottomRight;
                    f.Origin = Anchor.Centre;
                    f.Position = new Vector2(-76, -36);
                })),
            };
        }

        private ScheduledDelegate? changeLogoDepthDelegate;

        public void StartTrackingLogo(OsuLogo logo, float duration = 0, Easing easing = Easing.None)
        {
            changeLogoDepthDelegate?.Cancel();
            changeLogoDepthDelegate = null;

            logoTrackingContainer.StartTracking(logo, duration, easing);
            RequestLogoInFront?.Invoke(true);
        }

        public void StopTrackingLogo()
        {
            logoTrackingContainer.StopTracking();

            if (game != null)
                changeLogoDepthDelegate = Scheduler.AddDelayed(() => RequestLogoInFront?.Invoke(false), transition_duration);
        }

        protected override void PopIn()
        {
            this.MoveToY(0, transition_duration, Easing.OutQuint)
                .FadeIn(transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.MoveToY(HEIGHT, transition_duration, Easing.OutQuint)
                .FadeOut(transition_duration, Easing.OutQuint);
        }

        public void SetButtons(IReadOnlyList<ScreenFooterButton> buttons)
        {
            temporarilyHiddenButtons.Clear();
            overlays.Clear();

            clearActiveOverlayContainer();

            var oldButtons = buttonsFlow.ToArray();

            for (int i = 0; i < oldButtons.Length; i++)
            {
                var oldButton = oldButtons[i];

                buttonsFlow.Remove(oldButton, false);
                hiddenButtonsContainer.Add(oldButton);

                if (buttons.Count > 0)
                    makeButtonDisappearToRight(oldButton, i, oldButtons.Length, true);
                else
                    makeButtonDisappearToBottom(oldButton, i, oldButtons.Length, true);
            }

            for (int i = 0; i < buttons.Count; i++)
            {
                var newButton = buttons[i];

                if (newButton.Overlay != null)
                {
                    newButton.Action = () => showOverlay(newButton.Overlay);
                    overlays.Add(newButton.Overlay);
                }

                Debug.Assert(!newButton.IsLoaded);
                buttonsFlow.Add(newButton);

                int index = i;

                // ensure transforms are added after LoadComplete to not be aborted by the FinishTransforms call.
                newButton.OnLoadComplete += _ =>
                {
                    if (oldButtons.Length > 0)
                        makeButtonAppearFromLeft(newButton, index, buttons.Count, 240);
                    else
                        makeButtonAppearFromBottom(newButton, index);
                };
            }
        }

        private ShearedOverlayContainer? activeOverlay;
        private VisibilityContainer? activeFooterContent;

        private readonly List<ScreenFooterButton> temporarilyHiddenButtons = new List<ScreenFooterButton>();

        public IDisposable RegisterActiveOverlayContainer(ShearedOverlayContainer overlay, out VisibilityContainer? footerContent)
        {
            if (activeOverlay != null)
            {
                throw new InvalidOperationException(@"Cannot set overlay content while one is already present. " +
                                                    $@"The previous overlay ({activeOverlay.GetType().Name}) should be hidden first.");
            }

            activeOverlay = overlay;

            Debug.Assert(temporarilyHiddenButtons.Count == 0);

            var targetButton = buttonsFlow.SingleOrDefault(b => b.Overlay == overlay);

            temporarilyHiddenButtons.AddRange(targetButton != null
                ? buttonsFlow.SkipWhile(b => b != targetButton).Skip(1)
                : buttonsFlow);

            for (int i = temporarilyHiddenButtons.Count - 1; i >= 0; i--)
            {
                var button = temporarilyHiddenButtons[i];
                buttonsFlow.Remove(button, false);
                hiddenButtonsContainer.Add(button);

                makeButtonDisappearToBottom(button, 0, 0, false);
            }

            updateColourScheme(overlay.ColourProvider.Hue);

            footerContent = overlay.CreateFooterContent();
            activeFooterContent = footerContent;
            var content = footerContent;

            if (content != null)
                footerContentContainer.Child = content;

            if (temporarilyHiddenButtons.Count > 0)
                this.Delay(60).Schedule(() => content?.Show());
            else
                content?.Show();

            return new InvokeOnDisposal(clearActiveOverlayContainer);
        }

        private void clearActiveOverlayContainer()
        {
            if (activeOverlay == null)
                return;

            Debug.Assert(activeFooterContent != null);
            activeFooterContent.Hide();

            double timeUntilRun = activeFooterContent.LatestTransformEndTime - Time.Current;

            for (int i = 0; i < temporarilyHiddenButtons.Count; i++)
            {
                var button = temporarilyHiddenButtons[i];
                hiddenButtonsContainer.Remove(button, false);
                buttonsFlow.Add(button);

                makeButtonAppearFromBottom(button, 0);
            }

            temporarilyHiddenButtons.Clear();

            updateColourScheme(OverlayColourScheme.Aquamarine.GetHue());

            activeFooterContent.Delay(timeUntilRun).Expire();
            activeFooterContent = null;
            activeOverlay = null;
        }

        private void updateColourScheme(int hue)
        {
            colourProvider.ChangeColourScheme(hue);

            background.FadeColour(colourProvider.Background5, 150, Easing.OutQuint);

            foreach (var button in buttonsFlow)
                button.UpdateDisplay();
        }

        private void makeButtonAppearFromLeft(ScreenFooterButton button, int index, int count, float startDelay)
            => button.AppearFromLeft(startDelay + (count - index) * delay_per_button);

        private void makeButtonAppearFromBottom(ScreenFooterButton button, int index)
            => button.AppearFromBottom(index * delay_per_button);

        private void makeButtonDisappearToRight(ScreenFooterButton button, int index, int count, bool expire)
            => button.DisappearToRight((count - index) * delay_per_button, expire);

        private void makeButtonDisappearToBottom(ScreenFooterButton button, int index, int count, bool expire)
            => button.DisappearToBottom((count - index) * delay_per_button, expire);

        private void showOverlay(OverlayContainer overlay)
        {
            foreach (var o in overlays.Where(o => o != overlay))
                o.Hide();

            overlay.ToggleVisibility();
        }

        private void onBackPressed()
        {
            if (activeOverlay != null)
            {
                if (activeOverlay.OnBackButton())
                    return;

                activeOverlay.Hide();
                return;
            }

            OnBack?.Invoke();
        }

        public partial class BackReceptor : Drawable, IKeyBindingHandler<GlobalAction>
        {
            public Action? OnBackPressed;

            public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
            {
                if (e.Repeat)
                    return false;

                switch (e.Action)
                {
                    case GlobalAction.Back:
                        OnBackPressed?.Invoke();
                        return true;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
            {
            }
        }
    }
}

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
        private FillFlowContainer<ScreenFooterButton> leftButtonsFlow = null!;
        private Container<ScreenFooterButton> removedLeftButtonsContainer = null!;
        private LogoTrackingContainer logoTrackingContainer = null!;

        // TODO: this should take the screen's colourProvider instead. hardcode plum for now as daily challenge is the only usage shown to users
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

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
                leftButtonsFlow = new FillFlowContainer<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = 12f + ScreenBackButton.BUTTON_WIDTH + padding },
                    Y = 10f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7, 0),
                    AutoSizeAxes = Axes.Both
                },
                BackButton = new ScreenBackButton
                {
                    Margin = new MarginPadding { Bottom = 15f, Left = 12f },
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = onBackPressed,
                },
                removedLeftButtonsContainer = new Container<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = 12f + ScreenBackButton.BUTTON_WIDTH + padding },
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

        public void SetLeftButtons(IReadOnlyList<ScreenFooterButton> buttons)
        {
            temporarilyHiddenLeftButtons.Clear();
            overlays.Clear();

            clearActiveOverlayContainer();

            var oldButtons = leftButtonsFlow.ToArray();

            for (int i = 0; i < oldButtons.Length; i++)
            {
                var oldButton = oldButtons[i];

                leftButtonsFlow.Remove(oldButton, false);
                removedLeftButtonsContainer.Add(oldButton);

                if (buttons.Count > 0)
                    makeLeftButtonDisappearToRight(oldButton, i, oldButtons.Length, true);
                else
                    makeLeftButtonDisappearToBottom(oldButton, i, oldButtons.Length, true);
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
                leftButtonsFlow.Add(newButton);

                int index = i;

                // ensure transforms are added after LoadComplete to not be aborted by the FinishTransforms call.
                newButton.OnLoadComplete += _ =>
                {
                    if (oldButtons.Length > 0)
                        makeLeftButtonAppearFromLeft(newButton, index, buttons.Count, 240);
                    else
                        makeLeftButtonAppearFromBottom(newButton, index);
                };
            }
        }

        private ShearedOverlayContainer? activeOverlay;
        private Container? contentContainer;

        private readonly List<ScreenFooterButton> temporarilyHiddenLeftButtons = new List<ScreenFooterButton>();

        public IDisposable RegisterActiveOverlayContainer(ShearedOverlayContainer overlay, out VisibilityContainer? footerContent)
        {
            if (activeOverlay != null)
            {
                throw new InvalidOperationException(@"Cannot set overlay content while one is already present. " +
                                                    $@"The previous overlay ({activeOverlay.GetType().Name}) should be hidden first.");
            }

            activeOverlay = overlay;

            Debug.Assert(temporarilyHiddenLeftButtons.Count == 0);

            var targetButton = leftButtonsFlow.SingleOrDefault(b => b.Overlay == overlay);

            temporarilyHiddenLeftButtons.AddRange(targetButton != null
                ? leftButtonsFlow.SkipWhile(b => b != targetButton).Skip(1)
                : leftButtonsFlow);

            for (int i = 0; i < temporarilyHiddenLeftButtons.Count; i++)
                makeLeftButtonDisappearToBottom(temporarilyHiddenLeftButtons[i], 0, 0, false);

            var fallbackPosition = leftButtonsFlow.Any()
                ? leftButtonsFlow.ToSpaceOfOtherDrawable(Vector2.Zero, this)
                : BackButton.ToSpaceOfOtherDrawable(BackButton.LayoutRectangle.TopRight + new Vector2(5f, 0f), this);

            var targetPosition = targetButton?.ToSpaceOfOtherDrawable(targetButton.LayoutRectangle.TopRight, this) ?? fallbackPosition;

            updateColourScheme(overlay.ColourProvider.Hue);

            footerContent = overlay.CreateFooterContent();

            var content = footerContent ?? Empty();

            Add(contentContainer = new Container
            {
                Y = -15f,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Left = targetPosition.X },
                Child = content,
            });

            if (temporarilyHiddenLeftButtons.Count > 0)
                this.Delay(60).Schedule(() => content.Show());
            else
                content.Show();

            return new InvokeOnDisposal(clearActiveOverlayContainer);
        }

        private void clearActiveOverlayContainer()
        {
            if (activeOverlay == null)
                return;

            Debug.Assert(contentContainer != null);
            contentContainer.Child.Hide();

            double timeUntilRun = contentContainer.Child.LatestTransformEndTime - Time.Current;

            for (int i = 0; i < temporarilyHiddenLeftButtons.Count; i++)
                makeLeftButtonAppearFromBottom(temporarilyHiddenLeftButtons[i], 0);

            temporarilyHiddenLeftButtons.Clear();

            updateColourScheme(OverlayColourScheme.Plum.GetHue());

            contentContainer.Delay(timeUntilRun).Expire();
            contentContainer = null;
            activeOverlay = null;
        }

        private void updateColourScheme(int hue)
        {
            colourProvider.ChangeColourScheme(hue);

            background.FadeColour(colourProvider.Background5, 150, Easing.OutQuint);

            foreach (var button in leftButtonsFlow)
                button.UpdateDisplay();
        }

        private void makeLeftButtonAppearFromLeft(ScreenFooterButton button, int index, int count, float startDelay)
            => button.AppearFromLeft(startDelay + (count - index) * delay_per_button);

        private void makeLeftButtonAppearFromBottom(ScreenFooterButton button, int index)
            => button.AppearFromBottom(index * delay_per_button);

        private void makeLeftButtonDisappearToRight(ScreenFooterButton button, int index, int count, bool expire)
            => button.DisappearToRight((count - index) * delay_per_button, expire);

        private void makeLeftButtonDisappearToBottom(ScreenFooterButton button, int index, int count, bool expire)
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

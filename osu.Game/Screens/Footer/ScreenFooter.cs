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
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2.Footer;
using osuTK;

namespace osu.Game.Screens.Footer
{
    public partial class ScreenFooter : VisibilityContainer
    {
        private const int padding = 60;
        private const float delay_per_button = 30;

        public const int HEIGHT = 60;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        public Action? OnBack;

        // todo: this assumes all screens with a footer will use aquamarine colour scheme. this might not be the case when we show the footer in other screens than song select.
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public void SetButtons(IReadOnlyList<ScreenFooterButton> buttons)
        {
            overlays.Clear();

            var oldButtons = buttonsFlow.ToArray();

            for (int i = 0; i < oldButtons.Length; i++)
            {
                var oldButton = oldButtons[i];

                buttonsFlow.Remove(oldButton, false);
                removedButtonsContainer.Add(oldButton);

                if (buttons.Count > 0)
                    fadeButtonToLeft(oldButton, i, oldButtons.Length);
                else
                    fadeButtonToBottom(oldButton, i, oldButtons.Length);

                Scheduler.AddDelayed(() => oldButton.Expire(), oldButton.TopLevelContent.LatestTransformEndTime - Time.Current);
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
                        fadeButtonFromRight(newButton, index, buttons.Count, 240);
                    else
                        fadeButtonFromBottom(newButton, index);
                };
            }
        }

        private void fadeButtonFromRight(ScreenFooterButton button, int index, int count, float startDelay)
        {
            button.TopLevelContent
                  .MoveToX(-300f)
                  .FadeOut();

            button.TopLevelContent
                  .Delay(startDelay + (count - index) * delay_per_button)
                  .MoveToX(0f, 240, Easing.OutCubic)
                  .FadeIn(240, Easing.OutCubic);
        }

        private void fadeButtonFromBottom(ScreenFooterButton button, int index)
        {
            button.TopLevelContent
                  .MoveToY(100f)
                  .FadeOut();

            button.TopLevelContent
                  .Delay(index * delay_per_button)
                  .MoveToY(0f, 240, Easing.OutCubic)
                  .FadeIn(240, Easing.OutCubic);
        }

        private void fadeButtonToLeft(ScreenFooterButton button, int index, int count)
        {
            button.TopLevelContent
                  .Delay((count - index) * delay_per_button)
                  .FadeOut(240, Easing.InOutCubic)
                  .MoveToX(300f, 360, Easing.InOutCubic);
        }

        private void fadeButtonToBottom(ScreenFooterButton button, int index, int count)
        {
            button.TopLevelContent
                  .Delay((count - index) * delay_per_button)
                  .FadeOut(240, Easing.InOutCubic)
                  .MoveToY(100f, 240, Easing.InOutCubic);
        }

        private void showOverlay(OverlayContainer overlay)
        {
            foreach (var o in overlays)
            {
                if (o == overlay)
                    o.ToggleVisibility();
                else
                    o.Hide();
            }
        }

        private BackButtonV2 backButton = null!;
        private FillFlowContainer<ScreenFooterButton> buttonsFlow = null!;
        private Container<ScreenFooterButton> removedButtonsContainer = null!;
        private LogoTrackingContainer logoTrackingContainer = null!;

        public ScreenFooter(BackReceptor? receptor = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            if (receptor == null)
                Add(receptor = new BackReceptor());

            receptor.OnBackPressed = () => backButton.TriggerClick();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                buttonsFlow = new FillFlowContainer<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = 12f + BackButtonV2.BUTTON_WIDTH + padding },
                    Y = 10f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7, 0),
                    AutoSizeAxes = Axes.Both
                },
                backButton = new BackButtonV2
                {
                    Margin = new MarginPadding { Bottom = 10f, Left = 12f },
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = () => OnBack?.Invoke(),
                },
                removedButtonsContainer = new Container<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = 12f + BackButtonV2.BUTTON_WIDTH + padding },
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

        public void StartTrackingLogo(OsuLogo logo, float duration = 0, Easing easing = Easing.None) => logoTrackingContainer.StartTracking(logo, duration, easing);
        public void StopTrackingLogo() => logoTrackingContainer.StopTracking();

        protected override void PopIn()
        {
            this.MoveToY(0, 400, Easing.OutQuint)
                .FadeIn(400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.MoveToY(HEIGHT, 400, Easing.OutQuint)
                .FadeOut(400, Easing.OutQuint);
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

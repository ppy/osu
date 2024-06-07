// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Footer
{
    public partial class ScreenFooter : OverlayContainer
    {
        private const int padding = 60;
        private const float delay_per_button = 30;

        public const int HEIGHT = 60;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        private FillFlowContainer<ScreenFooterButton> buttonsFlow = null!;
        private Container<ScreenFooterButton> removedButtonsContainer = null!;

        public ScreenFooter()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
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
                    Margin = new MarginPadding { Left = 12f + ScreenBackButton.BUTTON_WIDTH + padding },
                    Y = 10f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7, 0),
                    AutoSizeAxes = Axes.Both
                },
                new ScreenBackButton
                {
                    Margin = new MarginPadding { Bottom = 10f, Left = 12f },
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = () => { },
                },
                removedButtonsContainer = new Container<ScreenFooterButton>
                {
                    Margin = new MarginPadding { Left = 12f + ScreenBackButton.BUTTON_WIDTH + padding },
                    Y = 10f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    AutoSizeAxes = Axes.Both,
                },
            };
        }

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
                    makeButtonDisappearToRightAndExpire(oldButton, i, oldButtons.Length);
                else
                    makeButtonDisappearToBottomAndExpire(oldButton, i, oldButtons.Length);
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

        private void makeButtonAppearFromLeft(ScreenFooterButton button, int index, int count, float startDelay)
            => button.AppearFromLeft(startDelay + (count - index) * delay_per_button);

        private void makeButtonAppearFromBottom(ScreenFooterButton button, int index)
            => button.AppearFromBottom(index * delay_per_button);

        private void makeButtonDisappearToRightAndExpire(ScreenFooterButton button, int index, int count)
            => button.DisappearToRightAndExpire((count - index) * delay_per_button);

        private void makeButtonDisappearToBottomAndExpire(ScreenFooterButton button, int index, int count)
            => button.DisappearToBottomAndExpire((count - index) * delay_per_button);

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
    }
}

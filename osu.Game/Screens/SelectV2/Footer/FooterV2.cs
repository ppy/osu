// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2.Footer
{
    public partial class FooterV2 : VisibilityContainer
    {
        private const int height = 60;
        private const int padding = 60;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        /// <param name="button">The button to be added.</param>
        /// <param name="overlay">The <see cref="OverlayContainer"/> to be toggled by this button.</param>
        public void AddButton(FooterButtonV2 button, OverlayContainer? overlay = null)
        {
            if (overlay != null)
            {
                overlays.Add(overlay);
                button.Action = () => showOverlay(overlay);
                button.OverlayState.BindTo(overlay.State);
            }

            buttons.Add(button);
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

        private Box background = null!;
        private BackButtonV2 backButton = null!;
        private FillFlowContainer<FooterButtonV2> buttons = null!;

        public FooterV2()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                backButton = new BackButtonV2
                {
                    Margin = new MarginPadding { Bottom = 10f, Left = 12f },
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = () => { },
                },
                buttons = new FillFlowContainer<FooterButtonV2>
                {
                    Margin = new MarginPadding { Left = 12f + BackButtonV2.BUTTON_WIDTH + padding },
                    Y = 10f,
                    RelativePositionAxes = Axes.X,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7, 0),
                    AutoSizeAxes = Axes.Both
                },
            };
        }

        private const float off_screen_y = 100;
        private const float buttons_pop_delay = 30;

        protected override void PopIn()
        {
            background.MoveToY(0, 400, Easing.OutQuint);

            ScheduleAfterChildren(() => backButton.MoveToY(0, 400, Easing.OutQuint));

            buttons.Delay(buttons_pop_delay)
                   .MoveToX(0, 400, Easing.OutQuint)
                   .FadeIn(400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            buttons.MoveToX(-0.5f, 400, Easing.OutQuint)
                   .FadeOut(400, Easing.OutQuint);

            background.Delay(buttons_pop_delay).MoveToY(off_screen_y, 400, Easing.OutQuint);
            backButton.Delay(buttons_pop_delay).MoveToY(off_screen_y, 400, Easing.OutQuint);
        }
    }
}

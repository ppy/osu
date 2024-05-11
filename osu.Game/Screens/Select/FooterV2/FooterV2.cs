// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterV2 : VisibilityContainer
    {
        //Should be 60, setting to 50 for now for the sake of matching the current BackButton height.
        private const int height = 50;
        private const int padding = 80;

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
                buttons = new FillFlowContainer<FooterButtonV2>
                {
                    Margin = new MarginPadding { Left = TwoLayerButton.SIZE_EXTENDED.X + padding },
                    Y = 10f,
                    RelativePositionAxes = Axes.X,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(-FooterButtonV2.SHEAR_WIDTH + 7, 0),
                    AutoSizeAxes = Axes.Both
                },
            };
        }

        private const float off_screen_y = 100;
        private const float buttons_pop_delay = 30;

        protected override void PopIn()
        {
            background.MoveToY(0, 400, Easing.OutQuint);

            buttons.Delay(buttons_pop_delay)
                   .MoveToX(0, 400, Easing.OutQuint)
                   .FadeIn(400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            buttons.MoveToX(-0.5f, 400, Easing.OutQuint)
                   .FadeOut(400, Easing.OutQuint);

            background.Delay(buttons_pop_delay).MoveToY(off_screen_y, 400, Easing.OutQuint);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Select
{
    public class Footer : Container
    {
        private readonly Box modeLight;

        public const float HEIGHT = 50;

        public const int TRANSITION_LENGTH = 300;

        private const float padding = 80;

        private readonly FillFlowContainer<FooterButton> buttons;

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        /// <param name="button">THe button to be added.</param>
        /// <param name="overlay">The <see cref="OverlayContainer"/> to be toggled by this button.</param>
        public void AddButton(FooterButton button, OverlayContainer overlay)
        {
            overlays.Add(overlay);
            button.Action = () => showOverlay(overlay);

            AddButton(button);
        }

        /// <param name="button">Button to be added.</param>
        public void AddButton(FooterButton button)
        {
            button.Hovered = updateModeLight;
            button.HoverLost = updateModeLight;

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

        private void updateModeLight() => modeLight.FadeColour(buttons.FirstOrDefault(b => b.IsHovered)?.SelectedColour ?? Color4.Transparent, TRANSITION_LENGTH, Easing.OutQuint);

        public Footer()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                modeLight = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Position = new Vector2(0, -3),
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(TwoLayerButton.SIZE_EXTENDED.X + padding, 0),
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(padding, 0),
                    Children = new Drawable[]
                    {
                        buttons = new FillFlowContainer<FooterButton>
                        {
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(-FooterButton.SHEAR_WIDTH, 0),
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };

            updateModeLight();
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e) => true;
    }
}

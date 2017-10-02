﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens.Select
{
    public class Footer : Container
    {
        private readonly Box modeLight;

        private const float play_song_select_button_width = 100;
        private const float play_song_select_button_height = 50;

        public const float HEIGHT = 50;

        public const int TRANSITION_LENGTH = 300;

        private const float padding = 80;

        public Action OnBack;
        public Action OnStart;

        private readonly FillFlowContainer<FooterButton> buttons;

        public OsuLogo StartButton;

        /// <param name="text">Text on the button.</param>
        /// <param name="colour">Colour of the button.</param>
        /// <param name="hotkey">Hotkey of the button.</param>
        /// <param name="action">Action the button does.</param>
        /// <param name="depth">
        /// <para>Higher depth to be put on the left, and lower to be put on the right.</para>
        /// <para>Notice this is different to <see cref="Options.BeatmapOptionsOverlay"/>!</para>
        /// </param>
        public void AddButton(string text, Color4 colour, Action action, Key? hotkey = null, float depth = 0) => buttons.Add(new FooterButton
        {
            Text = text,
            Height = play_song_select_button_height,
            Width = play_song_select_button_width,
            Depth = depth,
            SelectedColour = colour,
            DeselectedColour = colour.Opacity(0.5f),
            Hotkey = hotkey,
            Hovered = updateModeLight,
            HoverLost = updateModeLight,
            Action = action,
        });

        private readonly List<OverlayContainer> overlays = new List<OverlayContainer>();

        /// <param name="text">Text on the button.</param>
        /// <param name="colour">Colour of the button.</param>
        /// <param name="hotkey">Hotkey of the button.</param>
        /// <param name="overlay">The <see cref="OverlayContainer"/> to be toggled by this button.</param>
        /// <param name="depth">
        /// <para>Higher depth to be put on the left, and lower to be put on the right.</para>
        /// <para>Notice this is different to <see cref="Options.BeatmapOptionsOverlay"/>!</para>
        /// </param>
        public void AddButton(string text, Color4 colour, OverlayContainer overlay, Key? hotkey = null, float depth = 0)
        {
            overlays.Add(overlay);
            AddButton(text, colour, () =>
            {
                foreach (var o in overlays)
                {
                    if (o == overlay)
                        o.ToggleVisibility();
                    else
                        o.Hide();
                }
            }, hotkey, depth);
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
                StartButton = new OsuLogo
                {
                    Anchor = Anchor.BottomRight,
                    Scale = new Vector2(0.4f),
                    Position = new Vector2(-70, -25),
                    Action = () => OnStart?.Invoke()
                },
                new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = () => OnBack?.Invoke()
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
                            Spacing = new Vector2(0.2f, 0),
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };

            updateModeLight();
        }

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => base.ReceiveMouseInputAt(screenSpacePos) || StartButton.ReceiveMouseInputAt(screenSpacePos);

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnClick(InputState state) => true;

        protected override bool OnDragStart(InputState state) => true;
    }
}

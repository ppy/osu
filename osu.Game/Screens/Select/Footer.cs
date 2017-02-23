// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens.Select
{
    public class Footer : Container
    {
        private Box modeLight;

        private const float play_song_select_button_width = 100;
        private const float play_song_select_button_height = 50;

        public const int TRANSITION_LENGTH = 300;

        private const float padding = 80;

        public override bool Contains(Vector2 screenSpacePos) => true;

        public Action OnBack;
        public Action OnStart;

        private FlowContainer buttons;

        public OsuLogo StartButton;

        public void AddButton(string text, Color4 colour, Action action)
        {
            var button = new FooterButton
            {
                Text = text,
                Height = play_song_select_button_height,
                Width = play_song_select_button_width,
                SelectedColour = colour,
                DeselectedColour = colour.Opacity(0.5f),
            };

            button.Hovered = () => updateModeLight(button);
            button.HoverLost = () => updateModeLight();
            button.Action = action;
            buttons.Add(button);
        }

        private void updateModeLight(FooterButton button = null)
        {
            modeLight.FadeColour(button?.SelectedColour ?? Color4.Transparent, TRANSITION_LENGTH, EasingTypes.OutQuint);
        }

        public Footer()
        {
            const float bottom_tool_height = 50;

            RelativeSizeAxes = Axes.X;
            Height = bottom_tool_height;
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
                    Action = () => OnBack?.Invoke(),
                },
                new FlowContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(BackButton.SIZE_EXTENDED.X + padding, 0),
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FlowDirections.Horizontal,
                    Spacing = new Vector2(padding, 0),
                    Children = new Drawable[]
                    {

                        buttons = new FlowContainer
                        {
                            Direction = FlowDirections.Horizontal,
                            Spacing = new Vector2(0.2f, 0),
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };

            updateModeLight();
        }
    }
}

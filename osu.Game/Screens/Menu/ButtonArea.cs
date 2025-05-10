// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class ButtonArea : Container, IStateful<Visibility>
    {
        public FlowContainerWithOrigin Flow;

        protected override Container<Drawable> Content => Flow;

        private readonly ButtonAreaBackground buttonAreaBackground;
        private Visibility state;

        public const float BUTTON_AREA_HEIGHT = 100;

        public ButtonArea()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1, BUTTON_AREA_HEIGHT),
                Alpha = 0,
                AlwaysPresent = true, // Always needs to be present for correct tracking on initial -> toplevel state change
                Children = new Drawable[]
                {
                    buttonAreaBackground = new ButtonAreaBackground(),
                    Flow = new FlowContainerWithOrigin
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(-ButtonSystem.WEDGE_WIDTH, 0),
                        Anchor = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                    }
                }
            };
        }

        public ButtonSystemState ButtonSystemState
        {
            set
            {
                switch (value)
                {
                    case ButtonSystemState.Exit:
                    case ButtonSystemState.Initial:
                    case ButtonSystemState.EnteringMode:
                        Hide();
                        break;

                    case ButtonSystemState.TopLevel:
                    case ButtonSystemState.Play:
                        Show();
                        break;
                }

                buttonAreaBackground.ButtonSystemState = value;
            }
        }

        public Visibility State
        {
            get => state;
            set
            {
                if (value == state) return;

                state = value;
                InternalChild.FadeTo(state == Visibility.Hidden ? 0 : 1, 300);
                StateChanged?.Invoke(state);
            }
        }

        public override void Hide() => State = Visibility.Hidden;

        public override void Show() => State = Visibility.Visible;

        [CanBeNull]
        public event Action<Visibility> StateChanged;

        private partial class ButtonAreaBackground : Box, IStateful<ButtonAreaBackgroundState>
        {
            private ButtonAreaBackgroundState state;

            public ButtonAreaBackground()
            {
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(2, 1);
                Colour = OsuColour.Gray(50);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            public ButtonAreaBackgroundState State
            {
                get => state;
                set
                {
                    if (value == state) return;

                    state = value;

                    switch (state)
                    {
                        case ButtonAreaBackgroundState.Flat:
                            this.ScaleTo(new Vector2(2, 0), 300, Easing.InSine);
                            break;

                        case ButtonAreaBackgroundState.Normal:
                            this.ScaleTo(Vector2.One, 400, Easing.OutQuint);
                            break;
                    }

                    StateChanged?.Invoke(state);
                }
            }

            public ButtonSystemState ButtonSystemState
            {
                set
                {
                    switch (value)
                    {
                        default:
                            State = ButtonAreaBackgroundState.Normal;
                            break;

                        case ButtonSystemState.Initial:
                        case ButtonSystemState.Exit:
                        case ButtonSystemState.EnteringMode:
                            State = ButtonAreaBackgroundState.Flat;
                            break;
                    }
                }
            }

            [CanBeNull]
            public event Action<ButtonAreaBackgroundState> StateChanged;
        }

        public enum ButtonAreaBackgroundState
        {
            Normal,
            Flat
        }
    }
}

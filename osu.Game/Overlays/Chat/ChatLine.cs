// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : Container
    {
        private Message message;
        public Message Message
        {
            get { return message; }
            set
            {
                if (message.Equals(value)) return;

                if (!IsLoaded)
                {
                    message = value;
                    return;
                }

                // Check if the children have to be redone
                bool childrenOutdated = message.Sender != value.Sender || message.Timestamp != value.Timestamp || !string.Equals(message.Content, value.Content);

                // Fade if the message type changed from local echo message to non local echo message
                if (message is LocalEchoMessage && !(value is LocalEchoMessage))
                {
                    this.FadeIn(echo_to_non_echo_fade_duration);
                    timestamp.FadeTo(timestamp_alpha, echo_to_non_echo_fade_duration);
                }

                message = value;

                if (childrenOutdated)
                {
                    Clear();
                    addChildren();
                }
            }
        }

        private static readonly Color4[] username_colours = {
            OsuColour.FromHex("588c7e"),
            OsuColour.FromHex("b2a367"),
            OsuColour.FromHex("c98f65"),
            OsuColour.FromHex("bc5151"),
            OsuColour.FromHex("5c8bd6"),
            OsuColour.FromHex("7f6ab7"),
            OsuColour.FromHex("a368ad"),
            OsuColour.FromHex("aa6880"),

            OsuColour.FromHex("6fad9b"),
            OsuColour.FromHex("f2e394"),
            OsuColour.FromHex("f2ae72"),
            OsuColour.FromHex("f98f8a"),
            OsuColour.FromHex("7daef4"),
            OsuColour.FromHex("a691f2"),
            OsuColour.FromHex("c894d3"),
            OsuColour.FromHex("d895b0"),

            OsuColour.FromHex("53c4a1"),
            OsuColour.FromHex("eace5c"),
            OsuColour.FromHex("ea8c47"),
            OsuColour.FromHex("fc4f4f"),
            OsuColour.FromHex("3d94ea"),
            OsuColour.FromHex("7760ea"),
            OsuColour.FromHex("af52c6"),
            OsuColour.FromHex("e25696"),

            OsuColour.FromHex("677c66"),
            OsuColour.FromHex("9b8732"),
            OsuColour.FromHex("8c5129"),
            OsuColour.FromHex("8c3030"),
            OsuColour.FromHex("1f5d91"),
            OsuColour.FromHex("4335a5"),
            OsuColour.FromHex("812a96"),
            OsuColour.FromHex("992861"),
        };

        public const float LEFT_PADDING = message_padding + padding * 2;

        private const float padding = 15;
        private const float message_padding = 200;
        private const float text_size = 20;

        private const float timestamp_alpha = 0.4f;

        private const float echo_alpha = 0.5f;
        private const double echo_to_non_echo_fade_duration = 400;

        private Action<User> loadProfile;

        private Color4 customUsernameColour;

        private OsuSpriteText timestamp;

        public ChatLine(Message message)
        {
            this.message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = padding, Right = padding };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, UserProfileOverlay profile)
        {
            customUsernameColour = colours.ChatBlue;
            loadProfile = u => profile?.ShowUser(u);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Alpha = message is LocalEchoMessage ? echo_alpha : 1.0f;
            addChildren();
        }

        private void addChildren()
        {
            bool hasBackground = !string.IsNullOrEmpty(message.Sender.Colour);
            Drawable username = new OsuSpriteText
            {
                Font = @"Exo2.0-BoldItalic",
                Text = $@"{message.Sender.Username}" + (hasBackground ? "" : ":"),
                Colour = hasBackground ? customUsernameColour : username_colours[message.Sender.Id % username_colours.Length],
                TextSize = text_size,
            };

            if (hasBackground)
            {
                // Background effect
                username = username.WithEffect(new EdgeEffect
                {
                    CornerRadius = 4,
                    Parameters = new EdgeEffectParameters
                    {
                        Radius = 1,
                        Colour = OsuColour.FromHex(message.Sender.Colour),
                        Type = EdgeEffectType.Shadow,
                    }
                }, d =>
                {
                    d.Padding = new MarginPadding { Left = 3, Right = 3, Bottom = 1, Top = -3 };
                    d.Y = 3;
                })
                // Drop shadow effect
                .WithEffect(new EdgeEffect
                {
                    CornerRadius = 4,
                    Parameters = new EdgeEffectParameters
                    {
                        Roundness = 1,
                        Offset = new Vector2(0, 3),
                        Radius = 3,
                        Colour = Color4.Black.Opacity(0.3f),
                        Type = EdgeEffectType.Shadow,
                    }
                });
            }

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(message_padding, text_size),
                    Children = new Drawable[]
                    {
                        timestamp = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-SemiBold",
                            Text = $@"{message.Timestamp.LocalDateTime:HH:mm:ss}",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                            Alpha = message is LocalEchoMessage ? 0.0f : timestamp_alpha,
                        },
                        new ClickableContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Child = username,
                            Action = () => loadProfile(message.Sender),
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = message_padding + padding },
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(t =>
                        {
                            t.TextSize = text_size;
                        })
                        {
                            Text = message.Content,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                }
            };
        }
    }
}

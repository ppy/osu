// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : Container
    {
        public readonly Message Message;

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

        private Color4 getUsernameColour(Message message)
        {
            if (!string.IsNullOrEmpty(message.Sender?.Colour))
                return OsuColour.FromHex(message.Sender.Colour);

            //todo: use User instead of Message when user_id is correctly populated.
            return username_colours[message.UserId % username_colours.Length];
        }

        public const float LEFT_PADDING = message_padding + padding * 2;

        private const float padding = 15;
        private const float message_padding = 200;
        private const float text_size = 20;

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = padding, Right = padding };

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(message_padding, text_size),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-SemiBold",
                            Text = $@"{Message.Timestamp.LocalDateTime:HH:mm:ss}",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                            Alpha = 0.4f,
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Text = $@"{Message.Sender.Username}:",
                            Colour = getUsernameColour(Message),
                            TextSize = text_size,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = message_padding + padding },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = Message.Content,
                            TextSize = text_size,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                }
            };
        }
    }
}

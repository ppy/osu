// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Online.Chat.Drawables
{
    public class ChatLine : Container
    {
        public readonly Message Message;

        const float padding = 200;
        const float text_size = 20;

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = 15, Right = 15 };

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(padding, text_size),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = $@"{Message.Timestamp.LocalDateTime:hh:mm:ss}",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                            Colour = Color4.Gray
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Text = $@"{Message.User.Name}:",
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
                    Padding = new MarginPadding { Left = padding + 15 },
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

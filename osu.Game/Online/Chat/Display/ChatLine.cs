//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Online.Chat.Display
{
    namespace osu.Online.Social
    {
        public class ChatLine : AutoSizeContainer
        {
            public readonly Message Message;

            public ChatLine(Message message)
            {
                this.Message = message;
            }

            const float padding = 200;
            const float text_size = 20;

            public override void Load(BaseGame game)
            {
                base.Load(game);

                RelativeSizeAxes = Axes.X;

                Children = new Drawable[]
                {
                    new Container
                    {
                        Size = new Vector2(padding, text_size),
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Text = Message.Timestamp.LocalDateTime.ToLongTimeString(),
                                TextSize = text_size,
                                Colour = new Color4(128, 128, 128, 255)
                            },
                            new SpriteText
                            {
                                Text = Message.User.Name,
                                TextSize = text_size,
                                Origin = Anchor.TopRight,
                                Anchor = Anchor.TopRight,
                            }
                        }
                    },
                    new AutoSizeContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Left = padding + 10 },
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Text = Message.Content,
                                TextSize = text_size,
                                RelativeSizeAxes = Axes.X,
                            }
                        }
                    }
                };
            }
        }
    }
}

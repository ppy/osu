//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

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

            public override void Load()
            {
                base.Load();

                RelativeSizeAxes = Axes.X;

                Add(new SpriteText
                {
                    Text = Message.Timestamp.ToLocalTime().ToLongTimeString(),
                    Colour = new Color4(128, 128, 128, 255)
                });

                Add(new SpriteText
                {
                    Text = Message.User.Name,
                    Origin = Anchor.TopRight,
                    RelativePositionAxes = Axes.X,
                    Position = new Vector2(0.2f,0),
                });

                Add(new SpriteText
                {
                    Text = Message.Content,
                    RelativePositionAxes = Axes.X,
                    Position = new Vector2(0.22f, 0),
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.78f, 1),
                });
            }
        }
    }
}

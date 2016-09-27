//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
            public ChatLine(Message msg)
            {
                SizeMode = InheritMode.X;

                Add(new Box
                {
                    SizeMode = InheritMode.XY,
                    Colour = Color4.Aqua,
                    Alpha = 0.2f
                });

                Add(new SpriteText
                {
                    Text = msg.Timestamp.ToLocalTime().ToLongTimeString(),
                    Colour = new Color4(128, 128, 128, 255)
                });

                Add(new SpriteText
                {
                    Text = msg.User.Name,
                    Origin = Anchor.TopRight,
                    PositionMode = InheritMode.X,
                    Position = new Vector2(0.14f,0),
                });

                Add(new SpriteText
                {
                    Text = msg.Content,
                    PositionMode = InheritMode.X,
                    Position = new Vector2(0.15f, 0),
                    SizeMode = InheritMode.X,
                    Size = new Vector2(0.85f, 1),
                });
            }
        }
    }
}

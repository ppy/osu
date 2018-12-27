// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Online.Multiplayer.GameTypes
{
    public class GameTypeTag : GameType
    {
        public override string Name => "Tag";

        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.fa_refresh,
                Size = new Vector2(size),
                Colour = colours.Blue,
                Shadow = false,
            };
        }
    }
}

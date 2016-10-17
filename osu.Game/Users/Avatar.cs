//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using OpenTK;

namespace osu.Framework.Graphics.Sprites
{
    public class Avatar : Sprite
    {
        public int userId = 0, rounding = 0;
        public int avatarSize = 50;


        public Avatar(int uId, int aSize, int round)
        {
            this.userId = uId;
            this.avatarSize = aSize;
            this.rounding = round;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public override async void Load(BaseGame game)
        {
            base.Load(game);
            
            Texture = await game.Textures.GetAsync($@"https://a.ppy.sh/{userId}");
            Size = new Vector2(avatarSize);
            CornerRadius = rounding;
        }
    }
}

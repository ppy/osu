﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Users
{
    public class UserCoverBackground : Sprite
    {
        private readonly User user;

        public UserCoverBackground(User user)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (!string.IsNullOrEmpty(user.CoverUrl))
                Texture = textures.Get(user.CoverUrl);
        }
    }
}

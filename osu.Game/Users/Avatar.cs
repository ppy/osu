// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Users
{
    public class Avatar : Container
    {
        private readonly User user;

        /// <summary>
        /// An avatar for specified user.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        public Avatar(User user = null)
        {
            this.user = user;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (textures == null)
                throw new ArgumentNullException(nameof(textures));

            Texture texture = null;
            if (user != null && user.Id > 1) texture = textures.Get($@"https://a.ppy.sh/{user.Id}");
            if (texture == null) texture = textures.Get(@"Online/avatar-guest");

            Add(new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = texture,
                FillMode = FillMode.Fit,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }
    }
}

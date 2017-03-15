// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Users
{
    public class Avatar : Container
    {
        public Drawable Sprite;

        private int userId;
        private OsuGame game;
        private Texture guestTexture;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game, TextureStore textures)
        {
            this.game = game;
            guestTexture = textures.Get(@"Online/avatar-guest");
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGame game, TextureStore textures)
        {
            this.game = game;

            guestTexture = textures.Get(@"Online/avatar-guest");
        }

        public int UserId
        {
            get { return userId; }
            set
            {
                if (userId == value)
                    return;

                userId = value;

                var newSprite = userId > 1 ? new OnlineSprite($@"https://a.ppy.sh/{userId}") : new Sprite { Texture = guestTexture };

                newSprite.FillMode = FillMode.Fit;

                if (game != null)
                {
                    newSprite.LoadAsync(game, s =>
                    {
                        Sprite?.FadeOut();
                        Sprite?.Expire();
                        Sprite = s;

                        Add(s);

                            //todo: fix this... clock dependencies are a pain
                            if (s.Clock != null)
                            s.FadeInFromZero(200);
                    });
                }
            }
        }

        public class OnlineSprite : Sprite
        {
            private readonly string url;

            public OnlineSprite(string url)
            {
                Debug.Assert(url != null);
                this.url = url;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(url);
            }
        }
    }
}

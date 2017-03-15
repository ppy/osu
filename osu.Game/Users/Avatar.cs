// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;

namespace osu.Game.Users
{
    public class Avatar : Container
    {
        public Drawable Sprite;

        private long userId;
        private OsuGameBase game;
        private Texture guestTexture;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuGameBase game, TextureStore textures)
        {
            this.game = game;
            guestTexture = textures.Get(@"Online/avatar-guest");
        }

        public long UserId
        {
            get { return userId; }
            set
            {
                if (userId == value)
                    return;

                userId = value;
                invalidateSprite();
            }
        }

        private Task loadTask;

        private void invalidateSprite()
        {
            Sprite?.FadeOut(100);
            Sprite?.Expire();
            Sprite = null;
        }

        private void updateSprite()
        {
            if (loadTask != null || Sprite != null) return;

            var newSprite = userId > 1 ? new OnlineSprite($@"https://a.ppy.sh/{userId}") : new Sprite { Texture = guestTexture };

            newSprite.FillMode = FillMode.Fill;

            loadTask = newSprite.LoadAsync(game, s =>
            {
                Sprite = s;
                Add(Sprite);

                Sprite.FadeInFromZero(200);
                loadTask = null;
            });
        }

        protected override void Update()
        {
            base.Update();

            //todo: should only be run when we are visible to the user.
            updateSprite();
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

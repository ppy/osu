// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

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

            var newSprite = userId > 1 ? new OnlineSprite($@"https://a.ppy.sh/{userId}", guestTexture) : new Sprite { Texture = guestTexture };

            newSprite.FillMode = FillMode.Fill;

            loadTask = newSprite.LoadAsync(game, s =>
            {
                Sprite = s;
                Add(Sprite);

                Sprite.FadeInFromZero(200);
                loadTask = null;
            });
        }

        private double timeVisible;

        private bool shouldUpdate => Sprite != null || timeVisible > 500;

        protected override void Update()
        {
            base.Update();

            if (!shouldUpdate)
            {
                //Special optimisation to not start loading until we are within bounds of our closest ScrollContainer parent.
                ScrollContainer scroll = null;
                IContainer cursor = this;
                while (scroll == null && (cursor = cursor.Parent) != null)
                    scroll = cursor as ScrollContainer;

                if (scroll?.ScreenSpaceDrawQuad.Intersects(ScreenSpaceDrawQuad) ?? true)
                    timeVisible += Time.Elapsed;
                else
                    timeVisible = 0;
            }

            if (shouldUpdate)
                updateSprite();
        }

        public class OnlineSprite : Sprite
        {
            private readonly string url;
            private readonly Texture fallbackTexture;

            public OnlineSprite(string url, Texture fallbackTexture = null)
            {
                Debug.Assert(url != null);
                this.url = url;
                this.fallbackTexture = fallbackTexture;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(url) ?? fallbackTexture;
            }
        }
    }
}

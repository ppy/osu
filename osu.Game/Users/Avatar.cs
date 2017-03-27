// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
        private const int time_before_load = 500;

        private Drawable sprite;
        private readonly User user;
        private readonly bool delayedLoad;

        /// <summary>
        /// An avatar for specified user.
        /// </summary>
        /// <param name="user">The user. A null value will get a placeholder avatar.</param>
        /// <param name="delayedLoad">Whether we should delay the load of the avatar until it has been on-screen for a specified duration.</param>
        public Avatar(User user = null, bool delayedLoad = true)
        {
            this.user = user;
            this.delayedLoad = delayedLoad;
        }

        private Action performLoad;
        private Task loadTask;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(TextureStore textures)
        {
            performLoad = () =>
            {
                Texture texture = null;
                if (user?.Id > 1) texture = textures.Get($@"https://a.ppy.sh/{user.Id}");
                if (texture == null) texture = textures.Get(@"Online/avatar-guest");

                sprite = new Sprite
                {
                    Texture = texture,
                    FillMode = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                };

                Schedule(() =>
                {
                    Add(sprite);
                    sprite.FadeInFromZero(150);
                });
            };
        }

        private double timeVisible;

        private bool shouldLoad => !delayedLoad || timeVisible > time_before_load;

        protected override void Update()
        {
            base.Update();

            if (!shouldLoad)
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

            if (shouldLoad && loadTask == null)
                (loadTask = Task.Factory.StartNew(performLoad, TaskCreationOptions.LongRunning)).ConfigureAwait(false);
        }
    }
}

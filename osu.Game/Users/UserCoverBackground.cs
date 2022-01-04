// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public class UserCoverBackground : ModelBackedDrawable<APIUser>
    {
        public APIUser User
        {
            get => Model;
            set => Model = value;
        }

        protected override Drawable CreateDrawable(APIUser user) => new Cover(user);

        protected override double LoadDelay => 300;

        /// <summary>
        /// Delay before the background is unloaded while off-screen.
        /// </summary>
        protected virtual double UnloadDelay => 5000;

        protected override DelayedLoadWrapper CreateDelayedLoadWrapper(Func<Drawable> createContentFunc, double timeBeforeLoad)
            => new DelayedLoadUnloadWrapper(createContentFunc, timeBeforeLoad, UnloadDelay);

        [LongRunningLoad]
        private class Cover : CompositeDrawable
        {
            private readonly APIUser user;

            public Cover(APIUser user)
            {
                this.user = user;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(LargeTextureStore textures)
            {
                if (user == null)
                {
                    InternalChild = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                    };
                }
                else
                {
                    InternalChild = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Texture = textures.Get(user.CoverUrl),
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    };
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                this.FadeInFromZero(400);
            }
        }
    }
}

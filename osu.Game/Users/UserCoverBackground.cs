// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public class UserCoverBackground : ModelBackedDrawable<User>
    {
        public User User
        {
            get => Model;
            set => Model = value;
        }

        [Resolved]
        private LargeTextureStore textures { get; set; }

        protected override Drawable CreateDrawable(User user)
        {
            if (user == null)
            {
                return new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                };
            }
            else
            {
                var sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get(user.CoverUrl),
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                };
                sprite.OnLoadComplete += d => d.FadeInFromZero(400);
                return sprite;
            }
        }
    }
}

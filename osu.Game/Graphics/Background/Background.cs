//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework;
using System.Threading.Tasks;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.Background
{
    class Background : Container
    {
        protected Sprite BackgroundSprite;

        string textureName;

        public Background(string textureName = @"Backgrounds/bg1")
        {
            this.textureName = textureName;
            RelativeSizeAxes = Axes.Both;
            Depth = float.MinValue;

            Add(BackgroundSprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.DarkGray
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            BackgroundSprite.Texture = textures.Get(textureName);
        }

        protected override void Update()
        {
            base.Update();
            BackgroundSprite.Scale = new Vector2(Math.Max(DrawSize.X / BackgroundSprite.DrawSize.X, DrawSize.Y / BackgroundSprite.DrawSize.Y));
        }
    }
}

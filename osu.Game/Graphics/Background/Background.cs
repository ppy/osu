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
    public class Background : BufferedContainer
    {
        public Sprite Sprite;

        string textureName;

        public Background(string textureName = @"")
        {
            this.textureName = textureName;
            RelativeSizeAxes = Axes.Both;
            Depth = float.MinValue;

            Add(Sprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.DarkGray
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (!string.IsNullOrEmpty(textureName))
                Sprite.Texture = textures.Get(textureName);
        }

        protected override void Update()
        {
            base.Update();
            Sprite.Scale = new Vector2(Math.Max(DrawSize.X / Sprite.DrawSize.X, DrawSize.Y / Sprite.DrawSize.Y));
        }
    }
}

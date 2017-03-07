﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    public class Background : BufferedContainer
    {
        public Sprite Sprite;

        private string textureName;

        public Background(string textureName = @"")
        {
            CacheDrawnFrameBuffer = true;

            this.textureName = textureName;
            RelativeSizeAxes = Axes.Both;
            Depth = float.MaxValue;

            Add(Sprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.DarkGray,
                FillMode = FillMode.Fill,
            });
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (!string.IsNullOrEmpty(textureName))
                Sprite.Texture = textures.Get(textureName);
        }
    }
}

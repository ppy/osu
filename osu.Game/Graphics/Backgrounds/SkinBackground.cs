// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Skinning;

namespace osu.Game.Graphics.Backgrounds
{
    internal class SkinBackground : Background
    {
        private readonly Skin skin;

        public SkinBackground(Skin skin, string fallbackTextureName)
            : base(fallbackTextureName)
        {
            this.skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Sprite.Texture = skin.GetTexture("menu-background") ?? Sprite.Texture;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Skinning;

namespace osu.Game.Graphics.Backgrounds
{
    internal partial class SkinBackground : Background
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

        public override bool Equals(Background? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return other.GetType() == GetType()
                   && ((SkinBackground)other).skin.SkinInfo.Equals(skin.SkinInfo);
        }
    }
}

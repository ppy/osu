// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skinnable element which uses a stable sprite and can therefore share implementation logic.
    /// </summary>
    public class SkinnableSprite : SkinnableDrawable
    {
        protected override bool ApplySizeRestrictionsToDefault => true;

        [Resolved]
        private TextureStore textures { get; set; }

        public SkinnableSprite(string textureName, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.ScaleDownToFit)
            : base(new SpriteComponent(textureName), allowFallback, confineMode)
        {
        }

        protected override Drawable CreateDefault(ISkinComponent component) => new Sprite { Texture = textures.Get(component.LookupName) };

        private class SpriteComponent : ISkinComponent
        {
            private readonly string textureName;

            public SpriteComponent(string textureName)
            {
                this.textureName = textureName;
            }

            public string LookupName => textureName;
        }
    }
}

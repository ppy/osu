// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    public class SkinnableSprite : SkinnableDrawable<Sprite>
    {
        protected override bool ApplySizeToDefault => true;

        private readonly string defaultName;

        protected override Sprite CreateDefault() => new Sprite { Texture = textures.Get(defaultName) };

        [Resolved]
        private TextureStore textures { get; set; }

        public SkinnableSprite(string name, string defaultName, Func<ISkinSource, bool> allowFallback = null, bool restrictSize = true)
            : base(name, allowFallback, restrictSize)
        {
            this.defaultName = defaultName;
        }
    }
}

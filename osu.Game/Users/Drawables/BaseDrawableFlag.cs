// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Users.Drawables
{
    public partial class BaseDrawableFlag : Sprite
    {
        protected readonly CountryCode CountryCode;

        public BaseDrawableFlag(CountryCode countryCode)
        {
            CountryCode = countryCode;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            ArgumentNullException.ThrowIfNull(ts);

            string textureName = CountryCode == CountryCode.Unknown ? "__" : CountryCode.ToString();
            Texture = ts.Get($@"Flags/{textureName}") ?? ts.Get(@"Flags/__");
        }
    }
}

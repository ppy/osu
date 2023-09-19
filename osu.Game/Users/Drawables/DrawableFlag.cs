// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;

namespace osu.Game.Users.Drawables
{
    public partial class DrawableFlag : Sprite, IHasTooltip
    {
        private readonly CountryCode countryCode;

        public LocalisableString TooltipText => countryCode == CountryCode.Unknown ? string.Empty : countryCode.GetDescription();

        public DrawableFlag(CountryCode countryCode)
        {
            this.countryCode = countryCode;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            ArgumentNullException.ThrowIfNull(ts);

            string textureName = countryCode == CountryCode.Unknown ? "__" : countryCode.ToString();
            Texture = ts.Get($@"Flags/{textureName}") ?? ts.Get(@"Flags/__");
        }
    }
}

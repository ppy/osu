// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;

namespace osu.Game.Users.Drawables
{
    public class DrawableFlag : Sprite, IHasTooltip
    {
        private readonly Country country;

        public LocalisableString TooltipText => country == default ? string.Empty : country.GetDescription();

        public DrawableFlag(Country country)
        {
            this.country = country;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            if (ts == null)
                throw new ArgumentNullException(nameof(ts));

            string textureName = country == default ? "__" : country.ToString();
            Texture = ts.Get($@"Flags/{textureName}") ?? ts.Get(@"Flags/__");
        }
    }
}

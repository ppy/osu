// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Users
{
    public class Country
    {
        /// <summary>
        /// The name of this country.
        /// </summary>
        [JsonProperty(@"name")]
        public string FullName;

        /// <summary>
        /// Two-letter flag acronym (ISO 3166 standard)
        /// </summary>
        [JsonProperty(@"code")]
        public string FlagName;
    }

    public class DrawableFlag : ModelBackedDrawable<Country>, IHasTooltip
    {
        private TextureStore textures;

        private readonly Country country;

        public Country Country
        {
            get => Model;
            set => Model = value;
        }

        public string TooltipText => Country?.FullName;

        public DrawableFlag(Country country = null)
        {
            this.country = country;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            textures = ts ?? throw new ArgumentNullException(nameof(ts));
            Country = country;
        }

        protected override Drawable CreateDrawable(Country country)
        {
            return new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get($@"Flags/{country?.FlagName ?? @"__"}"),
            };
        }
    }
}

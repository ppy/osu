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

    public class DrawableFlag : Container, IHasTooltip
    {
        private readonly Sprite sprite;
        private TextureStore textures;

        private Country country;

        public Country Country
        {
            get => country;
            set
            {
                if (value == country)
                    return;

                country = value;

                if (LoadState >= LoadState.Ready)
                    sprite.Texture = getFlagTexture();
            }
        }

        public string TooltipText => country?.FullName;

        public DrawableFlag(Country country = null)
        {
            this.country = country;

            Children = new Drawable[]
            {
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            if (ts == null)
                throw new ArgumentNullException(nameof(ts));

            textures = ts;
            sprite.Texture = getFlagTexture();
        }

        private Texture getFlagTexture() => textures.Get($@"Flags/{country?.FlagName ?? @"__"}");
    }
}

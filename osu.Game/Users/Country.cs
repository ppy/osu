// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym;

        /// <summary>
        /// Two-letter flag acronym (ISO 3166 standard)
        /// </summary>
        [JsonProperty(@"code")]
        public string FlagName;
    }

    public class DrawableFlag : Container
    {
        private readonly Sprite sprite;
        private TextureStore textures;

        private string flagName;
        public string FlagName
        {
            get { return flagName; }
            set
            {
                if (value == flagName) return;
                flagName = value;
                sprite.Texture = textures.Get($@"Flags/{flagName}");
            }
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore ts)
        {
            textures = ts;
            sprite.Texture = textures.Get($@"Flags/{flagName}");
        }

        public DrawableFlag(string name = @"__")
        {
            flagName = name;

            Children = new Drawable[]
            {
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }
    }
}

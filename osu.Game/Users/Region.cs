// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Users
{
    public class Region : IHasDrawableRepresentation<DrawableFlag>
    {
        /// <summary>
        /// The name of this team.
        /// </summary>
        public string FullName;

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym;

        /// <summary>
        /// Two-letter flag acronym (ISO 3166 standard)
        /// </summary>
        public string FlagName;

        public DrawableFlag CreateDrawable()
        {
            return new DrawableFlag(FlagName);
        }
    }

    public class DrawableFlag : Container
    {
        private Sprite sprite;
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

        public DrawableFlag(string name)
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

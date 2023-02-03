// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.SkinComponents
{
    public static class SkinnableSpriteStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinComponents.SkinnableSprite";

        /// <summary>
        /// "Sprite name"
        /// </summary>
        public static LocalisableString SpriteName => new TranslatableString(getKey(@"sprite_name"), "Sprite name");

        /// <summary>
        /// "The filename of the sprite"
        /// </summary>
        public static LocalisableString SpriteNameDescription => new TranslatableString(getKey(@"sprite_name_description"), "The filename of the sprite");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}

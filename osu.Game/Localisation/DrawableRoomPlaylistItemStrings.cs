// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DrawableRoomPlaylistItemStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DrawableRoomPlaylistItem";

        /// <summary>
        /// "You have completed this beatmap"
        /// </summary>
        public static LocalisableString CompletedTooltip => new TranslatableString(getKey(@"completed_tooltip"), @"You have completed this beatmap");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

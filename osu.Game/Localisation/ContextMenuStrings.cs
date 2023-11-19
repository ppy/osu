// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ContextMenuStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ContextMenu";

        /// <summary>
        /// "View profile"
        /// </summary>
        public static LocalisableString ViewProfile => new TranslatableString(getKey(@"view_profile"), @"View profile");

        /// <summary>
        /// "View beatmap"
        /// </summary>
        public static LocalisableString ViewBeatmap => new TranslatableString(getKey(@"view_beatmap"), @"View beatmap");

        /// <summary>
        /// "Invite player"
        /// </summary>
        public static LocalisableString InvitePlayer => new TranslatableString(getKey(@"invite_player"), @"Invite player");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

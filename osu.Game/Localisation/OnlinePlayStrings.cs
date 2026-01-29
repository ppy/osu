// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OnlinePlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OnlinePlay";

        /// <summary>
        /// "Playlist durations longer than 2 weeks require an active osu!supporter tag."
        /// </summary>
        public static LocalisableString SupporterOnlyDurationNotice => new TranslatableString(getKey(@"supporter_only_duration_notice"), @"Playlist durations longer than 2 weeks require an active osu!supporter tag.");

        /// <summary>
        /// "Can&#39;t invite this user as you have blocked them or they have blocked you."
        /// </summary>
        public static LocalisableString InviteFailedUserBlocked => new TranslatableString(getKey(@"cant_invite_this_user_as"), @"Can't invite this user as you have blocked them or they have blocked you.");

        /// <summary>
        /// "Can&#39;t invite this user as they have opted out of non-friend communications."
        /// </summary>
        public static LocalisableString InviteFailedUserOptOut => new TranslatableString(getKey(@"cant_invite_this_user_as1"), @"Can't invite this user as they have opted out of non-friend communications.");

        /// <summary>
        /// "Add to playlist"
        /// </summary>
        public static LocalisableString FooterButtonPlaylistAdd => new TranslatableString(getKey(@"footer_button_playlist_add"), @"Add to playlist");

        /// <summary>
        /// "Freemods"
        /// </summary>
        public static LocalisableString FooterButtonFreemods => new TranslatableString(getKey(@"footer_button_freemods"), @"Freemods");

        /// <summary>
        /// "Freestyle"
        /// </summary>
        public static LocalisableString FooterButtonFreestyle => new TranslatableString(getKey(@"footer_button_freestyle"), @"Freestyle");

        /// <summary>
        /// "{0} item(s)"
        /// </summary>
        public static LocalisableString PlaylistTrayItems(int count) => new TranslatableString(getKey(@"playlist_tray_items"), @"{0} item(s)", count);

        /// <summary>
        /// "Manage items on previous screen"
        /// </summary>
        public static LocalisableString PlaylistTrayDescription => new TranslatableString(getKey(@"playlist_tray_description"), @"Manage items on previous screen");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

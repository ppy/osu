// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OnlinePlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OnlinePlay";

        /// <summary>
        /// "Multiplayer"
        /// </summary>
        public static LocalisableString MultiplayerScreenTitle => new TranslatableString(getKey(@"multiplayer_screen_title"), @"Multiplayer");

        /// <summary>
        /// "Playlists"
        /// </summary>
        public static LocalisableString PlaylistsScreenTitle => new TranslatableString(getKey(@"playlists_screen_title"), @"Playlists");

        /// <summary>
        /// "Lounge"
        /// </summary>
        public static LocalisableString LoungeTitle => new TranslatableString(getKey(@"lounge_title"), @"Lounge");

        /// <summary>
        /// "Room"
        /// </summary>
        public static LocalisableString RoomTitle => new TranslatableString(getKey(@"room_title"), @"Room");

        /// <summary>
        /// "Playlist"
        /// </summary>
        public static LocalisableString PlaylistTitle => new TranslatableString(getKey(@"playlist_title"), @"Playlist");

        /// <summary>
        /// "Song Selection"
        /// </summary>
        public static LocalisableString SongSelectionTitle => new TranslatableString(getKey(@"song_selection_title"), @"Song Selection");

        /// <summary>
        /// "Style Selection"
        /// </summary>
        public static LocalisableString StyleSelectionTitle => new TranslatableString(getKey(@"style_selection_title"), @"Style Selection");

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
        /// "Manage items on previous screen"
        /// </summary>
        public static LocalisableString PlaylistTrayDescription => new TranslatableString(getKey(@"playlist_tray_description"), @"Manage items on previous screen");

        /// <summary>
        /// "Beatmap queue"
        /// </summary>
        public static LocalisableString MultiplayerBeatmapQueue => new TranslatableString(getKey(@"multiplayer_beatmap_queue"), @"Beatmap queue");

        /// <summary>
        /// "Progress"
        /// </summary>
        public static LocalisableString PlaylistProgress => new TranslatableString(getKey(@"playlist_progress"), @"Progress");

        /// <summary>
        /// "Leaderboard"
        /// </summary>
        public static LocalisableString PlaylistLeaderboard => new TranslatableString(getKey(@"playlist_leaderboard"), @"Leaderboard");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString Difficulty => new TranslatableString(getKey(@"difficulty"), @"Difficulty");

        /// <summary>
        /// "Chat"
        /// </summary>
        public static LocalisableString Chat => new TranslatableString(getKey(@"chat"), @"Chat");

        /// <summary>
        /// "Close playlist"
        /// </summary>
        public static LocalisableString ClosePlaylist => new TranslatableString(getKey(@"close_playlist"), @"Close playlist");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

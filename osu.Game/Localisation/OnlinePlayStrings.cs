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
        /// "Closed {0}"
        /// </summary>
        public static LocalisableString RoomClosed(LocalisableString time) => new TranslatableString(getKey(@"room_closed"), @"Closed {0}", time);

        /// <summary>
        /// "Closed"
        /// </summary>
        public static LocalisableString RoomClosedRecently => new TranslatableString(getKey(@"room_closed_recently"), @"Closed");

        /// <summary>
        /// "Closing soon"
        /// </summary>
        public static LocalisableString RoomClosingSoon => new TranslatableString(getKey(@"room_closing_soon"), @"Closing soon");

        /// <summary>
        /// "Closing {0}"
        /// </summary>
        public static LocalisableString RoomClosing(LocalisableString time) => new TranslatableString(getKey(@"room_closing"), @"Closing {0}", time);

        /// <summary>
        /// "Open"
        /// </summary>
        public static LocalisableString RoomModeFilterOpen => new TranslatableString(getKey(@"room_mode_filter_open"), @"Open");

        /// <summary>
        /// "Recently Ended"
        /// </summary>
        public static LocalisableString RoomModeFilterEnded => new TranslatableString(getKey(@"room_mode_filter_ended"), @"Recently Ended");

        /// <summary>
        /// "Participated"
        /// </summary>
        public static LocalisableString RoomModeFilterParticipated => new TranslatableString(getKey(@"room_mode_filter_participated"), @"Participated");

        /// <summary>
        /// "Owned"
        /// </summary>
        public static LocalisableString RoomModeFilterOwned => new TranslatableString(getKey(@"room_mode_filter_owned"), @"Owned");

        /// <summary>
        /// "Ready to play"
        /// </summary>
        public static LocalisableString RoomStatusReadyToPlay => new TranslatableString(getKey(@"room_status_ready_to_play"), @"Ready to play");

        /// <summary>
        /// "Create copy"
        /// </summary>
        public static LocalisableString CreateCopy => new TranslatableString(getKey(@"create_copy"), @"Create copy");

        /// <summary>
        /// "Close playlist"
        /// </summary>
        public static LocalisableString ClosePlaylist => new TranslatableString(getKey(@"close_playlist"), @"Close playlist");

        /// <summary>
        /// "Join Room"
        /// </summary>
        public static LocalisableString JoinRoom => new TranslatableString(getKey(@"join_room"), @"Join Room");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

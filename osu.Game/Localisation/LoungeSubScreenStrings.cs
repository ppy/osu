// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LoungeSubScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.LoungeSubScreen";

        /// <summary>
        /// "Privacy setting"
        /// </summary>
        public static LocalisableString RoomFilterPrivacySetting => new TranslatableString(getKey(@"room_filter_privacy_setting"), @"Privacy setting");

        /// <summary>
        /// "Show in-progress rooms"
        /// </summary>
        public static LocalisableString RoomFilterInProgress => new TranslatableString(getKey(@"room_filter_in_progress"), @"Show in-progress rooms");

        /// <summary>
        /// "You can still join multiplayer rooms while they are in progress, and either spectate or wait for the next round."
        /// </summary>
        public static LocalisableString RoomFilterInProgressDescription => new TranslatableString(getKey(@"room_filter_in_progress_description"), @"You can still join multiplayer rooms while they are in progress, and either spectate or wait for the next round.");

        /// <summary>
        /// "Show full rooms"
        /// </summary>
        public static LocalisableString RoomFilterFullRooms => new TranslatableString(getKey(@"room_filter_full_rooms"), @"Show full rooms");

        /// <summary>
        /// "Some rooms may have an upper player limit set by the room&#39;s host."
        /// </summary>
        public static LocalisableString RoomFilterFullRoomsDescription => new TranslatableString(getKey(@"room_filter_full_rooms_description"), @"Some rooms may have an upper player limit set by the room's host.");

        /// <summary>
        /// "Category"
        /// </summary>
        public static LocalisableString PlaylistFilterCategory => new TranslatableString(getKey(@"playlist_filter_category"), @"Category");

        /// <summary>
        /// "State"
        /// </summary>
        public static LocalisableString RoomFilterState => new TranslatableString(getKey(@"room_filter_state"), @"State");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class EventsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Events";

        /// <summary>
        /// "&lt;strong&gt;&lt;em&gt;{0}&lt;/em&gt;&lt;/strong&gt; unlocked the &quot;&lt;strong&gt;{1}&lt;/strong&gt;&quot; medal!"
        /// </summary>
        public static LocalisableString Achievement(string user, string achievement) => new TranslatableString(getKey(@"achievement"), @"<strong><em>{0}</em></strong> unlocked the ""<strong>{1}</strong>"" medal!", user, achievement);

        /// <summary>
        /// "{0} has been played {1} times!"
        /// </summary>
        public static LocalisableString BeatmapPlaycount(string beatmap, string count) => new TranslatableString(getKey(@"beatmap_playcount"), @"{0} has been played {1} times!", beatmap, count);

        /// <summary>
        /// "{0} by &lt;strong&gt;{1}&lt;/strong&gt; has been {2}!"
        /// </summary>
        public static LocalisableString BeatmapsetApprove(string beatmapset, string user, string approval) => new TranslatableString(getKey(@"beatmapset_approve"), @"{0} by <strong>{1}</strong> has been {2}!", beatmapset, user, approval);

        /// <summary>
        /// "{0} has been deleted."
        /// </summary>
        public static LocalisableString BeatmapsetDelete(string beatmapset) => new TranslatableString(getKey(@"beatmapset_delete"), @"{0} has been deleted.", beatmapset);

        /// <summary>
        /// "{0} has been revived from eternal slumber by &lt;strong&gt;{1}&lt;/strong&gt;."
        /// </summary>
        public static LocalisableString BeatmapsetRevive(string beatmapset, string user) => new TranslatableString(getKey(@"beatmapset_revive"), @"{0} has been revived from eternal slumber by <strong>{1}</strong>.", beatmapset, user);

        /// <summary>
        /// "&lt;strong&gt;&lt;em&gt;{0}&lt;/em&gt;&lt;/strong&gt; has updated the beatmap &quot;&lt;em&gt;{1}&lt;/em&gt;&quot;"
        /// </summary>
        public static LocalisableString BeatmapsetUpdate(string user, string beatmapset) => new TranslatableString(getKey(@"beatmapset_update"), @"<strong><em>{0}</em></strong> has updated the beatmap ""<em>{1}</em>""", user, beatmapset);

        /// <summary>
        /// "&lt;strong&gt;&lt;em&gt;{0}&lt;/em&gt;&lt;/strong&gt; has submitted a new beatmap &quot;{1}&quot;"
        /// </summary>
        public static LocalisableString BeatmapsetUpload(string user, string beatmapset) => new TranslatableString(getKey(@"beatmapset_upload"), @"<strong><em>{0}</em></strong> has submitted a new beatmap ""{1}""", user, beatmapset);

        /// <summary>
        /// "This user hasn&#39;t done anything notable recently!"
        /// </summary>
        public static LocalisableString Empty => new TranslatableString(getKey(@"empty"), @"This user hasn't done anything notable recently!");

        /// <summary>
        /// "&lt;strong&gt;&lt;em&gt;{0}&lt;/em&gt;&lt;/strong&gt; achieved rank #{1} on &lt;em&gt;{2}&lt;/em&gt; ({3})"
        /// </summary>
        public static LocalisableString Rank(string user, string rank, string beatmap, string mode) => new TranslatableString(getKey(@"rank"), @"<strong><em>{0}</em></strong> achieved rank #{1} on <em>{2}</em> ({3})", user, rank, beatmap, mode);

        /// <summary>
        /// "&lt;strong&gt;&lt;em&gt;{0}&lt;/em&gt;&lt;/strong&gt; has lost first place on &lt;em&gt;{1}&lt;/em&gt; ({2})"
        /// </summary>
        public static LocalisableString RankLost(string user, string beatmap, string mode) => new TranslatableString(getKey(@"rank_lost"), @"<strong><em>{0}</em></strong> has lost first place on <em>{1}</em> ({2})", user, beatmap, mode);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; has once again chosen to support osu! - thanks for your generosity!"
        /// </summary>
        public static LocalisableString UserSupportAgain(string user) => new TranslatableString(getKey(@"user_support_again"), @"<strong>{0}</strong> has once again chosen to support osu! - thanks for your generosity!", user);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; has become an osu!supporter - thanks for your generosity!"
        /// </summary>
        public static LocalisableString UserSupportFirst(string user) => new TranslatableString(getKey(@"user_support_first"), @"<strong>{0}</strong> has become an osu!supporter - thanks for your generosity!", user);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; has received the gift of osu!supporter!"
        /// </summary>
        public static LocalisableString UserSupportGift(string user) => new TranslatableString(getKey(@"user_support_gift"), @"<strong>{0}</strong> has received the gift of osu!supporter!", user);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; has changed their username to &lt;strong&gt;&lt;em&gt;{1}&lt;/em&gt;&lt;/strong&gt;!"
        /// </summary>
        public static LocalisableString UsernameChange(string previousUsername, string user) => new TranslatableString(getKey(@"username_change"), @"<strong>{0}</strong> has changed their username to <strong><em>{1}</em></strong>!", previousUsername, user);

        /// <summary>
        /// "approved"
        /// </summary>
        public static LocalisableString BeatmapsetStatusApproved => new TranslatableString(getKey(@"beatmapset_status.approved"), @"approved");

        /// <summary>
        /// "loved"
        /// </summary>
        public static LocalisableString BeatmapsetStatusLoved => new TranslatableString(getKey(@"beatmapset_status.loved"), @"loved");

        /// <summary>
        /// "qualified"
        /// </summary>
        public static LocalisableString BeatmapsetStatusQualified => new TranslatableString(getKey(@"beatmapset_status.qualified"), @"qualified");

        /// <summary>
        /// "ranked"
        /// </summary>
        public static LocalisableString BeatmapsetStatusRanked => new TranslatableString(getKey(@"beatmapset_status.ranked"), @"ranked");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
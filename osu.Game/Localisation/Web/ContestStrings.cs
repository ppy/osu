// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ContestStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Contest";

        /// <summary>
        /// "Compete in more ways than just clicking circles."
        /// </summary>
        public static LocalisableString HeaderSmall => new TranslatableString(getKey(@"header.small"), @"Compete in more ways than just clicking circles.");

        /// <summary>
        /// "Community Contests"
        /// </summary>
        public static LocalisableString HeaderLarge => new TranslatableString(getKey(@"header.large"), @"Community Contests");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString IndexNavTitle => new TranslatableString(getKey(@"index.nav_title"), @"listing");

        /// <summary>
        /// "Please sign in to vote."
        /// </summary>
        public static LocalisableString VotingLoginRequired => new TranslatableString(getKey(@"voting.login_required"), @"Please sign in to vote.");

        /// <summary>
        /// "Voting for this contest has ended"
        /// </summary>
        public static LocalisableString VotingOver => new TranslatableString(getKey(@"voting.over"), @"Voting for this contest has ended");

        /// <summary>
        /// "Show voted"
        /// </summary>
        public static LocalisableString VotingShowVotedOnly => new TranslatableString(getKey(@"voting.show_voted_only"), @"Show voted");

        /// <summary>
        /// "It doesn&#39;t look like you played any beatmaps that qualify for this contest!"
        /// </summary>
        public static LocalisableString VotingBestOfNonePlayed => new TranslatableString(getKey(@"voting.best_of.none_played"), @"It doesn't look like you played any beatmaps that qualify for this contest!");

        /// <summary>
        /// "Vote"
        /// </summary>
        public static LocalisableString VotingButtonAdd => new TranslatableString(getKey(@"voting.button.add"), @"Vote");

        /// <summary>
        /// "Remove vote"
        /// </summary>
        public static LocalisableString VotingButtonRemove => new TranslatableString(getKey(@"voting.button.remove"), @"Remove vote");

        /// <summary>
        /// "You have used up all your votes"
        /// </summary>
        public static LocalisableString VotingButtonUsedUp => new TranslatableString(getKey(@"voting.button.used_up"), @"You have used up all your votes");

        /// <summary>
        /// "entry"
        /// </summary>
        public static LocalisableString EntryDefault => new TranslatableString(getKey(@"entry._"), @"entry");

        /// <summary>
        /// "Please sign in to enter the contest."
        /// </summary>
        public static LocalisableString EntryLoginRequired => new TranslatableString(getKey(@"entry.login_required"), @"Please sign in to enter the contest.");

        /// <summary>
        /// "You cannot enter contests while restricted or silenced."
        /// </summary>
        public static LocalisableString EntrySilencedOrRestricted => new TranslatableString(getKey(@"entry.silenced_or_restricted"), @"You cannot enter contests while restricted or silenced.");

        /// <summary>
        /// "We are currently preparing this contest. Please wait patiently!"
        /// </summary>
        public static LocalisableString EntryPreparation => new TranslatableString(getKey(@"entry.preparation"), @"We are currently preparing this contest. Please wait patiently!");

        /// <summary>
        /// "Drop your entry here"
        /// </summary>
        public static LocalisableString EntryDropHere => new TranslatableString(getKey(@"entry.drop_here"), @"Drop your entry here");

        /// <summary>
        /// "Download .osz"
        /// </summary>
        public static LocalisableString EntryDownload => new TranslatableString(getKey(@"entry.download"), @"Download .osz");

        /// <summary>
        /// "Only .jpg and .png files are accepted for this contest."
        /// </summary>
        public static LocalisableString EntryWrongTypeArt => new TranslatableString(getKey(@"entry.wrong_type.art"), @"Only .jpg and .png files are accepted for this contest.");

        /// <summary>
        /// "Only .osu files are accepted for this contest."
        /// </summary>
        public static LocalisableString EntryWrongTypeBeatmap => new TranslatableString(getKey(@"entry.wrong_type.beatmap"), @"Only .osu files are accepted for this contest.");

        /// <summary>
        /// "Only .mp3 files are accepted for this contest."
        /// </summary>
        public static LocalisableString EntryWrongTypeMusic => new TranslatableString(getKey(@"entry.wrong_type.music"), @"Only .mp3 files are accepted for this contest.");

        /// <summary>
        /// "Entries for this contest can only be up to {0}."
        /// </summary>
        public static LocalisableString EntryTooBig(string limit) => new TranslatableString(getKey(@"entry.too_big"), @"Entries for this contest can only be up to {0}.", limit);

        /// <summary>
        /// "Download Entry"
        /// </summary>
        public static LocalisableString BeatmapsDownload => new TranslatableString(getKey(@"beatmaps.download"), @"Download Entry");

        /// <summary>
        /// "votes"
        /// </summary>
        public static LocalisableString VoteList => new TranslatableString(getKey(@"vote.list"), @"votes");

        /// <summary>
        /// "{0} vote|{0} votes"
        /// </summary>
        public static LocalisableString VoteCount(string countDelimited) => new TranslatableString(getKey(@"vote.count"), @"{0} vote|{0} votes", countDelimited);

        /// <summary>
        /// "{0} point|{0} points"
        /// </summary>
        public static LocalisableString VotePoints(string countDelimited) => new TranslatableString(getKey(@"vote.points"), @"{0} point|{0} points", countDelimited);

        /// <summary>
        /// "Ended {0}"
        /// </summary>
        public static LocalisableString DatesEnded(string date) => new TranslatableString(getKey(@"dates.ended"), @"Ended {0}", date);

        /// <summary>
        /// "Ended"
        /// </summary>
        public static LocalisableString DatesEndedNoDate => new TranslatableString(getKey(@"dates.ended_no_date"), @"Ended");

        /// <summary>
        /// "Starts {0}"
        /// </summary>
        public static LocalisableString DatesStartsDefault(string date) => new TranslatableString(getKey(@"dates.starts._"), @"Starts {0}", date);

        /// <summary>
        /// "soon™"
        /// </summary>
        public static LocalisableString DatesStartsSoon => new TranslatableString(getKey(@"dates.starts.soon"), @"soon™");

        /// <summary>
        /// "Entry Open"
        /// </summary>
        public static LocalisableString StatesEntry => new TranslatableString(getKey(@"states.entry"), @"Entry Open");

        /// <summary>
        /// "Voting Started"
        /// </summary>
        public static LocalisableString StatesVoting => new TranslatableString(getKey(@"states.voting"), @"Voting Started");

        /// <summary>
        /// "Results Out"
        /// </summary>
        public static LocalisableString StatesResults => new TranslatableString(getKey(@"states.results"), @"Results Out");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
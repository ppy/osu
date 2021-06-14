// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class MatchesStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Matches";

        /// <summary>
        /// "deleted beatmap"
        /// </summary>
        public static LocalisableString MatchBeatmapDeleted => new TranslatableString(getKey(@"match.beatmap-deleted"), @"deleted beatmap");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString MatchDifference(string difference) => new TranslatableString(getKey(@"match.difference"), @"by {0}", difference);

        /// <summary>
        /// "FAILED"
        /// </summary>
        public static LocalisableString MatchFailed => new TranslatableString(getKey(@"match.failed"), @"FAILED");

        /// <summary>
        /// "Multi Matches"
        /// </summary>
        public static LocalisableString MatchHeader => new TranslatableString(getKey(@"match.header"), @"Multi Matches");

        /// <summary>
        /// "(match in progress)"
        /// </summary>
        public static LocalisableString MatchInProgress => new TranslatableString(getKey(@"match.in-progress"), @"(match in progress)");

        /// <summary>
        /// "match in progress"
        /// </summary>
        public static LocalisableString MatchInProgressSpinnerLabel => new TranslatableString(getKey(@"match.in_progress_spinner_label"), @"match in progress");

        /// <summary>
        /// "Loading events..."
        /// </summary>
        public static LocalisableString MatchLoadingEvents => new TranslatableString(getKey(@"match.loading-events"), @"Loading events...");

        /// <summary>
        /// "{0} wins"
        /// </summary>
        public static LocalisableString MatchWinner(string team) => new TranslatableString(getKey(@"match.winner"), @"{0} wins", team);

        /// <summary>
        /// "{0} left the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerLeft(string user) => new TranslatableString(getKey(@"match.events.player-left"), @"{0} left the match", user);

        /// <summary>
        /// "{0} joined the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerJoined(string user) => new TranslatableString(getKey(@"match.events.player-joined"), @"{0} joined the match", user);

        /// <summary>
        /// "{0} has been kicked from the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerKicked(string user) => new TranslatableString(getKey(@"match.events.player-kicked"), @"{0} has been kicked from the match", user);

        /// <summary>
        /// "{0} created the match"
        /// </summary>
        public static LocalisableString MatchEventsMatchCreated(string user) => new TranslatableString(getKey(@"match.events.match-created"), @"{0} created the match", user);

        /// <summary>
        /// "the match was disbanded"
        /// </summary>
        public static LocalisableString MatchEventsMatchDisbanded => new TranslatableString(getKey(@"match.events.match-disbanded"), @"the match was disbanded");

        /// <summary>
        /// "{0} became the host"
        /// </summary>
        public static LocalisableString MatchEventsHostChanged(string user) => new TranslatableString(getKey(@"match.events.host-changed"), @"{0} became the host", user);

        /// <summary>
        /// "a player left the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerLeftNoUser => new TranslatableString(getKey(@"match.events.player-left-no-user"), @"a player left the match");

        /// <summary>
        /// "a player joined the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerJoinedNoUser => new TranslatableString(getKey(@"match.events.player-joined-no-user"), @"a player joined the match");

        /// <summary>
        /// "a player has been kicked from the match"
        /// </summary>
        public static LocalisableString MatchEventsPlayerKickedNoUser => new TranslatableString(getKey(@"match.events.player-kicked-no-user"), @"a player has been kicked from the match");

        /// <summary>
        /// "the match was created"
        /// </summary>
        public static LocalisableString MatchEventsMatchCreatedNoUser => new TranslatableString(getKey(@"match.events.match-created-no-user"), @"the match was created");

        /// <summary>
        /// "the match was disbanded"
        /// </summary>
        public static LocalisableString MatchEventsMatchDisbandedNoUser => new TranslatableString(getKey(@"match.events.match-disbanded-no-user"), @"the match was disbanded");

        /// <summary>
        /// "the host was changed"
        /// </summary>
        public static LocalisableString MatchEventsHostChangedNoUser => new TranslatableString(getKey(@"match.events.host-changed-no-user"), @"the host was changed");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString MatchScoreStatsAccuracy => new TranslatableString(getKey(@"match.score.stats.accuracy"), @"Accuracy");

        /// <summary>
        /// "Combo"
        /// </summary>
        public static LocalisableString MatchScoreStatsCombo => new TranslatableString(getKey(@"match.score.stats.combo"), @"Combo");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString MatchScoreStatsScore => new TranslatableString(getKey(@"match.score.stats.score"), @"Score");

        /// <summary>
        /// "Head-to-head"
        /// </summary>
        public static LocalisableString MatchTeamTypesHeadToHead => new TranslatableString(getKey(@"match.team-types.head-to-head"), @"Head-to-head");

        /// <summary>
        /// "Tag Co-op"
        /// </summary>
        public static LocalisableString MatchTeamTypesTagCoop => new TranslatableString(getKey(@"match.team-types.tag-coop"), @"Tag Co-op");

        /// <summary>
        /// "Team VS"
        /// </summary>
        public static LocalisableString MatchTeamTypesTeamVs => new TranslatableString(getKey(@"match.team-types.team-vs"), @"Team VS");

        /// <summary>
        /// "Tag Team VS"
        /// </summary>
        public static LocalisableString MatchTeamTypesTagTeamVs => new TranslatableString(getKey(@"match.team-types.tag-team-vs"), @"Tag Team VS");

        /// <summary>
        /// "Blue Team"
        /// </summary>
        public static LocalisableString MatchTeamsBlue => new TranslatableString(getKey(@"match.teams.blue"), @"Blue Team");

        /// <summary>
        /// "Red Team"
        /// </summary>
        public static LocalisableString MatchTeamsRed => new TranslatableString(getKey(@"match.teams.red"), @"Red Team");

        /// <summary>
        /// "Highest Score"
        /// </summary>
        public static LocalisableString GameScoringTypeScore => new TranslatableString(getKey(@"game.scoring-type.score"), @"Highest Score");

        /// <summary>
        /// "Highest Accuracy"
        /// </summary>
        public static LocalisableString GameScoringTypeAccuracy => new TranslatableString(getKey(@"game.scoring-type.accuracy"), @"Highest Accuracy");

        /// <summary>
        /// "Highest Combo"
        /// </summary>
        public static LocalisableString GameScoringTypeCombo => new TranslatableString(getKey(@"game.scoring-type.combo"), @"Highest Combo");

        /// <summary>
        /// "Score V2"
        /// </summary>
        public static LocalisableString GameScoringTypeScorev2 => new TranslatableString(getKey(@"game.scoring-type.scorev2"), @"Score V2");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
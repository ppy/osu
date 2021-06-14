// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class TournamentStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Tournament";

        /// <summary>
        /// "There are no tournaments running at the moment, please check back later!"
        /// </summary>
        public static LocalisableString IndexNoneRunning => new TranslatableString(getKey(@"index.none_running"), @"There are no tournaments running at the moment, please check back later!");

        /// <summary>
        /// "Registration: {0} to {1}"
        /// </summary>
        public static LocalisableString IndexRegistrationPeriod(string start, string end) => new TranslatableString(getKey(@"index.registration_period"), @"Registration: {0} to {1}", start, end);

        /// <summary>
        /// "Community Tournaments"
        /// </summary>
        public static LocalisableString IndexHeaderTitle => new TranslatableString(getKey(@"index.header.title"), @"Community Tournaments");

        /// <summary>
        /// "Registered players"
        /// </summary>
        public static LocalisableString IndexItemRegistered => new TranslatableString(getKey(@"index.item.registered"), @"Registered players");

        /// <summary>
        /// "Active Tournaments"
        /// </summary>
        public static LocalisableString IndexStateCurrent => new TranslatableString(getKey(@"index.state.current"), @"Active Tournaments");

        /// <summary>
        /// "Past Tournaments"
        /// </summary>
        public static LocalisableString IndexStatePrevious => new TranslatableString(getKey(@"index.state.previous"), @"Past Tournaments");

        /// <summary>
        /// "Support Your Team"
        /// </summary>
        public static LocalisableString ShowBanner => new TranslatableString(getKey(@"show.banner"), @"Support Your Team");

        /// <summary>
        /// "You are registered for this tournament.&lt;br&gt;&lt;br&gt;Please note that this does &lt;b&gt;not&lt;/b&gt; mean you have been assigned to a team.&lt;br&gt;&lt;br&gt;Further instructions will be sent to you via email closer to the tournament date, so please ensure your osu! account&#39;s email address is valid!"
        /// </summary>
        public static LocalisableString ShowEntered => new TranslatableString(getKey(@"show.entered"), @"You are registered for this tournament.<br><br>Please note that this does <b>not</b> mean you have been assigned to a team.<br><br>Further instructions will be sent to you via email closer to the tournament date, so please ensure your osu! account's email address is valid!");

        /// <summary>
        /// "Information Page"
        /// </summary>
        public static LocalisableString ShowInfoPage => new TranslatableString(getKey(@"show.info_page"), @"Information Page");

        /// <summary>
        /// "Please {0} to view registration details!"
        /// </summary>
        public static LocalisableString ShowLoginToRegister(string login) => new TranslatableString(getKey(@"show.login_to_register"), @"Please {0} to view registration details!", login);

        /// <summary>
        /// "You are not registered for this tournament."
        /// </summary>
        public static LocalisableString ShowNotYetEntered => new TranslatableString(getKey(@"show.not_yet_entered"), @"You are not registered for this tournament.");

        /// <summary>
        /// "Sorry, you do not meet the rank requirements for this tournament!"
        /// </summary>
        public static LocalisableString ShowRankTooLow => new TranslatableString(getKey(@"show.rank_too_low"), @"Sorry, you do not meet the rank requirements for this tournament!");

        /// <summary>
        /// "Registrations close on {0}"
        /// </summary>
        public static LocalisableString ShowRegistrationEnds(string date) => new TranslatableString(getKey(@"show.registration_ends"), @"Registrations close on {0}", date);

        /// <summary>
        /// "Cancel Registration"
        /// </summary>
        public static LocalisableString ShowButtonCancel => new TranslatableString(getKey(@"show.button.cancel"), @"Cancel Registration");

        /// <summary>
        /// "Sign me up!"
        /// </summary>
        public static LocalisableString ShowButtonRegister => new TranslatableString(getKey(@"show.button.register"), @"Sign me up!");

        /// <summary>
        /// "End"
        /// </summary>
        public static LocalisableString ShowPeriodEnd => new TranslatableString(getKey(@"show.period.end"), @"End");

        /// <summary>
        /// "Start"
        /// </summary>
        public static LocalisableString ShowPeriodStart => new TranslatableString(getKey(@"show.period.start"), @"Start");

        /// <summary>
        /// "Registration for this tournament has not yet opened."
        /// </summary>
        public static LocalisableString ShowStateBeforeRegistration => new TranslatableString(getKey(@"show.state.before_registration"), @"Registration for this tournament has not yet opened.");

        /// <summary>
        /// "This tournament has concluded. Check the information page for results."
        /// </summary>
        public static LocalisableString ShowStateEnded => new TranslatableString(getKey(@"show.state.ended"), @"This tournament has concluded. Check the information page for results.");

        /// <summary>
        /// "Registration for this tournament has closed. Check the information page for latest updates."
        /// </summary>
        public static LocalisableString ShowStateRegistrationClosed => new TranslatableString(getKey(@"show.state.registration_closed"), @"Registration for this tournament has closed. Check the information page for latest updates.");

        /// <summary>
        /// "This tournament is currently in progress. Check the information page for more details."
        /// </summary>
        public static LocalisableString ShowStateRunning => new TranslatableString(getKey(@"show.state.running"), @"This tournament is currently in progress. Check the information page for more details.");

        /// <summary>
        /// "{0} to {1}"
        /// </summary>
        public static LocalisableString TournamentPeriod(string start, string end) => new TranslatableString(getKey(@"tournament_period"), @"{0} to {1}", start, end);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Online
{
    /// <summary>
    /// A component that keeps track of the latest statistics for the local user.
    /// </summary>
    public partial class LocalUserStatisticsProvider : Component
    {
        private readonly Bindable<UserStatisticsUpdate> statisticsUpdate = new Bindable<UserStatisticsUpdate>();

        /// <summary>
        /// A bindable communicating updates to the local user's statistics on any ruleset.
        /// This does not guarantee the presence of old statistics, as it is invoked on initial population of statistics.
        /// </summary>
        public IBindable<UserStatisticsUpdate> StatisticsUpdate => statisticsUpdate;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<string, UserStatistics> statisticsCache = new Dictionary<string, UserStatistics>();
        private readonly Dictionary<string, GetUserRequest> statisticsRequests = new Dictionary<string, GetUserRequest>();

        /// <summary>
        /// Returns the <see cref="UserStatistics"/> currently available for the given ruleset.
        /// This may return null if the requested statistics has not been fetched before yet.
        /// </summary>
        /// <param name="ruleset">The ruleset to return the corresponding <see cref="UserStatistics"/> for.</param>
        public UserStatistics? GetStatisticsFor(RulesetInfo ruleset) => statisticsCache.GetValueOrDefault(ruleset.ShortName);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            api.LocalUser.BindValueChanged(_ => initialiseStatistics(), true);
        }

        private void initialiseStatistics()
        {
            statisticsCache.Clear();

            foreach (var ruleset in rulesets.AvailableRulesets.Where(r => r.IsLegacyRuleset()))
                RefetchStatistics(ruleset);
        }

        public void RefetchStatistics(RulesetInfo ruleset)
        {
            if (statisticsRequests.TryGetValue(ruleset.ShortName, out var previousRequest))
                previousRequest.Cancel();

            var request = statisticsRequests[ruleset.ShortName] = new GetUserRequest(api.LocalUser.Value.Id, ruleset);
            request.Success += u => UpdateStatistics(u.Statistics, ruleset);
            api.Queue(request);
        }

        protected void UpdateStatistics(UserStatistics newStatistics, RulesetInfo ruleset)
        {
            var oldStatistics = statisticsCache.GetValueOrDefault(ruleset.ShortName);

            statisticsRequests.Remove(ruleset.ShortName);
            statisticsCache[ruleset.ShortName] = newStatistics;
            statisticsUpdate.Value = new UserStatisticsUpdate(ruleset, oldStatistics, newStatistics);
        }
    }

    public record UserStatisticsUpdate(RulesetInfo Ruleset, UserStatistics? OldStatistics, UserStatistics NewStatistics);
}

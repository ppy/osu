// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
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
        /// <summary>
        /// Invoked whenever a change occured to the statistics of any ruleset,
        /// either due to change in local user (log out and log in) or as a result of score submission.
        /// </summary>
        /// <remarks>
        /// This does not guarantee the presence of the old statistics,
        /// specifically in the case of initial population or change in local user.
        /// </remarks>
        public event Action<UserStatisticsUpdate>? StatisticsUpdated;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly Dictionary<string, UserStatistics> statisticsCache = new Dictionary<string, UserStatistics>();

        /// <summary>
        /// Returns the <see cref="UserStatistics"/> currently available for the given ruleset.
        /// This may return null if the requested statistics has not been fetched before yet.
        /// </summary>
        /// <param name="ruleset">The ruleset to return the corresponding <see cref="UserStatistics"/> for.</param>
        public UserStatistics? GetStatisticsFor(RulesetInfo ruleset) => statisticsCache.GetValueOrDefault(ruleset.ShortName);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            api.LocalUser.BindValueChanged(_ =>
            {
                // queuing up requests directly on user change is unsafe, as the API status may have not been updated yet.
                // schedule a frame to allow the API to be in its correct state sending requests.
                Schedule(initialiseStatistics);
            }, true);
        }

        private void initialiseStatistics()
        {
            statisticsCache.Clear();

            if (api.LocalUser.Value == null || api.LocalUser.Value.Id <= 1)
                return;

            foreach (var ruleset in rulesets.AvailableRulesets.Where(r => r.IsLegacyRuleset()))
                RefetchStatistics(ruleset);
        }

        public void RefetchStatistics(RulesetInfo ruleset, Action<UserStatisticsUpdate>? callback = null)
        {
            if (!ruleset.IsLegacyRuleset())
                throw new InvalidOperationException($@"Retrieving statistics is not supported for ruleset {ruleset.ShortName}");

            var request = new GetUserRequest(api.LocalUser.Value.Id, ruleset);
            request.Success += u => UpdateStatistics(u.Statistics, ruleset, callback);
            api.Queue(request);
        }

        protected void UpdateStatistics(UserStatistics newStatistics, RulesetInfo ruleset, Action<UserStatisticsUpdate>? callback = null)
        {
            var oldStatistics = statisticsCache.GetValueOrDefault(ruleset.ShortName);
            statisticsCache[ruleset.ShortName] = newStatistics;

            var update = new UserStatisticsUpdate(ruleset, oldStatistics, newStatistics);
            callback?.Invoke(update);
            StatisticsUpdated?.Invoke(update);
        }
    }

    public record UserStatisticsUpdate(RulesetInfo Ruleset, UserStatistics? OldStatistics, UserStatistics NewStatistics);
}

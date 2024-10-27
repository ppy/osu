// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <summary>
        /// The statistics of the local user for the game-wide selected ruleset.
        /// </summary>
        public IBindable<UserStatistics?> Statistics => statistics;

        private readonly Bindable<UserStatistics?> statistics = new Bindable<UserStatistics?>();

        private readonly Dictionary<string, UserStatistics> allStatistics = new Dictionary<string, UserStatistics>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            statistics.BindValueChanged(v =>
            {
                if (api.LocalUser.Value != null && v.NewValue != null)
                    api.LocalUser.Value.Statistics = v.NewValue;
            });

            ruleset.BindValueChanged(_ => updateStatisticsBindable());

            api.LocalUser.BindValueChanged(_ =>
            {
                allStatistics.Clear();
                updateStatisticsBindable();
            }, true);
        }

        private GetUserRequest? currentRequest;

        private void updateStatisticsBindable() => Schedule(() =>
        {
            statistics.Value = null;

            if (api.LocalUser.Value == null || api.LocalUser.Value.OnlineID <= 1 || !ruleset.Value.IsLegacyRuleset())
            {
                statistics.Value = new UserStatistics();
                return;
            }

            if (currentRequest?.CompletionState == APIRequestCompletionState.Waiting)
            {
                currentRequest.Cancel();
                currentRequest = null;
            }

            if (allStatistics.TryGetValue(ruleset.Value.ShortName, out var existing))
                statistics.Value = existing;
            else
                requestStatistics(ruleset.Value);
        });

        private void requestStatistics(RulesetInfo ruleset)
        {
            currentRequest = new GetUserRequest(api.LocalUser.Value.OnlineID, ruleset);
            currentRequest.Success += u => statistics.Value = allStatistics[ruleset.ShortName] = u.Statistics;
            api.Queue(currentRequest);
        }

        /// <summary>
        /// Returns the <see cref="UserStatistics"/> currently available for the given ruleset.
        /// This may return null if the requested statistics has not been fetched yet.
        /// </summary>
        /// <param name="ruleset">The ruleset to return the corresponding <see cref="UserStatistics"/> for.</param>
        internal UserStatistics? GetStatisticsFor(RulesetInfo ruleset) => allStatistics.GetValueOrDefault(ruleset.ShortName);

        internal void UpdateStatistics(UserStatistics statistics, RulesetInfo statisticsRuleset)
        {
            allStatistics[statisticsRuleset.ShortName] = statistics;

            if (statisticsRuleset.ShortName == ruleset.Value.ShortName)
                updateStatisticsBindable();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select
{
    public class DifficultyRecommender : Component, IOnlineComponent
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Dictionary<RulesetInfo, double> recommendedStarDifficulty = new Dictionary<RulesetInfo, double>();

        [BackgroundDependencyLoader]
        private void load()
        {
            api.Register(this);

            ruleset.ValueChanged += _ => bestRulesetOrder = null;
        }

        /// <summary>
        /// Find the recommended difficulty from a selection of available difficulties for the current local user.
        /// </summary>
        /// <remarks>
        /// This requires the user to be online for now.
        /// </remarks>
        /// <param name="beatmaps">A collection of beatmaps to select a difficulty from.</param>
        /// <returns>The recommended difficulty, or null if a recommendation could not be provided.</returns>
        public BeatmapInfo GetRecommendedBeatmap(IEnumerable<BeatmapInfo> beatmaps)
        {
            if (!recommendedStarDifficulty.Any())
                return null;

            BeatmapInfo beatmap = null;

            foreach (var r in getBestRulesetOrder())
            {
                if (!recommendedStarDifficulty.TryGetValue(ruleset.Value, out var stars))
                    break;

                beatmap = beatmaps.Where(b => b.Ruleset.Equals(r)).OrderBy(b =>
                {
                    var difference = b.StarDifficulty - stars;
                    return difference >= 0 ? difference * 2 : difference * -1; // prefer easier over harder
                }).FirstOrDefault();

                if (beatmap != null)
                    break;
            }

            return beatmap;
        }

        private void calculateRecommendedDifficulties()
        {
            rulesets.AvailableRulesets.ForEach(rulesetInfo =>
            {
                var req = new GetUserRequest(api.LocalUser.Value.Id, rulesetInfo);

                req.Success += result =>
                {
                    // algorithm taken from https://github.com/ppy/osu-web/blob/e6e2825516449e3d0f3f5e1852c6bdd3428c3437/app/Models/User.php#L1505
                    recommendedStarDifficulty[rulesetInfo] = Math.Pow((double)(result.Statistics.PP ?? 0), 0.4) * 0.195;
                };

                api.Queue(req);
            });
        }

        private IEnumerable<RulesetInfo> bestRulesetOrder;

        private IEnumerable<RulesetInfo> getBestRulesetOrder()
        {
            if (bestRulesetOrder != null)
                return bestRulesetOrder;

            var otherRulesets = recommendedStarDifficulty.ToList()
                                                         .Where(pair => !pair.Key.Equals(ruleset.Value))
                                                         .OrderBy(pair => pair.Value)
                                                         .Select(pair => pair.Key)
                                                         .Reverse();

            var rulesetList = new List<RulesetInfo>(new[] { ruleset.Value });
            rulesetList.AddRange(otherRulesets);

            bestRulesetOrder = rulesetList;
            return rulesetList;
        }

        public void APIStateChanged(IAPIProvider api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    calculateRecommendedDifficulties();
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            api.Unregister(this);
        }
    }
}

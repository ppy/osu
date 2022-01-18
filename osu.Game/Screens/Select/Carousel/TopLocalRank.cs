// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Screens.Select.Carousel
{
    public class TopLocalRank : UpdateableRank
    {
        private readonly BeatmapInfo beatmapInfo;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private RealmContextFactory realmFactory { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        public TopLocalRank(BeatmapInfo beatmapInfo)
            : base(null)
        {
            this.beatmapInfo = beatmapInfo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ruleset.ValueChanged += _ => fetchAndLoadTopScore();

            fetchAndLoadTopScore();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreSubscription = realmFactory.Context.All<ScoreInfo>()
                                            .Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} = $0", beatmapInfo.ID)
                                            .QueryAsyncWithNotifications((_, changes, ___) =>
                                            {
                                                if (changes == null)
                                                    return;

                                                fetchTopScoreRank();
                                            });
        }

        private IDisposable scoreSubscription;

        private ScheduledDelegate scheduledRankUpdate;

        private void fetchAndLoadTopScore()
        {
            // TODO: this lookup likely isn't required, we can use the results of the subscription directly.
            var rank = fetchTopScoreRank();

            scheduledRankUpdate = Scheduler.Add(() =>
            {
                Rank = rank;

                // Required since presence is changed via IsPresent override
                Invalidate(Invalidation.Presence);
            });
        }

        // We're present if a rank is set, or if there is a pending rank update (IsPresent = true is required for the scheduler to run).
        public override bool IsPresent => base.IsPresent && (Rank != null || scheduledRankUpdate?.Completed == false);

        private ScoreRank? fetchTopScoreRank()
        {
            if (realmFactory == null || beatmapInfo == null || ruleset?.Value == null || api?.LocalUser.Value == null)
                return null;

            using (var realm = realmFactory.CreateContext())
            {
                return realm.All<ScoreInfo>()
                            .AsEnumerable()
                            // TODO: update to use a realm filter directly (or at least figure out the beatmap part to reduce scope).
                            .Where(s => s.UserID == api.LocalUser.Value.Id && s.BeatmapInfoID == beatmapInfo.ID && s.RulesetID == ruleset.Value.ID && !s.DeletePending)
                            .OrderByDescending(s => s.TotalScore)
                            .FirstOrDefault()
                            ?.Rank;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            scoreSubscription?.Dispose();
        }
    }
}

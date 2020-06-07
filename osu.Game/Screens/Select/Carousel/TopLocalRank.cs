// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Screens.Select.Carousel
{
    public class TopLocalRank : UpdateableRank
    {
        private readonly BeatmapInfo beatmap;

        [Resolved]
        private ScoreManager scores { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private IBindable<WeakReference<ScoreInfo>> itemUpdated;
        private IBindable<WeakReference<ScoreInfo>> itemRemoved;

        public TopLocalRank(BeatmapInfo beatmap)
            : base(null)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            itemUpdated = scores.ItemUpdated.GetBoundCopy();
            itemUpdated.BindValueChanged(scoreChanged);

            itemRemoved = scores.ItemRemoved.GetBoundCopy();
            itemRemoved.BindValueChanged(scoreChanged);

            ruleset.ValueChanged += _ => fetchAndLoadTopScore();

            fetchAndLoadTopScore();
        }

        private void scoreChanged(ValueChangedEvent<WeakReference<ScoreInfo>> weakScore)
        {
            if (weakScore.NewValue.TryGetTarget(out var score))
            {
                if (score.BeatmapInfoID == beatmap.ID)
                    fetchAndLoadTopScore();
            }
        }

        private ScheduledDelegate scheduledRankUpdate;

        private void fetchAndLoadTopScore()
        {
            var rank = fetchTopScore()?.Rank;
            scheduledRankUpdate = Schedule(() =>
            {
                Rank = rank;

                // Required since presence is changed via IsPresent override
                Invalidate(Invalidation.Presence);
            });
        }

        // We're present if a rank is set, or if there is a pending rank update (IsPresent = true is required for the scheduler to run).
        public override bool IsPresent => base.IsPresent && (Rank != null || scheduledRankUpdate?.Completed == false);

        private ScoreInfo fetchTopScore()
        {
            if (scores == null || beatmap == null || ruleset?.Value == null || api?.LocalUser.Value == null)
                return null;

            return scores.QueryScores(s => s.UserID == api.LocalUser.Value.Id && s.BeatmapInfoID == beatmap.ID && s.RulesetID == ruleset.Value.ID && !s.DeletePending)
                         .OrderByDescending(s => s.TotalScore)
                         .FirstOrDefault();
        }
    }
}

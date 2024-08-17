// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osuTK;
using Realms;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class TopGlobalRank : CompositeDrawable
    {
        private readonly BeatmapInfo beatmapInfo;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly UpdateableRank updateable;

        private GetScoresRequest? getScoresRequest;

        private IDisposable? scoreSubscription;
        private Task? realmWriteTask;

        public ScoreRank? DisplayedRank => updateable.Rank;

        public TopGlobalRank(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;

            AutoSizeAxes = Axes.Both;

            InternalChild = updateable = new UpdateableRank
            {
                Size = new Vector2(40, 20),
                Alpha = 0,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(rulesetInfo =>
            {
                scoreSubscription?.Dispose();
                scoreSubscription = realm.RegisterForNotifications(r =>
                        r.All<ScoreInfo>()
                         .Filter($"{nameof(ScoreInfo.User)}.{nameof(RealmUser.OnlineID)} == $0"
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $1"
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                 + $" && {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $2"
                                 + $" && {nameof(ScoreInfo.DeletePending)} == false", api.LocalUser.Value.Id, beatmapInfo.ID, ruleset.Value.ShortName),
                    (sender, changes) => localScoresChanged(sender, changes, rulesetInfo.NewValue));

                getScoresRequest?.Cancel();
                if (beatmapInfo.UserRank.GetRankByRulesetInfo(rulesetInfo.NewValue) == null)
                {
                    getScoresRequest = new GetScoresRequest(beatmapInfo, rulesetInfo.NewValue);
                    getScoresRequest.Success += scores =>
                    {
                        if (scores.Scores.Count > 0)
                        {
                            SoloScoreInfo? topScore = scores.UserScore?.Score;
                            if (topScore != null)
                            {
                                rankChanged(rulesetInfo.NewValue, topScore.Rank);
                            }
                        }
                    };

                    api.Queue(getScoresRequest);
                }

                updateRank(rulesetInfo.NewValue);
            }, true);

            void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes, RulesetInfo rulesetInfo)
            {
                // This subscription may fire from changes to linked beatmaps, which we don't care about.
                // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
                if (changes?.HasCollectionChanges() == false)
                    return;

                ScoreInfo? topScore = sender?.MaxBy(info => (info.TotalScore, -info.Date.UtcDateTime.Ticks));
                ScoreRank? oldRank = beatmapInfo.UserRank.GetRankByRulesetInfo(rulesetInfo);
                if (topScore != null && (oldRank == null || (int)oldRank < (int)topScore.Rank))
                {
                    // Update global rank, if a new local replay it's ranked.
                    // Update global rank, if a new local replay it will be a new record for an unranked beatmap.
                    // Update global rank, if a new local replay ruleset doesn't equals original beatmap ruleset.
                    if (topScore.Ranked || beatmapInfo.Status.GrantsPerformancePoints() == false || topScore.Ruleset.ShortName != beatmapInfo.Ruleset.ShortName)
                    {
                        rankChanged(rulesetInfo, topScore.Rank);
                    }
                }
            }

            void updateRank(RulesetInfo rulesetInfo, IRealmCollection<ScoreInfo>? sender = null)
            {
                if (beatmapInfo.UserRank.GetRankByRulesetInfo(rulesetInfo) != null)
                {
                    // Try show global rank
                    updateable.Rank = beatmapInfo.UserRank.GetRankByRulesetInfo(rulesetInfo);
                    updateable.Alpha = 1;
                }
                else if (sender != null)
                {
                    // Try show local rank
                    ScoreInfo? topScore = sender.MaxBy(info => (info.TotalScore, -info.Date.UtcDateTime.Ticks));
                    updateable.Rank = topScore?.Rank;
                    updateable.Alpha = topScore != null ? 1 : 0;
                }
                else
                {
                    updateable.Alpha = 0;
                }
            }

            void rankChanged(RulesetInfo rulesetInfo, ScoreRank newRank)
            {
                Scheduler.AddOnce(setNewRank);

                void setNewRank()
                {
                    if (realmWriteTask?.IsCompleted == false)
                    {
                        Scheduler.AddOnce(setNewRank);
                        return;
                    }

                    realmWriteTask = realm.WriteAsync(r =>
                    {
                        BeatmapInfo? beatmapInfo = r.Find<BeatmapInfo>(this.beatmapInfo.ID);

                        if (beatmapInfo == null)
                            return;

                        var userRank = beatmapInfo.UserRank;
                        if ((int?)userRank.GetRankByRulesetInfo(rulesetInfo) != (int)newRank)
                        {
                            userRank.SetRankByRulesetInfo(rulesetInfo, newRank);
                        }

                        updateRank(rulesetInfo);
                    });
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            scoreSubscription?.Dispose();
            Schedule(() =>
            {
                getScoresRequest?.Cancel();
            });
        }
    }
}

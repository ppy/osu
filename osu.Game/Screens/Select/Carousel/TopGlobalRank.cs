// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
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

            ruleset.BindValueChanged(_ =>
            {
                scoreSubscription?.Dispose();
                scoreSubscription = realm.RegisterForNotifications(r =>
                        r.All<ScoreInfo>()
                         .Filter($"{nameof(ScoreInfo.User)}.{nameof(RealmUser.OnlineID)} == $0"
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $1"
                                 + $" && {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                 + $" && {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $2"
                                 + $" && {nameof(ScoreInfo.DeletePending)} == false", api.LocalUser.Value.Id, beatmapInfo.ID, ruleset.Value.ShortName),
                    localScoresChanged);
                if (beatmapInfo.UserRank.Rank >= (int)ScoreRank.F)
                {
                    updateRank();
                }
                else if (beatmapInfo.UserRank.Exists)
                {
                    getScoresRequest = new GetScoresRequest(beatmapInfo, beatmapInfo.Ruleset);
                    getScoresRequest.Success += scores =>
                    {
                        if (scores.Scores.Count > 0)
                        {
                            SoloScoreInfo? topScore = scores.UserScore?.Score;
                            if (topScore != null)
                            {
                                rankChanged(topScore.Rank);
                            }
                            else if (beatmapInfo.UserRank.Exists)
                            {
                                // Makes ignore search before getting first ranked score.
                                // It's broken updating ranks between devices.
                                realmWriteTask = realm.WriteAsync(r =>
                                {
                                    BeatmapInfo? beatmapInfo = r.Find<BeatmapInfo>(this.beatmapInfo.ID);

                                    if (beatmapInfo == null)
                                        return;

                                    beatmapInfo.UserRank.Exists = false;
                                });
                            }
                        }
                    };

                    api.Queue(getScoresRequest);
                }
            }, true);

            void localScoresChanged(IRealmCollection<ScoreInfo> sender, ChangeSet? changes)
            {
                // This subscription may fire from changes to linked beatmaps, which we don't care about.
                // It's currently not possible for a score to be modified after insertion, so we can safely ignore callbacks with only modifications.
                if (changes?.HasCollectionChanges() == false)
                    return;

                ScoreInfo? topScore = sender?.MaxBy(info => (info.TotalScore, -info.Date.UtcDateTime.Ticks));
                if (topScore != null && topScore.Ranked && beatmapInfo.UserRank.Rank < (int)topScore.Rank)
                {
                    rankChanged(topScore.Rank);
                }

                updateRank(sender);
            }

            updateRank();

            void updateRank(IRealmCollection<ScoreInfo>? sender = null)
            {
                if (beatmapInfo.UserRank.Rank >= (int)ScoreRank.F)
                {
                    // Try show global rank
                    updateable.Rank = (ScoreRank?)beatmapInfo.UserRank.Rank;
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

            void rankChanged(ScoreRank newRank)
            {
                Scheduler.AddOnce(writeNewRank);

                void writeNewRank()
                {
                    if (realmWriteTask?.IsCompleted == false)
                    {
                        Scheduler.AddOnce(writeNewRank);
                        return;
                    }

                    realmWriteTask = realm.WriteAsync(r =>
                    {
                        BeatmapInfo? beatmapInfo = r.Find<BeatmapInfo>(this.beatmapInfo.ID);

                        if (beatmapInfo == null)
                            return;

                        var userRank = beatmapInfo.UserRank;
                        if (userRank.Rank != (int)newRank)
                        {
                            userRank.Exists = true;
                            userRank.Rank = (int)newRank;
                        }

                        updateRank();
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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class LeaderboardProvider : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private IQueryable<ScoreInfo> getLocalScoresFor(Realm r, BeatmapInfo beatmap, RulesetInfo ruleset)
        {
            return r.All<ScoreInfo>().Filter($"{nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.ID)} == $0"
                                             + $" AND {nameof(ScoreInfo.BeatmapInfo)}.{nameof(BeatmapInfo.Hash)} == {nameof(ScoreInfo.BeatmapHash)}"
                                             + $" AND {nameof(ScoreInfo.Ruleset)}.{nameof(RulesetInfo.ShortName)} == $1"
                                             + $" AND {nameof(ScoreInfo.DeletePending)} == false", beatmap.ID, ruleset.ShortName);
        }

        public IEnumerable<ScoreInfo> GetLocalScoresFor(BeatmapInfo beatmap, RulesetInfo ruleset) => realm.Run(r => getLocalScoresFor(r, beatmap, ruleset)).AsEnumerable();

        public IDisposable SubscribeToLocalScores(BeatmapInfo beatmap, RulesetInfo ruleset, NotificationCallbackDelegate<ScoreInfo> onChange)
            => realm.RegisterForNotifications(r => getLocalScoresFor(r, beatmap, ruleset), onChange);

        public Task<(IEnumerable<ScoreInfo> best, ScoreInfo? userScore)> GetOnlineScoresAsync(BeatmapInfo beatmap, RulesetInfo ruleset, IReadOnlyList<Mod>? mods, BeatmapLeaderboardScope scope,
                                                                                              CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Mod>? requestMods = mods;

            if (mods != null && !mods.Any())
                // add nomod for the request
                requestMods = new Mod[] { new ModNoMod() };

            var tcs = new TaskCompletionSource<(IEnumerable<ScoreInfo>, ScoreInfo?)>();
            var newRequest = new GetScoresRequest(beatmap, ruleset, scope, requestMods);
            newRequest.Success += response =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled(cancellationToken);
                    return;
                }

                // Request may have changed since fetch request.
                IEnumerable<ScoreInfo> newScores = response.Scores.Select(s => s.ToScoreInfo(rulesets, beatmap)).OrderByTotalScore().ToArray();
                var userScore = response.UserScore?.CreateScoreInfo(rulesets, beatmap);

                tcs.SetResult((newScores, userScore));
            };
            newRequest.Failure += ex =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled(cancellationToken);
                    return;
                }

                tcs.SetException(ex);
            };
            api.Queue(newRequest);
            return tcs.Task;
        }
    }
}

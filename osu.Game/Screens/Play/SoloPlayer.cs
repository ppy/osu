// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Play
{
    public partial class SoloPlayer : SubmittingPlayer
    {
        public SoloPlayer(StateTrackingLeaderboardProvider? leaderboardScores, PlayerConfiguration? configuration = null)
            : base(configuration)
        {
            this.leaderboardScores.AddRange(leaderboardScores?.Scores.Value?.best ?? []);
        }

        protected override APIRequest<APIScoreToken>? CreateTokenRequest()
        {
            int beatmapId = Beatmap.Value.BeatmapInfo.OnlineID;
            int rulesetId = Ruleset.Value.OnlineID;

            if (beatmapId <= 0)
                return null;

            if (!Ruleset.Value.IsLegacyRuleset())
                return null;

            return new CreateSoloScoreRequest(Beatmap.Value.BeatmapInfo, rulesetId, Game.VersionHash);
        }

        private readonly BindableList<ScoreInfo> leaderboardScores = new BindableList<ScoreInfo>();

        protected override GameplayLeaderboard CreateGameplayLeaderboard() =>
            new SoloGameplayLeaderboard(Score.ScoreInfo.User)
            {
                AlwaysVisible = { Value = false },
                Scores = { BindTarget = leaderboardScores }
            };

        protected override bool ShouldExitOnTokenRetrievalFailure(Exception exception) => false;

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            IBeatmapInfo beatmap = score.ScoreInfo.BeatmapInfo!;

            Debug.Assert(beatmap.OnlineID > 0);

            return new SubmitSoloScoreRequest(score.ScoreInfo, token, beatmap.OnlineID);
        }
    }
}

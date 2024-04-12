// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public partial class SoloPlayer : SubmittingPlayer
    {
        public SoloPlayer()
            : this(null)
        {
        }

        protected SoloPlayer(PlayerConfiguration configuration = null)
            : base(configuration)
        {
        }

        protected override APIRequest<APIScoreToken> CreateTokenRequest()
        {
            int beatmapId = Beatmap.Value.BeatmapInfo.OnlineID;
            int rulesetId = Ruleset.Value.OnlineID;

            if (beatmapId <= 0)
                return null;

            if (!Ruleset.Value.IsLegacyRuleset())
                return null;

            return new CreateSoloScoreRequest(Beatmap.Value.BeatmapInfo, rulesetId, Game.VersionHash);
        }

        public readonly BindableList<ScoreInfo> LeaderboardScores = new BindableList<ScoreInfo>();

        protected override GameplayLeaderboard CreateGameplayLeaderboard() =>
            new SoloGameplayLeaderboard(Score.ScoreInfo.User)
            {
                AlwaysVisible = { Value = false },
                Scores = { BindTarget = LeaderboardScores }
            };

        protected override bool ShouldExitOnTokenRetrievalFailure(Exception exception) => false;

        protected override Task ImportScore(Score score)
        {
            // Before importing a score, stop binding the leaderboard with its score source.
            // This avoids a case where the imported score may cause a leaderboard refresh
            // (if the leaderboard's source is local).
            LeaderboardScores.UnbindBindings();

            return base.ImportScore(score);
        }

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            IBeatmapInfo beatmap = score.ScoreInfo.BeatmapInfo;

            Debug.Assert(beatmap!.OnlineID > 0);

            return new SubmitSoloScoreRequest(score.ScoreInfo, token, beatmap.OnlineID);
        }

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score)
        {
            AllowRetry = true,
            ShowUserStatistics = true,
            Scores = { BindTarget = LeaderboardScores }
        };
    }
}

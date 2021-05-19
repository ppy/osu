// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class SoloPlayer : SubmittingPlayer
    {
        protected override APIRequest<APIScoreToken> CreateTokenRequest()
        {
            if (!(Beatmap.Value.BeatmapInfo.OnlineBeatmapID is int beatmapId))
                return null;

            if (!(Ruleset.Value.ID is int rulesetId))
                return null;

            return new CreateSoloScoreRequest(beatmapId, rulesetId, Game.VersionHash);
        }

        protected override bool HandleTokenRetrievalFailure(Exception exception) => false;

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            Debug.Assert(Beatmap.Value.BeatmapInfo.OnlineBeatmapID != null);

            int beatmapId = Beatmap.Value.BeatmapInfo.OnlineBeatmapID.Value;

            return new SubmitSoloScoreRequest(beatmapId, token, score.ScoreInfo);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class SoloPlayer : SubmittingPlayer
    {
        public override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, int token)
        {
            throw new System.NotImplementedException();
        }

        protected override APIRequest<APIScoreToken> CreateTokenRequestRequest()
        {
            throw new System.NotImplementedException();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A player instance which supports submitting scores to an online store.
    /// </summary>
    public abstract class SubmittingPlayer : Player
    {
        protected long? Token { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        protected SubmittingPlayer(PlayerConfiguration configuration = null)
            : base(configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Token = null;

            bool failed = false;

            var req = CreateTokenRequestRequest();
            req.Success += r => Token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                if (string.IsNullOrEmpty(e.Message))
                    Logger.Error(e, "Failed to retrieve a score submission token.");
                else
                    Logger.Log($"You are not able to submit a score: {e.Message}", level: LogLevel.Important);

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            };

            api.Queue(req);

            while (!failed && !Token.HasValue)
                Thread.Sleep(1000);
        }

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            Debug.Assert(Token != null);

            var tcs = new TaskCompletionSource<bool>();
            var request = CreateSubmissionRequest(score, Token.Value);

            request.Success += s =>
            {
                score.ScoreInfo.OnlineScoreID = s.ID;
                tcs.SetResult(true);
            };

            request.Failure += e =>
            {
                Logger.Error(e, "Failed to submit score");
                tcs.SetResult(false);
            };

            api.Queue(request);
            await tcs.Task.ConfigureAwait(false);
        }

        protected abstract APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token);

        protected abstract APIRequest<APIScoreToken> CreateTokenRequestRequest();
    }
}

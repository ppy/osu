// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Play
{
    public class TimeshiftPlayer : Player
    {
        public Action Exited;

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        private readonly PlaylistItem playlistItem;

        [Resolved]
        private APIAccess api { get; set; }

        public TimeshiftPlayer(PlaylistItem playlistItem)
        {
            this.playlistItem = playlistItem;
        }

        private int? token;

        [BackgroundDependencyLoader]
        private void load()
        {
            token = null;

            bool failed = false;

            var req = new CreateRoomScoreRequest(roomId.Value ?? 0, playlistItem.ID);
            req.Success += r => token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                Logger.Error(e, "Failed to retrieve a score submission token.");

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            };

            api.Queue(req);

            while (!failed && !token.HasValue)
                Thread.Sleep(1000);
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Exited?.Invoke();

            return false;
        }

        protected override ScoreInfo CreateScore()
        {
            submitScore();
            return base.CreateScore();
        }

        private void submitScore()
        {
            var score = base.CreateScore();

            score.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());

            Debug.Assert(token != null);

            var request = new SubmitRoomScoreRequest(token.Value, roomId.Value ?? 0, playlistItem.ID, score);
            request.Failure += e => Logger.Error(e, "Failed to submit score");
            api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }

        protected override Results CreateResults(ScoreInfo score) => new MatchResults(score);
    }
}

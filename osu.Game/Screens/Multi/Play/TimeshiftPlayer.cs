// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Logging;
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
        private readonly Room room;
        private readonly int playlistItemId;

        [Resolved]
        private APIAccess api { get; set; }

        public TimeshiftPlayer(Room room, int playlistItemId)
        {
            this.room = room;
            this.playlistItemId = playlistItemId;
        }

        private int? token;

        [BackgroundDependencyLoader]
        private void load()
        {
            token = null;

            bool failed = false;

            var req = new CreateRoomScoreRequest(room.RoomID.Value ?? 0, playlistItemId);
            req.Success += r => token = r.ID;
            req.Failure += e =>
            {
                failed = true;

                Logger.Error(e, "Failed to retrieve a score submission token.");

                Schedule(() =>
                {
                    ValidForResume = false;
                    Exit();
                });
            };

            api.Queue(req);

            while (!failed && !token.HasValue)
                Thread.Sleep(1000);
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

            var request = new SubmitRoomScoreRequest(token.Value, room.RoomID.Value ?? 0, playlistItemId, score);
            request.Failure += e => Logger.Error(e, "Failed to submit score");
            api.Queue(request);
        }

        protected override Results CreateResults(ScoreInfo score) => new MatchResults(score, room);
    }
}

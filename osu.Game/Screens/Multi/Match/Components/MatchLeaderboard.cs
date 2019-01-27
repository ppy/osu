// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class MatchLeaderboard : Leaderboard<MatchLeaderboardScope, APIRoomScoreInfo>
    {
        public Action<IEnumerable<APIRoomScoreInfo>> ScoresLoaded;

        public Room Room
        {
            get => bindings.Room;
            set => bindings.Room = value;
        }

        private readonly RoomBindings bindings = new RoomBindings();

        [BackgroundDependencyLoader]
        private void load()
        {
            bindings.RoomID.BindValueChanged(id =>
            {
                if (id == null)
                    return;

                Scores = null;
                UpdateScores();
            }, true);
        }

        protected override APIRequest FetchScores(Action<IEnumerable<APIRoomScoreInfo>> scoresCallback)
        {
            if (bindings.RoomID.Value == null)
                return null;

            var req = new GetRoomScoresRequest(bindings.RoomID.Value ?? 0);

            req.Success += r =>
            {
                scoresCallback?.Invoke(r);
                ScoresLoaded?.Invoke(r);
            };

            return req;
        }

        protected override LeaderboardScore CreateDrawableScore(APIRoomScoreInfo model, int index) => new MatchLeaderboardScore(model, index);
    }

    public enum MatchLeaderboardScope
    {
        Overall
    }
}

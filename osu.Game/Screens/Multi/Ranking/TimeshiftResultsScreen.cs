// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking
{
    public class TimeshiftResultsScreen : ResultsScreen
    {
        private readonly int roomId;
        private readonly PlaylistItem playlistItem;

        private LoadingSpinner loadingLayer;

        public TimeshiftResultsScreen(ScoreInfo score, int roomId, PlaylistItem playlistItem, bool allowRetry = true)
            : base(score, allowRetry)
        {
            this.roomId = roomId;
            this.playlistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                X = -10,
                State = { Value = Score == null ? Visibility.Visible : Visibility.Hidden },
                Padding = new MarginPadding { Bottom = TwoLayerButton.SIZE_EXTENDED.Y }
            });
        }

        protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
        {
            var req = new GetRoomPlaylistScoresRequest(roomId, playlistItem.ID);

            req.Success += r =>
            {
                scoresCallback?.Invoke(r.Scores.Where(s => s.ID != Score?.OnlineScoreID).Select(s => s.CreateScoreInfo(playlistItem)));
                loadingLayer.Hide();
            };

            req.Failure += _ => loadingLayer.Hide();

            return req;
        }
    }
}

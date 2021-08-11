// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerResultsScreen : PlaylistsResultsScreen
    {
        private readonly SortedDictionary<int, BindableInt> teamScores;

        public MultiplayerResultsScreen(ScoreInfo score, long roomId, PlaylistItem playlistItem, SortedDictionary<int, BindableInt> teamScores)
            : base(score, roomId, playlistItem, false, false)
        {
            this.teamScores = teamScores;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (teamScores.Count == 2)
            {
                LoadComponentAsync(new MatchScoreDisplay
                {
                    Team1Score = { BindTarget = teamScores.First().Value },
                    Team2Score = { BindTarget = teamScores.Last().Value },
                }, AddInternal);
            }
        }
    }
}

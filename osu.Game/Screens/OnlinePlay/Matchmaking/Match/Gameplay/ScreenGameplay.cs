// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.Gameplay
{
    public partial class ScreenGameplay : MultiplayerPlayer
    {
        public ScreenGameplay(Room room, PlaylistItem playlistItem, MultiplayerRoomUser[] users)
            : base(room, playlistItem, users)
        {
        }

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            Scheduler.Add(() =>
            {
                if (this.IsCurrentScreen())
                    this.Exit();
            });
        }
    }
}

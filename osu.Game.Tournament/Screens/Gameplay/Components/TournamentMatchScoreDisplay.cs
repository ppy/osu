// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TournamentMatchScoreDisplay : MatchScoreDisplay
    {
        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            Team1Score.BindTo(ipc.Score1);
            Team2Score.BindTo(ipc.Score2);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.RoundWarmup
{
    /// <summary>
    /// Shown during <see cref="MatchmakingStage.RoundWarmupTime"/>
    /// </summary>
    public partial class SubScreenRoundWarmup : MatchmakingSubScreen
    {
        public override PanelDisplayStyle PlayersDisplayStyle => PanelDisplayStyle.Grid;
        public override Drawable PlayersDisplayArea => this;

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.MoveToX(0);
        }
    }
}

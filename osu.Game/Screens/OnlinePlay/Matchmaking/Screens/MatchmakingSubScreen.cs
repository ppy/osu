// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens
{
    public partial class MatchmakingSubScreen : Screen
    {
        public MatchmakingSubScreen()
        {
            RelativePositionAxes = Axes.X;
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.MoveToX(1).MoveToX(0, 200);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.MoveToX(-1, 200);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.MoveToX(0, 200);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            this.MoveToX(1, 200);
            return false;
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle
{
    public partial class IdleScreen : MatchmakingSubScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new PlayerPanelList
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.MoveToX(0);
        }
    }
}

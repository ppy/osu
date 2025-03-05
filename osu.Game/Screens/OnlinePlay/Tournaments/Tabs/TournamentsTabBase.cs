// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs
{
    public abstract partial class TournamentsTabBase : CompositeDrawable
    {
        [Resolved]
        protected TournamentInfo TournamentInfo { get; private set; } = null!;

        public abstract TournamentsTab TabType { get; }

        protected TournamentsTabBase()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0.0f;
        }
    }
}

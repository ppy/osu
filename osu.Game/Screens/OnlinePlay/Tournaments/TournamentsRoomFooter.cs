// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Tournaments.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsRoomFooter : CompositeDrawable
    {
        public TournamentsRoomSubScreen TournamentScreen;

        private TournamentsRoomFooterButton[] tabButtons = [];

        public TournamentsRoomFooter(TournamentsRoomSubScreen subScreen)
        {
            TournamentScreen = subScreen;
            TournamentScreen.TournamentInfo.UpdateTabVisibility = updateTabVisibility;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Direction = FillDirection.Horizontal,
                Position = new Vector2(170, 0),
                Spacing = new Vector2(8),
                Children = tabButtons = createFooterButtons()
            };
        }
        private TournamentsRoomFooterButton createFooterButton(TournamentsTabs tab)
        {
            return new TournamentsRoomFooterButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                TabType = tab,
                Action = () => TournamentScreen.ChangeTab(tab),
                TabText = TournamentsRoomSubScreen.GetTournamentsTabsName(tab),
                Alpha = TournamentScreen.TournamentInfo.GetTabVisibility(tab) ? 1.0f : 0.0f, // Want this to hide tab on creation if not visible
            };
        }

        private TournamentsRoomFooterButton[] createFooterButtons()
        {
            return [..
                from TournamentsTabs visible_tab in Enum.GetValues(typeof(TournamentsTabs))
                select createFooterButton(visible_tab)];
        }

        private void updateTabVisibility(TournamentsTabs tab, bool is_visible)
        {
            foreach (TournamentsRoomFooterButton child in tabButtons)
            {
                if (child.TabType == tab)
                {
                    if (is_visible) child.Show(); else child.Hide();
                }
            }
        }
    }
}

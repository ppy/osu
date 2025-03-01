// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Screens.OnlinePlay.Tournaments.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsRoomFooter : CompositeDrawable
    {
        public TournamentsRoomSubScreen TournamentScreen;

        [Resolved]
        private TournamentInfo tournamentInfo { get; set; } = null!;

        private TournamentsRoomFooterButton[] tabButtons = [];

        public TournamentsRoomFooter(TournamentsRoomSubScreen subScreen)
        {
            TournamentScreen = subScreen;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            tournamentInfo.UpdateTabVisibility = updateTabVisibility;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // For some reason after hiding disabled footer buttons, they still show on screen.
            // Tried turning all footer buttons' AlwaysPresent to false.
            // I am literally stuck with this, cannot make it work.
            foreach (TournamentsRoomFooterButton tabButton in tabButtons)
            {
                if (!tournamentInfo.GetTabVisibility(tabButton.TabType))
                    tabButton.Hide();
                Logger.Log("Visibility - " + tabButton.TabType + ", " + tabButton.Alpha + ", " +
                tournamentInfo.GetTabVisibility(tabButton.TabType).ToString());
            }
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
                // Alpha = TournamentScreen.TournamentInfo.GetTabVisibility(tab) ? 1.0f : 0.0f, // Want this to hide tab on creation if not visible
            };
        }

        private TournamentsRoomFooterButton[] createFooterButtons()
        {
            return [..
                from TournamentsTabs tab in Enum.GetValues(typeof(TournamentsTabs))
                select createFooterButton(tab)];
        }

        private void updateTabVisibility(TournamentsTabs tab, bool is_visible)
        {
            Logger.Log("Updated Visibility - " + tab + ", " + is_visible.ToString());
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

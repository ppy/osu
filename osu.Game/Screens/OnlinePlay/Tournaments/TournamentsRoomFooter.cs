// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsRoomFooter : Container
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Direction = FillDirection.Horizontal,
                Position = new Vector2(170, 0),
                Children = createFooterButtons()
            };
        }

        private TournamentsRoomFooterButton createFooterButton(TournamentsTabs tab)
        {
            return new TournamentsRoomFooterButton
            {
                TabType = tab,
                Text = TournamentsRoomSubScreen.GetTournamentsTabsName(tab),
            };
        }

        private TournamentsRoomFooterButton[] createFooterButtons()
        {
            return (
                from TournamentsTabs tab in Enum.GetValues(typeof(TournamentsTabs))
                select createFooterButton(tab)
                ).ToArray();
        }
    }

    public partial class TournamentsRoomFooterButton : FooterButton
    {
        /// <summary>
        /// The <see cref="TournamentsTabs"/> type this button will display when pressed.
        /// </summary>
        public TournamentsTabs TabType;

        /// <summary>
        /// The currentTabType that is opened. Dictated by TournamentInfo.
        /// </summary>
        private Bindable<TournamentsTabs> currentTabType = new();

        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                // For some reason using normal Alpha doesnt hide the button on load
                if (value != IsVisible)
                    Content.Alpha = value ? 1.0f : 0.0f;
                isVisible = value;
            }
        }

        /// <summary>
        /// True if this button is normally hidden, but needs showing.
        /// </summary>
        public bool IsForced;

        private bool isCurrent;
        public bool IsCurrent
        {
            get => isCurrent;
            set
            {
                if (value != isCurrent)
                    DeselectedColour = value ? CurrentColour : BaseColour;
                isCurrent = value;
            }
        }

        public Colour4 CurrentColour;
        public Colour4 BaseColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TournamentInfo tournamentInfo)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            // todo : Not perfectly happy with the colours, but they are fine for now.
            SelectedColour = colours.BlueLighter;
            CurrentColour = colours.BlueLight;
            BaseColour = colours.Pink1.Opacity(0.8f);
            DeselectedColour = BaseColour;

            // Setting Enabled is to trigger updateDisplay in FooterButton
            // This was done to avoid the possibility of tabButton not reverting color after currentTabType changed.
            currentTabType.BindTo(tournamentInfo.CurrentTabType);
            currentTabType.BindValueChanged((e) =>
            {
                IsCurrent = e.NewValue == TabType;
                Enabled.Value = !Enabled.Value;
                Enabled.Value = !Enabled.Value;
            }, true);

            tournamentInfo.UpdateTabVisibility += (tab, b) => IsVisible = tab == TabType && b != IsVisible ? b : IsVisible;
            IsVisible = tournamentInfo.GetTabVisibility(TabType);
            Action = () => currentTabType.Value = TabType;
        }
    }
}

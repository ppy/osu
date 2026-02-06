// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Overlays.Team.Header;
using osu.Game.Overlays.Team.Header.Components;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Team
{
    public partial class TeamProfileHeader : TabControlOverlayHeader<TeamTabs>
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        protected override OverlayTitle CreateTitle() => new TeamHeaderTitle();

        protected override Drawable CreateBackground() => Empty();

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                new TopHeaderContainer
                {
                    RelativeSizeAxes = Axes.X,
                    TeamData = { BindTarget = TeamData },
                },
                new BottomHeaderContainer
                {
                    RelativeSizeAxes = Axes.X,
                    TeamData = { BindTarget = TeamData },
                },
            }
        };

        protected override Drawable CreateTabControlContent() => new TeamRulesetSelector
        {
            TeamData = { BindTarget = TeamData },
        };

        private partial class TeamHeaderTitle : OverlayTitle
        {
            public TeamHeaderTitle()
            {
                Title = PageTitleStrings.MainTeamsControllerShow;
                Icon = OsuIcon.Team;
            }
        }
    }

    public enum TeamTabs
    {
        [LocalisableDescription(typeof(TeamsStrings), nameof(TeamsStrings.HeaderLinksShow))]
        Info,
    }
}

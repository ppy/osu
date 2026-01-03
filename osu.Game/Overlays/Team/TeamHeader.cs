// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Team
{
    public partial class TeamHeader : TabControlOverlayHeader<TeamTabs>
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        protected override OverlayTitle CreateTitle() => new TeamHeaderTitle();

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

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard
{
    public class DashboardOverlayHeader : TabControlOverlayHeader<DashboardOverlayTabs>
    {
        protected override OverlayTitle CreateTitle() => new DashboardTitle();

        private class DashboardTitle : OverlayTitle
        {
            public DashboardTitle()
            {
                Title = HomeStrings.UserTitle;
                Description = "view your friends and other information";
                IconTexture = "Icons/Hexacons/social";
            }
        }
    }

    [LocalisableEnum(typeof(DashboardOverlayTabsEnumLocalisationMapper))]
    public enum DashboardOverlayTabs
    {
        Friends,

        [Description("Currently Playing")]
        CurrentlyPlaying
    }

    public class DashboardOverlayTabsEnumLocalisationMapper : EnumLocalisationMapper<DashboardOverlayTabs>
    {
        public override LocalisableString Map(DashboardOverlayTabs value)
        {
            switch (value)
            {
                case DashboardOverlayTabs.Friends:
                    return FriendsStrings.TitleCompact;                  

                case DashboardOverlayTabs.CurrentlyPlaying:
                    return @"Currently Playing";

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}

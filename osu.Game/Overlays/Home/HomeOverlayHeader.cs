// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using System.ComponentModel;
using osu.Framework.Extensions;

namespace osu.Game.Overlays.Home
{
    public class HomeOverlayHeader : TabControlOverlayHeader<HomeOverlayTabs>
    {
        protected override ScreenTitle CreateTitle() => new HomeTitle
        {
            Current = { BindTarget = Current }
        };

        private class HomeTitle : ScreenTitle
        {
            public readonly Bindable<HomeOverlayTabs> Current = new Bindable<HomeOverlayTabs>();

            public HomeTitle()
            {
                Title = "home";
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Current.BindValueChanged(current => Section = current.NewValue.GetDescription().ToLower(), true);
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/rankings");
        }
    }

    public enum HomeOverlayTabs
    {
        Dashboard,
        Friends,

        [Description(@"forum subscriptions")]
        Forum,

        [Description(@"modding watchlist")]
        Modding,

        [Description(@"account settings")]
        Settings
    }
}

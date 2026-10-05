// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public abstract partial class OnlineUserPanel : CompositeDrawable, IFilterable
    {
        public readonly APIUser User;

        public readonly Bindable<bool> CanSpectate = new Bindable<bool>();

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        protected OnlineUserPanel(APIUser user)
        {
            User = user;
            FilterTerms = new LocalisableString[] { User.Username };
        }

        protected void BeginSpectating()
        {
            performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User)));
        }

        public IEnumerable<LocalisableString> FilterTerms { get; }

        public bool FilteringActive { set; get; }

        public bool MatchingFilter
        {
            set
            {
                if (value)
                    Show();
                else
                    Hide();
            }
        }
    }
}

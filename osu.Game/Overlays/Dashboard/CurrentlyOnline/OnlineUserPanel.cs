// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    public abstract partial class OnlineUserPanel : CompositeDrawable, IFilterable
    {
        public readonly APIUser User;

        protected IBindable<bool> CanSpectate => canSpectate;
        private readonly Bindable<bool> canSpectate = new Bindable<bool>();

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private MetadataClient? metadataClient { get; set; }

        protected OnlineUserPanel(APIUser user)
        {
            User = user;
            FilterTerms = new LocalisableString[] { User.Username };
        }

        protected override void Update()
        {
            base.Update();

            // TODO: we probably don't want to do this every frame.
            var activity = metadataClient?.GetPresence(User.Id)?.Activity;

            switch (activity)
            {
                default:
                    canSpectate.Value = false;
                    break;

                case UserActivity.InSoloGame:
                case UserActivity.InMultiplayerGame:
                case UserActivity.InPlaylistGame:
                    canSpectate.Value = true;
                    break;
            }
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

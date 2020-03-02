// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : BreadcrumbControlOverlayHeader
    {
        public readonly Bindable<APIChangelogBuild> Build = new Bindable<APIChangelogBuild>();

        public Action ListingSelected;

        public UpdateStreamBadgeArea Streams;

        private const string listing_string = "listing";

        public ChangelogHeader()
        {
            TabControl.AddItem(listing_string);
            Current.ValueChanged += e =>
            {
                if (e.NewValue == listing_string)
                    ListingSelected?.Invoke();
            };

            Build.ValueChanged += showBuild;

            Streams.Current.ValueChanged += e =>
            {
                if (e.NewValue?.LatestBuild != null && !e.NewValue.Equals(Build.Value?.UpdateStream))
                    Build.Value = e.NewValue.LatestBuild;
            };
        }

        private ChangelogHeaderTitle title;

        private void showBuild(ValueChangedEvent<APIChangelogBuild> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue.ToString());

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue.ToString());
                Current.Value = e.NewValue.ToString();

                updateCurrentStream();

                title.Version = e.NewValue.UpdateStream.DisplayName;
            }
            else
            {
                Current.Value = listing_string;
                Streams.Current.Value = null;
                title.Version = null;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/changelog");

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                Streams = new UpdateStreamBadgeArea(),
            }
        };

        protected override ScreenTitle CreateTitle() => title = new ChangelogHeaderTitle();

        public void Populate(List<APIUpdateStream> streams)
        {
            Streams.Populate(streams);
            updateCurrentStream();
        }

        private void updateCurrentStream()
        {
            if (Build.Value == null)
                return;

            Streams.Current.Value = Streams.Items.FirstOrDefault(s => s.Name == Build.Value.UpdateStream.Name);
        }

        private class ChangelogHeaderTitle : ScreenTitle
        {
            public string Version
            {
                set => Section = value ?? listing_string;
            }

            public ChangelogHeaderTitle()
            {
                Title = "changelog";
                Version = null;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}

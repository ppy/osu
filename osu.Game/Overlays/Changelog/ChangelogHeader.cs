﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogHeader : BreadcrumbControlOverlayHeader
    {
        public readonly Bindable<APIChangelogBuild> Current = new Bindable<APIChangelogBuild>();

        public Action ListingSelected;

        public UpdateStreamBadgeArea Streams;

        private const string listing_string = "listing";

        public ChangelogHeader()
        {
            BreadcrumbControl.AddItem(listing_string);
            BreadcrumbControl.Current.ValueChanged += e =>
            {
                if (e.NewValue == listing_string)
                    ListingSelected?.Invoke();
            };

            Current.ValueChanged += showBuild;

            Streams.Current.ValueChanged += e =>
            {
                if (e.NewValue?.LatestBuild != null && e.NewValue != Current.Value?.UpdateStream)
                    Current.Value = e.NewValue.LatestBuild;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BreadcrumbControl.AccentColour = colours.Violet;
            TitleBackgroundColour = colours.GreyVioletDarker;
            ControlBackgroundColour = colours.GreyVioletDark;
        }

        private ChangelogHeaderTitle title;

        private void showBuild(ValueChangedEvent<APIChangelogBuild> e)
        {
            if (e.OldValue != null)
                BreadcrumbControl.RemoveItem(e.OldValue.ToString());

            if (e.NewValue != null)
            {
                BreadcrumbControl.AddItem(e.NewValue.ToString());
                BreadcrumbControl.Current.Value = e.NewValue.ToString();

                Streams.Current.Value = Streams.Items.FirstOrDefault(s => s.Name == e.NewValue.UpdateStream.Name);

                title.Version = e.NewValue.UpdateStream.DisplayName;
            }
            else
            {
                BreadcrumbControl.Current.Value = listing_string;
                Streams.Current.Value = null;
                title.Version = null;
            }
        }

        protected override Drawable CreateBackground() => new HeaderBackground();

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

        public class HeaderBackground : Sprite
        {
            public HeaderBackground()
            {
                RelativeSizeAxes = Axes.Both;
                FillMode = FillMode.Fill;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = textures.Get(@"Headers/changelog");
            }
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

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Violet;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}

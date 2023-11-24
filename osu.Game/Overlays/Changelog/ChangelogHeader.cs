// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogHeader : BreadcrumbControlOverlayHeader
    {
        public readonly Bindable<APIChangelogBuild> Build = new Bindable<APIChangelogBuild>();

        public Action ListingSelected;

        public ChangelogUpdateStreamControl Streams;

        public static LocalisableString ListingString => LayoutStrings.HeaderChangelogIndex;

        private readonly Bindable<APIUpdateStream> currentStream = new Bindable<APIUpdateStream>();

        private Box streamsBackground;

        public ChangelogHeader()
        {
            TabControl.AddItem(ListingString);
            Current.ValueChanged += e =>
            {
                if (e.NewValue == ListingString)
                    ListingSelected?.Invoke();
            };

            Build.ValueChanged += showBuild;

            currentStream.ValueChanged += e =>
            {
                if (e.NewValue?.LatestBuild != null && !e.NewValue.Equals(Build.Value?.UpdateStream))
                    Build.Value = e.NewValue.LatestBuild;
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            streamsBackground.Colour = colourProvider.Background5;
        }

        private void showBuild(ValueChangedEvent<APIChangelogBuild> e)
        {
            if (e.OldValue != null)
                TabControl.RemoveItem(e.OldValue.ToString());

            if (e.NewValue != null)
            {
                TabControl.AddItem(e.NewValue.ToString());
                Current.Value = e.NewValue.ToString();

                updateCurrentStream();
            }
            else
            {
                Current.Value = ListingString;
                currentStream.Value = null;
            }
        }

        protected override Drawable CreateBackground() => new OverlayHeaderBackground(@"Headers/changelog");

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                streamsBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING - ChangelogUpdateStreamItem.PADDING,
                        Vertical = 20
                    },
                    Child = Streams = new ChangelogUpdateStreamControl { Current = currentStream },
                }
            }
        };

        protected override OverlayTitle CreateTitle() => new ChangelogHeaderTitle();

        public void Populate(List<APIUpdateStream> streams)
        {
            Streams.Populate(streams);
            updateCurrentStream();
        }

        private void updateCurrentStream()
        {
            if (Build.Value == null)
                return;

            currentStream.Value = Streams.Items.FirstOrDefault(s => s.Name == Build.Value.UpdateStream.Name);
        }

        private partial class ChangelogHeaderTitle : OverlayTitle
        {
            public ChangelogHeaderTitle()
            {
                Title = PageTitleStrings.MainChangelogControllerDefault;
                Description = NamedOverlayComponentStrings.ChangelogDescription;
                Icon = HexaconsIcons.Devtools;
            }
        }
    }
}

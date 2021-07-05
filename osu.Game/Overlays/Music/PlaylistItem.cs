// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistItem : OsuRearrangeableListItem<BeatmapSetInfo>, IFilterable
    {
        public readonly Bindable<BeatmapSetInfo> SelectedSet = new Bindable<BeatmapSetInfo>();

        public Action<BeatmapSetInfo> RequestSelection;

        private TextFlowContainer text;
        private IEnumerable<Drawable> titleSprites;

        private ILocalisedBindableString title;
        private ILocalisedBindableString artist;

        private Color4 selectedColour;
        private Color4 artistColour;

        public PlaylistItem(BeatmapSetInfo item)
            : base(item)
        {
            Padding = new MarginPadding { Left = 5 };

            FilterTerms = item.Metadata.SearchableTerms;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationManager localisation)
        {
            selectedColour = colours.Yellow;
            artistColour = colours.Gray9;
            HandleColour = colours.Gray5;

            title = localisation.GetLocalisedString(new RomanisableString(Model.Metadata.TitleUnicode, Model.Metadata.Title));
            artist = localisation.GetLocalisedString(new RomanisableString(Model.Metadata.ArtistUnicode, Model.Metadata.Artist));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            artist.BindValueChanged(_ => recreateText(), true);

            SelectedSet.BindValueChanged(set =>
            {
                if (set.OldValue?.Equals(Model) != true && set.NewValue?.Equals(Model) != true)
                    return;

                foreach (Drawable s in titleSprites)
                    s.FadeColour(set.NewValue.Equals(Model) ? selectedColour : Color4.White, FADE_DURATION);
            }, true);
        }

        protected override Drawable CreateContent() => text = new OsuTextFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        };

        private void recreateText()
        {
            text.Clear();

            // space after the title to put a space between the title and artist
            titleSprites = text.AddText(title.Value + @"  ", sprite => sprite.Font = OsuFont.GetFont(weight: FontWeight.Regular)).OfType<SpriteText>();

            text.AddText(artist.Value, sprite =>
            {
                sprite.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                sprite.Colour = artistColour;
                sprite.Padding = new MarginPadding { Top = 1 };
            });
        }

        protected override bool OnClick(ClickEvent e)
        {
            RequestSelection?.Invoke(Model);
            return true;
        }

        private bool inSelectedCollection = true;

        public bool InSelectedCollection
        {
            get => inSelectedCollection;
            set
            {
                if (inSelectedCollection == value)
                    return;

                inSelectedCollection = value;
                updateFilter();
            }
        }

        public IEnumerable<string> FilterTerms { get; }

        private bool matchingFilter = true;

        public bool MatchingFilter
        {
            get => matchingFilter && inSelectedCollection;
            set
            {
                if (matchingFilter == value)
                    return;

                matchingFilter = value;
                updateFilter();
            }
        }

        private void updateFilter() => this.FadeTo(MatchingFilter ? 1 : 0, 200);

        public bool FilteringActive { get; set; }
    }
}

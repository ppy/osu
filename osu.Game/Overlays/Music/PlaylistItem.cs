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
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public partial class PlaylistItem : OsuRearrangeableListItem<Live<BeatmapSetInfo>>, IFilterable
    {
        public readonly Bindable<Live<BeatmapSetInfo>> SelectedSet = new Bindable<Live<BeatmapSetInfo>>();

        public Action<Live<BeatmapSetInfo>> RequestSelection;

        private TextFlowContainer text;
        private ITextPart titlePart;

        [Resolved]
        private OsuColour colours { get; set; }

        public PlaylistItem(Live<BeatmapSetInfo> item)
            : base(item)
        {
            Padding = new MarginPadding { Left = 5 };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HandleColour = colours.Gray5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Model.PerformRead(m =>
            {
                var metadata = m.Metadata;

                var title = new RomanisableString(metadata.TitleUnicode, metadata.Title);
                var artist = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);

                titlePart = text.AddText(title, sprite => sprite.Font = OsuFont.GetFont(weight: FontWeight.Regular));
                titlePart.DrawablePartsRecreated += _ => updateSelectionState(SelectedSet.Value, applyImmediately: true);

                text.AddText(@"  "); // to separate the title from the artist.
                text.AddText(artist, sprite =>
                {
                    sprite.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                    sprite.Colour = colours.Gray9;
                    sprite.Padding = new MarginPadding { Top = 1 };
                });

                SelectedSet.BindValueChanged(set => updateSelectionState(set.NewValue));
                updateSelectionState(SelectedSet.Value, applyImmediately: true);
            });
        }

        private bool selected;

        private void updateSelectionState(Live<BeatmapSetInfo> selectedSet, bool applyImmediately = false)
        {
            bool wasSelected = selected;
            selected = selectedSet?.Equals(Model) == true;

            // Immediate updates should forcibly set correct state regardless of previous state.
            // This ensures that the initial state is correctly applied.
            if (wasSelected == selected && !applyImmediately)
                return;

            foreach (Drawable s in titlePart.Drawables)
                s.FadeColour(selected ? colours.Yellow : Color4.White, applyImmediately ? 0 : FADE_DURATION);
        }

        protected override Drawable CreateContent() => new DelayedLoadWrapper(text = new OsuTextFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        });

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

        public IEnumerable<LocalisableString> FilterTerms => Model.PerformRead(m => m.Metadata.GetSearchableTerms()).Select(s => (LocalisableString)s).ToArray();

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

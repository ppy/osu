// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        private ITextPart titlePart;

        [Resolved]
        private OsuColour colours { get; set; }

        public PlaylistItem(BeatmapSetInfo item)
            : base(item)
        {
            Padding = new MarginPadding { Left = 5 };

            FilterTerms = item.Metadata.GetSearchableTerms();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HandleColour = colours.Gray5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedSet.BindValueChanged(set =>
            {
                if (set.OldValue?.Equals(Model) != true && set.NewValue?.Equals(Model) != true)
                    return;

                updateSelectionState(false);
            }, true);
        }

        private void updateSelectionState(bool instant)
        {
            foreach (Drawable s in titlePart.Drawables)
                s.FadeColour(SelectedSet.Value?.Equals(Model) == true ? colours.Yellow : Color4.White, instant ? 0 : FADE_DURATION);
        }

        protected override Drawable CreateContent() => text = new OsuTextFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
        };

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            var title = new RomanisableString(Model.Metadata.TitleUnicode, Model.Metadata.Title);
            var artist = new RomanisableString(Model.Metadata.ArtistUnicode, Model.Metadata.Artist);

            titlePart = text.AddText(title, sprite => sprite.Font = OsuFont.GetFont(weight: FontWeight.Regular));
            updateSelectionState(true);
            titlePart.DrawablePartsRecreated += _ => updateSelectionState(true);

            text.AddText(@"  "); // to separate the title from the artist.

            text.AddText(artist, sprite =>
            {
                sprite.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                sprite.Colour = colours.Gray9;
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

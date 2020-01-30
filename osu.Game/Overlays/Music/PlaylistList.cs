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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistList : RearrangeableListContainer<BeatmapSetInfo>
    {
        public Action<BeatmapSetInfo> RequestSelection;

        public readonly Bindable<BeatmapSetInfo> SelectedSet = new Bindable<BeatmapSetInfo>();

        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public void Filter(string searchTerm) => ((PlaylistListFlowContainer)ListContainer).SearchTerm = searchTerm;

        public BeatmapSetInfo FirstVisibleSet => ListContainer.FlowingChildren.Cast<DrawablePlaylistListItem>().FirstOrDefault(i => i.MatchingFilter)?.Model;

        private void removeBeatmapSets(IEnumerable<BeatmapSetInfo> sets) => Schedule(() =>
        {
            foreach (var item in sets)
                Items.Remove(ListContainer.Children.Select(d => d.Model).FirstOrDefault(m => m == item));
        });

        protected override DrawableRearrangeableListItem<BeatmapSetInfo> CreateDrawable(BeatmapSetInfo item) => new DrawablePlaylistListItem(item)
        {
            SelectedSet = { BindTarget = SelectedSet },
            RequestSelection = set => RequestSelection?.Invoke(set)
        };

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override FillFlowContainer<DrawableRearrangeableListItem<BeatmapSetInfo>> CreateListFillFlowContainer() => new PlaylistListFlowContainer
        {
            Spacing = new Vector2(0, 3),
            LayoutDuration = 200,
            LayoutEasing = Easing.OutQuint,
        };
    }

    public class PlaylistListFlowContainer : SearchContainer<DrawableRearrangeableListItem<BeatmapSetInfo>>
    {
    }

    public class DrawablePlaylistListItem : DrawableRearrangeableListItem<BeatmapSetInfo>, IFilterable
    {
        private const float fade_duration = 100;

        public readonly Bindable<BeatmapSetInfo> SelectedSet = new Bindable<BeatmapSetInfo>();
        public Action<BeatmapSetInfo> RequestSelection;

        private PlaylistItemHandle handle;
        private TextFlowContainer text;
        private IEnumerable<Drawable> titleSprites;
        private ILocalisedBindableString titleBind;
        private ILocalisedBindableString artistBind;

        private Color4 hoverColour;
        private Color4 artistColour;

        public DrawablePlaylistListItem(BeatmapSetInfo item)
            : base(item)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = 5 };

            FilterTerms = item.Metadata.SearchableTerms;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationManager localisation)
        {
            hoverColour = colours.Yellow;
            artistColour = colours.Gray9;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Content = new[]
                {
                    new Drawable[]
                    {
                        handle = new PlaylistItemHandle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(12),
                            Colour = colours.Gray5,
                            Alpha = 0
                        },
                        text = new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Left = 5 },
                        },
                    }
                },
                ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
            };

            titleBind = localisation.GetLocalisedString(new LocalisedString((Model.Metadata.TitleUnicode, Model.Metadata.Title)));
            artistBind = localisation.GetLocalisedString(new LocalisedString((Model.Metadata.ArtistUnicode, Model.Metadata.Artist)));

            artistBind.BindValueChanged(_ => recreateText(), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedSet.BindValueChanged(set =>
            {
                if (set.OldValue != Model && set.NewValue != Model)
                    return;

                foreach (Drawable s in titleSprites)
                    s.FadeColour(set.NewValue == Model ? hoverColour : Color4.White, fade_duration);
            }, true);
        }

        private void recreateText()
        {
            text.Clear();

            //space after the title to put a space between the title and artist
            titleSprites = text.AddText(titleBind.Value + @"  ", sprite => sprite.Font = OsuFont.GetFont(weight: FontWeight.Regular)).OfType<SpriteText>();

            text.AddText(artistBind.Value, sprite =>
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

        protected override bool IsDraggableAt(Vector2 screenSpacePos) => handle.HandlingDrag;

        protected override bool OnHover(HoverEvent e)
        {
            handle.UpdateHoverState(true);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e) => handle.UpdateHoverState(false);

        public IEnumerable<string> FilterTerms { get; }

        private bool matching = true;

        public bool MatchingFilter
        {
            get => matching;
            set
            {
                if (matching == value) return;

                matching = value;

                this.FadeTo(matching ? 1 : 0, 200);
            }
        }

        public bool FilteringActive { get; set; }

        private class PlaylistItemHandle : SpriteIcon
        {
            public bool HandlingDrag { get; private set; }
            private bool isHovering;

            public PlaylistItemHandle()
            {
                Icon = FontAwesome.Solid.Bars;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                base.OnMouseDown(e);

                HandlingDrag = true;
                UpdateHoverState(isHovering);

                return false;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);

                HandlingDrag = false;
                UpdateHoverState(isHovering);
            }

            public void UpdateHoverState(bool hovering)
            {
                isHovering = hovering;

                if (isHovering || HandlingDrag)
                    this.FadeIn(fade_duration);
                else
                    this.FadeOut(fade_duration);
            }
        }
    }
}

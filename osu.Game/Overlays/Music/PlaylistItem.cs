// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistItem : Container, IFilterable
    {
        private const float fade_duration = 100;

        private Color4 hoverColour;
        private Color4 artistColour;

        private SpriteIcon handle;
        private TextFlowContainer text;
        private IEnumerable<SpriteText> titleSprites;
        private UnicodeBindableString titleBind;
        private UnicodeBindableString artistBind;

        private readonly FillFlowContainer<PlaylistItem> playlist;
        public readonly BeatmapSetInfo BeatmapSetInfo;

        public Action<BeatmapSetInfo> OnSelect;

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value == selected) return;
                selected = value;

                FinishTransforms(true);
                foreach (SpriteText s in titleSprites)
                    s.FadeColour(Selected ? hoverColour : Color4.White, fade_duration);
            }
        }

        public PlaylistItem(FillFlowContainer<PlaylistItem> playlist, BeatmapSetInfo setInfo)
        {
            this.playlist = playlist;
            BeatmapSetInfo = setInfo;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Top = 3, Bottom = 3 };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationEngine localisation)
        {
            hoverColour = colours.Yellow;
            artistColour = colours.Gray9;

            var metadata = BeatmapSetInfo.Metadata;
            FilterTerms = metadata.SearchableTerms;

            Children = new Drawable[]
            {
                handle = new PlaylistItemHandle(playlist)
                {
                    Colour = colours.Gray5,
                },
                text = new OsuTextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = 20 },
                    ContentIndent = 10f,
                },
            };

            titleBind = localisation.GetUnicodePreference(metadata.TitleUnicode, metadata.Title);
            artistBind = localisation.GetUnicodePreference(metadata.ArtistUnicode, metadata.Artist);

            artistBind.ValueChanged += newText => recreateText();
            artistBind.TriggerChange();
        }

        private void recreateText()
        {
            text.Clear();

            //space after the title to put a space between the title and artist
            titleSprites = text.AddText(titleBind.Value + @"  ", sprite =>
            {
                sprite.TextSize = 16;
                sprite.Font = @"Exo2.0-Regular";
            });

            text.AddText(artistBind.Value, sprite =>
            {
                sprite.TextSize = 14;
                sprite.Font = @"Exo2.0-Bold";
                sprite.Colour = artistColour;
                sprite.Padding = new MarginPadding { Top = 1 };
            });
        }

        protected override bool OnHover(InputState state)
        {
            handle.FadeIn(fade_duration);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            handle.FadeOut(fade_duration);
        }

        protected override bool OnClick(InputState state)
        {
            OnSelect?.Invoke(BeatmapSetInfo);
            return true;
        }

        public string[] FilterTerms { get; private set; }

        private bool matching = true;

        public bool MatchingFilter
        {
            get { return matching; }
            set
            {
                if (matching == value) return;

                matching = value;

                this.FadeTo(matching ? 1 : 0, 200);
            }
        }

        private class PlaylistItemHandle : SpriteIcon
        {
            private readonly FillFlowContainer<PlaylistItem> playlist;

            private const int line_height = 22;
            private const int rearrange_buffer = 3;

            public PlaylistItemHandle(FillFlowContainer<PlaylistItem> playlist)
            {
                this.playlist = playlist;
                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopLeft;
                Size = new Vector2(12);
                Icon = FontAwesome.fa_bars;
                Alpha = 0f;
                Margin = new MarginPadding { Left = 5, Top = 2 };
            }

            protected override bool OnDragStart(InputState state) => true;

            protected override bool OnDrag(InputState state)
            {
                int src = (int)Parent.Depth;
                int dst = getIndex(state.Mouse.Position.Y + Parent.Position.Y);

                if (src == dst)
                    return true;

                if (src < dst)
                {
                    for (int i = src + 1; i <= dst; i++)
                        playlist.ChangeChildDepth(playlist[i], i - 1);
                }
                else
                {
                    for (int i = dst; i < src; i++)
                        playlist.ChangeChildDepth(playlist[i], i + 1);
                }

                playlist.ChangeChildDepth(Parent as PlaylistItem, dst);
                return true;
            }

            private int getIndex(float position) {
                IReadOnlyList<PlaylistItem> items = playlist.Children;

                // Binary Search without matching exact
                int min = 0;
                int max = items.Count - 1;
                while (min <= max)
                {
                    int m = (min + max) / 2;
                    if (items[m].Y < position)
                        min = m + 1;
                    else if (items[m].Y > position)
                        max = m - 1;
                }

                int index = Math.Max(0, min - 1);
                // Only move if mouse falls within buffer
                if (position - items[index].Y > rearrange_buffer && position - items[index].Y < line_height - rearrange_buffer) {
                    return (int)items[index].Depth;
                }
                return (int)Parent.Depth;
            }
        }
    }
}

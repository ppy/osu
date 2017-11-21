﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
    public class PlaylistItem : Container, IFilterable, IDraggable
    {
        private const float fade_duration = 100;

        private Color4 hoverColour;
        private Color4 artistColour;

        private SpriteIcon handle;
        private TextFlowContainer text;
        private IEnumerable<SpriteText> titleSprites;
        private UnicodeBindableString titleBind;
        private UnicodeBindableString artistBind;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        public Action<BeatmapSetInfo> OnSelect;

        public bool IsDraggable => handle.IsHovered;

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

        public PlaylistItem(BeatmapSetInfo setInfo)
        {
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
                handle = new PlaylistItemHandle
                {
                    Colour = colours.Gray5
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

        public IEnumerable<string> FilterTerms { get; private set; }

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

            public PlaylistItemHandle()
            {
                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopLeft;
                Size = new Vector2(12);
                Icon = FontAwesome.fa_bars;
                Alpha = 0f;
                Margin = new MarginPadding { Left = 5, Top = 2 };
            }
        }
    }

    public interface IDraggable : IDrawable
    {
        /// <summary>
        /// Whether this <see cref="IDraggable"/> can be dragged in its current state.
        /// </summary>
        bool IsDraggable { get; }
    }
}

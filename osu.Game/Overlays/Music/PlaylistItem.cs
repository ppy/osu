// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Music
{
    public class PlaylistItem : Container, IFilterable, IDraggable
    {
        private const float fade_duration = 100;

        private Color4 hoverColour;
        private Color4 artistColour;

        private SpriteIcon handle;
        private TextFlowContainer text;
        private IEnumerable<Drawable> titleSprites;
        private ILocalisedBindableString titleBind;
        private ILocalisedBindableString artistBind;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        public Action<BeatmapSetInfo> OnSelect;

        public bool IsDraggable { get; private set; }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            IsDraggable = handle.IsHovered;
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            IsDraggable = false;
            return base.OnMouseUp(e);
        }

        private bool selected;

        public bool Selected
        {
            get => selected;
            set
            {
                if (value == selected) return;

                selected = value;

                FinishTransforms(true);
                foreach (Drawable s in titleSprites)
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
        private void load(OsuColour colours, LocalisationManager localisation)
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

            titleBind = localisation.GetLocalisedString(new LocalisedString((metadata.TitleUnicode, metadata.Title)));
            artistBind = localisation.GetLocalisedString(new LocalisedString((metadata.ArtistUnicode, metadata.Artist)));

            artistBind.BindValueChanged(_ => recreateText(), true);
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

        protected override bool OnHover(HoverEvent e)
        {
            handle.FadeIn(fade_duration);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            handle.FadeOut(fade_duration);
        }

        protected override bool OnClick(ClickEvent e)
        {
            OnSelect?.Invoke(BeatmapSetInfo);
            return true;
        }

        public IEnumerable<string> FilterTerms { get; private set; }

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
            public PlaylistItemHandle()
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Size = new Vector2(12);
                Icon = FontAwesome.Solid.Bars;
                Alpha = 0f;
                Margin = new MarginPadding { Left = 5 };
            }

            public override bool HandlePositionalInput => IsPresent;
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

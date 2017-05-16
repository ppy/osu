// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistItem : Container, IFilterable
    {
        private const float fade_duration = 100;

        private Color4 hoverColour;

        private TextAwesome handle;
        private OsuSpriteText title;

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

                Flush(true);
                title.FadeColour(Selected ? hoverColour : Color4.White, fade_duration);
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
            BeatmapMetadata metadata = BeatmapSetInfo.Metadata;

            FilterTerms = metadata.SearchableTerms;

            Children = new Drawable[]
            {
                handle = new TextAwesome
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    TextSize = 12,
                    Colour = colours.Gray5,
                    Icon = FontAwesome.fa_bars,
                    Alpha = 0f,
                    Margin = new MarginPadding { Left = 5 },
                    Padding = new MarginPadding { Top = 2 },
                },
                new FillFlowContainer<OsuSpriteText>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = 20 },
                    Spacing = new Vector2(5f, 0f),
                    Children = new []
                    {
                        title = new OsuSpriteText
                        {
                            TextSize = 16,
                            Font = @"Exo2.0-Regular",
                            Current = localisation.GetUnicodePreference(metadata.TitleUnicode, metadata.Title),
                        },
                        new OsuSpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Colour = colours.Gray9,
                            Padding = new MarginPadding { Top = 1 },
                            Current = localisation.GetUnicodePreference(metadata.ArtistUnicode, metadata.Artist),
                        }
                    }
                },
            };

            hoverColour = colours.Yellow;
        }

        protected override bool OnHover(Framework.Input.InputState state)
        {
            handle.FadeIn(fade_duration);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            handle.FadeOut(fade_duration);
        }

        protected override bool OnClick(Framework.Input.InputState state)
        {
            OnSelect?.Invoke(BeatmapSetInfo);
            return true;
        }

        public string[] FilterTerms { get; private set; }

        private bool matching = true;

        public bool MatchingCurrentFilter
        {
            set
            {
                if (matching == value) return;

                matching = value;

                FadeTo(matching ? 1 : 0, 200);
            }
            get
            {
                return matching;
            }
        }
    }
}

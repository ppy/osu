// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistItem : Container
    {
        private const float fade_duration = 100;
        private Color4 currentColour;

        private readonly TextAwesome icon;
        private readonly IEnumerable<OsuSpriteText> title, artist;

        public readonly int Index;
        public readonly BeatmapSetInfo RepresentedSet;
        public Action<BeatmapSetInfo, int> OnSelect;

        private bool current;
        public bool Current
        {
            get { return current; }
            set
            {
                if (value == current) return;
                current = value;

                Flush(true);
                foreach (OsuSpriteText t in title)
                    t.FadeColour(Current ? currentColour : Color4.White, fade_duration);
            }
        }

        public PlaylistItem(BeatmapSetInfo set, int index)
        {
            Index = index;
            RepresentedSet = set;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Top = 3, Bottom = 3 };

            FillFlowContainer<OsuSpriteText> textContainer = new FillFlowContainer<OsuSpriteText>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Left = 20 },
                Spacing = new Vector2(5f, 0f),
            };

            Children = new Drawable[]
            {
                icon = new TextAwesome
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    TextSize = 12,
                    Icon = FontAwesome.fa_bars,
                    Alpha = 0f,
                    Margin = new MarginPadding { Left = 5 },
                    Padding = new MarginPadding { Top = 2 },
                },
                textContainer,
            };

            textContainer.Add(title = splitText(RepresentedSet.Metadata.Title, 16, @"Exo2.0-Regular", new MarginPadding(0)));
            textContainer.Add(artist = splitText(RepresentedSet.Metadata.Artist, 14, @"Exo2.0-Bold", new MarginPadding { Top = 1 }));
        }

        private IEnumerable<OsuSpriteText> splitText(string text, int textSize, string font, MarginPadding padding)
        {
            List<OsuSpriteText> sprites = new List<OsuSpriteText>();

            foreach (string w in text.Split(' '))
            {
                sprites.Add(new OsuSpriteText
                {
                    TextSize = textSize,
                    Font = font,
                    Text = w,
                    Padding = padding,
                });
            }

            return sprites;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            foreach (OsuSpriteText t in artist)
                t.Colour = colours.Gray9;

            icon.Colour = colours.Gray5;
            currentColour = colours.Yellow;
        }

        protected override bool OnHover(Framework.Input.InputState state)
        {
            icon.FadeIn(fade_duration);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            icon.FadeOut(fade_duration);
        }

        protected override bool OnClick(Framework.Input.InputState state)
        {
            OnSelect?.Invoke(RepresentedSet, Index);
            return true;
        }
    }
}

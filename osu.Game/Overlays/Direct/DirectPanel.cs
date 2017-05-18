// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Direct
{
    public abstract class DirectPanel : Container
    {
        protected virtual Sprite Background { get; }
        protected virtual OsuSpriteText Title { get; }
        protected virtual OsuSpriteText Artist { get; }
        protected virtual OsuSpriteText Author { get; }
        protected virtual OsuSpriteText Source { get; }
        protected virtual Statistic PlayCount { get; }
        protected virtual Statistic FavouriteCount { get; }
        protected virtual FillFlowContainer DifficultyIcons { get; }

        private BeatmapSetInfo setInfo;

        [BackgroundDependencyLoader]
        private void load(LocalisationEngine localisation)
        {
            Title.Current = localisation.GetUnicodePreference(setInfo.Metadata.TitleUnicode, setInfo.Metadata.Title);
            Artist.Current = localisation.GetUnicodePreference(setInfo.Metadata.ArtistUnicode, setInfo.Metadata.Artist);
            Author.Text = setInfo.Metadata.Author;
            Source.Text = @"from " + setInfo.Metadata.Source;

            foreach (var b in setInfo.Beatmaps)
                DifficultyIcons.Add(new DifficultyIcon(b));
        }

        public DirectPanel(BeatmapSetInfo setInfo)
        {
            this.setInfo = setInfo;
        }

        public class Statistic : FillFlowContainer
        {
            private readonly SpriteText text;

            private int value;
            public int Value
            {
                get { return value; }
                set
                {
                    this.value = value;
                    text.Text = string.Format("{0:n0}", Value);
                }
            }

            public Statistic(FontAwesome icon, int value = 0)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5f, 0f);

                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Font = @"Exo2.0-SemiBoldItalic",
                    },
                    new TextAwesome
                    {
                        Icon = icon,
                        Shadow = true,
                        TextSize = 14,
                        Margin = new MarginPadding { Top = 1 },
                    },
                };

                Value = value;
            }
        }
    }
}

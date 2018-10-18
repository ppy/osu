// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTitle : FillFlowContainer<OsuSpriteText>
    {
        private readonly OsuSpriteText beatmapTitle, beatmapDash, beatmapArtist;

        public float TextSize
        {
            set { beatmapTitle.TextSize = beatmapDash.TextSize = beatmapArtist.TextSize = value; }
        }

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            set
            {
                if (value == beatmap) return;
                beatmap = value;

                if (IsLoaded)
                    updateText();
            }
        }

        public BeatmapTitle()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;

            Children = new[]
            {
                beatmapTitle = new OsuSpriteText { Font = @"Exo2.0-BoldItalic", },
                beatmapDash = new OsuSpriteText { Font = @"Exo2.0-BoldItalic", },
                beatmapArtist = new OsuSpriteText { Font = @"Exo2.0-RegularItalic", },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateText();
        }

        private void updateText()
        {
            if (beatmap == null)
            {
                beatmapTitle.Text = "Changing map";
                beatmapDash.Text = beatmapArtist.Text = string.Empty;
            }
            else
            {
                beatmapTitle.Text = new LocalisedString((beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title));
                beatmapDash.Text = @" - ";
                beatmapArtist.Text = new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist));
            }
        }
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTitle : CompositeDrawable
    {
        private readonly OsuSpriteText beatmapTitle, beatmapDash, beatmapArtist;

        public float TextSize
        {
            set { beatmapTitle.TextSize = beatmapDash.TextSize = beatmapArtist.TextSize = value; }
        }

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        public BeatmapTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new[]
                {
                    beatmapTitle = new OsuSpriteText { Font = @"Exo2.0-BoldItalic", },
                    beatmapDash = new OsuSpriteText { Font = @"Exo2.0-BoldItalic", },
                    beatmapArtist = new OsuSpriteText { Font = @"Exo2.0-RegularItalic", },
                }
            };

            Beatmap.BindValueChanged(v => updateText());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateText();
        }

        private void updateText()
        {
            if (!IsLoaded)
                return;

            if (Beatmap.Value == null)
            {
                beatmapTitle.Text = "Changing map";
                beatmapDash.Text = beatmapArtist.Text = string.Empty;
            }
            else
            {
                beatmapTitle.Text = new LocalisedString((Beatmap.Value.Metadata.TitleUnicode, Beatmap.Value.Metadata.Title));
                beatmapDash.Text = @" - ";
                beatmapArtist.Text = new LocalisedString((Beatmap.Value.Metadata.ArtistUnicode, Beatmap.Value.Metadata.Artist));
            }
        }
    }
}

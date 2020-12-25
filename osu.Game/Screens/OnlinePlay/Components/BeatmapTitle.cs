// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class BeatmapTitle : OnlinePlayComposite
    {
        private readonly LinkFlowContainer textFlow;

        public BeatmapTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = textFlow = new LinkFlowContainer { AutoSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Playlist.CollectionChanged += (_, __) => updateText();

            updateText();
        }

        private float textSize = OsuFont.DEFAULT_FONT_SIZE;

        public float TextSize
        {
            get => textSize;
            set
            {
                if (textSize == value)
                    return;

                textSize = value;

                updateText();
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private void updateText()
        {
            if (LoadState < LoadState.Loading)
                return;

            textFlow.Clear();

            var beatmap = Playlist.FirstOrDefault()?.Beatmap;

            if (beatmap == null)
            {
                textFlow.AddText("No beatmap selected", s =>
                {
                    s.Font = s.Font.With(size: TextSize);
                    s.Colour = colours.PinkLight;
                });
            }
            else
            {
                textFlow.AddLink(new[]
                {
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((beatmap.Value.Metadata.ArtistUnicode, beatmap.Value.Metadata.Artist)),
                        Font = OsuFont.GetFont(size: TextSize),
                    },
                    new OsuSpriteText
                    {
                        Text = " - ",
                        Font = OsuFont.GetFont(size: TextSize),
                    },
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((beatmap.Value.Metadata.TitleUnicode, beatmap.Value.Metadata.Title)),
                        Font = OsuFont.GetFont(size: TextSize),
                    }
                }, LinkAction.OpenBeatmap, beatmap.Value.OnlineBeatmapID.ToString(), "Open beatmap");
            }
        }
    }
}

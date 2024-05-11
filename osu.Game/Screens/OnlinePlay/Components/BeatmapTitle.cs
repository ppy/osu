// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class BeatmapTitle : OnlinePlayComposite
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
            Playlist.CollectionChanged += (_, _) => updateText();

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
        private OsuColour colours { get; set; } = null!;

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
                var metadataInfo = beatmap.Metadata;

                string artistUnicode = string.IsNullOrEmpty(metadataInfo.ArtistUnicode) ? metadataInfo.Artist : metadataInfo.ArtistUnicode;
                string titleUnicode = string.IsNullOrEmpty(metadataInfo.TitleUnicode) ? metadataInfo.Title : metadataInfo.TitleUnicode;

                var title = new RomanisableString($"{artistUnicode} - {titleUnicode}".Trim(), $"{metadataInfo.Artist} - {metadataInfo.Title}".Trim());

                textFlow.AddLink(title, LinkAction.OpenBeatmap, beatmap.OnlineID.ToString(), "Open beatmap");
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTitle : MultiplayerComposite
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
            CurrentItem.BindValueChanged(v => updateText(), true);
        }

        private float textSize = OsuSpriteText.FONT_SIZE;

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

            var beatmap = CurrentItem.Value?.Beatmap;

            if (beatmap == null)
                textFlow.AddText("No beatmap selected", s =>
                {
                    s.TextSize = TextSize;
                    s.Colour = colours.PinkLight;
                });
            else
            {
                textFlow.AddLink(new[]
                {
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = " - ",
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title)),
                        TextSize = TextSize,
                    }
                }, null, LinkAction.OpenBeatmap, beatmap.OnlineBeatmapID.ToString(), "Open beatmap");
            }
        }
    }
}

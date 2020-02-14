// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetOnlineStatusPill : CircularContainer
    {
        private readonly OsuSpriteText statusText;
        private readonly Box background;

        private BeatmapSetOnlineStatus status;

        public BeatmapSetOnlineStatus Status
        {
            get => status;
            set
            {
                if (status == value)
                    return;

                status = value;

                Alpha = value == BeatmapSetOnlineStatus.None ? 0 : 1;
                statusText.Text = value.ToString().ToUpperInvariant();
            }
        }

        public float TextSize
        {
            get => statusText.Font.Size;
            set => statusText.Font = statusText.Font.With(size: value);
        }

        public MarginPadding TextPadding
        {
            get => statusText.Padding;
            set => statusText.Padding = value;
        }

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public BeatmapSetOnlineStatusPill()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                statusText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
            };

            Status = BeatmapSetOnlineStatus.None;
        }
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetOnlineStatusPill : CircularContainer
    {
        private readonly OsuSpriteText statusText;

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
            get => statusText.TextSize;
            set => statusText.TextSize = value;
        }

        public MarginPadding TextPadding
        {
            get => statusText.Padding;
            set => statusText.Padding = value;
        }

        public BeatmapSetOnlineStatusPill()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                statusText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = @"Exo2.0-Bold",
                },
            };

            Status = BeatmapSetOnlineStatus.None;
        }
    }
}

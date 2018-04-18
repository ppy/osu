// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        private BeatmapSetOnlineStatus status = BeatmapSetOnlineStatus.None;
        public BeatmapSetOnlineStatus Status
        {
            get { return status; }
            set
            {
                if (value == status) return;
                status = value;

                statusText.Text = Enum.GetName(typeof(BeatmapSetOnlineStatus), Status)?.ToUpper();
            }
        }

        public BeatmapSetOnlineStatusPill(float textSize, MarginPadding textPadding)
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
                    TextSize = textSize,
                    Padding = textPadding,
                },
            };
        }
    }
}

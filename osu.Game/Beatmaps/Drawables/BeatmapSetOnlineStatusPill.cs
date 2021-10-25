// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapSetOnlineStatusPill : CircularContainer
    {
        private BeatmapSetOnlineStatus status;

        public BeatmapSetOnlineStatus Status
        {
            get => status;
            set
            {
                if (status == value)
                    return;

                status = value;

                if (IsLoaded)
                    updateState();
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

        private readonly OsuSpriteText statusText;
        private readonly Box background;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private OverlayColourProvider? colourProvider { get; set; }

        public BeatmapSetOnlineStatusPill()
        {
            Masking = true;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                statusText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
            };

            Status = BeatmapSetOnlineStatus.None;
            TextPadding = new MarginPadding { Horizontal = 5, Bottom = 1 };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateState();
        }

        private void updateState()
        {
            Alpha = Status == BeatmapSetOnlineStatus.None ? 0 : 1;

            statusText.Text = Status.GetLocalisableDescription().ToUpper();

            if (colourProvider != null)
                statusText.Colour = status == BeatmapSetOnlineStatus.Graveyard ? colourProvider.Background1 : colourProvider.Background3;
            else
                statusText.Colour = status == BeatmapSetOnlineStatus.Graveyard ? colours.GreySeafoamLight : Color4.Black;

            background.Colour = OsuColour.ForBeatmapSetOnlineStatus(Status) ?? colourProvider?.Light1 ?? colours.GreySeafoamLighter;
        }
    }
}

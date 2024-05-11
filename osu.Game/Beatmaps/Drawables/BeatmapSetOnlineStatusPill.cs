// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class BeatmapSetOnlineStatusPill : CircularContainer, IHasTooltip
    {
        private const double animation_duration = 400;

        private BeatmapOnlineStatus status;

        public BeatmapOnlineStatus Status
        {
            get => status;
            set
            {
                if (status == value)
                    return;

                status = value;

                if (IsLoaded)
                {
                    AutoSizeDuration = (float)animation_duration;
                    AutoSizeEasing = Easing.OutQuint;

                    updateState();
                }
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

            Alpha = 0;

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

            Status = BeatmapOnlineStatus.None;
            TextPadding = new MarginPadding { Horizontal = 5, Bottom = 1 };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
            FinishTransforms(true);
        }

        private void updateState()
        {
            if (Status == BeatmapOnlineStatus.None)
            {
                this.FadeOut(animation_duration, Easing.OutQuint);
                return;
            }

            this.FadeIn(animation_duration, Easing.OutQuint);

            Color4 statusTextColour;

            if (colourProvider != null)
                statusTextColour = status == BeatmapOnlineStatus.Graveyard ? colourProvider.Background1 : colourProvider.Background3;
            else
                statusTextColour = status == BeatmapOnlineStatus.Graveyard ? colours.GreySeaFoamLight : Color4.Black;

            statusText.FadeColour(statusTextColour, animation_duration, Easing.OutQuint);
            background.FadeColour(OsuColour.ForBeatmapSetOnlineStatus(Status) ?? colourProvider?.Light1 ?? colours.GreySeaFoamLighter, animation_duration, Easing.OutQuint);

            statusText.Text = Status.GetLocalisableDescription().ToUpper();
        }

        public LocalisableString TooltipText
        {
            get
            {
                switch (Status)
                {
                    case BeatmapOnlineStatus.LocallyModified:
                        return SongSelectStrings.LocallyModifiedTooltip;
                }

                return string.Empty;
            }
        }
    }
}

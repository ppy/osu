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
        /// <summary>
        /// Whether to show <see cref="BeatmapOnlineStatus.None"/> as "unknown" instead of fading out.
        /// </summary>
        public bool ShowUnknownStatus { get; init; }

        /// <summary>
        /// Whether changing status performs transition transforms.
        /// </summary>
        public bool Animated { get; init; } = true;

        public BeatmapOnlineStatus Status
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

        private BeatmapOnlineStatus status;

        public float TextSize
        {
            init => statusText.Font = statusText.Font.With(size: value);
        }

        public MarginPadding TextPadding
        {
            init => statusText.Padding = value;
        }

        private readonly OsuSpriteText statusText;
        private readonly Box background;

        private const double animation_duration = 400;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private OverlayColourProvider? colourProvider { get; set; }

        public BeatmapSetOnlineStatusPill()
        {
            AutoSizeAxes = Axes.Both;
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
            TextPadding = new MarginPadding { Horizontal = 4, Bottom = 1 };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
            FinishTransforms(true);
        }

        private void updateState()
        {
            double duration = Animated ? animation_duration : 0;

            if (Status == BeatmapOnlineStatus.None && !ShowUnknownStatus)
            {
                this.FadeOut(duration, Easing.OutQuint);
                return;
            }

            // The autosize animation on this component is intended to animate horizontal sizing only.
            // To avoid vertical autosize animating from zero to non-zero, only apply the duration
            // after we have a valid size.
            if (Height > 0)
            {
                AutoSizeDuration = (float)duration;
                AutoSizeEasing = Easing.OutQuint;
            }

            this.FadeIn(duration, Easing.OutQuint);

            // Handle the case where transition from hidden to non-hidden may cause
            // a fade from a colour that doesn't make sense (due to not being able to see the previous colour).
            if (Alpha == 0)
                duration = 0;

            Color4 statusTextColour;

            if (colourProvider != null)
                statusTextColour = status == BeatmapOnlineStatus.Graveyard ? colourProvider.Background1 : colourProvider.Background3;
            else
                statusTextColour = status == BeatmapOnlineStatus.Graveyard ? colours.GreySeaFoamLight : Color4.Black;

            statusText.FadeColour(statusTextColour, duration, Easing.OutQuint);
            background.FadeColour(OsuColour.ForBeatmapSetOnlineStatus(Status) ?? colourProvider?.Light1 ?? colours.GreySeaFoamLighter, duration, Easing.OutQuint);

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

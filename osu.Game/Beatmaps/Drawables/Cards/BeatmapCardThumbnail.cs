// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardThumbnail : Container
    {
        public BindableBool Dimmed { get; } = new BindableBool();

        public new MarginPadding Padding
        {
            get => foreground.Padding;
            set => foreground.Padding = value;
        }

        private readonly UpdateableOnlineBeatmapSetCover cover;
        private readonly Container foreground;
        private readonly PlayButton playButton;
        private readonly SmoothCircularProgress progress;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public BeatmapCardThumbnail(APIBeatmapSet beatmapSetInfo)
        {
            InternalChildren = new Drawable[]
            {
                cover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.List)
                {
                    RelativeSizeAxes = Axes.Both,
                    OnlineInfo = beatmapSetInfo
                },
                foreground = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        playButton = new PlayButton(beatmapSetInfo)
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        progress = new SmoothCircularProgress
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(50),
                            InnerRadius = 0.2f
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            progress.Colour = colourProvider.Highlight1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Dimmed.BindValueChanged(_ => updateState());

            playButton.Playing.BindValueChanged(_ => updateState(), true);
            ((IBindable<double>)progress.Current).BindTo(playButton.Progress);

            FinishTransforms(true);
        }

        private void updateState()
        {
            bool shouldDim = Dimmed.Value || playButton.Playing.Value;

            playButton.FadeTo(shouldDim ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            cover.FadeColour(shouldDim ? OsuColour.Gray(0.2f) : Color4.White, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}

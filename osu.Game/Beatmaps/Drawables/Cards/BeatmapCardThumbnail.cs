// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public class BeatmapCardThumbnail : Container
    {
        public BindableBool Dimmed { get; } = new BindableBool();

        private readonly APIBeatmapSet beatmapSetInfo;

        private readonly UpdateableOnlineBeatmapSetCover cover;
        private readonly PlayButton playButton;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public BeatmapCardThumbnail(APIBeatmapSet beatmapSetInfo)
        {
            this.beatmapSetInfo = beatmapSetInfo;

            InternalChildren = new Drawable[]
            {
                cover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.List)
                {
                    RelativeSizeAxes = Axes.Both,
                    OnlineInfo = beatmapSetInfo
                },
                playButton = new PlayButton(),
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Dimmed.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            playButton.FadeTo(Dimmed.Value ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            cover.FadeColour(Dimmed.Value ? OsuColour.Gray(0.2f) : Color4.White, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class DimmableVideo : UserDimContainer
    {
        private readonly VideoSprite video;
        private DrawableVideo drawableVideo;

        public DimmableVideo(VideoSprite video)
        {
            this.video = video;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            initializeVideo(false);
        }

        protected override void LoadComplete()
        {
            ShowVideo.BindValueChanged(_ => initializeVideo(true), true);
            base.LoadComplete();
        }

        protected override bool ShowDimContent => ShowVideo.Value && DimLevel < 1;

        private void initializeVideo(bool async)
        {
            if (video == null)
                return;

            if (drawableVideo != null)
                return;

            if (!ShowVideo.Value)
                return;

            drawableVideo = new DrawableVideo(video);

            if (async)
                LoadComponentAsync(drawableVideo, Add);
            else
                Add(drawableVideo);
        }

        private class DrawableVideo : Container
        {
            public DrawableVideo(VideoSprite video)
            {
                RelativeSizeAxes = Axes.Both;
                Masking = true;

                video.RelativeSizeAxes = Axes.Both;
                video.FillMode = FillMode.Fit;
                video.Anchor = Anchor.Centre;
                video.Origin = Anchor.Centre;

                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    video,
                });
            }

            [BackgroundDependencyLoader]
            private void load(GameplayClock clock)
            {
                if (clock != null)
                    Clock = clock;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class DimmableVideo : UserDimContainer
    {
        private readonly VideoSprite video;
        private readonly int offset;
        private DrawableVideo drawableVideo;

        public DimmableVideo(VideoSprite video, int offset)
        {
            this.video = video;
            this.offset = offset;
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

        protected override bool ShowDimContent => IgnoreUserSettings.Value || (ShowVideo.Value && DimLevel < 1);

        private void initializeVideo(bool async)
        {
            if (video == null)
                return;

            if (drawableVideo != null)
                return;

            if (!ShowVideo.Value && !IgnoreUserSettings.Value)
                return;

            drawableVideo = new DrawableVideo(video, offset);

            if (async)
                LoadComponentAsync(drawableVideo, Add);
            else
                Add(drawableVideo);
        }

        private class DrawableVideo : Container
        {
            private readonly Drawable cover;
            private readonly int offset;
            private readonly ManualClock videoClock;
            private bool videoStarted;

            public DrawableVideo(VideoSprite video, int offset)
            {
                this.offset = offset;

                RelativeSizeAxes = Axes.Both;
                Masking = true;

                video.RelativeSizeAxes = Axes.Both;
                video.FillMode = FillMode.Fit;
                video.Anchor = Anchor.Centre;
                video.Origin = Anchor.Centre;

                videoClock = new ManualClock();
                video.Clock = new FramedClock(videoClock);

                AddRangeInternal(new[]
                {
                    video,
                    cover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                });
            }

            [BackgroundDependencyLoader]
            private void load(GameplayClock clock)
            {
                if (clock != null)
                    Clock = clock;
            }

            protected override void Update()
            {
                if (videoClock != null && Clock.CurrentTime > offset)
                {
                    if (!videoStarted)
                    {
                        cover.FadeOut(500);
                        videoStarted = true;
                    }

                    // handle seeking before the video starts (break skipping, replay seek)
                    videoClock.CurrentTime = Clock.CurrentTime - offset;
                }

                base.Update();
            }
        }
    }
}

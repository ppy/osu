// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardVideo : Container
    {
        public readonly StoryboardVideo Video;
        private VideoSprite videoSprite;
        private ManualClock videoClock;
        private GameplayClock clock;

        private bool videoStarted;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(GameplayClock clock, IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
            if (clock == null)
                return;

            this.clock = clock;

            var path = beatmap.Value.BeatmapSetInfo?.Files?.Find(f => f.Filename.Equals(Video.Path, StringComparison.OrdinalIgnoreCase))?.FileInfo.StoragePath;

            if (path == null)
                return;

            var stream = textureStore.GetStream(path);

            if (stream == null)
                return;

            AddInternal(videoSprite = new VideoSprite(stream)
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AlwaysPresent = true,
                Alpha = 0,
                Position = new Vector2(Video.XOffset, Video.YOffset)
            });

            videoClock = new ManualClock();
            videoSprite.Clock = new FramedClock(videoClock);
        }

        protected override void Update()
        {
            if (clock != null && clock.CurrentTime > Video.StartTime)
            {
                if (!videoStarted)
                {
                    videoSprite.FadeIn(500);
                    videoStarted = true;
                }

                // handle seeking before the video starts (break skipping, replay seek)
                videoClock.CurrentTime = clock.CurrentTime - Video.StartTime;
            }

            base.Update();
        }
    }
}

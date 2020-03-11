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

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardVideo : Container
    {
        public readonly StoryboardVideo Video;
        private VideoSprite videoSprite;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
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
                Alpha = 0
            });
        }

        protected override void LoadComplete()
        {
            using (videoSprite.BeginAbsoluteSequence(Video.StartTime))
            {
                videoSprite.Clock = new FramedOffsetClock(Clock)
                {
                    Offset = -Video.StartTime
                };

                videoSprite.FadeIn(500);
            }

            base.LoadComplete();
        }
    }
}

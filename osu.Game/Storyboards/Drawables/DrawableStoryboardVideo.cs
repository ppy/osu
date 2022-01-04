// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;
using osu.Game.Extensions;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardVideo : CompositeDrawable
    {
        public readonly StoryboardVideo Video;
        private Video video;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
            string path = beatmap.Value.BeatmapSetInfo?.Files.FirstOrDefault(f => f.Filename.Equals(Video.Path, StringComparison.OrdinalIgnoreCase))?.File.GetStoragePath();

            if (path == null)
                return;

            var stream = textureStore.GetStream(path);

            if (stream == null)
                return;

            InternalChild = video = new Video(stream, false)
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fill,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (video == null) return;

            using (video.BeginAbsoluteSequence(Video.StartTime))
            {
                Schedule(() => video.PlaybackPosition = Time.Current - Video.StartTime);
                video.FadeIn(500);
            }
        }
    }
}

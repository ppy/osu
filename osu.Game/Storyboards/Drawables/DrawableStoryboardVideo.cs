// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardVideo : CompositeDrawable
    {
        public readonly StoryboardVideo Video;

        private Video? drawableVideo;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
            string? path = beatmap.Value.BeatmapSetInfo?.GetPathForFile(Video.Path);

            if (path == null)
                return;

            var stream = textureStore.GetStream(path);

            if (stream == null)
                return;

            InternalChild = drawableVideo = new Video(stream, false)
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

            if (drawableVideo == null) return;

            using (drawableVideo.BeginAbsoluteSequence(Video.StartTime))
            {
                Schedule(() => drawableVideo.PlaybackPosition = Time.Current - Video.StartTime);
                drawableVideo.FadeIn(500);
            }
        }
    }
}

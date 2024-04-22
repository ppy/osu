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
    public partial class DrawableStoryboardVideo : CompositeDrawable
    {
        public readonly StoryboardVideo Video;

        private Video? drawableVideo;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            // In osu-stable, a mapper can add a scale command for a storyboard.
            // This allows scaling based on the video's absolute size.
            //
            // If not specified we take up the full available space.
            bool useRelative = !video.TimelineGroup.Scale.HasCommands;

            RelativeSizeAxes = useRelative ? Axes.Both : Axes.None;
            AutoSizeAxes = useRelative ? Axes.None : Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
            var stream = textureStore.GetStream(Video.Path);

            if (stream == null)
                return;

            InternalChild = drawableVideo = new Video(stream, false)
            {
                RelativeSizeAxes = RelativeSizeAxes,
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

                using (drawableVideo.BeginDelayedSequence(drawableVideo.Duration - 500))
                    drawableVideo.FadeOut(500);
            }
        }
    }
}

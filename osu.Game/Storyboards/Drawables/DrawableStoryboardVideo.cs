// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Screens.Play;
using osuTK;

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

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textureStore)
        {
            var stream = textureStore.GetStream(Video.Path);

            if (stream == null)
                return;

            InternalChild = drawableVideo = new Video(stream, false)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            };
        }

        private ScreenWithBeatmapBackground? parentScreen;
        private DrawableStoryboard? parentStoryboard;

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

            parentScreen = this.FindClosestParent<ScreenWithBeatmapBackground>();
            parentStoryboard = this.FindClosestParent<DrawableStoryboard>();
        }

        protected override void Update()
        {
            base.Update();

            if (drawableVideo != null)
            {
                // Stable fits storyboard videos to always take up the full window width.
                // Since we're inside `DrawableStoryboard`, we have to do a bit of messy stuff to make this happen.
                //
                // A future refactor could see videos moved out of the storyboard hierarchy, although that may come with other consequences.
                Vector2 fittableSize = parentScreen?.DrawSize ?? DrawSize;
                float storyboardScale = parentStoryboard?.AppliedScale.X ?? 1;

                drawableVideo.Size = new Vector2(fittableSize.X, fittableSize.X / drawableVideo.FillAspectRatio) / storyboardScale;
            }
        }
    }
}

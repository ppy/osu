// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardVideo : CompositeDrawable
    {
        public readonly StoryboardVideo Video;

        private DrawableVideo? drawableVideo;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            // In osu-stable, a mapper can add a scale command for a storyboard video.
            // This allows scaling based on the video's absolute size.
            //
            // If not specified we take up the full available space.
            bool useRelative = !video.Commands.Scale.Any();

            RelativeSizeAxes = useRelative ? Axes.Both : Axes.None;
            AutoSizeAxes = useRelative ? Axes.None : Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textureStore, StoryboardTriggerController triggerController)
        {
            var stream = textureStore.GetStream(Video.Path);

            if (stream == null)
                return;

            InternalChild = drawableVideo = CreateDrawableVideo(stream, false);
            drawableVideo.RelativeSizeAxes = RelativeSizeAxes;
            drawableVideo.FillMode = FillMode.Fill;
            drawableVideo.Anchor = Anchor.Centre;
            drawableVideo.Origin = Anchor.Centre;
            drawableVideo.Alpha = 0;

            Video.ApplyTransforms(drawableVideo, triggerController);
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

        protected virtual DrawableVideo CreateDrawableVideo(Stream stream, bool startAtCurrentTime) => new DrawableVideo(stream, startAtCurrentTime);

        protected partial class DrawableVideo : Video, IFlippable, IVectorScalable
        {
            private bool flipH;

            public bool FlipH
            {
                get => flipH;
                set
                {
                    if (flipH == value)
                        return;

                    flipH = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            private bool flipV;

            public bool FlipV
            {
                get => flipV;
                set
                {
                    if (flipV == value)
                        return;

                    flipV = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            private Vector2 vectorScale = Vector2.One;

            public Vector2 VectorScale
            {
                get => vectorScale;
                set
                {
                    if (vectorScale == value)
                        return;

                    if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(VectorScale)} must be finite, but is {value}.");

                    vectorScale = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            protected override Vector2 DrawScale
                => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y) * VectorScale;

            public override Anchor Origin => StoryboardExtensions.AdjustOrigin(base.Origin, VectorScale, FlipH, FlipV);

            protected override VideoSprite CreateSprite() => new ColouredDimmableVideoSprite(this);

            public DrawableVideo(Stream stream, bool startAtCurrentTime = true)
                : base(stream, startAtCurrentTime)
            {
            }
        }
    }
}

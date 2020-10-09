// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Skinning;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardAnimation : SkinReloadableDrawable, IFlippable, IVectorScalable
    {
        public StoryboardAnimation Animation { get; }

        private TextureAnimation drawableTextureAnimation;

        [Resolved]
        private TextureStore storyboardTextureStore { get; set; }

        private readonly List<string> texturePathsRaw = new List<string>();
        private readonly List<string> texturePaths = new List<string>();

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
                if (Math.Abs(value.X) < Precision.FLOAT_EPSILON)
                    value.X = Precision.FLOAT_EPSILON;
                if (Math.Abs(value.Y) < Precision.FLOAT_EPSILON)
                    value.Y = Precision.FLOAT_EPSILON;

                if (vectorScale == value)
                    return;

                if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(VectorScale)} must be finite, but is {value}.");

                vectorScale = value;
                Invalidate(Invalidation.MiscGeometry);
            }
        }

        public override bool RemoveWhenNotAlive => false;

        protected override Vector2 DrawScale
            => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y) * VectorScale;

        public override Anchor Origin
        {
            get
            {
                var origin = base.Origin;

                if (FlipH)
                {
                    if (origin.HasFlag(Anchor.x0))
                        origin = Anchor.x2 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                    else if (origin.HasFlag(Anchor.x2))
                        origin = Anchor.x0 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                }

                if (FlipV)
                {
                    if (origin.HasFlag(Anchor.y0))
                        origin = Anchor.y2 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                    else if (origin.HasFlag(Anchor.y2))
                        origin = Anchor.y0 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                }

                return origin;
            }
        }

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

        public DrawableStoryboardAnimation(StoryboardAnimation animation)
        {
            Animation = animation;
            Origin = animation.Origin;
            Position = animation.InitialPosition;

            LifetimeStart = animation.StartTime;
            LifetimeEnd = animation.EndTime;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap)
        {
            InternalChild = drawableTextureAnimation = new TextureAnimation
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Loop = Animation.LoopType == AnimationLoopType.LoopForever
            };

            for (var frame = 0; frame < Animation.FrameCount; frame++)
            {
                var framePath = Animation.Path.Replace(".", frame + ".");
                texturePathsRaw.Add(Path.GetFileNameWithoutExtension(framePath));

                var path = beatmap.Value.BeatmapSetInfo.Files.Find(f => f.Filename.Equals(framePath, StringComparison.OrdinalIgnoreCase))?.FileInfo.StoragePath;
                texturePaths.Add(path);
            }

            Animation.ApplyTransforms(this);
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            drawableTextureAnimation.ClearFrames();

            for (var frame = 0; frame < Animation.FrameCount; frame++)
            {
                var texture = skin?.GetTexture(texturePathsRaw[frame]) ?? storyboardTextureStore?.Get(texturePaths[frame]);

                if (texture == null)
                    continue;

                drawableTextureAnimation.AddFrame(texture, Animation.FrameDelay);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardSprite : CompositeDrawable, IFlippable, IVectorScalable
    {
        public StoryboardSprite Sprite { get; }

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
                    if (origin.HasFlagFast(Anchor.x0))
                        origin = Anchor.x2 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                    else if (origin.HasFlagFast(Anchor.x2))
                        origin = Anchor.x0 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                }

                if (FlipV)
                {
                    if (origin.HasFlagFast(Anchor.y0))
                        origin = Anchor.y2 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                    else if (origin.HasFlagFast(Anchor.y2))
                        origin = Anchor.y0 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                }

                return origin;
            }
        }

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

        public DrawableStoryboardSprite(StoryboardSprite sprite)
        {
            Sprite = sprite;
            Origin = sprite.Origin;
            Position = sprite.InitialPosition;

            LifetimeStart = sprite.StartTime;
            LifetimeEnd = sprite.EndTime;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore, Storyboard storyboard)
        {
            var drawable = storyboard.CreateSpriteFromResourcePath(Sprite.Path, textureStore);

            if (drawable != null)
                InternalChild = drawable;

            Sprite.ApplyTransforms(this);
        }
    }
}

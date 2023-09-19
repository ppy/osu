// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardSprite : CompositeDrawable, IFlippable, IVectorScalable
    {
        public StoryboardSprite Sprite { get; }

        private Sprite sprite = null!;

        public DrawableStoryboardSprite(StoryboardSprite sprite)
        {
            AutoSizeAxes = Axes.Both;

            Sprite = sprite;
            Origin = sprite.Origin;
            Position = sprite.InitialPosition;

            LifetimeStart = sprite.StartTime;
            LifetimeEnd = sprite.EndTimeForDisplay;
        }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore, Storyboard storyboard)
        {
            InternalChild = sprite = new Sprite
            {
                Texture = textureStore.Get(Sprite.Path)
            };

            if (sprite.Texture == null && storyboard.UseSkinSprites)
            {
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }

            Sprite.ApplyTransforms(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateMetrics();
        }

        #region Storyboard element shared code (copy paste until we refactor)

        private bool flipH;

        public bool FlipH
        {
            get => flipH;
            set
            {
                if (flipH == value)
                    return;

                flipH = value;
                updateMetrics();
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
                updateMetrics();
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
                updateMetrics();
            }
        }

        public override bool RemoveWhenNotAlive => false;

        private Anchor customOrigin;

        public override Anchor Origin
        {
            get => base.Origin;
            set
            {
                customOrigin = value;

                // actual origin update will be handled by the following method call.
                updateMetrics();
            }
        }

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && sprite.IsPresent && base.IsPresent;

        private void updateMetrics()
        {
            if (!IsLoaded)
                return;

            // Vector scale and flip is applied to our child to isolate it from external Scale (that can be applied by the storyboard itself).
            InternalChild.Scale = new Vector2(FlipH ? -1 : 1, FlipV ? -1 : 1) * VectorScale;

            Anchor resolvedOrigin = StoryboardExtensions.AdjustOrigin(customOrigin, VectorScale, FlipH, FlipV);

            // Likewise, origin has to be adjusted based on flip and vector scale usage.
            // The original "storyboard" origin is stored in customOrigin.
            base.Origin = resolvedOrigin;

            InternalChild.Anchor = resolvedOrigin;
            InternalChild.Origin = resolvedOrigin;
        }

        #endregion

        private void skinSourceChanged() => sprite.Texture = skin.GetTexture(Sprite.Path);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin.IsNotNull())
                skin.SourceChanged -= skinSourceChanged;
        }
    }
}

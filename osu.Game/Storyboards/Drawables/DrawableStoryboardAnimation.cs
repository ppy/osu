// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardAnimation : TextureAnimation, IFlippable, IVectorScalable
    {
        public StoryboardAnimation Animation { get; }

        public DrawableStoryboardAnimation(StoryboardAnimation animation)
        {
            Animation = animation;
            Origin = animation.Origin;
            Position = animation.InitialPosition;
            Loop = animation.LoopType == AnimationLoopType.LoopForever;

            LifetimeStart = animation.StartTime;
            LifetimeEnd = animation.EndTimeForDisplay;
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        [Resolved]
        private IBeatSyncProvider beatSyncProvider { get; set; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore, Storyboard storyboard)
        {
            int frameIndex = 0;

            Texture frameTexture = textureStore.Get(getFramePath(frameIndex));

            if (frameTexture != null)
            {
                // sourcing from storyboard.
                for (frameIndex = 0; frameIndex < Animation.FrameCount; frameIndex++)
                    AddFrame(textureStore.Get(getFramePath(frameIndex)), Animation.FrameDelay);
            }
            else if (storyboard.UseSkinSprites)
            {
                // fallback to skin if required.
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }

            Animation.ApplyTransforms(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Framework animation class tries its best to synchronise the animation at LoadComplete,
            // but in some cases (such as fast forward) this results in an incorrect start offset.
            //
            // In the case of storyboard animations, we want to synchronise with game time perfectly
            // so let's get a correct time based on gameplay clock and earliest transform.
            PlaybackPosition = beatSyncProvider.Clock.CurrentTime - Animation.EarliestTransformTime;

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
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

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

        private void skinSourceChanged()
        {
            ClearFrames();

            // When reading from a skin, we match stables weird behaviour where `FrameCount` is ignored
            // and resources are retrieved until the end of the animation.
            foreach (var texture in skin.GetTextures(Path.GetFileNameWithoutExtension(Animation.Path)!, default, default, true, string.Empty, out _))
                AddFrame(texture, Animation.FrameDelay);
        }

        private string getFramePath(int i) => Animation.Path.Replace(".", $"{i}.");

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin != null)
                skin.SourceChanged -= skinSourceChanged;
        }
    }
}

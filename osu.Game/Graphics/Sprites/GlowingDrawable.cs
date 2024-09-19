// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics.Sprites
{
    public abstract partial class GlowingDrawable : BufferedContainer
    {
        // Inflate draw quad to prevent glow from trimming at the edges.
        // Padding won't suffice since it will affect drawable position in cases when it's not centered.
        protected override Quad ComputeScreenSpaceDrawQuad()
            => base.ComputeScreenSpaceDrawQuad().AABBFloat.Inflate(new Vector2(Blur.KernelSize(BlurSigma.X), Blur.KernelSize(BlurSigma.Y)));

        public ColourInfo GlowColour
        {
            get => EffectColour;
            set
            {
                EffectColour = value;
                BackgroundColour = value.MultiplyAlpha(0f);
            }
        }

        protected GlowingDrawable()
            : base(cachedFrameBuffer: true)
        {
            AutoSizeAxes = Axes.Both;
            RedrawOnScale = false;
            DrawOriginal = true;
            Child = CreateDrawable();
        }

        protected abstract Drawable CreateDrawable();
    }
}

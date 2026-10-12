// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders.Types;

namespace osu.Game.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct DimParameters
    {
        public UniformVector4 DimColour;
    }

    public class ColouredDimmableSharedData : IDisposable
    {
        private IRenderer? renderer;

        public bool IsInitialized() => renderer != null;

        public IUniformBuffer<DimParameters>? OffsetParametersBuffer { get; private set; }
        public IUniformBuffer<DimParameters>? ZeroOffsetParametersBuffer { get; private set; }

        public void Initialize(IRenderer renderer)
        {
            // This relies on renderer being constant, need to check if that's the case, TODO
            if (IsInitialized()) return;

            this.renderer = renderer;

            OffsetParametersBuffer = renderer.CreateUniformBuffer<DimParameters>();
            ZeroOffsetParametersBuffer = renderer.CreateUniformBuffer<DimParameters>();
        }

        public void SetDrawColourOffset(Colour4 DrawColourOffset)
        {
            if (!IsInitialized()) return;

            OffsetParametersBuffer!.Data = new DimParameters
            {
                DimColour = new UniformVector4
                {
                    X = DrawColourOffset.R,
                    Y = DrawColourOffset.G,
                    Z = DrawColourOffset.B,
                    W = DrawColourOffset.A
                },
            };

            // Has to be set every frame, otherwise crashes with OpenGL(Experimental) renderer
            ZeroOffsetParametersBuffer!.Data = new DimParameters
            {
                DimColour = new UniformVector4
                {
                    X = 0.0f,
                    Y = 0.0f,
                    Z = 0.0f,
                    W = 1.0f
                },
            };
        }

        public void Dispose()
        {
            OffsetParametersBuffer?.Dispose();
            ZeroOffsetParametersBuffer?.Dispose();
        }
    }

    /// <summary>
    /// An interface for drawable objects whose dim colour can be adjusted.
    /// </summary>
    public interface IColouredDimmable : IDrawable
    {
        IColouredDimmable DrawColourOffsetSource
        {
            get
            {
                if (Parent is IColouredDimmable colouredDimmableParent)
                    return colouredDimmableParent.DrawColourOffsetSource;
                else if (Parent?.Parent is IColouredDimmable colouredDimmableGrandparent)
                    // Needed to skip intermediate containers.
                    return colouredDimmableGrandparent.DrawColourOffsetSource;

                return this;
            }
        }

        Colour4 DrawColourOffset
        {
            get
            {
                if (DrawColourInfo.Blending == BlendingParameters.Additive)
                    // Additive Drawables shouldn't have any offset, because things underneath them
                    // already have it added to them. This does rely on the fact that there is
                    // something non-additive (but still IColouredDimmable) behind this sprite,
                    // which seems to be the case with storyboards, as beatmap background is always enabled.
                    return Colour4.Black;

                IColouredDimmable source = DrawColourOffsetSource;

                if (source != null && source != this)
                    return source.DrawColourOffset;

                return Colour4.Black;
            }
        }

        ColouredDimmableSharedData? ColouredDimmableSharedData
        {
            get
            {
                IColouredDimmable source = DrawColourOffsetSource;

                if (source != null && source != this)
                    return source.ColouredDimmableSharedData;

                return null;
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
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

        public bool IsInitialised => renderer != null;

        private IUniformBuffer<DimParameters>? buffer;

        private DimParameters dimParameters;

        private ulong lastWrittenFrame = ulong.MaxValue;

        public void SetDrawColourOffset(Colour4 DrawColourOffset)
        {
            dimParameters = new DimParameters
            {
                DimColour = new UniformVector4
                {
                    X = DrawColourOffset.R,
                    Y = DrawColourOffset.G,
                    Z = DrawColourOffset.B,
                    W = DrawColourOffset.A
                },
            };

            lastWrittenFrame = ulong.MaxValue;
        }

        public IUniformBuffer<DimParameters> GetBuffer(IRenderer renderer)
        {
            if (this.renderer == null || buffer == null)
            {
                // Assumes that renderer cannot change during runtime.
                this.renderer = renderer;
                buffer = renderer.CreateUniformBuffer<DimParameters>();
            }

            if (lastWrittenFrame != renderer.FrameIndex)
            {
                buffer.Data = dimParameters;
                lastWrittenFrame = renderer.FrameIndex;
            }

            return buffer;
        }

        public void Dispose()
        {
            renderer?.ScheduleDisposal(static d => d.dispose(), this);
            GC.SuppressFinalize(this);
        }

        private void dispose()
        {
            Debug.Assert(IsInitialised);

            buffer?.Dispose();
        }
    }

    /// <summary>
    /// An interface for objects that provide colour offset to drawables.
    /// </summary>
    public interface IColouredDimmableSource : IDrawable
    {
        ColouredDimmableSharedData? ColouredDimmableSharedData { get; }
    }
}

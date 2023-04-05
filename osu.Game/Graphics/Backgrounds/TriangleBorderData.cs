// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using osu.Framework.Graphics.Shaders.Types;

namespace osu.Game.Graphics.Backgrounds
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct TriangleBorderData
    {
        public UniformFloat Thickness;
        public UniformFloat TexelSize;
        private readonly UniformPadding8 pad1;
    }
}

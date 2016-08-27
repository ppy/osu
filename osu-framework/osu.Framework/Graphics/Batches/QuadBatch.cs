//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Graphics.ES20;
using osu.Framework.Graphics.OpenGL.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osu.Framework.Graphics.Batches
{
    public class QuadBatch<T> : VertexBatch<T> where T : struct, IEquatable<T>
    {
        public QuadBatch(int size, int fixedBufferAmount)
            : base(size, fixedBufferAmount)
        {
        }

        protected override VertexBuffer<T> CreateVertexBuffer()
        {
            return new QuadVertexBuffer<T>(Size, BufferUsageHint.DynamicDraw);
        }
    }
}

//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Framework.Graphics.Batches
{
    public abstract class VertexBatch<T> : IVertexBatch where T : struct, IEquatable<T>
    {
        public List<VertexBuffer<T>> VertexBuffers = new List<VertexBuffer<T>>();

        public int Size { get; private set; }

        private int changeBeginIndex = -1;
        private int changeEndIndex = -1;

        private int currentVertexBuffer = 0;
        private int currentVertex = 0;
        private int lastVertex = 0;

        private int fixedBufferAmount;

        private VertexBuffer<T> CurrentVertexBuffer => VertexBuffers[currentVertexBuffer];

        public VertexBatch(int size, int fixedBufferAmount)
        {
            // Vertex buffers of size 0 don't make any sense. Let's not blindly hope for good behavior of OpenGL.
            Debug.Assert(size > 0);

            Size = size;
            this.fixedBufferAmount = fixedBufferAmount;
        }

        #region Disposal
        ~VertexBatch()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
                foreach (VertexBuffer<T> vbo in VertexBuffers)
                    vbo.Dispose();
        }
        #endregion

        public void ResetCounters()
        {
            changeBeginIndex = -1;
            currentVertexBuffer = 0;
            currentVertex = 0;
            lastVertex = 0;
        }

        protected abstract VertexBuffer<T> CreateVertexBuffer();

        public void Add(T v)
        {
            while (currentVertexBuffer >= VertexBuffers.Count)
                VertexBuffers.Add(CreateVertexBuffer());

            VertexBuffer<T> vertexBuffer = CurrentVertexBuffer;

            if (!vertexBuffer.Vertices[currentVertex].Equals(v))
            {
                if (changeBeginIndex == -1)
                    changeBeginIndex = currentVertex;

                changeEndIndex = currentVertex + 1;
            }

            vertexBuffer.Vertices[currentVertex] = v;
            ++currentVertex;

            if (currentVertex >= vertexBuffer.Vertices.Length)
            {
                Draw();
                lastVertex = currentVertex = 0;
            }
        }

        public void Draw()
        {
            if (currentVertex == lastVertex)
                return;

            GLWrapper.SetActiveBatch(this);

            VertexBuffer<T> vertexBuffer = CurrentVertexBuffer;
            if (changeBeginIndex >= 0)
                vertexBuffer.UpdateRange(changeBeginIndex, changeEndIndex);

            vertexBuffer.DrawRange(lastVertex, currentVertex);

            // When using multiple buffers we advance to the next one with every draw to prevent contention on the same buffer with future vertex updates.
            currentVertexBuffer = (currentVertexBuffer + 1) % fixedBufferAmount;
            currentVertex = 0;

            lastVertex = currentVertex;
            changeBeginIndex = -1;
        }
    }
}

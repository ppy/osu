//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using osu.Framework.Cached;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using osu.Framework.Graphics.OpenGL.Textures;
using Scheduler = osu.Framework.Threading.Scheduler;

namespace osu.Framework.Graphics.OpenGL
{
    public static class GLWrapper
    {
        public const int MAX_BATCHES = 3;

        public static Rectangle Scissor { get; private set; }
        public static Rectangle Viewport { get; private set; }
        public static Rectangle Ortho { get; private set; }
        public static Matrix4 ProjectionMatrix { get; private set; }

        public static Rectangle ViewportZeroBased => new Rectangle(0, Game.Window.Height - Viewport.Height, Viewport.Width, Viewport.Height);

        public static bool UsingBackbuffer => lastFrameBuffer == 0;

        public static int CurrentBatchIndex { get; private set; }

        /// <summary>
        /// Check whether we have an initialised and non-disposed GL context.
        /// </summary>
        public static bool HasContext => GraphicsContext.CurrentContext != null;

        private static Cached<int> maxTextureSizeBacking = new Cached<int>();
        public static int MaxTextureSize => maxTextureSizeBacking.Refresh(() => GL.GetInteger(GetPName.MaxTextureSize));

        private static Scheduler resetScheduler = new Scheduler();

        internal static void Reset()
        {
            resetScheduler.Update();

            lastBoundTexture = -1;

            lastSrcBlend = BlendingFactorSrc.Zero;
            lastDestBlend = BlendingFactorDest.Zero;

            lastFrameBuffer = 0;

            viewportStack.Clear();
            orthoStack.Clear();
            scissorStack.Clear();

            Scissor = Rectangle.Empty;
            Viewport = Rectangle.Empty;
            Ortho = Rectangle.Empty;

            PushViewport();
            PushScissor();

            CurrentBatchIndex = (CurrentBatchIndex + 1) % MAX_BATCHES;
        }

        /// <summary>
        /// Enqueues a texture to be uploaded in the next frame.
        /// </summary>
        /// <param name="texture">The texture to be uploaded.</param>
        public static void EnqueueTextureUpload(TextureGL texture)
        {
            if (!HasContext) return;

            resetScheduler.Add(() => texture.Upload());
        }

        private static int[] lastBoundBuffers = new int[2];
        /// <summary>
        /// Bind an OpenGL buffer object.
        /// </summary>
        /// <param name="target">The buffer type to bind.</param>
        /// <param name="buffer">The buffer ID to bind.</param>
        /// <returns>Whether an actual bind call was necessary. This value is false when repeatedly binding the same buffer.</returns>
        public static bool BindBuffer(BufferTarget target, int buffer)
        {
            int bufferIndex = (int)(target - BufferTarget.ArrayBuffer);
            if (lastBoundBuffers[bufferIndex] == buffer)
                return false;

            lastBoundBuffers[bufferIndex] = buffer;
            GL.BindBuffer(target, buffer);
            return true;
        }

        private static IVertexBatch lastActiveBatch;
        /// <summary>
        /// Sets the last vertex batch used for drawing.
        /// <para>
        /// This is done so that various methods that change GL state can force-draw the batch
        /// before continuing with the state change.
        /// </para>
        /// </summary>
        /// <param name="batch">The batch.</param>
        internal static void SetActiveBatch(IVertexBatch batch)
        {
            lastActiveBatch = batch;
        }

        private static int lastBoundTexture = -1;
        /// <summary>
        /// Binds a texture to darw with.
        /// </summary>
        /// <param name="textureId"></param>
        public static void BindTexture(int textureId, TextureUnit unit = TextureUnit.Texture0)
        {
            if (lastBoundTexture != textureId)
            {
                lastActiveBatch?.Draw();

                GL.BindTexture(TextureTarget.Texture2D, textureId);
                lastBoundTexture = textureId;
            }
        }

        private static BlendingFactorSrc lastSrcBlend;
        private static BlendingFactorDest lastDestBlend;

        /// <summary>
        /// Sets the blending function to draw with.
        /// </summary>
        /// <param name="src">The source blending factor.</param>
        /// <param name="dest">The destination blending factor.</param>
        public static void SetBlend(BlendingFactorSrc src, BlendingFactorDest dest)
        {
            if (lastSrcBlend == src && lastDestBlend == dest)
                return;

            lastActiveBatch?.Draw();

            GL.BlendFunc(src, dest);

            lastSrcBlend = src;
            lastDestBlend = dest;
        }

        private static int lastFrameBuffer = 0;
        /// <summary>
        /// Binds a framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer to bind.</param>
        /// <returns>The last bound framebuffer.</returns>
        public static int BindFrameBuffer(int frameBuffer)
        {
            if (lastFrameBuffer == frameBuffer)
                return lastFrameBuffer;

            int last = lastFrameBuffer;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            lastFrameBuffer = frameBuffer;

            return last;
        }

        private static Stack<Rectangle> viewportStack = new Stack<Rectangle>();
        /// <summary>
        /// Applies a new viewport rectangle.
        /// </summary>
        /// <param name="viewport">The viewport rectangle.</param>
        public static void PushViewport(Rectangle? viewport = null)
        {
            Rectangle actualRect = Rectangle.Empty;

            if (viewport.HasValue)
            {
                actualRect = viewport.Value;

                if (actualRect.Width < 0)
                {
                    actualRect.X += viewport.Value.Width;
                    actualRect.Width = -viewport.Value.Width;
                }

                if (actualRect.Height < 0)
                {
                    actualRect.Y += viewport.Value.Height;
                    actualRect.Height = -viewport.Value.Height;
                }
            }
            else
            {
                actualRect = new Rectangle(0, 0, Game.Window.Width, Game.Window.Height);
            }

            PushOrtho();

            viewportStack.Push(Viewport);

            if (Viewport == actualRect)
                return;
            Viewport = actualRect;

            GL.Viewport(Viewport.Left, Viewport.Top, Viewport.Width, Viewport.Height);
        }

        /// <summary>
        /// Applies the last viewport rectangle.
        /// </summary>
        public static void PopViewport()
        {
            if (viewportStack.Count == 0)
                return;

            PopOrtho();

            Rectangle actualRect = viewportStack.Pop();

            if (Viewport == actualRect)
                return;
            Viewport = actualRect;

            GL.Viewport(Viewport.Left, Viewport.Top, Viewport.Width, Viewport.Height);
        }

        private static Stack<Rectangle> orthoStack = new Stack<Rectangle>();
        /// <summary>
        /// Applies a new orthographic projection rectangle.
        /// </summary>
        /// <param name="ortho">The orthographic projection rectangle.</param>
        public static void PushOrtho(Rectangle? ortho = null)
        {
            Rectangle actualRect = ortho ?? new Rectangle(0, 0, Game.Window.Width, Game.Window.Height);

            orthoStack.Push(Ortho);

            if (Ortho == actualRect)
                return;
            Ortho = actualRect;

            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(Ortho.Left, Ortho.Right, Ortho.Bottom, Ortho.Top, -1, 1);
            Shader.SetGlobalProperty(@"g_ProjMatrix", ProjectionMatrix);
        }

        /// <summary>
        /// Applies the last orthographic projection rectangle.
        /// </summary>
        public static void PopOrtho()
        {
            if (orthoStack.Count == 0)
                return;

            Rectangle actualRect = orthoStack.Pop();

            if (Ortho == actualRect)
                return;
            Ortho = actualRect;

            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(Ortho.Left, Ortho.Right, Ortho.Bottom, Ortho.Top, -1, 1);
            Shader.SetGlobalProperty(@"g_ProjMatrix", ProjectionMatrix);
        }

        private static Stack<Rectangle> scissorStack = new Stack<Rectangle>();

        /// <summary>
        /// Applies a new scissor rectangle.
        /// </summary>
        /// <param name="rect">The scissor rectangle.</param>
        public static void PushScissor(Rectangle? rect = null)
        {
            Rectangle actualRect = rect ?? Viewport;
            if (rect != null)
            {
                actualRect.X += Viewport.X;
                actualRect.Y += Viewport.Y;
            }

            // Ensure the rectangle only has positive width and height. (Required by OGL)
            if (actualRect.Width < 0)
            {
                actualRect.X += actualRect.Width;
                actualRect.Width = -actualRect.Width;
            }

            if (actualRect.Height < 0)
            {
                actualRect.Y += actualRect.Height;
                actualRect.Height = -actualRect.Height;
            }

            scissorStack.Push(Scissor);

            if (Scissor == actualRect)
                return;
            Scissor = actualRect;

            GL.Scissor(Scissor.X, Viewport.Height - Scissor.Bottom, Scissor.Width, Scissor.Height);
        }

        /// <summary>
        /// Applies the last scissor rectangle.
        /// </summary>
        public static void PopScissor()
        {
            if (scissorStack.Count == 0)
                return;

            Rectangle actualRect = scissorStack.Pop();

            if (Scissor == actualRect)
                return;

            Scissor = actualRect;

            GL.Scissor(Scissor.X, Viewport.Height - Scissor.Bottom, Scissor.Width, Scissor.Height);
        }

        /// <summary>
        /// Deletes a framebuffer.
        /// </summary>
        /// <param name="frameBuffer">The framebuffer to delete.</param>
        internal static void DeleteFramebuffer(int frameBuffer)
        {
            if (frameBuffer == -1 || !HasContext) return;

            resetScheduler.Add(() =>
            {
                GL.DeleteFramebuffer(frameBuffer);
            });
        }

        /// <summary>
        /// Deletes a buffer object.
        /// </summary>
        /// <param name="vboId">The buffer object to delete.</param>
        internal static void DeleteBuffer(int vboId)
        {
            if (!HasContext) return;

            resetScheduler.Add(() =>
            {
                GL.DeleteBuffer(vboId);
            });
        }

        /// <summary>
        /// Deletes textures.
        /// </summary>
        /// <param name="ids">An array of textures to delete.</param>
        internal static void DeleteTextures(params int[] ids)
        {
            if (!HasContext) return;

            resetScheduler.Add(() =>
            {
                GL.DeleteTextures(ids.Length, ids);
            });
        }

        /// <summary>
        /// Deletes a shader program.
        /// </summary>
        /// <param name="shader">The shader program to delete.</param>
        internal static void DeleteProgram(Shader shader)
        {
            if (!HasContext) return;

            resetScheduler.Add(() =>
            {
                GL.DeleteProgram(shader);
            });
        }

        /// <summary>
        /// Deletes a shader part.
        /// </summary>
        /// <param name="shaderPart">The shader part to delete.</param>
        internal static void DeleteShader(ShaderPart shaderPart)
        {
            if (!HasContext) return;

            resetScheduler.Add(() =>
            {
                GL.DeleteShader(shaderPart);
            });
        }

        public static int CurrentShader { get; private set; }

        public static void UseProgram(int shader)
        {
            if (!HasContext) return;

            resetScheduler.Add(() =>
            {
                if (CurrentShader == shader) return;

                GL.UseProgram(shader);
                CurrentShader = shader;
            });
        }
    }
}

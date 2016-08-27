//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Windows.Forms;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace osu.Framework.Framework
{
    public abstract class BasicGameHost : Container
    {
        public abstract BasicGameWindow Window { get; }
        public abstract GLControl GLControl { get; }
        public abstract bool IsActive { get; }

        public event EventHandler Activated;
        public event EventHandler Deactivated;
        public event EventHandler Exiting;
        public event EventHandler Idle;

        public override bool IsVisible => true;

        public override Vector2 Size => new Vector2(Window?.Form.ClientSize.Width ?? 0, Window?.Form.ClientSize.Height ?? 0);

        protected virtual void OnActivated(object sender, EventArgs args)
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDeactivated(object sender, EventArgs args)
        {
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnExiting(object sender, EventArgs args)
        {
            Exiting?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnIdle(object sender, EventArgs args)
        {
            GLWrapper.Reset();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UpdateSubTree();
            DrawSubTree();

            Idle?.Invoke(this, EventArgs.Empty);

            GLControl.SwapBuffers();
        }

        private bool exitRequested;
        public void Exit()
        {
            exitRequested = true;
        }

        public virtual void Run()
        {
            Window.ClientSizeChanged += delegate { Invalidate(); };

            GLControl.Initialize();

            Exception error = null;

            try
            {
                Application.Idle += OnApplicationIdle;
                Application.Run(Window.Form);
            }
            catch (OutOfMemoryException e)
            {
                error = e;
            }
            finally
            {
                Application.Idle -= OnApplicationIdle;

                if (error == null || !(error is OutOfMemoryException))
                    //we don't want to attempt a safe shutdown is memory is low; it may corrupt database files.
                    OnExiting(this, null);
            }
        }

        protected virtual void OnApplicationIdle(object sender, EventArgs e)
        {
            if (exitRequested)
                Window.Close();
            else
                OnIdle(sender, e);
        }

        public void Load(Game game)
        {
            game.SetHost(this);
            Add(game);
        }
    }
}

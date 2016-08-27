//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Framework.Input.Handlers
{
    public abstract class InputHandler
    {
        /// <summary>
        /// Used to initialize resources specific to this InputHandler. It gets called once.
        /// </summary>
        /// <returns>Success of the initialization.</returns>
        public abstract bool Initialize();

        /// <summary>
        /// Used to clean up resources specific to this InputHandler. It gets called once and only after Initialize has been called and returned true.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets called whenever the resolution of the OsuGame or the desktop changes.
        /// </summary>
        public virtual void OnResolutionChange()
        {

        }

        /// <summary>
        /// Gets called for every frame of the Game. This should be used to update the public state of this InputHandler according to external circumstances.
        /// </summary>
        /// <param name="isActive">Denotes whether this input is currently active and will be used for controlling the main cursor.</param>
        public abstract void UpdateInput(bool isActive);

        /// <summary>
        /// Indicates whether this InputHandler is currently delivering input by the user. When handling input the OsuGame uses the first InputHandler which is active.
        /// </summary>
        public abstract bool IsActive
        {
            get;
        }

        /// <summary>
        /// Indicated how high of a priority this handler has. The active handler with the highest priority is controlling the cursor at any given time.
        /// </summary>
        public abstract int Priority
        {
            get;
        }
    }

    public class InputHandlerComparer : IComparer<InputHandler>
    {
        public int Compare(InputHandler h1, InputHandler h2)
        {
            return h2.Priority.CompareTo(h1.Priority);
        }
    }
}

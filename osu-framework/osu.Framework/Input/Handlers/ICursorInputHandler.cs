//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;
using OpenTK;

namespace osu.Framework.Input.Handlers
{
    public interface ICursorInputHandler
    {
        /// <summary>
        /// Set the current position of this handler externally. This is called whenever another CursorInputHandler updates the global
        /// mouse position. This is not called when this CursorInputHandler updates the global mouse position.
        /// </summary>
        /// <param name="pos">The position to be set to.</param>
        void SetPosition(Vector2 pos);

        /// <summary>
        /// The current position of this CursorInputHandler.
        /// </summary>
        Vector2? Position
        {
            get;
        }

        Vector2 Size
        {
            get;
        }

        /// <summary>
        /// The current state of the left mouse button of this CursorInputHandler. A value of null means the global button state is not updated.
        /// </summary>
        bool? Left
        {
            get;
        }

        /// <summary>
        /// The current state of the right mouse button of this CursorInputHandler. A value of null means the global button state is not updated.
        /// </summary>
        bool? Right
        {
            get;
        }

        /// <summary>
        /// The current state of the middle mouse button of this CursorInputHandler. A value of null means the global button state is not updated.
        /// </summary>
        bool? Middle
        {
            get;
        }

        /// <summary>
        /// The current state of the back mouse button of this CursorInputHandler. A value of null means the global button state is not updated.
        /// </summary>
        bool? Back
        {
            get;
        }

        /// <summary>
        /// The current state of the forward mouse button of this CursorInputHandler. A value of null means the global button state is not updated.
        /// </summary>
        bool? Forward
        {
            get;
        }

        bool? WheelUp { get; }
        bool? WheelDown { get; }
        

        /// <summary>
        /// If the cursor had intermediate positions between the position at the previous frame and the new position at the current frame,
        /// then they are contained here. This is mainly used for generating smoother cursor trails.
        /// </summary>
        List<Vector2> IntermediatePositions
        {
            get;
        }

        bool Clamping { get; set; }
    }
}

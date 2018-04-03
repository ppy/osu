// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Platform;
using OpenTK;

namespace osu.Game.Input.Handlers
{
    public abstract class ReplayInputHandler : InputHandler
    {
        /// <summary>
        /// A function that converts coordinates from gamefield to screen space.
        /// </summary>
        public Func<Vector2, Vector2> GamefieldToScreenSpace { protected get; set; }

        /// <summary>
        /// Update the current frame based on an incoming time value.
        /// There are cases where we return a "must-use" time value that is different from the input.
        /// This is to ensure accurate playback of replay data.
        /// </summary>
        /// <param name="time">The time which we should use for finding the current frame.</param>
        /// <returns>The usable time value. If null, we should not advance time as we do not have enough data.</returns>
        public abstract double? SetFrameFromTime(double time);

        public override bool Initialize(GameHost host) => true;

        public override bool IsActive => true;

        public override int Priority => 0;

        public class ReplayState<T> : InputState
            where T : struct
        {
            public List<T> PressedActions;

            public override InputState Clone()
            {
                var clone = (ReplayState<T>)base.Clone();
                clone.PressedActions = new List<T>(PressedActions);
                return clone;
            }
        }
    }
}

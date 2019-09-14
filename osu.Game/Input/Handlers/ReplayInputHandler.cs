// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;

namespace osu.Game.Input.Handlers
{
    public abstract class ReplayInputHandler : GameplayInputHandler
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
    }
}

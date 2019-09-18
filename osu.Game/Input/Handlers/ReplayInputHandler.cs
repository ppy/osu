// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Framework.Input.States;
using osu.Framework.Platform;
using osu.Game.Rulesets.UI;
using osuTK;

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

        public class ReplayState<T> : IInput
            where T : struct
        {
            public List<T> PressedActions;

            public void Apply(InputState state, IInputStateChangeHandler handler)
            {
                if (!(state is RulesetInputManagerInputState<T> inputState))
                    throw new InvalidOperationException($"{nameof(ReplayState<T>)} should only be applied to a {nameof(RulesetInputManagerInputState<T>)}");

                var lastPressed = inputState.LastReplayState?.PressedActions ?? new List<T>();
                var released = lastPressed.Except(PressedActions).ToArray();
                var pressed = PressedActions.Except(lastPressed).ToArray();

                inputState.LastReplayState = this;

                handler.HandleInputStateChange(new ReplayStateChangeEvent<T>(state, this, released, pressed));
            }
        }

        public class ReplayStateChangeEvent<T> : InputStateChangeEvent
        {
            public readonly T[] ReleasedActions;
            public readonly T[] PressedActions;

            public ReplayStateChangeEvent(InputState state, IInput input, T[] releasedActions, T[] pressedActions)
                : base(state, input)
            {
                ReleasedActions = releasedActions;
                PressedActions = pressedActions;
            }
        }
    }
}

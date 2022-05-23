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
using osu.Game.Rulesets.Replays;
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

        public class ReplayState<T> : IInput
            where T : struct
        {
            public List<T> PressedActions;

            public void Apply(InputState state, IInputStateChangeHandler handler)
            {
                if (!(state is RulesetInputManagerInputState<T> inputState))
                    throw new InvalidOperationException($"{nameof(ReplayState<T>)} should only be applied to a {nameof(RulesetInputManagerInputState<T>)}");

                T[] released = Array.Empty<T>();
                T[] pressed = Array.Empty<T>();

                var lastPressed = inputState.LastReplayState?.PressedActions;

                if (lastPressed == null || lastPressed.Count == 0)
                {
                    pressed = PressedActions.ToArray();
                }
                else if (PressedActions.Count == 0)
                {
                    released = lastPressed.ToArray();
                }
                else if (!lastPressed.SequenceEqual(PressedActions))
                {
                    released = lastPressed.Except(PressedActions).ToArray();
                    pressed = PressedActions.Except(lastPressed).ToArray();
                }

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

        /// <summary>
        /// An <see cref="IInput"/> that is triggered when a frame containing replay statistics arrives.
        /// </summary>
        public class ReplayStatisticsFrameInput : IInput
        {
            /// <summary>
            /// The frame containing the statistics.
            /// </summary>
            public ReplayFrame Frame;

            public void Apply(InputState state, IInputStateChangeHandler handler)
            {
                handler.HandleInputStateChange(new ReplayStatisticsFrameEvent(state, this, Frame));
            }
        }

        /// <summary>
        /// An <see cref="InputStateChangeEvent"/> that is triggered when a frame containing replay statistics arrives.
        /// </summary>
        public class ReplayStatisticsFrameEvent : InputStateChangeEvent
        {
            /// <summary>
            /// The frame containing the statistics.
            /// </summary>
            public readonly ReplayFrame Frame;

            public ReplayStatisticsFrameEvent(InputState state, IInput input, ReplayFrame frame)
                : base(state, input)
            {
                Frame = frame;
            }
        }
    }
}

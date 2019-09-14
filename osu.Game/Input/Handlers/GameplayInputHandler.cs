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

namespace osu.Game.Input.Handlers
{
    public class GameplayInputHandler : InputHandler
    {
        public override bool Initialize(GameHost host) => true;

        public override bool IsActive => true;

        public override int Priority => 0;

        public class GameplayInputState<T> : IInput
            where T : struct
        {
            public List<T> PressedActions;

            public void Apply(InputState state, IInputStateChangeHandler handler)
            {
                if (!(state is RulesetInputManagerInputState<T> inputState))
                    throw new InvalidOperationException($"{nameof(GameplayInputState<T>)} should only be applied to a {nameof(RulesetInputManagerInputState<T>)}");

                var lastPressed = inputState.LastGameplayState?.PressedActions ?? new List<T>();
                inputState.LastGameplayState = this;

                handler.HandleInputStateChange(new GameplayStateChangeEvent<T>(state, this)
                {
                    PressedActions = PressedActions.Except(lastPressed).ToArray(),
                    ReleasedActions = lastPressed.Except(PressedActions).ToArray(),
                });
            }
        }

        public class GameplayStateChangeEvent<T> : InputStateChangeEvent
            where T : struct
        {
            public T[] PressedActions;
            public T[] ReleasedActions;

            public GameplayStateChangeEvent(InputState state, IInput input)
                : base(state, input)
            {
            }
        }
    }
}

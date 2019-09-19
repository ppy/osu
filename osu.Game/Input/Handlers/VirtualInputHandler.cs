// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;

namespace osu.Game.Input.Handlers
{
    public class VirtualInputHandler<T> : VirtualInputHandler
        where T : struct
    {
        public new List<T> Actions => base.Actions.Cast<T>().ToList();

        public override List<IInput> GetPendingInputs() => new List<IInput>()
        {
            new GameplayInputState<T>
            {
                PressedActions = Actions,
            }
        };
    }

    public class VirtualInputHandler : GameplayInputHandler
    {
        public readonly List<object> Actions = new List<object>();
    }
}

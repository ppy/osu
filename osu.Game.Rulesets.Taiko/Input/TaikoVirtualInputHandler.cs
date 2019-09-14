// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Game.Input.Handlers;

namespace osu.Game.Rulesets.Taiko.Input
{
    public class TaikoVirtualInputHandler : VirtualInputHandler
    {
        public override List<IInput> GetPendingInputs() => new List<IInput>
        {
            new GameplayInputState<TaikoAction>
            {
                PressedActions = Actions.Cast<TaikoAction>().ToList(),
            }
        };
    }
}

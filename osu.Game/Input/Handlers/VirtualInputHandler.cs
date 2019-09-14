// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;

namespace osu.Game.Input.Handlers
{
    public class VirtualInputHandler : GameplayInputHandler
    {
        public readonly List<object> Actions = new List<object>();

        public override List<IInput> GetPendingInputs() => new List<IInput>();
    }
}

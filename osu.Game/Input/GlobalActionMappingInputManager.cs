// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;

namespace osu.Game.Input
{
    public class GlobalActionMappingInputManager : ActionMappingInputManager<GlobalAction>
    {
        protected override IDictionary<KeyCombination, GlobalAction> CreateDefaultMappings() => new Dictionary<KeyCombination, GlobalAction>
        {
            { Key.F8, GlobalAction.ToggleChat },
            { Key.F9, GlobalAction.ToggleSocial },
            { new[] { Key.LControl, Key.LAlt, Key.R }, GlobalAction.ResetInputSettings },
            { new[] { Key.LControl, Key.T }, GlobalAction.ToggleToolbar },
            { new[] { Key.LControl, Key.O }, GlobalAction.ToggleSettings },
            { new[] { Key.LControl, Key.D }, GlobalAction.ToggleDirect },
        };
    }
}

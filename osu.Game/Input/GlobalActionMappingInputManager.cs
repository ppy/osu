// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;

namespace osu.Game.Input
{
    public class GlobalActionMappingInputManager : ActionMappingInputManager<OsuAction>
    {
        protected override IDictionary<KeyCombination, OsuAction> CreateDefaultMappings() => new Dictionary<KeyCombination, OsuAction>
        {
            { Key.F8, OsuAction.ToggleChat },
            { Key.F9, OsuAction.ToggleSocial },
        };
    }
}

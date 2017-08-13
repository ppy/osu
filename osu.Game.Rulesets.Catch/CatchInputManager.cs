// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using OpenTK.Input;

namespace osu.Game.Rulesets.Catch
{
    public class CatchInputManager : DatabasedKeyBindingInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset) : base(ruleset, simultaneousMode: SimultaneousBindingMode.Unique)
        {
        }

        protected override IEnumerable<KeyBinding> CreateDefaultMappings() => new[]
        {
            new KeyBinding( Key.Z, CatchAction.MoveLeft),
            new KeyBinding( Key.Left, CatchAction.MoveLeft),
            new KeyBinding( Key.X, CatchAction.MoveRight),
            new KeyBinding( Key.Right, CatchAction.MoveRight),
            new KeyBinding( Key.LShift, CatchAction.Dash),
            new KeyBinding( Key.RShift, CatchAction.Dash),
        };
    }

    public enum CatchAction
    {
        [Description("Move left")]
        MoveLeft,
        [Description("Move right")]
        MoveRight,
        [Description("Engage dash")]
        Dash
    }
}

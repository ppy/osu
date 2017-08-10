// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Input;
using OpenTK.Input;

namespace osu.Game.Rulesets.Catch
{
    public class CatchInputManager : ActionMappingInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset) : base(ruleset, allowConcurrentActions: true)
        {
        }

        protected override IDictionary<KeyCombination, CatchAction> CreateDefaultMappings() => new Dictionary<KeyCombination, CatchAction>
        {
            { Key.Z, CatchAction.MoveLeft },
            { Key.Left, CatchAction.MoveLeft },
            { Key.X, CatchAction.MoveRight },
            { Key.Right, CatchAction.MoveRight },
            { Key.LShift, CatchAction.Dash },
            { Key.RShift, CatchAction.Dash },
        };
    }

    public enum CatchAction
    {
        MoveLeft,
        MoveRight,
        Dash
    }
}

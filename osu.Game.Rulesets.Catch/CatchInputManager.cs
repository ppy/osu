// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;
using static osu.Game.Input.Handlers.ReplayInputHandler;
using static osu.Game.Rulesets.Catch.Replays.CatchFramedReplayInputHandler;

namespace osu.Game.Rulesets.Catch
{
    public class CatchInputManager : RulesetInputManager<CatchAction>
    {
        public CatchInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        public override void HandleInputStateChange(InputStateChangeEvent inputStateChange)
        {
            if (inputStateChange is ReplayStateChangeEvent<CatchAction> replayStateChange)
            {
                var replayState = (RulesetInputManagerInputState<CatchAction>)replayStateChange.State;
                var lastState = (CatchReplayState)replayState.LastReplayState;

                if (lastState.CatcherX != null)
                {
                    foreach (Drawable drawable in NonPositionalInputQueue)
                    {
                        if (drawable is Catcher catcher)
                            catcher.UpdatePosition(lastState.CatcherX.Value);
                    }
                }
            }

            base.HandleInputStateChange(inputStateChange);
        }
    }

    public enum CatchAction
    {
        [Description("Move left")]
        MoveLeft,

        [Description("Move right")]
        MoveRight,

        [Description("Engage dash")]
        Dash,
    }
}

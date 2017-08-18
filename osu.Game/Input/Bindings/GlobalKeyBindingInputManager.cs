// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;

namespace osu.Game.Input.Bindings
{
    public class GlobalKeyBindingInputManager : DatabasedKeyBindingInputManager<GlobalAction>
    {
        private readonly Drawable handler;

        public GlobalKeyBindingInputManager(OsuGameBase game)
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        public override IEnumerable<KeyBinding> DefaultKeyBindings => new[]
        {
            new KeyBinding(Key.F8, GlobalAction.ToggleChat),
            new KeyBinding(Key.F9, GlobalAction.ToggleSocial),
            new KeyBinding(new[] { Key.LControl, Key.LAlt, Key.R }, GlobalAction.ResetInputSettings),
            new KeyBinding(new[] { Key.LControl, Key.T }, GlobalAction.ToggleToolbar),
            new KeyBinding(new[] { Key.LControl, Key.O }, GlobalAction.ToggleSettings),
            new KeyBinding(new[] { Key.LControl, Key.D }, GlobalAction.ToggleDirect),
        };

        protected override IEnumerable<Drawable> GetKeyboardInputQueue() =>
            handler == null ? base.GetKeyboardInputQueue() : new[] { handler }.Concat(base.GetKeyboardInputQueue());
    }

    public enum GlobalAction
    {
        [Description("Toggle chat overlay")]
        ToggleChat,
        [Description("Toggle social overlay")]
        ToggleSocial,
        [Description("Reset input settings")]
        ResetInputSettings,
        [Description("Toggle toolbar")]
        ToggleToolbar,
        [Description("Toggle settings")]
        ToggleSettings,
        [Description("Toggle osu!direct")]
        ToggleDirect,
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;

namespace osu.Game.Input.Bindings
{
    public class GlobalKeyBindingInputManager : DatabasedKeyBindingInputManager<GlobalAction>, IHandleGlobalInput
    {
        private readonly Drawable handler;

        public GlobalKeyBindingInputManager(OsuGameBase game)
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        public override IEnumerable<KeyBinding> DefaultKeyBindings => GlobalKeyBindings.Concat(InGameKeyBindings);

        public IEnumerable<KeyBinding> GlobalKeyBindings => new[]
        {
            new KeyBinding(InputKey.F8, GlobalAction.ToggleChat),
            new KeyBinding(InputKey.F9, GlobalAction.ToggleSocial),
            new KeyBinding(new[] { InputKey.Control, InputKey.Alt, InputKey.R }, GlobalAction.ResetInputSettings),
            new KeyBinding(new[] { InputKey.Control, InputKey.T }, GlobalAction.ToggleToolbar),
            new KeyBinding(new[] { InputKey.Control, InputKey.O }, GlobalAction.ToggleSettings),
            new KeyBinding(new[] { InputKey.Up }, GlobalAction.IncreaseVolume),
            new KeyBinding(new[] { InputKey.MouseWheelUp }, GlobalAction.IncreaseVolume),
            new KeyBinding(new[] { InputKey.Down }, GlobalAction.DecreaseVolume),
            new KeyBinding(new[] { InputKey.MouseWheelDown }, GlobalAction.DecreaseVolume),
        };

        public IEnumerable<KeyBinding> InGameKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.Tilde, GlobalAction.QuickRetry)
        };

        protected override IEnumerable<Drawable> KeyBindingInputQueue =>
            handler == null ? base.KeyBindingInputQueue : new[] { handler }.Concat(base.KeyBindingInputQueue);
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
        [Description("Increase Volume")]
        IncreaseVolume,
        [Description("Decrease Volume")]
        DecreaseVolume,

        // In-Game Keybindings
        [Description("Skip Cutscene")]
        SkipCutscene,
        [Description("Quick Retry (Hold)")]
        QuickRetry,
    }
}

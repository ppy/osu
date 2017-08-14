// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;

namespace osu.Game.Input.Bindings
{
    public class GlobalBindingInputManager : DatabasedKeyBindingInputManager<GlobalAction>
    {
        private readonly Drawable handler;

        public GlobalBindingInputManager(OsuGameBase game)
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        public override IEnumerable<KeyBinding> DefaultMappings => new[]
        {
            new KeyBinding(Key.F8, GlobalAction.ToggleChat),
            new KeyBinding(Key.F9, GlobalAction.ToggleSocial),
            new KeyBinding(new[] { Key.LControl, Key.LAlt, Key.R }, GlobalAction.ResetInputSettings),
            new KeyBinding(new[] { Key.LControl, Key.T }, GlobalAction.ToggleToolbar),
            new KeyBinding(new[] { Key.LControl, Key.O }, GlobalAction.ToggleSettings),
            new KeyBinding(new[] { Key.LControl, Key.D }, GlobalAction.ToggleDirect),
        };

        protected override bool PropagateKeyDown(IEnumerable<Drawable> drawables, InputState state, KeyDownEventArgs args)
        {
            if (handler != null)
                drawables = new[] { handler }.Concat(drawables);

            // always handle ourselves before all children.
            return base.PropagateKeyDown(drawables, state, args);
        }

        protected override bool PropagateKeyUp(IEnumerable<Drawable> drawables, InputState state, KeyUpEventArgs args)
        {
            if (handler != null)
                drawables = new[] { handler }.Concat(drawables);

            // always handle ourselves before all children.
            return base.PropagateKeyUp(drawables, state, args);
        }
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

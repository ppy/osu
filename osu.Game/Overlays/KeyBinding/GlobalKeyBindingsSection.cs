// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;
using osu.Game.Graphics;

namespace osu.Game.Overlays.KeyBinding
{
    public class GlobalKeyBindingsSection : KeyBindingsSection
    {
        private readonly string name;

        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => name;

        public GlobalKeyBindingsSection(KeyBindingInputManager manager, string name)
        {
            this.name = name;

            Defaults = manager.DefaultKeyBindings;
        }
    }
}
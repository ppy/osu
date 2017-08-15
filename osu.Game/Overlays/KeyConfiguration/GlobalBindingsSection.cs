// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;
using osu.Game.Graphics;

namespace osu.Game.Overlays.KeyConfiguration
{
    public class GlobalBindingsSection : KeyBindingsSection
    {
        private readonly string name;

        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => name;

        public GlobalBindingsSection(KeyBindingInputManager manager, string name)
        {
            this.name = name;

            Defaults = manager.DefaultMappings;
        }
    }
}
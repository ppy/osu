// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays.KeyBinding
{
    public class GlobalKeyBindingsSection : SettingsSection
    {
        public override FontAwesome Icon => FontAwesome.fa_osu_hot;
        public override string Header => "Global";

        public GlobalKeyBindingsSection(KeyBindingInputManager manager)
        {
            Add(new DefaultBindingsSubsection(manager));
        }

        private class DefaultBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => string.Empty;

            public DefaultBindingsSubsection(KeyBindingInputManager manager)
                : base(null)
            {
                Defaults = manager.DefaultKeyBindings;
            }
        }
    }
}
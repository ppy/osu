// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays.KeyBinding
{
    public class GlobalKeyBindingsSection : SettingsSection
    {
        public override FontAwesome Icon => FontAwesome.fa_osu_hot;
        public override string Header => "Global";

        public GlobalKeyBindingsSection(GlobalActionContainer manager)
        {
            Add(new DefaultBindingsSubsection(manager));
            Add(new InGameKeyBindingsSubsection(manager));
        }


        private class DefaultBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => string.Empty;

            public DefaultBindingsSubsection(GlobalActionContainer manager)
                : base(null)
            {
                Defaults = manager.GlobalKeyBindings;
            }
        }

        private class InGameKeyBindingsSubsection : KeyBindingsSubsection
        {
            protected override string Header => "In Game";

            public InGameKeyBindingsSubsection(GlobalActionContainer manager) : base(null)
            {
                Defaults = manager.InGameKeyBindings;
            }
        }
    }
}

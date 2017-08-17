// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class RulesetBindingsSection : KeyBindingsSection
    {
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => Ruleset.Name;

        public RulesetBindingsSection(RulesetInfo ruleset)
        {
            Ruleset = ruleset;

            Defaults = ruleset.CreateInstance().GetDefaultKeyBindings();
        }
    }
}
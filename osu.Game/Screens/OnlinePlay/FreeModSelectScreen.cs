// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay
{
    public class FreeModSelectScreen : ModSelectScreen
    {
        protected override bool AllowCustomisation => false;
        protected override bool ShowTotalMultiplier => false;

        public new Func<Mod, bool> IsValidMod
        {
            get => base.IsValidMod;
            set => base.IsValidMod = m => m.HasImplementation && m.UserPlayable && value.Invoke(m);
        }

        public FreeModSelectScreen()
        {
            IsValidMod = _ => true;
        }

        protected override ModColumn CreateModColumn(ModType modType, Key[] toggleKeys = null) => new ModColumn(modType, true, toggleKeys);
    }
}

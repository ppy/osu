// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class UserModSelectScreen : ModSelectScreen
    {
        protected override ModColumn CreateModColumn(ModType modType, Key[] toggleKeys = null) => new UserModColumn(modType, false, toggleKeys);

        private class UserModColumn : ModColumn
        {
            public UserModColumn(ModType modType, bool allowBulkSelection, [CanBeNull] Key[] toggleKeys = null)
                : base(modType, allowBulkSelection, toggleKeys)
            {
            }

            protected override ModPanel CreateModPanel(Mod mod) => new IncompatibilityDisplayingModPanel(mod);
        }
    }
}

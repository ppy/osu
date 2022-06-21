// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// Static factory class for <see cref="IModHotkeyHandler"/>s.
    /// </summary>
    public static class ModHotkeyHandler
    {
        /// <summary>
        /// Creates an appropriate <see cref="IModHotkeyHandler"/> for the given <paramref name="modType"/>.
        /// </summary>
        public static IModHotkeyHandler Create(ModType modType)
        {
            switch (modType)
            {
                case ModType.DifficultyReduction:
                case ModType.DifficultyIncrease:
                case ModType.Automation:
                    return SequentialModHotkeyHandler.Create(modType);

                default:
                    return new NoopModHotkeyHandler();
            }
        }
    }
}

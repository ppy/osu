// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for a mod which can adjust <see cref="PlayerConfiguration"/> settings.
    /// <see cref="PlayerConfiguration"/> will adjust before <see cref="Player"/> loaded.
    /// </summary>
    public interface IApplicableToPlayerConfiguration : IApplicableMod
    {
        void ApplyConfiguration(PlayerConfiguration configuration);
    }
}

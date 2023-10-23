// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public interface IApplicableToPlayerConfiguration : IApplicableMod
    {
        void ApplyConfiguration(PlayerConfiguration configuration);
    }
}

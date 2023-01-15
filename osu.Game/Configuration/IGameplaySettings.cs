// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Configuration
{
    /// <summary>
    /// A settings provider which generally sources from <see cref="OsuConfigManager"/> (global user settings)
    /// but can allow overriding settings by caching more locally. For instance, in the editor compose screen.
    /// </summary>
    /// <remarks>
    /// More settings can be moved into this interface as required.
    /// </remarks>
    [Cached]
    public interface IGameplaySettings
    {
        IBindable<float> ComboColourNormalisationAmount { get; }

        IBindable<float> PositionalHitsoundsLevel { get; }
    }
}

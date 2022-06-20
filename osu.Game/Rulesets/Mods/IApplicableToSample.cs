// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Audio;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make adjustments to a sample.
    /// </summary>
    public interface IApplicableToSample : IApplicableMod
    {
        void ApplyToSample(IAdjustableAudioComponent sample);
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Mixing;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that add effects to the track mixer.
    /// </summary>
    public interface IApplicableToTrackMixer : IApplicableMod
    {
        void ApplyToTrackMixer(IAudioMixer mixer);
    }
}

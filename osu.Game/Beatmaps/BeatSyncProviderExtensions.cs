// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Beatmaps
{
    public static class BeatSyncProviderExtensions
    {
        /// <summary>
        /// Whether the beat sync provider is currently in a kiai section. Should make everything more epic.
        /// </summary>
        public static bool CheckIsKiaiTime(this IBeatSyncProvider provider) => provider.ControlPoints?.EffectPointAt(provider.Clock.CurrentTime).KiaiMode == true;
    }
}

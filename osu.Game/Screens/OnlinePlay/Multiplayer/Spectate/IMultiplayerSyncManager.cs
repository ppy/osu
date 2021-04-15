// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public interface IMultiplayerSyncManager
    {
        IAdjustableClock Master { get; }

        void AddSlave(IMultiplayerSlaveClock clock);

        void RemoveSlave(IMultiplayerSlaveClock clock);
    }
}

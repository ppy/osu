// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public interface IMultiplayerSlaveClock : IAdjustableClock
    {
        IBindable<bool> WaitingOnFrames { get; }

        double LastFrameTime { get; }

        bool IsCatchingUp { get; set; }
    }
}

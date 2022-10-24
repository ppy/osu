// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Online;
using osu.Game.Online.API;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract class RoomPollingComponent : PollingComponent
    {
        [Resolved]
        protected IAPIProvider API { get; private set; }

        [Resolved]
        protected IRoomManager RoomManager { get; private set; }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Online.API.Requests
{
    public class GetRoomsRequest : APIRequest<List<Room>>
    {
        private readonly PrimaryFilter primaryFilter;

        public GetRoomsRequest(PrimaryFilter primaryFilter)
        {
            this.primaryFilter = primaryFilter;
        }

        protected override string Target
            => primaryFilter switch
            {
                PrimaryFilter.Open => "rooms",
                PrimaryFilter.Owned => "rooms/owned",
                PrimaryFilter.Participated => "rooms/participated",
                PrimaryFilter.RecentlyEnded => "rooms/ended",
                _ => throw new ArgumentException($"Unknown enum member {nameof(PrimaryFilter)} {primaryFilter}"),
            };
    }
}

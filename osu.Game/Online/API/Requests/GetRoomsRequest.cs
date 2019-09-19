// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        {
            get
            {
                string target = "rooms";

                switch (primaryFilter)
                {
                    case PrimaryFilter.Open:
                        break;

                    case PrimaryFilter.Owned:
                        target += "/owned";
                        break;

                    case PrimaryFilter.Participated:
                        target += "/participated";
                        break;

                    case PrimaryFilter.RecentlyEnded:
                        target += "/ended";
                        break;
                }

                return target;
            }
        }
    }
}

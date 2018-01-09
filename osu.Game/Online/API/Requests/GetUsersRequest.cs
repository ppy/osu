// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUsersRequest : APIRequest<List<RankingEntry>>
    {
        protected override string Target => @"rankings/osu/performance";
    }

    public class RankingEntry
    {
        [JsonProperty]
        public User User;
    }
}

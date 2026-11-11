// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class SearchUsersResponse
    {
        [JsonProperty("total")]
        public int Total;

        public List<APIUser> Users => data.Users;

        [JsonProperty("user")]
        private UserData data = null!;

        [Serializable]
        private class UserData
        {
            [JsonProperty("data")]
            public List<APIUser> Users = new List<APIUser>();
        }
    }
}

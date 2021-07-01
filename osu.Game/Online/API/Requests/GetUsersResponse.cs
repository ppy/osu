// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUsersResponse : ResponseWithCursor
    {
        [JsonProperty("users")]
        public List<User> Users;
    }
}

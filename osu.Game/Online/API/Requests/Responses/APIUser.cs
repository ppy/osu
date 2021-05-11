// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUser
    {
        [JsonProperty]
        public User User;
    }
}

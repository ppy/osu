// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class TeamMembersResponse
    {
        [JsonProperty("items")]
        public ICollection<APITeamMember> Items { get; set; } = Array.Empty<APITeamMember>();

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}

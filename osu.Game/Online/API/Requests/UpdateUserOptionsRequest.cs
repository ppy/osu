// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.Http;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UpdateUserOptionsRequest : APIRequest<APIMe>
    {
        [JsonProperty(@"user")]
        public UserSettingUpdate? UserSettings { get; set; }

        [Serializable]
        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class UserSettingUpdate
        {
            [JsonProperty(@"pm_friends_only")]
            public bool? PMFriendsOnly { get; set; }
        }

        protected override string Target => @"me/options";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            req.ContentType = @"application/json";
            req.AddRaw(JsonConvert.SerializeObject(this));
            return req;
        }
    }
}

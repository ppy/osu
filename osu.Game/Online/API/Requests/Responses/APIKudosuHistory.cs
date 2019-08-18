// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIKudosuHistory
    {
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt;

        [JsonProperty("amount")]
        private int amount
        {
            set => Amount = Math.Abs(value);
        }

        public int Amount;

        [JsonProperty("post")]
        public ModdingPost Post;

        public class ModdingPost
        {
            [JsonProperty("url")]
            public string Url;

            [JsonProperty("title")]
            public string Title;
        }

        [JsonProperty("giver")]
        public KudosuGiver Giver;

        public class KudosuGiver
        {
            [JsonProperty("url")]
            public string Url;

            [JsonProperty("username")]
            public string Username;
        }

        [JsonProperty("action")]
        private string action
        {
            set
            {
                string parsed = value.Contains(".") ? value.Split('.')[0].Pascalize() + value.Split('.')[1].Pascalize() : value.Pascalize();

                Action = (KudosuAction)Enum.Parse(typeof(KudosuAction), parsed);
            }
        }

        public KudosuAction Action;
    }
}

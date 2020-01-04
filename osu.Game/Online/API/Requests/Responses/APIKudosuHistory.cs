// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIKudosuHistory
    {
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt;

        [JsonProperty("amount")]
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

        public KudosuSource Source;

        public KudosuAction Action;

        [JsonProperty("action")]
        private string action
        {
            set
            {
                // incoming action may contain a prefix. if it doesn't, it's a legacy forum event.

                string[] split = value.Split('.');

                if (split.Length > 1)
                    Enum.TryParse(split.First().Replace("_", ""), true, out Source);
                else
                    Source = KudosuSource.Forum;

                Enum.TryParse(split.Last(), true, out Action);
            }
        }
    }

    public enum KudosuSource
    {
        Unknown,
        AllowKudosu,
        Delete,
        DenyKudosu,
        Forum,
        Recalculate,
        Restore,
        Vote
    }

    public enum KudosuAction
    {
        Give,
        Reset,
        Revoke,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using System;
using System.Net;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APINewsPost
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        private string author;

        [JsonProperty("author")]
        public string Author
        {
            get => author;
            set => author = WebUtility.HtmlDecode(value);
        }

        [JsonProperty("edit_url")]
        public string EditUrl { get; set; }

        [JsonProperty("first_image")]
        public string FirstImage { get; set; }

        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        private string title;

        [JsonProperty("title")]
        public string Title
        {
            get => title;
            set => title = WebUtility.HtmlDecode(value);
        }

        private string preview;

        [JsonProperty("preview")]
        public string Preview
        {
            get => preview;
            set => preview = WebUtility.HtmlDecode(value);
        }
    }
}

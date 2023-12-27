// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISystemTitle : IEquatable<APISystemTitle>
    {
        [JsonProperty(@"image")]
        public string Image { get; set; } = string.Empty;

        [JsonProperty(@"url")]
        public string Url { get; set; } = string.Empty;

        public bool Equals(APISystemTitle? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Image == other.Image && Url == other.Url;
        }

        public override bool Equals(object? obj) => obj is APISystemTitle other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Image, Url);
    }
}

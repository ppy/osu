// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMenuImage : IEquatable<APIMenuImage>
    {
        /// <summary>
        /// A URL pointing to the image which should be displayed. Generally should be an @2x image filename.
        /// </summary>
        [JsonProperty(@"image")]
        public string Image { get; init; } = string.Empty;

        /// <summary>
        /// A URL that should be opened on clicking the image.
        /// </summary>
        [JsonProperty(@"url")]
        public string Url { get; init; } = string.Empty;

        /// <summary>
        /// The time at which this item should begin displaying. If <c>null</c>, will display immediately.
        /// </summary>
        [JsonProperty(@"begins")]
        public DateTimeOffset? Begins { get; set; }

        /// <summary>
        /// The time at which this item should stop displaying. If <c>null</c>, will display indefinitely.
        /// </summary>
        [JsonProperty(@"expires")]
        public DateTimeOffset? Expires { get; set; }

        public bool Equals(APIMenuImage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Image == other.Image && Url == other.Url;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((APIMenuImage)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Image, Url);
        }
    }
}

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

        public bool IsCurrent =>
            (Begins == null || Begins < DateTimeOffset.UtcNow) &&
            (Expires == null || Expires > DateTimeOffset.UtcNow);

        /// <summary>
        /// The time at which this item should begin displaying. If <c>null</c>, will display immediately.
        /// </summary>
        [JsonProperty(@"begins")]
        public DateTimeOffset? Begins { get; init; }

        /// <summary>
        /// The time at which this item should stop displaying. If <c>null</c>, will display indefinitely.
        /// </summary>
        [JsonProperty(@"expires")]
        public DateTimeOffset? Expires { get; init; }

        public bool Equals(APIMenuImage? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Image == other.Image && Url == other.Url && Begins == other.Begins && Expires == other.Expires;
        }

        public override bool Equals(object? other) => other is APIMenuImage content && Equals(content);

        public override int GetHashCode()
        {
            return HashCode.Combine(Image, Url, Begins, Expires);
        }
    }
}

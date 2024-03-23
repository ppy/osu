// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMenuContent : IEquatable<APIMenuContent>
    {
        /// <summary>
        /// Images which should be displayed in rotation.
        /// </summary>
        [JsonProperty(@"images")]
        public APIMenuImage[] Images { get; init; } = Array.Empty<APIMenuImage>();

        public bool Equals(APIMenuContent? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Images.SequenceEqual(other.Images);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType()) return false;

            return Equals((APIMenuContent)obj);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (var image in Images)
                hash.Add(image.GetHashCode());

            return hash.ToHashCode();
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUpdateStream : IEquatable<APIUpdateStream>
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_featured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("latest_build")]
        public APIChangelogBuild LatestBuild { get; set; }

        [JsonProperty("user_count")]
        public int UserCount { get; set; }

        public bool Equals(APIUpdateStream other) => Id == other?.Id;

        internal static readonly Dictionary<string, Color4> KNOWN_STREAMS = new Dictionary<string, Color4>
        {
            ["stable40"] = new Color4(102, 204, 255, 255),
            ["stable"] = new Color4(34, 153, 187, 255),
            ["beta40"] = new Color4(255, 221, 85, 255),
            ["cuttingedge"] = new Color4(238, 170, 0, 255),
            [OsuGameBase.CLIENT_STREAM_NAME] = new Color4(237, 18, 33, 255),
            ["web"] = new Color4(136, 102, 238, 255)
        };

        public ColourInfo Colour => KNOWN_STREAMS.TryGetValue(Name, out var colour) ? colour : new Color4(0, 0, 0, 255);
    }
}

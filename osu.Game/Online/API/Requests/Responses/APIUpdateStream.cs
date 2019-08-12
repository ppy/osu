// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        public bool Equals(APIUpdateStream other) => Id == other?.Id;

        public ColourInfo Colour
        {
            get
            {
                switch (Name)
                {
                    case "stable40":
                        return new Color4(102, 204, 255, 255);

                    case "stable":
                        return new Color4(34, 153, 187, 255);

                    case "beta40":
                        return new Color4(255, 221, 85, 255);

                    case "cuttingedge":
                        return new Color4(238, 170, 0, 255);

                    case OsuGameBase.CLIENT_STREAM_NAME:
                        return new Color4(237, 18, 33, 255);

                    case "web":
                        return new Color4(136, 102, 238, 255);

                    default:
                        return new Color4(0, 0, 0, 255);
                }
            }
        }
    }
}

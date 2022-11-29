// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Newtonsoft.Json;

namespace osu.Game.Online.Rooms
{
    // TODO: Remove disable below after merging https://github.com/ppy/osu-framework/pull/5548 and applying follow-up changes game-side.
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class APICreatedRoom : Room
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}

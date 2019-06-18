// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A beatmap choice by a team from a tournament's map pool.
    /// </summary>
    [Serializable]
    public class BeatmapChoice
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ChoiceType Type;

        public int BeatmapID;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamColour
    {
        Red,
        Blue
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChoiceType
    {
        Pick,
        Ban,
    }
}

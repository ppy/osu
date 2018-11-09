// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
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

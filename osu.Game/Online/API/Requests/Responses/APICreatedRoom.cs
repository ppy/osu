// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APICreatedRoom : Room
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}

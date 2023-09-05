// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISoloScoreToken : IAPIScoreToken
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        public ScoreTokenType Type => ScoreTokenType.Solo;
    }
}

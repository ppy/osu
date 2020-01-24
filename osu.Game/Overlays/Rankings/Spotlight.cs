// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Overlays.Rankings
{
    public class Spotlight
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("text")]
        public string Text;

        public override string ToString() => Text;
    }
}

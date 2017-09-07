// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Scoring
{
    public class OnlineScore : Score
    {
        [JsonProperty(@"mods")]
        private string[] modStrings { get; set; }

        public void GetModsFor(RulesetInfo ruleset)
        {
            Mods = ruleset.CreateInstance().GetAllMods().Where(mod => modStrings.Contains(mod.ShortenedName)).ToArray();
        }
    }
}

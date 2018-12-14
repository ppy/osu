// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Rulesets.Mods
{
    public interface IMod
    {
        /// <summary>
        /// The shortened name of this mod.
        /// </summary>
        [JsonProperty("acronym")]
        string Acronym { get; }
    }
}

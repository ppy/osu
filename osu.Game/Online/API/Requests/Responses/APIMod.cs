// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMod : IMod
    {
        public string Acronym { get; set; }
    }
}

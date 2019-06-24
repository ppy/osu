// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIMod : IMod
    {
        public string Acronym { get; set; }

        public bool Equals(IMod other) => Acronym == other?.Acronym;
    }
}

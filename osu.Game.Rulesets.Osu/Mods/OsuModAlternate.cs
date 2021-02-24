// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : ModAlternate<OsuHitObject, OsuAction>
    {
        protected override bool OnPressed(OsuAction action)
        {
            if (LastActionPressed == action)
                return true;

            LastActionPressed = action;

            return false;
        }
    }
}

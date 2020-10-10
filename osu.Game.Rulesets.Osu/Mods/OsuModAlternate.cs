// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : ModAlternate<OsuHitObject, OsuAction>
    {
        private OsuAction? lastActionPressed;
        private OsuAction? lastActionReleased;

        protected override void ResetActionStates()
        {
            lastActionPressed = lastActionReleased = null;
        }

        protected override bool OnPressed(OsuAction action)
        {
            if (!ShouldCheckForInput)
                return false;

            if (lastActionPressed == action)
                return true;

            lastActionPressed = action;
            return false;
        }

        protected override bool OnReleased(OsuAction action)
        {
            if (!ShouldCheckForInput)
                return false;

            if (lastActionReleased == action)
                return true;

            lastActionReleased = action;
            return false;
        }
    }
}

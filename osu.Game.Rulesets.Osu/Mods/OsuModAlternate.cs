// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;


namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : ModAlternate<OsuHitObject, OsuAction>
    {

        public override bool checkCorrectAction(OsuAction action)
        {
            if (isBreakTime.Value)
                return true;

            if (gameplayClock.CurrentTime < firstObjectValidJudgementTime)
                return true;

            switch (action)
            {
                case OsuAction.LeftButton:
                case OsuAction.RightButton:
                    break;

                // Any action which is not left or right button should be ignored.
                default:
                    return true;
            }

            if (lastActionPressed != action)
            {
                // User alternated correctly.
                lastActionPressed = action;
                return true;
            }

            ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
            return false;
        }
    }
}

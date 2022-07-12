// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : InputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"ST";
        public override string Description => @"You must only use one key!";

        protected override bool CheckCorrectAction(OsuAction action)
        {
            if (base.CheckCorrectAction(action))
                return true;

            if (LastActionPressed == null)
            {
                // First keypress, store the expected action.
                LastActionPressed = action;
                return true;
            }

            if (LastActionPressed == action)
            {
                // User singletapped correctly.
                return true;
            }

            Ruleset.Cursor.FlashColour(Colour4.Red, FLASH_DURATION, Easing.OutQuint);
            return false;
        }
    }
}

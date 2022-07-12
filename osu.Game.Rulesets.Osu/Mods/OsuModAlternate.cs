// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : InputBlockingMod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override string Description => @"Don't use the same key twice in a row!";
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;

        protected override bool CheckCorrectAction(OsuAction action)
        {
            if (base.CheckCorrectAction(action))
                return true;

            if (LastActionPressed != action)
            {
                // User alternated correctly.
                LastActionPressed = action;
                return true;
            }

            Ruleset.Cursor.FlashColour(Colour4.Red, FLASH_DURATION, Easing.OutQuint);
            return false;
        }
    }
}

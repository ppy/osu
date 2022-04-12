// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : InputBlockingMod
    {
        public override string Name => "Alternate";
        public override string Acronym => "AL";
        public override string Description => @"Don't use the same key twice in a row!";
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
        public override bool checkCorrectAction(OsuAction action)

        {
            if(base.checkCorrectAction(action))
                return true;

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

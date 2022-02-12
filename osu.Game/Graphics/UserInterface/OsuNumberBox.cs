// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuNumberBox : OsuTextBox
    {
        protected override bool AllowIme => false;

        protected override bool CanAddCharacter(char character) => character.IsAsciiDigit();
    }
}

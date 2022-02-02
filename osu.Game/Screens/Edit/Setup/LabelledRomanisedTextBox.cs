// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class LabelledRomanisedTextBox : LabelledTextBox
    {
        protected override OsuTextBox CreateTextBox() => new RomanisedTextBox();

        private class RomanisedTextBox : OsuTextBox
        {
            protected override bool AllowIme => false;

            protected override bool CanAddCharacter(char character)
                => MetadataUtils.IsRomanised(character);
        }
    }
}

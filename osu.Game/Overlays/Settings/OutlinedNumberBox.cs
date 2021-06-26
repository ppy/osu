// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Settings
{
    public class OutlinedNumberBox : OutlinedTextBox
    {
        protected override bool CanAddCharacter(char character) => char.IsNumber(character);
    }
}

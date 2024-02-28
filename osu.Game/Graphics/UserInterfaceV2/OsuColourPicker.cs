// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuColourPicker : ColourPicker
    {
        public OsuColourPicker()
        {
            CornerRadius = 10;
            Masking = true;
        }

        protected override HSVColourPicker CreateHSVColourPicker() => new OsuHSVColourPicker();
        protected override HexColourPicker CreateHexColourPicker() => new OsuHexColourPicker();
    }
}

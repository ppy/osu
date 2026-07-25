// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class OsuColourPicker : ColourPicker
    {
        /// <summary>
        /// Optional preset colours shown between the HSV and hex controls.
        /// </summary>
        public BindableList<Colour4> Suggestions { get; } = new BindableList<Colour4>();

        public OsuColourPicker()
        {
            CornerRadius = 10;
            Masking = true;
        }

        protected override HSVColourPicker CreateHSVColourPicker() => new OsuHSVColourPicker();

        protected override SwatchColourPicker? CreateSwatchColourPicker() => new OsuSwatchColourPicker(Suggestions);

        protected override HexColourPicker CreateHexColourPicker() => new OsuHexColourPicker();
    }
}

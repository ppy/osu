// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormNumberBox : FormTextBox
    {
        public bool AllowDecimals { get; init; }

        internal override InnerTextBox CreateTextBox() => new InnerNumberBox
        {
            AllowDecimals = AllowDecimals,
            SelectAllOnFocus = true,
        };

        internal partial class InnerNumberBox : InnerTextBox
        {
            public bool AllowDecimals { get; init; }

            public InnerNumberBox()
            {
                InputProperties = new TextInputProperties(TextInputType.Number, false);
            }

            protected override bool CanAddCharacter(char character)
                => char.IsAsciiDigit(character) || (AllowDecimals && CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.Contains(character));
        }
    }
}

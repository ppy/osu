// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormNumberBox : FormTextBox
    {
        private readonly bool allowDecimals;

        public FormNumberBox(bool allowDecimals = false)
        {
            this.allowDecimals = allowDecimals;
        }

        internal override InnerTextBox CreateTextBox() => new InnerNumberBox(allowDecimals)
        {
            SelectAllOnFocus = true,
        };

        internal partial class InnerNumberBox : InnerTextBox
        {
            public InnerNumberBox(bool allowDecimals)
            {
                InputProperties = new TextInputProperties(allowDecimals ? TextInputType.Decimal : TextInputType.Number, false);
            }
        }
    }
}

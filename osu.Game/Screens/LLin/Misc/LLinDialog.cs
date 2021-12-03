using System.Collections.Generic;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Screens.LLin.Plugins;

namespace osu.Game.Screens.LLin.Misc
{
    internal class LLinDialog : PopupDialog
    {
        private readonly ColourInfo defaultColorInfo = new ColourInfo();

        public LLinDialog(IconUsage icon, LocalisableString title, LocalisableString text, DialogOption[] options)
        {
            Icon = icon;
            HeaderText = title;
            BodyText = text;

            IList<PopupDialogButton> buttons = new List<PopupDialogButton>();

            foreach (var option in options)
            {
                if (option.Color.Equals(defaultColorInfo))
                    option.Color = ColourInfo.SingleColour(new SRGBColour());

                switch (option.Type)
                {
                    case OptionType.Confirm:
                        buttons.Add(new PopupDialogOkButton
                        {
                            Action = option.Action,
                            Text = option.Text,
                            ButtonColour = option.Color
                        });
                        break;

                    case OptionType.Cancel:
                        buttons.Add(new PopupDialogCancelButton
                        {
                            Action = option.Action,
                            Text = option.Text,
                            ButtonColour = option.Color
                        });
                        break;

                    default:
                        buttons.Add(new PopupDialogButton
                        {
                            Action = option.Action,
                            Text = option.Text,
                            ButtonColour = option.Color
                        });
                        break;
                }
            }

            Buttons = buttons;
        }
    }
}

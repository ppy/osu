// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Localisation;

namespace osu.Game.Tournament.Components
{
    public partial class IPCErrorDialog : PopupDialog
    {
        public IPCErrorDialog(LocalisableString headerText, LocalisableString bodyText)
        {
            Icon = FontAwesome.Regular.SadTear;
            HeaderText = headerText;
            BodyText = bodyText;
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = BaseStrings.Okay,
                    Action = () => Expire()
                }
            };
        }
    }
}

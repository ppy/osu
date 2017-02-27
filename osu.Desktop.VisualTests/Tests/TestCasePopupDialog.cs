// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCasePopupDialog : TestCase
    {
        public override string Name => @"Popup Dialog";

        public override string Description => @"With various dialogs";

        public override void Reset()
        {
            base.Reset();

            var firstDialog = new PopupDialog
            {
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.fa_trash_o,
                HeaderText = @"Confirm deletion of",
                BodyText = @"Ayase Rie - Yuima-ru*World TVver.",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOKButton
                    {
                        Text = @"I never want to see this again.",
                        Action = () => System.Console.WriteLine(@"OK"),
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Firetruck, I still want quick ranks!",
                        Action = () => System.Console.WriteLine(@"Cancel"),
                    },
                },
            };
            var secondDialog = new PopupDialog
            {
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.fa_gear,
                HeaderText = @"What do you want to do with",
                BodyText = "Camellia as \"Bang Riot\" - Blastix Riotz",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOKButton
                    {
                        Text = @"Manage collections",
                    },
                    new PopupDialogOKButton
                    {
                        Text = @"Delete...",
                    },
                    new PopupDialogOKButton
                    {
                        Text = @"Remove from unplayed",
                    },
                    new PopupDialogOKButton
                    {
                        Text = @"Clear local scores",
                    },
                    new PopupDialogOKButton
                    {
                        Text = @"Edit",
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Cancel",
                    },
                },
            };

            Add(firstDialog);
            Add(secondDialog);

            AddButton("dialog #1", firstDialog.ToggleVisibility);
            AddButton("dialog #2", secondDialog.ToggleVisibility);
        }
    }
}

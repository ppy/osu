// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseDialogManager : TestCase
    {
        public override string Name => @"Dialog Manager";

        public override string Description => @"Display dialogs";

        public override void Reset()
        {
            base.Reset();

            var manager = new DialogManager();
            Add(manager);

            AddButton("dialog #1", () => manager.Push(new PopupDialog
            {
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.fa_trash_o,
                HeaderText = @"Confirm deletion of",
                BodyText = @"Ayase Rie - Yuima-ru*World TVver.",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
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
            }));

            AddButton("dialog #2", () => manager.Push(new PopupDialog
            {
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.fa_gear,
                HeaderText = @"What do you want to do with",
                BodyText = "Camellia as \"Bang Riot\" - Blastix Riotz",
                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogOkButton
                    {
                        Text = @"Manage collections",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Delete...",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Remove from unplayed",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Clear local scores",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"Edit",
                    },
                    new PopupDialogCancelButton
                    {
                        Text = @"Cancel",
                    },
                },
            }));
        }
    }
}

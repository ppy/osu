//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using osu.Game.Overlays.PopUpDialogs;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCasePopUpDialog: TestCase
    {
        public override string Name => @"PopUpDialog";

        public override string Description => @"Test some Popup Dialogs";

        private TestPopUpDialog dialog;

        public override void Reset()
        {
            base.Reset();

            Children = new Drawable[]
            {
                dialog = new TestPopUpDialog(),
            };
            dialog.nextPopup = NextPopup;
            AddButton(@"Toggle Dialog", ToggleDialog);
        }


        public void ToggleDialog()
        {
            dialog.ToggleVisibility();
        }

        //Keep Toggle Dialog button wired up to the latest nest
        public void NextPopup()
        {
            TestPopUpDialog currDialog = dialog;
            dialog = new TestPopUpDialog();
            dialog.nextPopup = NextPopup;
            currDialog.Nest(dialog);
        }

        public class TestPopUpDialog : PopUpDialog
        {
            private static int nests = 0;
            public Action nextPopup;
            protected override FontAwesome icon => FontAwesome.fa_thumbs_up;
            protected override string title => "TEST";

            protected override Container<Drawable> CreateBody()
            {
                FlowContainer bodyCont = new FlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Drawable[]
                    {
                        new PopUpDialogButton
                        {
                            Text = $"Nest Test++ {nests}",
                            Colour = new Color4(238, 51, 153, 255),
                            BackgroundColour = new Color4(159, 14, 102, 255),
                            Width = button_width,
                            Height = button_height,
                            BackgroundWidth = button_background_width,
                            BackgroundHeight = button_height,
                            Action = () => 
                            {
                                nests++;
                                nextPopup.Invoke();
                            },
                        },
                        new PopUpDialogButton
                        {
                            Text = $"Nest Test-- {nests}",
                            Colour = new Color4(68, 170, 221, 225),
                            BackgroundColour = new Color4(14, 116, 145, 255),
                            Width = button_width,
                            Height = button_height,
                            BackgroundWidth = button_background_width,
                            BackgroundHeight = button_height,
                            Action = () =>
                            {
                                nests--;
                                nextPopup.Invoke();
                            },
                        }
                    }
                };
                return bodyCont;
            }
        }


    }
}

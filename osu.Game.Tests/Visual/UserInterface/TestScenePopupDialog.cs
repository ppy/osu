// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestScenePopupDialog : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PopupDialogOkButton),
            typeof(PopupDialogCancelButton),
            typeof(PopupDialogButton),
            typeof(DialogButton),
        };

        public TestScenePopupDialog()
        {
            AddStep("new popup", () =>
                Add(new TestPopupDialog
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = Framework.Graphics.Containers.Visibility.Visible },
                }));
        }

        private class TestPopupDialog : PopupDialog
        {
            public TestPopupDialog()
            {
                Icon = FontAwesome.Solid.AssistiveListeningSystems;

                HeaderText = @"This is a test popup";
                BodyText = "I can say lots of stuff and even wrap my words!";

                Buttons = new PopupDialogButton[]
                {
                    new PopupDialogCancelButton
                    {
                        Text = @"Yes. That you can.",
                    },
                    new PopupDialogOkButton
                    {
                        Text = @"You're a fake!",
                    },
                };
            }
        }
    }
}

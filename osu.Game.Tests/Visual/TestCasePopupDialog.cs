﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCasePopupDialog : OsuTestCase
    {
        public TestCasePopupDialog()
        {
            var popup = new PopupDialog
            {
                RelativeSizeAxes = Axes.Both,
                State = Framework.Graphics.Containers.Visibility.Visible,
                Icon = FontAwesome.fa_assistive_listening_systems,
                HeaderText = @"This is a test popup",
                BodyText = "I can say lots of stuff and even wrap my words!",
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
                }
            };

            Add(popup);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneFooterButtons : ScreenTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            //typeof(Screens.Select.SongSelect),
            typeof(Footer),
            typeof(FooterButton),
        };

        private Footer testFooter;
        private FooterButton dummyButton1, dummyButton2, dummyButton3;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            testFooter = new Footer();
            dummyButton1 = new FooterButton { SelectedColour = colours.Yellow, DeselectedColour = colours.Yellow.Opacity(0.5f), Text = @"mods" };
            dummyButton2 = new FooterButton { SelectedColour = colours.Green, DeselectedColour = colours.Green.Opacity(0.5f), Text = @"random" };
            dummyButton3 = new FooterButton { SelectedColour = colours.Blue, DeselectedColour = colours.Blue.Opacity(0.5f), Text = @"options" };

            testFooter.AddButton(dummyButton1);
            testFooter.AddButton(dummyButton2);
            testFooter.AddButton(dummyButton3);
            Add(testFooter);
        }
    }
}

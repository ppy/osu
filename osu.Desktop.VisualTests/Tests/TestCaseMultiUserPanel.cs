// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Screens.Multiplayer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiUserPanel : TestCase
    {
        private MultiUserPanel test;
        private FillFlowContainer panelContainer;
        public User User = new User
        {
            Id = 9492835,
            Username = @"Naeferith",
            Country = new Country
            {
                FullName = @"France",
                FlagName = @"FR",
            },
        };
        public override string Description => @"User pannel in a multiplayer room";

        private void action(int action)
        {
            switch (action)
            {
                case 0:
                    test.State = test.State == MultiUserPanel.UserState.Host ? MultiUserPanel.UserState.Guest : MultiUserPanel.UserState.Host;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            AddStep(@"Switch status", () => action(0));

            Add(panelContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Full,
                Size = new Vector2(0.9f, 0.5f),
                Children = new Drawable[]
                {
                    test = new MultiUserPanel(User),
                }
            });
        }
    }
}

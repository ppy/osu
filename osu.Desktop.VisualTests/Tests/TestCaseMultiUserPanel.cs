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
using System.Linq;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiUserPanel : TestCase
    {
        private MultiUserPanel host;
        private FillFlowContainer panelContainer;
        public override string Description => @"User pannel in a multiplayer room";

        private void action(int action)
        {
            switch (action)
            {
                case 0:
                    User User = new User
                    {
                        Id = 9492835,
                        Username = @"Naeferith",
                        Country = new Country
                        {
                            FullName = @"France",
                            FlagName = @"FR",
                        },
                    };
                    if (panelContainer.Children.Count() == 0)
                    {
                        panelContainer.Add(host = new MultiUserPanel(User));
                        host.State = MultiUserPanel.UserState.Host;
                    }
                    else panelContainer.Add(new MultiUserPanel(User));
                    break;
                case 1:
                    System.Random rnd = new System.Random();
                    int newHost = rnd.Next(panelContainer.Children.Count());
                    if (panelContainer.Children.Count() > 1) while (panelContainer.Children.ElementAt(newHost).Equals(host)) newHost = rnd.Next(panelContainer.Children.Count());
                    MultiUserPanel newHostPanel = (MultiUserPanel)panelContainer.Children.ElementAt(newHost);
                    host.State = MultiUserPanel.UserState.Guest;
                    host = newHostPanel;
                    host.State = MultiUserPanel.UserState.Host;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            Add(panelContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Full,
                Size = new Vector2(0.9f, 0.5f),
            });

            AddStep(@"Add Panel", () => action(0));
            AddStep(@"Random Host", () => action(1));
            
        }
    }
}

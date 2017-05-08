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
using osu.Framework.MathUtils;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using System.Linq;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiUserPanel : TestCase
    {
        private MultiUserPanelContainer panelContainer;
        public override string Description => @"User pannel in a multiplayer room";

        private void addPanel()
        {
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
            panelContainer.Add(new MultiUserPanel(User));
        }

        private void rotHost()
        {
            if (panelContainer.Children.Count() > 1)
            {
                MultiUserPanel currentHost = panelContainer.Host;
                if (currentHost == null)
                {
                    currentHost = panelContainer.Children.First();
                    currentHost.Host = true;
                }
                else
                {
                    int newHost = 0;
                    while (!(panelContainer.Children.ElementAt(newHost).Host)) newHost += 1;
                    MultiUserPanel newHostPanel = (MultiUserPanel)panelContainer.Children.ElementAt(newHost);
                    if (newHost < panelContainer.Children.Count() - 1) newHost += 1;
                    else newHost = 0;
                    newHostPanel = (MultiUserPanel)panelContainer.Children.ElementAt(newHost);
                    foreach (MultiUserPanel panels in panelContainer.Children)
                    {
                        panels.Host = false;
                    }
                    newHostPanel.Host = true;
                }
            }
        }

        public override void Reset()
        {
            base.Reset();

            Add(panelContainer = new MultiUserPanelContainer()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Full,
                Size = new Vector2(0.9f, 0.5f),
            });

            AddStep(@"Add Panel", addPanel);
            AddStep(@"Host Rotation", rotHost);
        }
    }
}

// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Screens.Multiplayer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseMultiUserPanel : TestCase
    {
        private MultiUserPanel test;
        private FlowContainer panelContainer;
        public override string Name => @"MultiUserPanel";
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

            AddButton(@"Switch status", () => action(0));

            Add(panelContainer = new FlowContainer //Positionning container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FlowDirections.Horizontal,
                Size = new Vector2(0.9f, 0.5f),
                Children = new Drawable[]
                {
                    test = new MultiUserPanel("Naeferith"),
                    new MultiUserPanel(),
                }
            });
        }
    }
}

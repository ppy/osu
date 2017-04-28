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
    class TestCaseMultiRoomPanel : TestCase
    {
        private MultiRoomPanel test;
        private FlowContainer panelContainer;
        public override string Name => @"MultiRoomPanel";
        public override string Description => @"Select your favourite room";

        private void action(int action)
        {
            switch (action)
            {
                case 0:
                    test.State = test.State == MultiRoomPanel.PanelState.Free ? MultiRoomPanel.PanelState.Busy : MultiRoomPanel.PanelState.Free;
                    break;
            }
        }

        public override void Reset()
        {
            base.Reset();

            AddButton(@"ChangeState", () => action(0));

            Add(panelContainer = new FlowContainer //Positionning container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FlowDirections.Vertical,
                Size = new Vector2(0.4f, 0.5f),
                Children = new Drawable[]
                {
                    test = new MultiRoomPanel("Great Room Right Here", "Naeferith", 0),
                    new MultiRoomPanel("Relax it's the weekend", "Someone", 1),
                }
            });
        }
        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            foreach (MultiRoomPanel panel in panelContainer.Children)
            {
                panel.BorderThickness = 0;
                if (panel.Clicked == true)
                {
                    panel.BorderThickness = 3;
                    panel.Clicked = false;
                }
            }
            return base.OnMouseUp(state, args);
        }
    }
}

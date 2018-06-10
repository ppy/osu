// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralScreen : EditorScreen
    {
        private readonly Container content;

        public override string Title => "General";

        // protected override Container<Drawable> TransitionContent => content;

        public GeneralScreen()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    //Colour = OsuColour.FromHex("222d31"),
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            //Width = 0.55f,
                            //Padding = new MarginPadding
                            //{
                            //    Vertical = 35 - DrawableRoom.SELECTION_BORDER_WIDTH,
                            //    Right = 20 - DrawableRoom.SELECTION_BORDER_WIDTH
                            //},
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Margin = new MarginPadding { Left = 75, Top = 100 },
                                    Colour = Color4.White,
                                    Text = "Metadata",
                                    TextSize = 18,
                                    Font = @"Exo2.0-Bold",
                                },
                            }
                        }
                    },
                },
            };
        }
    }
}

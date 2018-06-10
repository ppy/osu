// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DifficultyScreen : EditorScreen
    {
        private readonly Container content;

        public override string Title => "Difficulty";

        // protected override Container<Drawable> TransitionContent => content;

        public DifficultyScreen()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.55f,
                            //Padding = new MarginPadding
                            //{
                            //    Vertical = 35 - DrawableRoom.SELECTION_BORDER_WIDTH,
                            //    Right = 20 - DrawableRoom.SELECTION_BORDER_WIDTH
                            //},
                            //Child = search = new SearchContainer
                            //{
                            //    RelativeSizeAxes = Axes.X,
                            //    AutoSizeAxes = Axes.Y,
                            //    Child = RoomsContainer = new RoomsFilterContainer
                            //    {
                            //        RelativeSizeAxes = Axes.X,
                            //        AutoSizeAxes = Axes.Y,
                            //        Direction = FillDirection.Vertical,
                            //        Spacing = new Vector2(10 - DrawableRoom.SELECTION_BORDER_WIDTH * 2),
                            //    },
                            //},
                        },
                    },
                },
            };
        }
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupCircularButton : TriangleButton
    {
        private const float size_x = 125;
        private const float size_y = 30;

        public SetupCircularButton()
        {
            Size = new Vector2(size_x, size_y);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Triangles.Alpha = 0;
            Content.CornerRadius = 15;
        }
    }
}

// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class BorderLayer : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public BorderLayer()
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Name = "Border",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderColour = Color4.White,
                    BorderThickness = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true
                    }
                },
                content = new Container { RelativeSizeAxes = Axes.Both }
            };
        }
    }
}

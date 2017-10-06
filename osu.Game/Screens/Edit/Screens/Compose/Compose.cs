// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Edit.Screens.Compose
{
    public class Compose : EditorScreen
    {
        public Compose()
        {
            Children = new[]
            {
                new Container
                {
                    Name = "Timeline",
                    RelativeSizeAxes = Axes.X,
                    Height = 110,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f)
                        },
                        new Container
                        {
                            Name = "Content",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Horizontal = 17, Vertical = 10 }
                        }
                    }
                }
            };
        }
    }
}

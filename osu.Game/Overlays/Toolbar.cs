//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class Toolbar : Container
    {
        const float height = 50;

        public override void Load()
        {
            base.Load();

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.9f)
                }
            };
        }
    }
}

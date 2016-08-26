//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.GameModes
{
    class FontTest : OsuGameMode
    {
        private FlowContainer flow;

        public override void Load()
        {
            base.Load();

            flow = new FlowContainer()
            {
                Anchor = Anchor.TopLeft,
                Direction = FlowDirection.VerticalOnly
            };
            Add(flow);

            for (int i = 1; i < 50; i++)
            {
                SpriteText text = new SpriteText()
                {
                    Text = $@"Font testy at size {i}",
                    Scale = i
                };

                flow.Add(text);
            }
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}

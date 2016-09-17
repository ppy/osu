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
using OpenTK;

namespace osu.Game.GameModes
{
    class FontTest : OsuGameMode
    {
        private FlowContainer flow;

		public override void Load()
		{
			base.Load();

			Drawable[] flowChildren = new Drawable[50];
			for (int i = 0; i < 50; i++)
			{
				int j = i + 1;
				flowChildren[i] = new SpriteText()
				{
					Text = $@"Font testy at size {j}",
					Scale = new Vector2(j)
				};
			}

			Add(flow = new FlowContainer()
			{
				Anchor = Anchor.TopLeft,
				Direction = FlowDirection.VerticalOnly,
				Children = flowChildren,
			});


		}

        protected override void Update()
        {
            base.Update();
        }
    }
}

// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;


namespace osu.Framework.Graphics
{
	class KeyCounter : OsuComponent
	{
		public bool isCounting = true;
		internal FlowContainer flow;

		public override void Load()
		{
			base.Load();

			Children = new Drawable[]
			{
				flow = new BoundsBypassFlowContainer
				{
					Direction = FlowDirection.HorizontalOnly,
					LayoutEasing = EasingTypes.Out,
				}
			};
		}

		public override bool Contains(Vector2 screenSpacePos) => true;

		public void AddKey(Count addCount) 
		{
			addCount.keyCounter = this;
			flow.Add(addCount);
		}
	}
}
